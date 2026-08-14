using Ivr.Domain.Ports;
using System.Security.Cryptography;
using System.Text;

namespace Ivr.Infrastructure.Callbacks;

public enum CallbackTransportOutcome
{
    Accepted,
    DuplicateAccepted,
    BlockedByCore,
    ReviewRequired,
    RejectedStale,
    IdempotencyConflict,
    Invalid,
    AuthRejected,
    TransientFailure,
}

public sealed record CallbackTransportResult(
    CallbackTransportOutcome Outcome,
    int? HttpStatus,
    string Code,
    string? SafeError = null);

public static class CallbackPayloadIntegrity
{
    public static void EnsureValid(Persistence.Outbox.CallbackOutboxMessage message)
    {
        ArgumentNullException.ThrowIfNull(message);
        string actual = Convert.ToHexString(
            SHA256.HashData(Encoding.UTF8.GetBytes(message.PayloadJson)));
        if (!string.Equals(actual, message.PayloadSha256, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Callback payload hash mismatch.");
        }
    }
}

public interface ITargetV1CallbackTransport
{
    public Task<CallbackTransportResult> SendAsync(
        Persistence.Outbox.CallbackOutboxMessage message,
        CancellationToken cancellationToken);
}

public interface ICurrentGoldenHourCallbackTransport
{
    public Task<CallbackTransportResult> SendAsync(
        Persistence.Outbox.CallbackOutboxMessage message,
        CancellationToken cancellationToken);
}

public sealed record CallbackCircuitState(
    bool IsOpen,
    int ConsecutiveTransientFailures,
    DateTimeOffset? OpenUntil,
    string Readiness);

public sealed class CallbackCircuitBreaker(
    TimeProvider timeProvider,
    Microsoft.Extensions.Options.IOptions<CallbackDeliveryOptions> options)
{
    private readonly object _sync = new();
    private int _consecutiveFailures;
    private DateTimeOffset? _openUntil;
    private bool _halfOpenProbeInProgress;

    public bool TryEnter()
    {
        lock (_sync)
        {
            DateTimeOffset now = timeProvider.GetUtcNow();
            if (_openUntil is null)
            {
                return true;
            }

            if (_openUntil > now || _halfOpenProbeInProgress)
            {
                return false;
            }

            _halfOpenProbeInProgress = true;
            return true;
        }
    }

    public void RecordSuccess()
    {
        lock (_sync)
        {
            _consecutiveFailures = 0;
            _openUntil = null;
            _halfOpenProbeInProgress = false;
        }
    }

    public void RecordTransientFailure()
    {
        lock (_sync)
        {
            _halfOpenProbeInProgress = false;
            _consecutiveFailures++;
            if (_consecutiveFailures >= options.Value.CircuitFailureThreshold)
            {
                _openUntil = timeProvider.GetUtcNow().AddSeconds(
                    options.Value.CircuitOpenSeconds);
            }
        }
    }

    public void RecordProbeAborted()
    {
        lock (_sync)
        {
            _halfOpenProbeInProgress = false;
        }
    }

    public CallbackCircuitState Snapshot()
    {
        lock (_sync)
        {
            bool open = _openUntil > timeProvider.GetUtcNow();
            bool halfOpen = _halfOpenProbeInProgress;
            return new CallbackCircuitState(
                open || halfOpen,
                _consecutiveFailures,
                _openUntil,
                open
                    ? "NOT_READY_CIRCUIT_OPEN"
                    : halfOpen
                        ? "NOT_READY_CIRCUIT_HALF_OPEN"
                        : "READY");
        }
    }
}

public sealed class RefreshingMockServiceTokenProvider(
    TimeProvider timeProvider,
    Microsoft.Extensions.Options.IOptions<CallbackDeliveryOptions> options) : IServiceTokenProvider
{
    private readonly object _sync = new();
    private ServiceAccessToken? _token;
    private int _generation;

    public ValueTask<ServiceAccessToken> GetAsync(
        string audience,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(audience);
        cancellationToken.ThrowIfCancellationRequested();
        lock (_sync)
        {
            DateTimeOffset now = timeProvider.GetUtcNow();
            if (_token is null
                || _token.ExpiresAt <= now.AddSeconds(options.Value.MockTokenRefreshSkewSeconds))
            {
                _generation++;
                _token = ServiceAccessToken.CreateTrusted(
                    string.Concat("mock-callback-token-", _generation.ToString(
                        System.Globalization.CultureInfo.InvariantCulture)),
                    now.AddSeconds(options.Value.MockTokenLifetimeSeconds));
            }

            return ValueTask.FromResult(_token);
        }
    }
}
