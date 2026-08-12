using Ivr.Domain.Errors;
using Ivr.Domain.Privacy;
using Ivr.Infrastructure.Persistence;
using Ivr.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Ivr.Infrastructure.Evidence;

public sealed class PostgresEvidenceStore(
    IDbContextFactory<IvrDbContext> dbContextFactory,
    TimeProvider timeProvider) : IEvidenceStore
{
    public async Task<EvidenceRecord> AppendAsync(
        EvidenceWrite request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        Validate(request);
        DateTimeOffset createdAt = timeProvider.GetUtcNow();
        var entity = new EvidenceEntity
        {
            EvidenceRef = request.EvidenceRef,
            Kind = request.Kind,
            CorrelationId = request.CorrelationId,
            WorkId = "RUNTIME",
            PayloadRef = request.PayloadRef,
            CreatedAt = createdAt,
        };
        await using IvrDbContext dbContext = await dbContextFactory.CreateDbContextAsync(
            cancellationToken);
        dbContext.Evidence.Add(entity);
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (
            exception.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            throw IvrErrors.IdempotencyConflict();
        }

        return new EvidenceRecord(
            request.EvidenceRef,
            request.Kind,
            request.CorrelationId,
            request.PayloadRef,
            createdAt);
    }

    private static void Validate(EvidenceWrite request)
    {
        if (string.IsNullOrWhiteSpace(request.EvidenceRef)
            || string.IsNullOrWhiteSpace(request.Kind)
            || string.IsNullOrWhiteSpace(request.CorrelationId)
            || string.IsNullOrWhiteSpace(request.PayloadRef))
        {
            throw IvrErrors.MalformedRequest("Evidence fields are required.");
        }

        PiiGuard.EnsureSafeText(request.EvidenceRef);
        PiiGuard.EnsureSafeText(request.Kind);
        PiiGuard.EnsureSafeText(request.CorrelationId);
        PiiGuard.EnsureSafeText(request.PayloadRef);
    }
}
