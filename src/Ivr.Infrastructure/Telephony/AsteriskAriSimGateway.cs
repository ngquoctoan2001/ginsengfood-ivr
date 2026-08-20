using System.Collections.Concurrent;
using System.Net.Http.Headers;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Ivr.Domain.Ports;
using Microsoft.Extensions.Options;

namespace Ivr.Infrastructure.Telephony;

public sealed class AsteriskAriOperationException(
    SimProviderDisposition disposition,
    string technicalErrorCode,
    bool channelHealthy,
    string message,
    Exception? innerException = null) : Exception(message, innerException)
{
    public SimProviderDisposition Disposition { get; } = disposition;

    public string TechnicalErrorCode { get; } = technicalErrorCode;

    public bool ChannelHealthy { get; } = channelHealthy;
}

/// <summary>
/// Minimal ARI adapter for the local Asterisk/MicroSIP lab. It never accepts a raw
/// telephone number and it has no call-recording operation.
/// </summary>
public sealed class AsteriskAriSimGateway(
    IHttpClientFactory httpClientFactory,
    IOptions<AsteriskAriOptions> options,
    TimeProvider timeProvider) : ISimGateway, IAsyncDisposable
{
    private sealed class AriCallState(string channelId, DateTimeOffset startedAt)
    {
        public string ChannelId { get; } = channelId;

        public DateTimeOffset StartedAt { get; } = startedAt;

        public DateTimeOffset? ConnectedAt { get; set; }

        public DateTimeOffset? EndedAt { get; set; }

        public SimProviderDisposition? TerminalDisposition { get; set; }

        public string? TechnicalErrorCode { get; set; }

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
        if (!string.Equals(destination, configured.DestinationAlias, StringComparison.Ordinal))
        {
            throw Failure(
                SimProviderDisposition.InvalidDestination,
                "ASTERISK_DESTINATION_NOT_ALLOWLISTED",
                true,
                "ARI refused a destination outside the pinned softphone alias.");
        }

        await EnsureEventPumpAsync(cancellationToken).ConfigureAwait(false);
        string channelId = string.Concat("ivr-lab-", Guid.NewGuid().ToString("N"));
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
                    ["endpoint"] = string.Concat("PJSIP/", destination),
                    ["app"] = configured.Application,
                    ["timeout"] = configured.DialTimeoutSeconds.ToString(
                        System.Globalization.CultureInfo.InvariantCulture),
                    ["channelId"] = channelId,
                    ["callerId"] = "IVR-LAB",
                },
                cancellationToken).ConfigureAwait(false);
            await EnsureSuccessAsync(response, "ASTERISK_DIAL_FAILED", cancellationToken)
                .ConfigureAwait(false);
            await state.ConnectedOrEnded.Task.WaitAsync(
                TimeSpan.FromSeconds(configured.DialTimeoutSeconds + 2),
                timeProvider,
                cancellationToken).ConfigureAwait(false);
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

    public async ValueTask PlayAsync(
        SimCallSession session,
        RenderedSpeech speech,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(session);
        ArgumentNullException.ThrowIfNull(speech);
        if (speech.Audio is null
            || !speech.Audio.ContentRef.StartsWith("sound:", StringComparison.Ordinal))
        {
            throw Failure(
                SimProviderDisposition.AudioError,
                "ASTERISK_AUDIO_REFERENCE_INVALID",
                true,
                "ARI playback requires a safe Asterisk sound reference.");
        }

        AriCallState state = GetCall(session.ProviderCallReference);
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
                ["media"] = speech.Audio.ContentRef,
                ["playbackId"] = string.Concat("play-", Guid.NewGuid().ToString("N")),
            },
            cancellationToken).ConfigureAwait(false);
        await EnsureSuccessAsync(response, "ASTERISK_PLAYBACK_FAILED", cancellationToken)
            .ConfigureAwait(false);
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
            Task.Delay(timeout, timeProvider, cancellationToken)).ConfigureAwait(false);
        if (completed == state.Dtmf.Task)
        {
            return new SimDtmfCapture(await state.Dtmf.Task.ConfigureAwait(false), false, null);
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
            disposition is not (SimProviderDisposition.NetworkError or SimProviderDisposition.SimError)));
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

        if (!state.EndedAt.HasValue)
        {
            using HttpResponseMessage response = await SendAsync(
                HttpMethod.Delete,
                string.Concat("/ari/channels/", Uri.EscapeDataString(state.ChannelId)),
                null,
                cancellationToken).ConfigureAwait(false);
            if (response.StatusCode != System.Net.HttpStatusCode.NotFound)
            {
                await EnsureSuccessAsync(response, "ASTERISK_HANGUP_FAILED", cancellationToken)
                    .ConfigureAwait(false);
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
                cancellationToken).ConfigureAwait(false);
            await EnsureSuccessAsync(response, "ASTERISK_HEALTH_FAILED", cancellationToken)
                .ConfigureAwait(false);
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
                    CancellationToken.None).ConfigureAwait(false);
            }

            socket.Dispose();
        }

        socketGate.Dispose();
        if (eventPump is not null)
        {
            try
            {
                await eventPump.ConfigureAwait(false);
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

        await socketGate.WaitAsync(cancellationToken).ConfigureAwait(false);
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
            await socket.ConnectAsync(eventUri, cancellationToken).ConfigureAwait(false);
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
        while (activeSocket.State == WebSocketState.Open)
        {
            using var message = new MemoryStream();
            WebSocketReceiveResult result;
            do
            {
                result = await activeSocket.ReceiveAsync(buffer, cancellationToken)
                    .ConfigureAwait(false);
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

        FailOpenCalls("ASTERISK_EVENT_STREAM_CLOSED");
    }

    private void ProcessEvent(JsonElement root)
    {
        if (!root.TryGetProperty("type", out JsonElement typeElement)
            || !root.TryGetProperty("channel", out JsonElement channelElement)
            || !channelElement.TryGetProperty("id", out JsonElement channelIdElement))
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

        if (string.Equals(type, "ChannelDtmfReceived", StringComparison.Ordinal)
            && root.TryGetProperty("digit", out JsonElement digitElement))
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
            int? cause = root.TryGetProperty("cause", out JsonElement causeElement)
                && causeElement.TryGetInt32(out int parsedCause)
                ? parsedCause
                : null;
            string? causeText = root.TryGetProperty("cause_txt", out JsonElement textElement)
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
            state.TerminalDisposition = SimProviderDisposition.NetworkError;
            state.TechnicalErrorCode = technicalCode;
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
            return await client.SendAsync(request, cancellationToken).ConfigureAwait(false);
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

        _ = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
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
        bool channelHealthy,
        string message,
        Exception? exception = null) =>
        new(disposition, code, channelHealthy, message, exception);
}
