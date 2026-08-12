namespace Ivr.Infrastructure.Correlation;

public static class CorrelationIdGenerator
{
    public static string Create()
    {
        string entropy = Guid.NewGuid().ToString("N");
        return $"corr-{string.Join('-', Enumerable.Range(0, 8).Select(
            index => entropy.Substring(index * 4, 4)))}";
    }
}
