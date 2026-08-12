namespace Ivr.Infrastructure.Correlation;

public interface ICorrelationContext
{
    public string? CorrelationId { get; }

    public string GetOrCreate();

    public IDisposable Push(string correlationId);
}
