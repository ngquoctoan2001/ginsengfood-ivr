using Ivr.Domain.Confirmation;
using Ivr.Domain.Ports;
using Ivr.Infrastructure.Repositories;

namespace Ivr.Worker.Normalization;

public sealed class ResultNormalizer(IResultRepository resultRepository)
{
    public static NormalizedResult Normalize(
        SimProviderDisposition? disposition,
        string? dtmfKey,
        string? technicalErrorCode,
        AttemptNormalizationContext attemptContext) => DispositionMapper.Normalize(
            disposition,
            dtmfKey,
            technicalErrorCode,
            attemptContext);

    public async Task<IReadOnlyList<NormalizationPersistenceResult>> RunBatchAsync(
        string workerId,
        int batchSize,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(workerId);
        if (batchSize is < 1 or > 512)
        {
            throw new ArgumentOutOfRangeException(nameof(batchSize));
        }

        List<NormalizationPersistenceResult> normalized = [];
        while (normalized.Count < batchSize)
        {
            NormalizationPersistenceResult? result = await resultRepository.NormalizeNextAsync(
                workerId,
                cancellationToken).ConfigureAwait(false);
            if (result is null)
            {
                break;
            }

            normalized.Add(result);
        }

        return normalized;
    }
}
