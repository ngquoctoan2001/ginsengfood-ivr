namespace Ivr.Infrastructure.Persistence;

internal sealed class FeatureFlagPersistenceSession
{
    private readonly AsyncLocal<IvrDbContext?> current = new();

    public IvrDbContext? Current
    {
        get => current.Value;
        set => current.Value = value;
    }
}
