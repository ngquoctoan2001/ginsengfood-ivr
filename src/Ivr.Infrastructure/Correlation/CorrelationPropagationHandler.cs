namespace Ivr.Infrastructure.Correlation;

public sealed class CorrelationPropagationHandler(ICorrelationContext correlationContext)
    : DelegatingHandler
{
    public const string HeaderName = "X-Correlation-Id";

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        request.Headers.Remove(HeaderName);
        request.Headers.TryAddWithoutValidation(HeaderName, correlationContext.GetOrCreate());

        return base.SendAsync(request, cancellationToken);
    }
}
