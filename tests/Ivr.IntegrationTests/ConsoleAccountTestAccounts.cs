using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Ivr.Api.Accounts;
using Ivr.Domain.Accounts;
using Ivr.Domain.Retention;
using Ivr.Infrastructure.Accounts;
using Ivr.Infrastructure.Persistence;
using Ivr.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Ivr.IntegrationTests;

/// <summary>
/// Seeding and sign-in helpers shared by the console account suites, so the three requested
/// W-0105 accounts are described in exactly one place.
/// </summary>
internal static class ConsoleAccountTestAccounts
{
    public const string Password = "123123123zZ*";

    private static readonly System.Text.Json.JsonSerializerOptions JsonOptions =
        new(System.Text.Json.JsonSerializerDefaults.Web);

    public static readonly ConsoleAccountSeed[] Requested =
    [
        new("admin", "Quản trị hệ thống", ConsoleAccountRoles.Admin, true),
        new("ngquoctoan2001", "Nguyễn Quốc Toàn", ConsoleAccountRoles.Operator, false),
        new("trcongphuc2003", "Trương Công Phúc", ConsoleAccountRoles.Operator, false),
    ];

    public static Task SeedRequestedAsync(IDbContextFactory<IvrDbContext> factory) =>
        SeedAsync(factory, Requested);

    public static async Task SeedAsync(
        IDbContextFactory<IvrDbContext> factory,
        params ConsoleAccountSeed[] accounts)
    {
        await using IvrDbContext dbContext = await factory.CreateDbContextAsync();
        DateTimeOffset now = DateTimeOffset.UtcNow;
        dbContext.ConsoleAccounts.AddRange(accounts.Select(account => new ConsoleAccountEntity
        {
            Id = Guid.NewGuid(),
            Username = account.Username,
            DisplayName = account.DisplayName,
            Role = account.Role,
            Status = ConsoleAccountStatuses.Active,
            IsBuiltin = account.IsBuiltin,
            PasswordHash = ConsolePasswordHasher.Hash(Password),
            PasswordChangedAt = now,
            CreatedAt = now,
            UpdatedAt = now,
            Version = 1,
            RetentionClass = RetentionDataClasses.StaffAccount,
        }));
        await dbContext.SaveChangesAsync();
    }

    public static async Task<ConsoleSignInApiResult> SignInAsync(
        HttpClient client,
        string username,
        string password,
        HttpStatusCode expectedStatus = HttpStatusCode.OK)
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            "/v1/ivr/order-confirmation/auth/sign-in")
        {
            Content = JsonContent.Create(new ConsoleSignInRequest(username, password)),
        };
        request.Headers.Add("X-Correlation-Id", $"corr-sign-in-{Guid.NewGuid():N}");
        using HttpResponseMessage response = await client.SendAsync(request);
        Assert.Equal(expectedStatus, response.StatusCode);
        if (expectedStatus != HttpStatusCode.OK)
        {
            Assert.Contains(
                "IVR_UNAUTHENTICATED",
                await response.Content.ReadAsStringAsync(),
                StringComparison.Ordinal);
            return null!;
        }

        return await response.Content.ReadFromJsonAsync<ConsoleSignInApiResult>()
            ?? throw new InvalidOperationException("Sign-in response was empty.");
    }

    /// <summary>Issues a request on behalf of a signed-in console session.</summary>
    public static async Task<HttpResponseMessage> SendAsync(
        HttpClient client,
        HttpMethod method,
        string path,
        ConsoleSignInApiResult session,
        bool includeActor = true)
    {
        using var request = new HttpRequestMessage(
            method,
            path.StartsWith("/v1/", StringComparison.Ordinal)
                ? path
                : $"/v1/ivr/order-confirmation{path}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", session.AccessToken);
        request.Headers.Add("X-Correlation-Id", $"corr-bearer-{Guid.NewGuid():N}");
        if (includeActor)
        {
            request.Headers.Add("X-Actor-Id", session.Session.Account.Username);
        }

        return await client.SendAsync(request);
    }

    /// <summary>Issues an audited, idempotent account mutation as a signed-in console session.</summary>
    public static async Task<TResponse> MutateAsync<TRequest, TResponse>(
        HttpClient client,
        HttpMethod method,
        string path,
        TRequest body,
        ConsoleSignInApiResult session,
        HttpStatusCode expectedStatus = HttpStatusCode.OK,
        string? expectedErrorCode = null)
    {
        using var request = new HttpRequestMessage(
            method,
            $"/v1/ivr/order-confirmation{path}")
        {
            Content = JsonContent.Create(body),
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", session.AccessToken);
        request.Headers.Add("X-Correlation-Id", $"corr-mutation-{Guid.NewGuid():N}");
        request.Headers.Add("X-Actor-Id", session.Session.Account.Username);
        request.Headers.Add("Idempotency-Key", $"key-{Guid.NewGuid():N}");
        using HttpResponseMessage response = await client.SendAsync(request);
        string payload = await response.Content.ReadAsStringAsync();
        Assert.Equal(expectedStatus, response.StatusCode);
        if (expectedErrorCode is not null)
        {
            Assert.Contains(expectedErrorCode, payload, StringComparison.Ordinal);
        }

        return expectedStatus == HttpStatusCode.OK
            ? System.Text.Json.JsonSerializer.Deserialize<TResponse>(payload, JsonOptions)
                ?? throw new InvalidOperationException("Mutation response was empty.")
            : default!;
    }
}

internal sealed record ConsoleAccountSeed(
    string Username,
    string DisplayName,
    string Role,
    bool IsBuiltin);
