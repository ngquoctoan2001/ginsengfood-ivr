using System.Collections.Concurrent;
using System.Net;
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

    /// <summary>
    /// The ARI REST operations the adapter makes. Each reads an unsuccessful answer its own way
    /// (W-0372 / K-63, <see cref="Unsuccessful"/>).
    /// </summary>
    private enum AriOperation
    {
        Originate,
        Playback,
        Hangup,
        HealthPing,
    }

    /// <summary>
    /// How long one ARI REST call, or the opening of the event stream, may take before the adapter
    /// gives up on it (W-0372 / K-63).
    /// <para>
    /// Neither had a bound of its own. A REST call waited out HttpClient's default hundred seconds,
    /// and the event stream's handshake waited for as long as the far end held the connection open.
    /// An Asterisk that drops packets instead of refusing them - a blackholed link, a process that
    /// has hung - answers neither, so a dispatch stalled that long before it failed, and a stalled
    /// handshake held every dial behind it, since only one at a time may open the stream.
    /// </para>
    /// <para>
    /// Ten seconds, because every one of these calls goes to the local Asterisk, the only one the
    /// options validator admits, and a healthy one answers them at once: an originate returns as
    /// soon as the channel is allocated, and the ringing is waited for on the event stream, under
    /// the dial's own timeout, not here. That leaves a slow Asterisk ample room and still bounds
    /// what a silent one costs a dispatch: one bound at the health check, where a dispatch meets
    /// Asterisk first, or two after an originate Asterisk did not reply to, whose channel is then
    /// hung up under the same bound.
    /// </para>
    /// </summary>
    private static readonly TimeSpan AriRequestTimeout = TimeSpan.FromSeconds(10);

    /// <summary>
    /// The code an ARI REST call that ran out of <see cref="AriRequestTimeout"/> fails under
    /// (W-0372 / K-63). Named because DialAsync looks for it: an originate that ended this way is
    /// the one failure after which a channel IVR never heard of may exist.
    /// </summary>
    private const string HttpTimeoutCode = "ASTERISK_HTTP_TIMEOUT";

    /// <summary>
    /// How long disposal waits for Asterisk to answer the close frame it sends (W-0369 / K-58).
    /// <para>
    /// Asterisk on a working link answers at once, so the wait only runs out on a far end that is
    /// not going to answer, and waiting longer buys nothing. Five seconds leaves a slow Asterisk
    /// plenty of room and still fits the worker's way out: its grace period (210 s) leaves thirty
    /// seconds after a full call drain (180 s), and a disposal that never ended used to spend all
    /// of them and be killed.
    /// </para>
    /// </summary>
    private static readonly TimeSpan EventStreamCloseTimeout = TimeSpan.FromSeconds(5);

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
            await EnsureSuccessAsync(response, AriOperation.Originate, cancellationToken);
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
        catch (AsteriskAriOperationException exception) when (string.Equals(
            exception.TechnicalErrorCode,
            HttpTimeoutCode,
            StringComparison.Ordinal))
        {
            // W-0372 / K-63. Asterisk did not reply to the originate, which is not to say it did not
            // carry it out: one that is slow rather than gone may have created the channel and be
            // ringing the customer's phone while this dial is recorded as failed and the customer
            // is called again later. IVR named the channel itself, so it can hang that name up
            // without having heard back. The failure reported is the originate's either way.
            calls.TryRemove(channelId, out _);
            await HangUpPossibleOrphanAsync(channelId);
            throw;
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
            throw ChannelAlreadyEnded();
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
        await EnsureSuccessAsync(response, AriOperation.Playback, cancellationToken);
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
                await EnsureSuccessAsync(response, AriOperation.Hangup, cancellationToken);
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
            await EnsureSuccessAsync(response, AriOperation.HealthPing, cancellationToken);
            return new SimGatewayHealth(
                simChannelId,
                SimChannelHealthState.Healthy,
                timeProvider.GetUtcNow(),
                null,
                !configured.RecordingEnabled);
        }
        catch (AsteriskAriOperationException)
        {
            // W-0372 / K-63. Every failure of the ping ends here, a silent Asterisk's included. That
            // one used to end in a TaskCanceledException a hundred seconds in, which went past this
            // catch and every named arm of the dispatch gateway, and was recorded against the SIM.
            // What a failure says about the SIM is not read here: the ping asks after Asterisk,
            // whichever channel it is for, and the dispatch gateway records Unavailable without a
            // word about the SIM (W-0369 / K-62).
            return new SimGatewayHealth(
                simChannelId,
                SimChannelHealthState.Unavailable,
                timeProvider.GetUtcNow(),
                null,
                !configured.RecordingEnabled);
        }
    }

    /// <summary>
    /// Closes the event stream and waits for its pump. Best-effort: nothing about the stream makes
    /// it hang or throw (W-0369 / K-58).
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        if (socket is not null)
        {
            if (socket.State == WebSocketState.Open)
            {
                await CloseEventStreamAsync(socket);
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
#pragma warning disable CA1031 // Disposal is best-effort: the pump's failure was handled where it happened.
            catch (Exception)
#pragma warning restore CA1031
            {
                // W-0369 / K-58. Whatever ended the pump has been dealt with already: its own catch
                // ended every call still open on the stream as a lost stream (K-47), and a stream
                // being closed has nobody left to report to. Only a WebSocketException used to be
                // swallowed here, so an exception an event threw past ProcessEvent (K-56) came out
                // of DisposeAsync - as would the OperationCanceledException the abort in
                // CloseEventStreamAsync leaves in the pump. The container stops at the first
                // disposable that throws: every service created before this one went undisposed,
                // and the worker ended its shutdown on an unhandled exception.
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

            // W-0372 / K-63. The handshake is bounded the way a REST call is. ClientWebSocket puts
            // no limit of its own on it, so an Asterisk that took the connection in and never
            // answered the upgrade held this dial here for good - and, through socketGate, every
            // dial after it.
            using var timeout = new CancellationTokenSource(AriRequestTimeout, timeProvider);
            using var bounded = CancellationTokenSource.CreateLinkedTokenSource(
                cancellationToken,
                timeout.Token);
            await socket.ConnectAsync(eventUri, bounded.Token);
            ClientWebSocket activeSocket = socket;
            eventPump = Task.Run(
                () => PumpEventsAsync(activeSocket, CancellationToken.None),
                CancellationToken.None);
        }
        catch (Exception exception) when (
            exception is HttpRequestException or WebSocketException
            || (exception is OperationCanceledException && !cancellationToken.IsCancellationRequested))
        {
            // W-0369 / K-62. Asterisk out of reach says nothing about the SIM this dial was for, so
            // the channel's health is left unsaid, as for a stream lost under a call (W-0367 /
            // K-57). Reported unhealthy, it put a strike on every SIM dialled while Asterisk was
            // down, and three inside DT-04's ten minutes left each in HEALTH_FAILED for good: the
            // admin enable path takes MOCK channels only. Holding dialling back while Asterisk is
            // down is SchedulerDispatchPump's job, not the channel's: this dispatch fails, and each
            // failed dispatch pushes the next start further off, doubling up to thirty seconds,
            // until one goes through.
            //
            // W-0372 / K-63. A handshake that ran out of time is the same Asterisk out of reach, and
            // fails the same way. The caller cancelling is not: that still leaves as the
            // OperationCanceledException it is.
            throw Failure(
                SimProviderDisposition.NetworkError,
                "ASTERISK_EVENT_STREAM_UNAVAILABLE",
                null,
                "The ARI event stream is unavailable.",
                exception);
        }
        finally
        {
            socketGate.Release();
        }
    }

    /// <summary>
    /// W-0369 / K-58. Sends the close frame and waits a bounded time for Asterisk to answer it.
    /// <para>
    /// The wait used to have no bound: an Asterisk that never answered held disposal, and the
    /// worker's shutdown with it, for as long as the process was allowed to run - the way the first
    /// tests to dispose an open stream hung on FakeAsterisk, until K-56 taught it to answer. A close
    /// that runs out of time, or fails on the way, ends in an abort instead. The abort is what makes
    /// the pump's pending receive return, so the wait for the pump that follows cannot inherit the
    /// same silence; the pump then ends any call still open as a lost stream, which is what an
    /// unanswered close is.
    /// </para>
    /// </summary>
    private async Task CloseEventStreamAsync(ClientWebSocket open)
    {
        using var timeout = new CancellationTokenSource(EventStreamCloseTimeout, timeProvider);
        try
        {
            await open.CloseAsync(
                WebSocketCloseStatus.NormalClosure,
                "shutdown",
                timeout.Token);
        }
#pragma warning disable CA1031 // However the close fails, the stream ends the same way: aborted.
        catch (Exception)
#pragma warning restore CA1031
        {
            open.Abort();
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

        // W-0372 / K-63. AriRequestTimeout, not HttpClient's default hundred seconds. The bound runs
        // on a source of its own so that its expiry can be told from the caller cancelling: a
        // worker shutting down still gets the OperationCanceledException it always got.
        using var timeout = new CancellationTokenSource(AriRequestTimeout, timeProvider);
        using var bounded = CancellationTokenSource.CreateLinkedTokenSource(
            cancellationToken,
            timeout.Token);
        try
        {
            return await client.SendAsync(request, bounded.Token);
        }
        catch (OperationCanceledException exception) when (!cancellationToken.IsCancellationRequested)
        {
            // W-0372 / K-63. Asterisk silent rather than refusing: a link that drops packets, or a
            // process that has hung. What used to end the request was HttpClient's own timeout, a
            // TaskCanceledException rather than the HttpRequestException caught below, so it left
            // the adapter as it was, and the dispatch gateway's catch-all recorded it as the SIM's
            // fault: ASTERISK_DISPATCH_TECHNICAL_FAILURE, the channel unhealthy. A silence says no
            // more about the SIM than a refusal does, and is reported the same way.
            throw Failure(
                SimProviderDisposition.NetworkError,
                HttpTimeoutCode,
                null,
                "The ARI HTTP endpoint did not answer in time.",
                exception);
        }
        catch (HttpRequestException exception)
        {
            // W-0369 / K-62. The SIM's health left unsaid, for the reason given where the event
            // stream fails to connect: Asterisk out of reach is not the SIM's fault, and it is the
            // dispatch pump's backoff after each failed dispatch, not a quarantined channel, that
            // holds dialling back while it lasts.
            throw Failure(
                SimProviderDisposition.NetworkError,
                "ASTERISK_HTTP_UNAVAILABLE",
                null,
                "The ARI HTTP endpoint is unavailable.",
                exception);
        }
    }

    /// <summary>
    /// W-0372 / K-63. Hangs up the channel named by an originate Asterisk did not reply to, in case
    /// it created the channel after all: an orphan, since this adapter no longer tracks it. However
    /// that goes - the channel hung up, a 404 because it never existed, a refusal, or no reply
    /// within <see cref="AriRequestTimeout"/> - the caller goes on to report the originate's own
    /// failure; this only tries.
    /// <para>
    /// Not under the dial's token, for the reason the dispatch gateway hangs up under none: a
    /// channel that may be ringing a customer is ended whatever else is going on, and the bound is
    /// what keeps that from holding anything up. If it fails, a channel that does exist rings until
    /// the dial timeout handed to Asterisk with the originate runs out; answered before then, it is
    /// a call nobody speaks on.
    /// </para>
    /// </summary>
    private async Task HangUpPossibleOrphanAsync(string channelId)
    {
        try
        {
            (await SendAsync(
                HttpMethod.Delete,
                string.Concat("/ari/channels/", Uri.EscapeDataString(channelId)),
                null,
                CancellationToken.None)).Dispose();
        }
#pragma warning disable CA1031 // Best-effort: the failure to report is the originate's, not this one's.
        catch (Exception)
#pragma warning restore CA1031
        {
            // Nothing more can be done from here: see the summary for what is left.
        }
    }

    private static async Task EnsureSuccessAsync(
        HttpResponseMessage response,
        AriOperation operation,
        CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        _ = await response.Content.ReadAsStringAsync(cancellationToken);
        throw Unsuccessful(operation, response.StatusCode);
    }

    /// <summary>
    /// What an unsuccessful ARI answer to <paramref name="operation"/> stands for, and above all
    /// what it says about the SIM (W-0372 / K-63). The table this implements is in
    /// specs/api/04-sim-adapter-contract.md.
    /// <para>
    /// The SIM's health used to be read off a single status: every answer but a 503 called the
    /// channel healthy, and a 503 called it unhealthy. Both halves were wrong. A 503 is Asterisk
    /// that has not finished booting; a 401 or 403 is IVR's credentials, or an ARI user that may
    /// only read; none of them is the SIM's doing. Called healthy after an answered call, such a
    /// refusal cleared the SIM's failure streak; called unhealthy, it put a strike on it.
    /// </para>
    /// <para>
    /// Only two answers now say anything about the SIM. A 500 to an originate is ARI's "Allocation
    /// failed": the channel driver could not create the outgoing channel, because the endpoint,
    /// trunk or device behind it is not there, and that counts against the SIM. A 404, 409 or 412
    /// to a playback is a channel that has gone, or is going, after it was answered - the customer
    /// or the network ended the call - so the SIM carried it; it is reported exactly as PlayAsync
    /// reports the same ending when ChannelDestroyed happens to arrive first, so that which of the
    /// two Asterisk sends first no longer decides what is recorded. Everything else - IVR's own
    /// request, its credentials, Asterisk itself, a status nobody expected - says nothing about the
    /// SIM. A 401 or 403 also has a code of its own, since it is the one refusal a person has to
    /// fix by hand, and it should not read as a dial or a playback that failed.
    /// </para>
    /// </summary>
    private static AsteriskAriOperationException Unsuccessful(
        AriOperation operation,
        HttpStatusCode status)
    {
        string technicalCode = operation switch
        {
            AriOperation.Originate => "ASTERISK_DIAL_FAILED",
            AriOperation.Playback => "ASTERISK_PLAYBACK_FAILED",
            AriOperation.Hangup => "ASTERISK_HANGUP_FAILED",
            AriOperation.HealthPing => "ASTERISK_HEALTH_FAILED",
            _ => throw new ArgumentOutOfRangeException(nameof(operation)),
        };
        return (operation, status) switch
        {
            (_, HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden) => Failure(
                SimProviderDisposition.NetworkError,
                "ASTERISK_HTTP_UNAUTHORIZED",
                null,
                "ARI refused IVR's credentials or their permissions."),
            (AriOperation.Originate, HttpStatusCode.InternalServerError) => Failure(
                SimProviderDisposition.NetworkError,
                technicalCode,
                false,
                "ARI could not allocate the outgoing channel."),
            (AriOperation.Playback,
                HttpStatusCode.NotFound or HttpStatusCode.Conflict or HttpStatusCode.PreconditionFailed) =>
                ChannelAlreadyEnded(),
            _ => Failure(
                SimProviderDisposition.NetworkError,
                technicalCode,
                null,
                "ARI returned an unsuccessful response."),
        };
    }

    /// <summary>
    /// The call ended before its playback: dropped, and the SIM healthy, since it carried the call.
    /// Asterisk says so either by ChannelDestroyed on the event stream or by the playback's own
    /// answer, whichever comes first, and both end here (W-0372 / K-63).
    /// </summary>
    private static AsteriskAriOperationException ChannelAlreadyEnded() => Failure(
        SimProviderDisposition.Dropped,
        "ASTERISK_CHANNEL_ALREADY_ENDED",
        true,
        "The ARI channel ended before playback.");

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
