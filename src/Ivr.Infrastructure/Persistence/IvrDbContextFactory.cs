using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Ivr.Infrastructure.Persistence;

internal sealed class IvrDbContextFactory : IDesignTimeDbContextFactory<IvrDbContext>
{
    public IvrDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<IvrDbContext>()
            .UseNpgsql(
                "Host=localhost;Port=55433;Database=ivr;Username=ivr;Password=unused")
            .Options;
        return new IvrDbContext(options);
    }
}
