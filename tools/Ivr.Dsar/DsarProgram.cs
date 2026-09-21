using System.Text.Json;
using System.Text.Json.Serialization;
using Ivr.Infrastructure.Audit;
using Ivr.Infrastructure.Configuration;
using Ivr.Infrastructure.Governance;
using Ivr.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Npgsql;

namespace Ivr.Dsar;

public static class DsarProgram
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
    };

    public static async Task<int> Main(string[] args)
    {
        ArgumentNullException.ThrowIfNull(args);
        if (args.Length == 0 || args.SequenceEqual(["--help"]))
        {
            Console.WriteLine("""
                IVR DSAR: preview by default; one order per invocation.
                --identity
                --order-code ORDER --request-ref REQUEST
                --order-code ORDER --request-ref REQUEST --execute --confirm-order ORDER --subject-verified
                Requires dsar-operator.json beside this application and IVR_DSAR_CONNECTION_STRING.
                The database credential and application/policy files must be restricted to the assigned operator.
                """);
            return 0;
        }

        string identity = OperatingSystem.IsWindows()
            ? $"{Environment.UserDomainName}\\{Environment.UserName}"
            : Environment.UserName;
        if (args.SequenceEqual(["--identity"]))
        {
            Console.WriteLine(JsonSerializer.Serialize(new { CurrentIdentity = identity }, JsonOptions));
            return 0;
        }

        try
        {
            DsarRequest request = DsarRequest.Parse(args);
            string policyFile = Path.Combine(AppContext.BaseDirectory, "dsar-operator.json");
            DsarOperatorPolicy policy = JsonSerializer.Deserialize<DsarOperatorPolicy>(
                await File.ReadAllTextAsync(policyFile), JsonOptions)
                ?? throw new InvalidOperationException("Operator policy is empty.");
            DsarCommand.Authorize(policy, identity);
            string connectionString = Environment.GetEnvironmentVariable("IVR_DSAR_CONNECTION_STRING")
                ?? throw new InvalidOperationException("IVR_DSAR_CONNECTION_STRING is missing.");
            var options = new DbContextOptionsBuilder<IvrDbContext>().UseNpgsql(CreateConnectionString(connectionString)).Options;
            var factory = new PooledDbContextFactory<IvrDbContext>(options);
            var service = new DsarService(factory, new PostgresAuditLogger(factory, TimeProvider.System), TimeProvider.System);
            DsarCommandResult result = await DsarCommand.ExecuteAsync(service, request, policy, identity);
            Console.WriteLine(JsonSerializer.Serialize(result, JsonOptions));
            return 0;
        }
        catch (UnauthorizedAccessException)
        {
            Console.Error.WriteLine("DSAR_REFUSED: current OS identity does not match the operator policy.");
            return 3;
        }
        catch (Exception error) when (error is ArgumentException or JsonException or IOException or InvalidOperationException)
        {
            Console.Error.WriteLine("DSAR_REFUSED: check arguments, operator policy and connection configuration; use --help.");
            return 2;
        }
        catch (Exception)
        {
            // Driver/SQL messages can contain connection or customer values. Do not print them.
            Console.Error.WriteLine("DSAR_FAILED: operation failed; retain the request reference and inspect restricted server diagnostics.");
            return 1;
        }
    }

    public static string CreateConnectionString(string connectionString) =>
        new NpgsqlConnectionStringBuilder(ServiceCollectionExtensions.WithoutGssNegotiation(connectionString))
        {
            ApplicationName = "Ivr.Dsar",
        }.ConnectionString;
}
