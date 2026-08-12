using Microsoft.EntityFrameworkCore;

namespace Ivr.Infrastructure.Persistence;

/// <summary>
/// Owns the future IVR PostgreSQL schema. Entity mappings start in P1-2.
/// </summary>
public sealed class IvrDbContext(DbContextOptions<IvrDbContext> options) : DbContext(options);
