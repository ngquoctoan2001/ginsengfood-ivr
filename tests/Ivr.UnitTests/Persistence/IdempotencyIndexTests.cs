using Ivr.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Ivr.UnitTests.Persistence;

/// <summary>
/// <c>ivr_idempotency_keys</c> is written on every mutating request that goes through an
/// idempotency store, so an index on it is maintained on that path whether or not anything can
/// use it. Until <c>W-0268</c> one of them could not: <c>expires_at</c> was indexed, four write
/// paths insert these rows, and none of them sets the column.
/// </summary>
public sealed class IdempotencyIndexTests
{
    [Fact]
    [Trait("TestId", "UT-SCHEMA-IDEMPOTENCY-INDEX-01")]
    public void TheIdempotencyTableCarriesNoIndexNoQueryCanUse()
    {
        IEntityType entity = RollingDeploySchemaCompatibilityTests.BuildModel()
            .FindEntityType(typeof(IdempotencyKeyEntity))!;

        string[] indexed = [.. entity.GetIndexes()
            .Select(index => string.Join("+", index.Properties.Select(property => property.Name)))
            .Order(StringComparer.Ordinal)];

        // Three of these are not this table's choice: PersistenceModelConfiguration gives every
        // RetainedEntity the same RetainUntil / LegalHoldUntil / AnonymizedAt trio, and the
        // retention sweep reads them. CreatedAt is the one index specs/database/04-indexes.md
        // names for this table -- "retention/purge scan" -- and RetentionTargetCatalog purges by
        // it. ExpiresAt was never in that spec.
        Assert.Equal(["AnonymizedAt", "CreatedAt", "LegalHoldUntil", "RetainUntil"], indexed);

        // The column stays, and that is deliberate rather than an oversight:
        // specs/database/02-tables.md declares expires_at for this table. Dropping it would
        // contradict the spec and, as a DropColumn, would owe the expand/contract dance that
        // UT-SCHEMA-BACKCOMPAT-01 enforces. Dropping only the index owes neither.
        Assert.NotNull(entity.FindProperty(nameof(IdempotencyKeyEntity.ExpiresAt)));
    }
}
