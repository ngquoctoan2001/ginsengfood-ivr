using System.Collections.Concurrent;
using System.Net.Http.Headers;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Ivr.Domain.Ports;
using Ivr.Domain.Speech;
using Microsoft.Extensions.Options;

namespace Ivr.Infrastructure.Telephony;

public sealed class AsteriskAriOperationException(
    SimProviderDisposition disposition,
    string technicalErrorCode,
    bool? channelHealthy,
    string message,
    Exception? innerException = null) : Exception(message, innerException)
{
    public SimProviderDisposition Disposition { get; } = disposition;

    public string TechnicalErrorCode { get; } = technicalErrorCode;

    /// <summary>
    /// What the failure says about the SIM channel, read as <see cref="SimDispositionReport.ChannelHealthy"/>
    /// is: <c>null</c> when it says nothing either way (W-0367 / K-57).
    /// </summary>
    public bool? ChannelHealthy { get; } = channelHealthy;
}

/// <summary>
/// Minimal ARI adapter for the local Asterisk/MicroSIP lab and, from PD-01.5, for a carrier trunk.
/// It never accepts a raw telephone number and it has no call-recording operation.
/// </summary>
/// <param name="trunkOptions">
/// PD-01.5. Absent for the lab, which is why it is optional rather than required: every existing
/// lab construction keeps working unchanged, and a deployment that has not configured a trunk
/// cannot accidentally take the production branch.
/// </param>
public sealed class AsteriskAriSimGateway(
    IHttpClientFactory httpClientFactory,
    IOptions<AsteriskAriOptions> options,
    TimeProvider timeProvider,
    IOptions<SipTrunkOptions>? trunkOptions = null) : ISimGateway, IAsyncDisposable
{
    private sealed class AriCallState(string channelId, DateTimeOffset startedAt)
    {
        public string ChannelId { get; } = channelId;

        public DateTimeOffset StartedAt { get; } = startedAt;

        public DateTimeOffset? ConnectedAt { get; set; }

        public DateTimeOffset? EndedAt { get; set; }

        public SimProviderDisposition? TerminalDisposition { get; set; }

        public string? TechnicalErrorCode { get; set; }

        // W-0362 / K-47. Ended on this side because the event stream failed. Asterisk never said
        // the channel is gone, so it may still be up - ringing, or with a customer on the line.
        public bool EndedWithoutAsterisk { get; set; }

        public TaskCompletionSource<bool> ConnectedOrEnded { get; } =
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        public TaskCompletionSource<string> Dtmf { get; } =
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        public TaskCompletionSource<bool> Ended { get; } =
            new(TaskCreationOptions.RunContinuationsAsynchronously);
    }

    private readonly ConcurrentDictionary<string, AriCallState> calls =
        new(StringComparer.Ordinal);
    private readonly SemaphoreSlim socketGate = new(1, 1);
    private ClientWebSocket? socket;
    private Task? eventPump;

    public async ValueTask<SimCallSession> DialAsync(
        SimDialRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        AsteriskAriOptions configured = options.Value;
        EnsureEnabled(configured);
        if (request.RecordingMode != SimRecordingMode.Disabled)
        {
            throw Failure(
                SimProviderDisposition.SimError,
                "ASTERISK_RECORDING_FORBIDDEN",
                true,
                "Call recording is forbidden in the softphone lab.");
        }

        string destination = request.DialAuthorization.RevealToTrustedGateway();

        // PD-01.5. The lab dials one pinned alias; the trunk dials whatever the vault resolved,
        // but only through the configured carrier. Both branches end in an allowlist, because the
        // question "may this dial happen" is the same question in both and only the answer differs.
        SipTrunkOptions? trunk = trunkOptions?.Value;
        bool production = trunk is { Enabled: true };
        string endpoint;
        string callerId;
        if (production)
        {
            // ProductionDialTokenVault emits sip:NUMBER@CarrierSipHost. Checking the host back
            // here is not redundant with that: it means a destination built for some other
            // carrier cannot be routed down this trunk even if it reaches this method.
            if (!destination.StartsWith("sip:", StringComparison.Ordinal)
                || !destination.EndsWith(
                    string.Concat("@", trunk!.CarrierSipHost),
                    StringComparison.Ordinal))
            {
                throw Failure(
                    SimProviderDisposition.InvalidDestination,
                    "ASTERISK_DESTINATION_NOT_ALLOWLISTED",
                    true,
                    "ARI refused a destination outside the configured carrier trunk.");
            }

            endpoint = string.Concat("PJSIP/", trunk.TrunkEndpoint, "/", destination);
            callerId = trunk.OutboundCallerId;
        }
        else
        {
            if (!string.Equals(destination, configured.DestinationAlias, StringComparison.Ordinal))
            {
                throw Failure(
                    SimProviderDisposition.InvalidDestination,
                    "ASTERISK_DESTINATION_NOT_ALLOWLISTED",
                    true,
                    "ARI refused a destination outside the pinned softphone alias.");
            }

            endpoint = string.Concat("PJSIP/", destination);
            callerId = "IVR-LAB";
        }

        await EnsureEventPumpAsync(cancellationToken);
        // The prefix is what a person reads first when reconciling an Asterisk log against a
        // carrier's call record, so a production channel must not announce itself as lab traffic.
        string channelId = string.Concat(
            production ? "ivr-trunk-" : "ivr-lab-",
            Guid.NewGuid().ToString("N"));
        var state = new AriCallState(channelId, timeProvider.GetUtcNow());
        if (!calls.TryAdd(channelId, state))
        {
            throw Failure(
                SimProviderDisposition.NetworkError,
                "ASTERISK_CHANNEL_COLLISION",
                true,
                "ARI channel allocation collided.");
        }

        try
        {
            using HttpResponseMessage response = await SendAsync(
                HttpMethod.Post,
                "/ari/channels",
                new Dictionary<string, string>
                {
                    ["endpoint"] = endpoint,
                    ["app"] = configured.Application,
                    ["timeout"] = configured.DialTimeoutSeconds.ToString(
                        System.Globalization.CultureInfo.InvariantCulture),
                    ["channelId"] = channelId,
                    ["callerId"] = callerId,
                },
                cancellationToken);
            await EnsureSuccessAsync(response, "ASTERISK_DIAL_FAILED", cancellationToken);
            await state.ConnectedOrEnded.Task.WaitAsync(
                TimeSpan.FromSeconds(configured.DialTimeoutSeconds + 2),
                timeProvider,
                cancellationToken);
            return new SimCallSession(
                request.AttemptId,
                request.SimChannelId,
                channelId,
                request.FencingGeneration,
                state.StartedAt,
                state.ConnectedAt.HasValue && !state.EndedAt.HasValue);
        }
        catch (TimeoutException)
        {
            state.EndedAt = timeProvider.GetUtcNow();
            state.TerminalDisposition = SimProviderDisposition.RingTimeout;
            state.TechnicalErrorCode = "ASTERISK_DIAL_TIMEOUT";
            return new SimCallSession(
                request.AttemptId,
                request.SimChannelId,
                channelId,
                request.FencingGeneration,
                state.StartedAt,
                false);
        }
        catch
        {
            calls.TryRemove(channelId, out _);
            throw;
        }
    }

    /// <summary>
    /// Turns rendered audio into the ARI <c>media</c> parameter.
    /// <para>
    /// ARI takes an ordered, comma-separated media list and plays it as one operation. Issuing
    /// one request per piece would let a hangup between two of them leave the customer having
    /// heard half an order — an opening and an amount with no items — and that half-call is
    /// indistinguishable, from the dialplan's side, from a complete one.
    /// </para>
    /// <para>
    /// Every piece is validated, not just the first. A playlist whose greeting is a valid sound
    /// reference and whose total is not would otherwise start playing and stop partway.
    /// </para>
    /// </summary>
    public static string BuildMediaList(RenderedAudio? audio)
    {
        if (audio is null || audio.Segments.IsDefaultOrEmpty)
        {
            throw Failure(
                SimProviderDisposition.AudioError,
                "ASTERISK_AUDIO_REFERENCE_INVALID",
                true,
                "ARI playback requires a safe Asterisk sound reference.");
        }

        foreach (RenderedAudioSegment segment in audio.Segments)
        {
            if (!segment.ContentRef.StartsWith("sound:", StringComparison.Ordinal)
                || segment.ContentRef.Contains(',', StringComparison.Ordinal))
            {
                // A comma inside one reference would split into two media entries and shift
                // every sentence after it by one position.
                throw Failure(
                    SimProviderDisposition.AudioError,
                    "ASTERISK_AUDIO_REFERENCE_INVALID",
                    true,
                    "ARI playback requires a safe Asterisk sound reference.");
            }
        }

        return string.Join(',', audio.Segments.Select(segment => segment.ContentRef));
    }

    public async ValueTask PlayAsync(
        SimCallSession session,
        RenderedSpeech speech,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(session);
        ArgumentNullException.ThrowIfNull(speech);
        string mediaList = BuildMediaList(speech.Audio);
        AriCallState state = GetCall(session.ProviderCallReference);
        if (state.EndedWithoutAsterisk)
        {
            // W-0365 / K-56. This side ended the call, because the event stream failed between the
            // answer and this playback. The branch below used to take it: Dropped, the channel
            // healthy, ASTERISK_CHANNEL_ALREADY_ENDED - a customer hanging up, on a channel
            // Asterisk never said had ended, with no word of the stream. It fails the way the same
            // loss one step later is reported: the stream's own code, and the answer
            // GetDispositionAsync gives for that call, channel health included - which since
            // W-0367 / K-57 says nothing about the SIM either way.
            SimDispositionReport lost = await GetDispositionAsync(session, cancellationToken);
            throw Failure(
                lost.Disposition,
                lost.TechnicalErrorCode!,
                lost.ChannelHealthy,
                "The ARI event stream was lost before playback; this side ended the call.");
        }

        if (state.EndedAt.HasValue)
        {
            throw Failure(
                SimProviderDisposition.Dropped,
                "ASTERISK_CHANNEL_ALREADY_ENDED",
                true,
                "The ARI channel ended before playback.");
        }

        using HttpResponseMessage response = await SendAsync(
            HttpMethod.Post,
            string.Concat("/ari/channels/", Uri.EscapeDataString(state.ChannelId), "/play"),
            new Dictionary<string, string>
            {
                ["media"] = mediaList,
                ["playbackId"] = string.Concat("play-", Guid.NewGuid().ToString("N")),
            },
            cancellationToken);
        await EnsureSuccessAsync(response, "ASTERISK_PLAYBACK_FAILED", cancellationToken);
    }

    public async ValueTask<SimDtmfCapture> CaptureDtmfAsync(
        SimCallSession session,
        TimeSpan timeout,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(session);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(timeout, TimeSpan.Zero);
        AriCallState state = GetCall(session.ProviderCallReference);
        Task completed = await Task.WhenAny(
            state.Dtmf.Task,
            state.Ended.Task,
            Task.Delay(timeout, timeProvider, cancellationToken));
        if (completed == state.Dtmf.Task)
        {
            return new SimDtmfCapture(await state.Dtmf.Task, false, null);
        }

        if (completed == state.Ended.Task)
        {
            return new SimDtmfCapture(null, false, state.TechnicalErrorCode);
        }

        cancellationToken.ThrowIfCancellationRequested();
        return new SimDtmfCapture(null, true, null);
    }

    public ValueTask<SimDispositionReport> GetDispositionAsync(
        SimCallSession session,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(session);
        AriCallState state = GetCall(session.ProviderCallReference);
        DateTimeOffset endedAt = state.EndedAt ?? timeProvider.GetUtcNow();
        SimProviderDisposition disposition = state.TerminalDisposition
            ?? (state.ConnectedAt.HasValue
                ? SimProviderDisposition.Answered
                : SimProviderDisposition.NetworkError);
        return ValueTask.FromResult(new SimDispositionReport(
            disposition,
            state.StartedAt,
            endedAt,
            state.TechnicalErrorCode,
            // W-0367 / K-57. A call this side ended because the event stream failed says nothing
            // about its SIM. The stream belongs to the adapter, and one stream serves every SIM
            // this worker drives: reading its loss as a network fault on each channel put a strike
            // on every SIM with a call up, and three short outages inside DT-04's ten minutes
            // took them all out of service. Nothing returned them: the admin enable path refuses
            // any channel that is not MOCK. The channel keeps whatever streak it had.
            state.EndedWithoutAsterisk
                ? null
                : disposition is not (SimProviderDisposition.NetworkError or SimProviderDisposition.SimError)));
    }

    public async ValueTask HangupAsync(
        SimCallSession session,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(session);
        if (!calls.TryGetValue(session.ProviderCallReference, out AriCallState? state))
        {
            return;
        }

        // W-0362 / K-47. A call the event stream's failure ended is hung up like a live one: only
        // this side considers it over, and the channel may still hold a customer. A call Asterisk
        // ended itself, or that its originate timeout ends, needs no DELETE, as before.
        if (!state.EndedAt.HasValue || state.EndedWithoutAsterisk)
        {
            using HttpResponseMessage response = await SendAsync(
                HttpMethod.Delete,
                string.Concat("/ari/channels/", Uri.EscapeDataString(state.ChannelId)),
                null,
                cancellationToken);
            if (response.StatusCode != System.Net.HttpStatusCode.NotFound)
            {
                await EnsureSuccessAsync(response, "ASTERISK_HANGUP_FAILED", cancellationToken);
            }
        }

        calls.TryRemove(session.ProviderCallReference, out _);
    }

    public async ValueTask<SimGatewayHealth> CheckHealthAsync(
        string simChannelId,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(simChannelId);
        AsteriskAriOptions configured = options.Value;
        EnsureEnabled(configured);
        try
        {
            using HttpResponseMessage response = await SendAsync(
                HttpMethod.Get,
                "/ari/asterisk/ping",
                null,
                cancellationToken);
            await EnsureSuccessAsync(response, "ASTERISK_HEALTH_FAILED", cancellationToken);
            return new SimGatewayHealth(
                simChannelId,
                SimChannelHealthState.Healthy,
                timeProvider.GetUtcNow(),
                null,
                !configured.RecordingEnabled);
        }
        catch (AsteriskAriOperationException)
        {
            return new SimGatewayHealth(
                simChannelId,
                SimChannelHealthState.Unavailable,
                timeProvider.GetUtcNow(),
                null,
                !configured.RecordingEnabled);
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (socket is not null)
        {
            if (socket.State == WebSocketState.Open)
            {
                await socket.CloseAsync(
                    WebSocketCloseStatus.NormalClosure,
                    "shutdown",
                    CancellationToken.None);
            }

            socket.Dispose();
        }

        socketGate.Dispose();
        if (eventPump is not null)
        {
            try
            {
                await eventPump;
            }
            catch (WebSocketException)
            {
                // Disposal intentionally closes the event stream.
            }
        }
    }

    internal static (SimProviderDisposition Disposition, string? TechnicalCode) MapHangup(
        int? cause,
        string? causeText)
    {
        return cause switch
        {
            16 => (SimProviderDisposition.Answered, null),
            17 => (SimProviderDisposition.Busy, null),
            18 or 19 => (SimProviderDisposition.RingTimeout, null),
            21 => (SimProviderDisposition.Rejected, null),
            1 or 3 or 20 => (SimProviderDisposition.Unreachable, null),
            28 => (SimProviderDisposition.InvalidDestination, null),
            34 or 38 or 41 or 42 or 44 =>
                (SimProviderDisposition.NetworkError, "ASTERISK_NETWORK_FAILURE"),
            _ when !string.IsNullOrWhiteSpace(causeText) =>
                (SimProviderDisposition.NetworkError, "ASTERISK_UNKNOWN_HANGUP"),
            _ => (SimProviderDisposition.NetworkError, "ASTERISK_TERMINAL_STATE_UNKNOWN"),
        };
    }

    private async Task EnsureEventPumpAsync(CancellationToken cancellationToken)
    {
        if (socket?.State == WebSocketState.Open && eventPump is { IsCompleted: false })
        {
            return;
        }

        await socketGate.WaitAsync(cancellationToken);
        try
        {
            if (socket?.State == WebSocketState.Open && eventPump is { IsCompleted: false })
            {
                return;
            }

            socket?.Dispose();
            socket = new ClientWebSocket();
            AsteriskAriOptions configured = options.Value;
            socket.Options.SetRequestHeader(
                "Authorization",
                BasicAuthorization(configured.Username, configured.Password));
            Uri eventUri = BuildWebSocketUri(configured);
            await socket.ConnectAsync(eventUri, cancellationToken);
            ClientWebSocket activeSocket = socket;
            eventPump = Task.Run(
                () => PumpEventsAsync(activeSocket, CancellationToken.None),
                CancellationToken.None);
        }
        catch (Exception exception) when (exception is HttpRequestException or WebSocketException)
        {
            throw Failure(
                SimProviderDisposition.NetworkError,
                "ASTERISK_EVENT_STREAM_UNAVAILABLE",
                false,
                "The ARI event stream is unavailable.",
                exception);
        }
        finally
        {
            socketGate.Release();
        }
    }

    private async Task PumpEventsAsync(
        ClientWebSocket activeSocket,
        CancellationToken cancellationToken)
    {
        var buffer = new byte[16_384];
        try
        {
            while (activeSocket.State == WebSocketState.Open)
            {
                using var message = new MemoryStream();
                WebSocketReceiveResult result;
                do
                {
                    result = await activeSocket.ReceiveAsync(buffer, cancellationToken);
                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        FailOpenCalls("ASTERISK_EVENT_STREAM_CLOSED");
                        return;
                    }

                    message.Write(buffer, 0, result.Count);
                    if (message.Length > 256_000)
                    {
                        FailOpenCalls("ASTERISK_EVENT_TOO_LARGE");
                        return;
                    }
                }
                while (!result.EndOfMessage);

                try
                {
                    using JsonDocument document = JsonDocument.Parse(message.ToArray());
                    ProcessEvent(document.RootElement);
                }
                catch (JsonException)
                {
                    // Ignore malformed provider telemetry; call timeouts remain fail-closed.
                }
            }
        }
        catch
        {
            // W-0362 / K-47. A stream that drops without a close frame arrives here as ReceiveAsync
            // throwing, and used to leave past every FailOpenCalls above. With nothing left to
            // deliver StasisStart, a keypress or ChannelDestroyed, an open call waited out its own
            // timeout and was recorded as the customer not answering or not pressing a key - an
            // attempt counted against them. Losing the stream is our fault, not theirs, so it ends
            // those calls the way a closed stream does: a network error, never counted. The
            // exception still goes where it always went.
            FailOpenCalls("ASTERISK_EVENT_STREAM_LOST");
            throw;
        }

        FailOpenCalls("ASTERISK_EVENT_STREAM_CLOSED");
    }

    private void ProcessEvent(JsonElement root)
    {
        // W-0365 / K-56. Each value's kind is checked before it is read. GetString and
        // TryGetProperty do not answer false on the wrong kind, they throw, and the throw went past
        // the pump's JsonException catch: one event with "type": 5 or "channel": "x" stopped the
        // pump, and every call open on it ended as a lost stream. An event whose type or channel
        // cannot be read is ignored, as text that does not parse always was.
        //
        // That catch stays narrow on purpose. An exception nobody foresaw, swallowed there, could
        // lose a StasisStart or a ChannelDestroyed, and that call would wait out its timeout and
        // be recorded as the customer's doing. Let through, it ends every open call, but as an
        // uncounted network error (K-47).
        if (root.ValueKind != JsonValueKind.Object
            || !root.TryGetProperty("type", out JsonElement typeElement)
            || typeElement.ValueKind != JsonValueKind.String
            || !root.TryGetProperty("channel", out JsonElement channelElement)
            || channelElement.ValueKind != JsonValueKind.Object
            || !channelElement.TryGetProperty("id", out JsonElement channelIdElement)
            || channelIdElement.ValueKind != JsonValueKind.String)
        {
            return;
        }

        string? type = typeElement.GetString();
        string? channelId = channelIdElement.GetString();
        if (channelId is null || !calls.TryGetValue(channelId, out AriCallState? state))
        {
            return;
        }

        if (string.Equals(type, "StasisStart", StringComparison.Ordinal))
        {
            state.ConnectedAt = timeProvider.GetUtcNow();
            state.ConnectedOrEnded.TrySetResult(true);
            return;
        }

        // W-0365 / K-56. Only a string is a digit. ARI sends every keypress as one, so a number
        // here is not a press this adapter can vouch for, and reading the number 1 as the key "1"
        // would confirm an order on it: a key is the one outcome acted on without asking again.
        // The event is ignored, and the call waits for a well-formed key or its own capture
        // timeout, as if the event had never come.
        if (string.Equals(type, "ChannelDtmfReceived", StringComparison.Ordinal)
            && root.TryGetProperty("digit", out JsonElement digitElement)
            && digitElement.ValueKind == JsonValueKind.String)
        {
            string? digit = digitElement.GetString();
            if (!string.IsNullOrWhiteSpace(digit))
            {
                state.Dtmf.TrySetResult(digit);
            }

            return;
        }

        if (string.Equals(type, "ChannelDestroyed", StringComparison.Ordinal))
        {
            // W-0365 / K-56. A cause that is not a number, or a text that is not a string, is read
            // as absent. The channel is gone whatever those fields hold, and a hangup dropped for
            // their sake would leave the call to time out as the customer not answering; MapHangup
            // names what is left, down to ASTERISK_TERMINAL_STATE_UNKNOWN.
            int? cause = root.TryGetProperty("cause", out JsonElement causeElement)
                && causeElement.ValueKind == JsonValueKind.Number
                && causeElement.TryGetInt32(out int parsedCause)
                ? parsedCause
                : null;
            string? causeText = root.TryGetProperty("cause_txt", out JsonElement textElement)
                && textElement.ValueKind == JsonValueKind.String
                ? textElement.GetString()
                : null;
            (state.TerminalDisposition, state.TechnicalErrorCode) = MapHangup(cause, causeText);
            state.EndedAt = timeProvider.GetUtcNow();
            state.ConnectedOrEnded.TrySetResult(false);
            state.Ended.TrySetResult(true);
        }
    }

    private void FailOpenCalls(string technicalCode)
    {
        foreach (AriCallState state in calls.Values)
        {
            // W-0362 / K-47. Only calls whose outcome is still unknown. One Asterisk already ended
            // keeps the cause Asterisk reported, and one that already captured a key keeps that
            // key: turning either into a network error would discard a known outcome, and call a
            // customer who pressed 1 all over again.
            if (state.EndedAt.HasValue || state.Dtmf.Task.IsCompleted)
            {
                continue;
            }

            state.TerminalDisposition = SimProviderDisposition.NetworkError;
            state.TechnicalErrorCode = technicalCode;
            state.EndedWithoutAsterisk = true;
            state.EndedAt = timeProvider.GetUtcNow();
            state.ConnectedOrEnded.TrySetResult(false);
            state.Ended.TrySetResult(true);
        }
    }

    private AriCallState GetCall(string providerCallReference)
    {
        return calls.TryGetValue(providerCallReference, out AriCallState? state)
            ? state
            : throw Failure(
                SimProviderDisposition.NetworkError,
                "ASTERISK_CALL_NOT_FOUND",
                false,
                "The ARI call state was not found.");
    }

    private async Task<HttpResponseMessage> SendAsync(
        HttpMethod method,
        string path,
        IReadOnlyDictionary<string, string>? query,
        CancellationToken cancellationToken)
    {
        AsteriskAriOptions configured = options.Value;
        using var request = new HttpRequestMessage(
            method,
            BuildUri(configured.BaseUrl, path, query));
        request.Headers.Authorization = AuthenticationHeaderValue.Parse(
            BasicAuthorization(configured.Username, configured.Password));
        HttpClient client = httpClientFactory.CreateClient(nameof(AsteriskAriSimGateway));
        try
        {
            return await client.SendAsync(request, cancellationToken);
        }
        catch (HttpRequestException exception)
        {
            throw Failure(
                SimProviderDisposition.NetworkError,
                "ASTERISK_HTTP_UNAVAILABLE",
                false,
                "The ARI HTTP endpoint is unavailable.",
                exception);
        }
    }

    private static async Task EnsureSuccessAsync(
        HttpResponseMessage response,
        string technicalCode,
        CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        _ = await response.Content.ReadAsStringAsync(cancellationToken);
        throw Failure(
            SimProviderDisposition.NetworkError,
            technicalCode,
            response.StatusCode != System.Net.HttpStatusCode.ServiceUnavailable,
            "ARI returned an unsuccessful response.");
    }

    private static Uri BuildUri(
        string baseUrl,
        string path,
        IReadOnlyDictionary<string, string>? query)
    {
        var builder = new UriBuilder(new Uri(new Uri(baseUrl.TrimEnd('/') + "/"), path.TrimStart('/')));
        if (query is not null)
        {
            builder.Query = string.Join(
                '&',
                query.Select(pair => string.Concat(
                    Uri.EscapeDataString(pair.Key),
                    "=",
                    Uri.EscapeDataString(pair.Value))));
        }

        return builder.Uri;
    }

    private static Uri BuildWebSocketUri(AsteriskAriOptions configured)
    {
        Uri httpUri = BuildUri(
            configured.BaseUrl,
            "/ari/events",
            new Dictionary<string, string>
            {
                ["app"] = configured.Application,
                ["subscribeAll"] = "false",
            });
        var builder = new UriBuilder(httpUri)
        {
            Scheme = httpUri.Scheme == "https" ? "wss" : "ws",
        };
        return builder.Uri;
    }

    private static string BasicAuthorization(string username, string password) =>
        string.Concat(
            "Basic ",
            Convert.ToBase64String(Encoding.UTF8.GetBytes(string.Concat(username, ":", password))));

    private static void EnsureEnabled(AsteriskAriOptions configured)
    {
        if (!configured.Enabled)
        {
            throw Failure(
                SimProviderDisposition.SimError,
                "ASTERISK_ADAPTER_DISABLED",
                false,
                "The Asterisk ARI adapter is disabled.");
        }
    }

    private static AsteriskAriOperationException Failure(
        SimProviderDisposition disposition,
        string code,
        bool? channelHealthy,
        string message,
        Exception? exception = null) =>
        new(disposition, code, channelHealthy, message, exception);
}
