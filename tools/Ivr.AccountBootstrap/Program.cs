using Ivr.Domain.Accounts;
using Ivr.Domain.Retention;
using Ivr.Infrastructure.Accounts;
using Ivr.Infrastructure.Persistence;
using Ivr.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

string environment = ReadArgument(args, "--environment")
    ?? Environment.GetEnvironmentVariable("IVR_ACCOUNT_BOOTSTRAP_ENVIRONMENT")
    ?? string.Empty;
if (environment is not ("local" or "lab"))
{
    throw new InvalidOperationException(
        "Account bootstrap is allowed only with --environment local or --environment lab.");
}

string connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__IvrDb")
    ?? Environment.GetEnvironmentVariable("IVR_ACCOUNT_BOOTSTRAP_CONNECTION_STRING")
    ?? string.Empty;
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "ConnectionStrings__IvrDb or IVR_ACCOUNT_BOOTSTRAP_CONNECTION_STRING is required.");
}

string password = Environment.GetEnvironmentVariable("IVR_ACCOUNT_BOOTSTRAP_PASSWORD")
    ?? ReadSecret(args.Contains("--password-stdin", StringComparer.Ordinal));
if (string.IsNullOrEmpty(password))
{
    throw new InvalidOperationException("The bootstrap password is required through secure input.");
}

BootstrapAccount[] requested =
[
    new("admin", "Quản trị hệ thống", ConsoleAccountRoles.Admin, true),
    new("ngquoctoan2001", "Nguyễn Quốc Toàn", ConsoleAccountRoles.Operator, false),
    new("trcongphuc2003", "Trương Công Phúc", ConsoleAccountRoles.Operator, false),
];
foreach (BootstrapAccount item in requested)
{
    if (!ConsolePasswordPolicy.IsValid(password, item.Username))
    {
        throw new InvalidOperationException(
            $"The supplied bootstrap password does not satisfy policy for {item.Username}.");
    }
}

var options = new DbContextOptionsBuilder<IvrDbContext>()
    .UseNpgsql(connectionString)
    .Options;
await using var dbContext = new IvrDbContext(options);
await dbContext.Database.MigrateAsync();

DateTimeOffset now = DateTimeOffset.UtcNow;
foreach (BootstrapAccount item in requested)
{
    ConsoleAccountEntity? existing = await dbContext.ConsoleAccounts
        .SingleOrDefaultAsync(account => account.Username == item.Username);
    if (existing is not null)
    {
        if (!string.Equals(existing.DisplayName, item.DisplayName, StringComparison.Ordinal)
            || !string.Equals(existing.Role, item.Role, StringComparison.Ordinal)
            || !string.Equals(existing.Status, ConsoleAccountStatuses.Active, StringComparison.Ordinal)
            || existing.IsBuiltin != item.IsBuiltin)
        {
            throw new InvalidOperationException(
                $"Existing account {item.Username} differs from the approved bootstrap manifest; no overwrite was performed.");
        }

        Console.WriteLine($"EXISTS {item.Username}");
        continue;
    }

    dbContext.ConsoleAccounts.Add(new ConsoleAccountEntity
    {
        Id = Guid.NewGuid(),
        Username = item.Username,
        DisplayName = item.DisplayName,
        Role = item.Role,
        Status = ConsoleAccountStatuses.Active,
        IsBuiltin = item.IsBuiltin,
        PasswordHash = ConsolePasswordHasher.Hash(password),
        PasswordChangedAt = now,
        CreatedAt = now,
        UpdatedAt = now,
        Version = 1,
        RetentionClass = RetentionDataClasses.StaffAccount,
    });
    dbContext.AuditLog.Add(new AuditLogEntity
    {
        AuditId = Guid.NewGuid(),
        ActorId = "account-bootstrap",
        ActorType = "service",
        Action = "ADMIN_ACCOUNT_BOOTSTRAP",
        TargetType = "console_account",
        TargetId = item.Username,
        Reason = $"W-0105 {environment} account bootstrap",
        CorrelationId = $"w0105-bootstrap-{Guid.NewGuid():N}",
        DataJson = "{}",
        CreatedAt = now,
        RetentionClass = "audit_log",
    });
    Console.WriteLine($"CREATED {item.Username}");
}

await dbContext.SaveChangesAsync();
Console.WriteLine("ACCOUNT_BOOTSTRAP_COMPLETE");

static string? ReadArgument(string[] arguments, string name)
{
    int index = Array.IndexOf(arguments, name);
    return index >= 0 && index + 1 < arguments.Length ? arguments[index + 1] : null;
}

static string ReadSecret(bool fromStandardInput)
{
    if (fromStandardInput || Console.IsInputRedirected)
    {
        return Console.ReadLine() ?? string.Empty;
    }

    Console.Write("Bootstrap password: ");
    var value = new System.Text.StringBuilder();
    while (true)
    {
        ConsoleKeyInfo key = Console.ReadKey(intercept: true);
        if (key.Key == ConsoleKey.Enter)
        {
            Console.WriteLine();
            return value.ToString();
        }

        if (key.Key == ConsoleKey.Backspace && value.Length > 0)
        {
            value.Length--;
        }
        else if (!char.IsControl(key.KeyChar))
        {
            value.Append(key.KeyChar);
        }
    }
}

internal sealed record BootstrapAccount(
    string Username,
    string DisplayName,
    string Role,
    bool IsBuiltin);
