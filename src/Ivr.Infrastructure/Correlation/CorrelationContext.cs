using Ivr.Domain.Privacy;

namespace Ivr.Infrastructure.Correlation;

public sealed class CorrelationContext : ICorrelationContext
{
    private static readonly AsyncLocal<Holder?> Current = new();

    public string? CorrelationId => Current.Value?.Value;

    public string GetOrCreate()
    {
        if (!string.IsNullOrWhiteSpace(CorrelationId))
        {
            return CorrelationId;
        }

        string generated = CorrelationIdGenerator.Create();
        Current.Value = new Holder(generated);
        return generated;
    }

    public IDisposable Push(string correlationId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(correlationId);
        PiiGuard.EnsureSafeText(correlationId);
        Holder? previous = Current.Value;
        Current.Value = new Holder(correlationId);
        return new RestoreScope(previous);
    }

    private sealed record Holder(string Value);

    private sealed class RestoreScope(Holder? previous) : IDisposable
    {
        private bool disposed;

        public void Dispose()
        {
            if (disposed)
            {
                return;
            }

            Current.Value = previous;
            disposed = true;
        }
    }
}
