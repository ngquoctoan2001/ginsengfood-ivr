using System.Data;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Ivr.Api.Auth;
using Ivr.Domain.Accounts;
using Ivr.Domain.Errors;
using Ivr.Domain.Privacy;
using Ivr.Domain.Retention;
using Ivr.Infrastructure.Accounts;
using Ivr.Infrastructure.Persistence;
using Ivr.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Ivr.Api.Accounts;

public sealed class ConsoleAccountService(
    IDbContextFactory<IvrDbContext> dbContextFactory,
    TimeProvider timeProvider)
{
    public static readonly TimeSpan SessionLifetime = TimeSpan.FromHours(8);
    private static readonly JsonSerializerOptions JsonOptions =
        new(JsonSerializerDefaults.Web);
    private readonly string dummyPasswordHash = ConsolePasswordHasher.Hash(
        "DummyCredential-Only-For-Constant-Work-1!");

    public async Task<ConsoleSignInApiResult> SignInAsync(
        ConsoleSignInRequest request,
        string correlationId,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        string username = ConsoleUsernamePolicy.Normalize(request.Username);
        if (!ConsoleUsernamePolicy.IsValid(username) || string.IsNullOrEmpty(request.Password))
        {
            _ = ConsolePasswordHasher.Verify(dummyPasswordHash, request.Password ?? string.Empty);
            throw IvrErrors.Unauthenticated();
        }

        await using IvrDbContext dbContext = await dbContextFactory.CreateDbContextAsync(
            cancellationToken);
        await using var transaction = await dbContext.Database.BeginTransactionAsync(
            IsolationLevel.Serializable,
            cancellationToken);
        ConsoleAccountEntity? account = await dbContext.ConsoleAccounts
            .FromSqlInterpolated(
                $"SELECT * FROM ivr_console_accounts WHERE username = {username} FOR UPDATE")
            .SingleOrDefaultAsync(cancellationToken);
        if (account is null)
        {
            _ = ConsolePasswordHasher.Verify(dummyPasswordHash, request.Password);
            await transaction.CommitAsync(cancellationToken);
            throw IvrErrors.Unauthenticated();
        }

        DateTimeOffset now = timeProvider.GetUtcNow();
        ConsolePasswordVerificationResult verification = ConsolePasswordHasher.Verify(
            account.PasswordHash,
            request.Password);
        if (!string.Equals(account.Status, ConsoleAccountStatuses.Active, StringComparison.Ordinal)
            || account.DeletedAt is not null
            || account.LockedUntil > now)
        {
            await transaction.CommitAsync(cancellationToken);
            throw IvrErrors.Unauthenticated();
        }

        // Reaching here means any lockout has already expired, because the guard above rejects a
        // live one. Clearing the counter as well as the timestamp is the part that matters: leave
        // the count sitting at the threshold and the very next failure re-locks immediately, so an
        // account that tripped the limit once is stuck at one attempt per fifteen minutes for the
        // rest of its life. Operators cannot reset their own password, so that state is an
        // administrator ticket for every mistyped-password streak.
        if (account.LockedUntil is not null)
        {
            account.FailedLoginCount = 0;
            account.LockedUntil = null;
        }

        if (verification == ConsolePasswordVerificationResult.Failed)
        {
            account.FailedLoginCount = Math.Min(100, account.FailedLoginCount + 1);
            account.LockedUntil = ConsoleLockoutPolicy.LockedUntil(
                account.FailedLoginCount,
                now);
            account.UpdatedAt = now;
            account.Version++;
            await dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            throw IvrErrors.Unauthenticated();
        }

        if (verification == ConsolePasswordVerificationResult.SuccessRehashNeeded)
        {
            account.PasswordHash = ConsolePasswordHasher.Hash(request.Password);
            account.PasswordChangedAt = now;
        }

        account.FailedLoginCount = 0;
        account.LockedUntil = null;
        account.LastLoginAt = now;
        account.UpdatedAt = now;
        account.Version++;

        string rawToken = CreateRawToken();
        var session = new ConsoleSessionEntity
        {
            Id = Guid.NewGuid(),
            AccountId = account.Id,
            TokenHash = HashToken(rawToken),
            CreatedAt = now,
            ExpiresAt = now.Add(SessionLifetime),
            RetentionClass = RetentionDataClasses.ConsoleSession,
        };
        dbContext.ConsoleSessions.Add(session);
        AppendAudit(
            dbContext,
            account.Username,
            "ACCOUNT_SIGN_IN",
            account.Username,
            null,
            correlationId,
            null,
            JsonSerializer.Serialize(new { session_expires_at = session.ExpiresAt }, JsonOptions),
            now);
        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return new ConsoleSignInApiResult(
            rawToken,
            "Bearer",
            new ConsoleSessionView(
                ToView(account, now),
                IvrRoles.PermissionsFor(account.Role).Order(StringComparer.Ordinal).ToArray(),
                session.ExpiresAt));
    }

    public async Task<AuthenticatedConsoleSession?> AuthenticateAsync(
        string rawToken,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(rawToken) || rawToken.Length > 128)
        {
            return null;
        }

        string tokenHash = HashToken(rawToken);
        await using IvrDbContext dbContext = await dbContextFactory.CreateDbContextAsync(
            cancellationToken);
        var record = await (
            from session in dbContext.ConsoleSessions.AsNoTracking()
            join account in dbContext.ConsoleAccounts.AsNoTracking()
                on session.AccountId equals account.Id
            where session.TokenHash == tokenHash
            select new { Session = session, Account = account })
            .SingleOrDefaultAsync(cancellationToken);
        DateTimeOffset now = timeProvider.GetUtcNow();
        if (record is null
            || record.Session.RevokedAt is not null
            || record.Session.ExpiresAt <= now
            || record.Account.DeletedAt is not null
            || !string.Equals(
                record.Account.Status,
                ConsoleAccountStatuses.Active,
                StringComparison.Ordinal))
        {
            return null;
        }

        return new AuthenticatedConsoleSession(
            record.Session.Id,
            record.Account.Id,
            record.Account.Username,
            record.Account.DisplayName,
            record.Account.Role,
            IvrRoles.PermissionsFor(record.Account.Role).Order(StringComparer.Ordinal).ToArray(),
            record.Session.ExpiresAt);
    }

    public async Task<bool> SignOutAsync(
        string rawToken,
        string correlationId,
        CancellationToken cancellationToken)
    {
        string tokenHash = HashToken(rawToken);
        await using IvrDbContext dbContext = await dbContextFactory.CreateDbContextAsync(
            cancellationToken);
        ConsoleSessionEntity? session = await dbContext.ConsoleSessions
            .SingleOrDefaultAsync(item => item.TokenHash == tokenHash, cancellationToken);
        if (session is null || session.RevokedAt is not null)
        {
            return false;
        }

        DateTimeOffset now = timeProvider.GetUtcNow();
        session.RevokedAt = now;
        session.RevokeReason = "SIGN_OUT";
        ConsoleAccountEntity account = await dbContext.ConsoleAccounts
            .SingleAsync(item => item.Id == session.AccountId, cancellationToken);
        AppendAudit(
            dbContext,
            account.Username,
            "ACCOUNT_SIGN_OUT",
            account.Username,
            null,
            correlationId,
            null,
            null,
            now);
        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<ConsoleAccountPageApiResult> ListAsync(
        int page,
        int pageSize,
        bool includeDeleted,
        CancellationToken cancellationToken)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);
        await using IvrDbContext dbContext = await dbContextFactory.CreateDbContextAsync(
            cancellationToken);
        IQueryable<ConsoleAccountEntity> query = dbContext.ConsoleAccounts.AsNoTracking();

        // Soft-deleted rows exist so audit identity survives and the username is never reassigned,
        // not because anyone wants to administer them. Returning them by default made the roster
        // grow monotonically and put rows in the list whose only remaining action is nothing at
        // all, so they are opt-in — and `total_count` follows the same filter or paging lies.
        if (!includeDeleted)
        {
            query = query.Where(item => item.DeletedAt == null);
        }

        int total = await query.CountAsync(cancellationToken);
        DateTimeOffset now = timeProvider.GetUtcNow();
        ConsoleAccountView[] items = (await query
                .OrderBy(item => item.Username)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToArrayAsync(cancellationToken))
            .Select(item => ToView(item, now))
            .ToArray();
        return new ConsoleAccountPageApiResult(page, pageSize, total, items);
    }

    public async Task<ConsoleAccountView> GetAsync(
        Guid accountId,
        CancellationToken cancellationToken)
    {
        await using IvrDbContext dbContext = await dbContextFactory.CreateDbContextAsync(
            cancellationToken);
        ConsoleAccountEntity account = await dbContext.ConsoleAccounts.AsNoTracking()
            .SingleOrDefaultAsync(item => item.Id == accountId, cancellationToken)
            ?? throw IvrErrors.NotFound("The account was not found.");
        return ToView(account, timeProvider.GetUtcNow());
    }

    public async Task<ConsoleAccountView> GetByUsernameAsync(
        string username,
        CancellationToken cancellationToken)
    {
        await using IvrDbContext dbContext = await dbContextFactory.CreateDbContextAsync(
            cancellationToken);
        ConsoleAccountEntity account = await dbContext.ConsoleAccounts.AsNoTracking()
            .SingleOrDefaultAsync(item => item.Username == username, cancellationToken)
            ?? throw IvrErrors.NotFound("The account was not found.");
        return ToView(account, timeProvider.GetUtcNow());
    }

    public Task<ConsoleAccountView> CreateAsync(
        CreateConsoleAccountRequest request,
        string actor,
        string correlationId,
        string idempotencyKey,
        CancellationToken cancellationToken) => ExecuteMutationAsync(
        "console-account:create",
        idempotencyKey,
        request,
        actor,
        correlationId,
        async (dbContext, now) =>
        {
            string username = ConsoleUsernamePolicy.Normalize(request.Username);
            ValidateAccountInput(username, request.DisplayName, request.Role, request.Password);
            ValidateReason(request.Reason);
            if (await dbContext.ConsoleAccounts.AnyAsync(
                    item => item.Username == username,
                    cancellationToken))
            {
                throw IvrErrors.AccountConflict();
            }

            var account = new ConsoleAccountEntity
            {
                Id = Guid.NewGuid(),
                Username = username,
                DisplayName = request.DisplayName.Trim(),
                Role = request.Role,
                Status = ConsoleAccountStatuses.Active,
                PasswordHash = ConsolePasswordHasher.Hash(request.Password),
                PasswordChangedAt = now,
                CreatedAt = now,
                UpdatedAt = now,
                Version = 1,
                RetentionClass = RetentionDataClasses.StaffAccount,
            };
            dbContext.ConsoleAccounts.Add(account);
            string after = StateJson(account);
            return new Mutation<ConsoleAccountView>(
                ToView(account, now),
                "ADMIN_ACCOUNT_CREATE",
                username,
                request.Reason,
                null,
                after);
        },
        cancellationToken);

    public Task<ConsoleAccountView> UpdateAsync(
        Guid accountId,
        UpdateConsoleAccountRequest request,
        string actor,
        string correlationId,
        string idempotencyKey,
        CancellationToken cancellationToken) => ExecuteMutationAsync(
        $"console-account:update:{accountId:D}",
        idempotencyKey,
        request,
        actor,
        correlationId,
        async (dbContext, now) =>
        {
            ValidateReason(request.Reason);
            ConsoleAccountEntity account = await RequireAccountAsync(
                dbContext,
                accountId,
                cancellationToken);
            EnsureVersion(account, request.Version);
            string before = StateJson(account);
            string displayName = request.DisplayName is null
                ? account.DisplayName
                : ValidateDisplayName(request.DisplayName);
            string role = request.Role ?? account.Role;
            string status = request.Status ?? account.Status;
            if (!ConsoleAccountRoles.IsDefined(role) || !ConsoleAccountStatuses.IsDefined(status))
            {
                throw IvrErrors.AccountPolicyViolation("The requested role or status is invalid.");
            }

            if (account.IsBuiltin
                && (!string.Equals(role, ConsoleAccountRoles.Admin, StringComparison.Ordinal)
                    || !string.Equals(status, ConsoleAccountStatuses.Active, StringComparison.Ordinal)))
            {
                throw IvrErrors.AccountPolicyViolation(
                    "The built-in admin role and active status cannot be changed.");
            }

            bool removesActiveAdmin =
                string.Equals(account.Role, ConsoleAccountRoles.Admin, StringComparison.Ordinal)
                && string.Equals(account.Status, ConsoleAccountStatuses.Active, StringComparison.Ordinal)
                && (!string.Equals(role, ConsoleAccountRoles.Admin, StringComparison.Ordinal)
                    || !string.Equals(status, ConsoleAccountStatuses.Active, StringComparison.Ordinal));
            if (removesActiveAdmin)
            {
                await EnsureAnotherActiveAdminAsync(dbContext, account.Id, cancellationToken);
            }

            bool revokeSessions = !string.Equals(account.Role, role, StringComparison.Ordinal)
                || !string.Equals(account.Status, status, StringComparison.Ordinal);
            account.DisplayName = displayName;
            account.Role = role;
            account.Status = status;
            account.UpdatedAt = now;
            account.Version++;
            if (revokeSessions)
            {
                await RevokeSessionsAsync(dbContext, account.Id, "ACCOUNT_CHANGED", now, cancellationToken);
            }

            return new Mutation<ConsoleAccountView>(
                ToView(account, now),
                "ADMIN_ACCOUNT_UPDATE",
                account.Username,
                request.Reason,
                before,
                StateJson(account));
        },
        cancellationToken);

    public Task<ConsoleAccountView> ResetPasswordAsync(
        Guid accountId,
        ResetConsolePasswordRequest request,
        string actor,
        string correlationId,
        string idempotencyKey,
        CancellationToken cancellationToken) => ExecuteMutationAsync(
        $"console-account:reset-password:{accountId:D}",
        idempotencyKey,
        request,
        actor,
        correlationId,
        async (dbContext, now) =>
        {
            ValidateReason(request.Reason);
            ConsoleAccountEntity account = await RequireAccountAsync(
                dbContext,
                accountId,
                cancellationToken);
            EnsureVersion(account, request.Version);
            if (!ConsolePasswordPolicy.IsValid(request.NewPassword, account.Username))
            {
                throw IvrErrors.AccountPolicyViolation(
                    "The password must be 12-128 characters with upper, lower, digit and symbol, and must not contain the username.");
            }

            string before = StateJson(account);
            account.PasswordHash = ConsolePasswordHasher.Hash(request.NewPassword);
            account.PasswordChangedAt = now;
            account.FailedLoginCount = 0;
            account.LockedUntil = null;
            account.UpdatedAt = now;
            account.Version++;
            await RevokeSessionsAsync(dbContext, account.Id, "PASSWORD_RESET", now, cancellationToken);
            return new Mutation<ConsoleAccountView>(
                ToView(account, now),
                "ADMIN_ACCOUNT_PASSWORD_RESET",
                account.Username,
                request.Reason,
                before,
                StateJson(account));
        },
        cancellationToken);

    public Task<ConsoleAccountView> DeleteAsync(
        Guid accountId,
        DeleteConsoleAccountRequest request,
        string actor,
        string correlationId,
        string idempotencyKey,
        CancellationToken cancellationToken) => ExecuteMutationAsync(
        $"console-account:delete:{accountId:D}",
        idempotencyKey,
        request,
        actor,
        correlationId,
        async (dbContext, now) =>
        {
            ValidateReason(request.Reason);
            ConsoleAccountEntity account = await RequireAccountAsync(
                dbContext,
                accountId,
                cancellationToken);
            EnsureVersion(account, request.Version);
            if (account.IsBuiltin)
            {
                throw IvrErrors.AccountPolicyViolation("The built-in admin account cannot be deleted.");
            }

            if (string.Equals(account.Role, ConsoleAccountRoles.Admin, StringComparison.Ordinal)
                && string.Equals(account.Status, ConsoleAccountStatuses.Active, StringComparison.Ordinal))
            {
                await EnsureAnotherActiveAdminAsync(dbContext, account.Id, cancellationToken);
            }

            string before = StateJson(account);
            account.Status = ConsoleAccountStatuses.Deleted;
            account.DeletedAt = now;
            account.UpdatedAt = now;
            account.Version++;
            await RevokeSessionsAsync(dbContext, account.Id, "ACCOUNT_DELETED", now, cancellationToken);
            return new Mutation<ConsoleAccountView>(
                ToView(account, now),
                "ADMIN_ACCOUNT_DELETE",
                account.Username,
                request.Reason,
                before,
                StateJson(account));
        },
        cancellationToken);

    private async Task<T> ExecuteMutationAsync<TRequest, T>(
        string scope,
        string idempotencyKey,
        TRequest request,
        string actor,
        string correlationId,
        Func<IvrDbContext, DateTimeOffset, Task<Mutation<T>>> mutate,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(idempotencyKey);
        ArgumentException.ThrowIfNullOrWhiteSpace(actor);
        string payloadHash = Convert.ToHexString(
            SHA256.HashData(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(request, JsonOptions))))
            .ToLowerInvariant();
        string lockKey = $"{scope}:{idempotencyKey}";
        PiiGuard.EnsureSafeText(idempotencyKey);

        await using IvrDbContext dbContext = await dbContextFactory.CreateDbContextAsync(
            cancellationToken);
        await using var transaction = await dbContext.Database.BeginTransactionAsync(
            IsolationLevel.Serializable,
            cancellationToken);
        await dbContext.Database.ExecuteSqlInterpolatedAsync(
            $"SELECT pg_advisory_xact_lock(hashtextextended({lockKey}, 0))",
            cancellationToken);
        IdempotencyKeyEntity? existing = await dbContext.IdempotencyKeys.FindAsync(
            [scope, idempotencyKey],
            cancellationToken);
        if (existing is not null)
        {
            if (!string.Equals(existing.PayloadHash, payloadHash, StringComparison.Ordinal))
            {
                throw IvrErrors.IdempotencyConflict();
            }

            T restored = JsonSerializer.Deserialize<T>(existing.ResponseSnapshotJson, JsonOptions)
                ?? throw new InvalidOperationException("Stored account response is invalid.");
            await transaction.CommitAsync(cancellationToken);
            return restored;
        }

        DateTimeOffset now = timeProvider.GetUtcNow();
        Mutation<T> mutation = await mutate(dbContext, now);
        string snapshot = JsonSerializer.Serialize(mutation.Response, JsonOptions);

        // Contact-only: this blob carries the account view, and therefore a staff display name
        // that the full guard would reject for containing an ordinary surname. Every free-text
        // input reaching it was already validated at its own boundary — the display name by
        // ConsoleDisplayNamePolicy and the reason by PiiGuard.EnsureSafeText — so what is left
        // to catch here is a contact number smuggled through a field that should not hold one.
        PiiGuard.EnsureSafeContactText(snapshot);
        AppendAudit(
            dbContext,
            actor,
            mutation.Action,
            mutation.TargetId,
            mutation.Reason,
            correlationId,
            mutation.BeforeStateJson,
            mutation.AfterStateJson,
            now);
        dbContext.IdempotencyKeys.Add(new IdempotencyKeyEntity
        {
            Scope = scope,
            Key = idempotencyKey,
            PayloadHash = payloadHash,
            ResponseSnapshotJson = snapshot,
            CreatedAt = now,
            ExpiresAt = now.AddHours(24),
            RetainUntil = now.AddHours(24),
            RetentionClass = RetentionDataClasses.IdempotencyKey,
        });

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (
            exception.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            throw IvrErrors.AccountConflict();
        }

        return mutation.Response;
    }

    private static async Task<ConsoleAccountEntity> RequireAccountAsync(
        IvrDbContext dbContext,
        Guid accountId,
        CancellationToken cancellationToken) =>
        await dbContext.ConsoleAccounts.SingleOrDefaultAsync(
            item => item.Id == accountId,
            cancellationToken)
        ?? throw IvrErrors.NotFound("The account was not found.");

    private static async Task EnsureAnotherActiveAdminAsync(
        IvrDbContext dbContext,
        Guid accountId,
        CancellationToken cancellationToken)
    {
        bool another = await dbContext.ConsoleAccounts.AnyAsync(
            item => item.Id != accountId
                && item.Role == ConsoleAccountRoles.Admin
                && item.Status == ConsoleAccountStatuses.Active
                && item.DeletedAt == null,
            cancellationToken);
        if (!another)
        {
            throw IvrErrors.AccountPolicyViolation("At least one active admin account is required.");
        }
    }

    private static Task<int> RevokeSessionsAsync(
        IvrDbContext dbContext,
        Guid accountId,
        string reason,
        DateTimeOffset now,
        CancellationToken cancellationToken) => dbContext.ConsoleSessions
        .Where(item => item.AccountId == accountId && item.RevokedAt == null)
        .ExecuteUpdateAsync(
            setters => setters
                .SetProperty(item => item.RevokedAt, now)
                .SetProperty(item => item.RevokeReason, reason),
            cancellationToken);

    private static void EnsureVersion(ConsoleAccountEntity account, long version)
    {
        if (account.Version != version)
        {
            throw IvrErrors.AccountConflict();
        }
    }

    private static void ValidateAccountInput(
        string username,
        string displayName,
        string role,
        string password)
    {
        if (!ConsoleUsernamePolicy.IsValid(username))
        {
            throw IvrErrors.AccountPolicyViolation(
                "The username must be 3-64 lowercase ASCII characters and start with a letter.");
        }

        _ = ValidateDisplayName(displayName);
        if (!ConsoleAccountRoles.IsDefined(role))
        {
            throw IvrErrors.AccountPolicyViolation("The role must be Admin or Operator.");
        }

        if (!ConsolePasswordPolicy.IsValid(password, username))
        {
            throw IvrErrors.AccountPolicyViolation(
                "The password must be 12-128 characters with upper, lower, digit and symbol, and must not contain the username.");
        }
    }

    /// <summary>
    /// Uses <see cref="ConsoleDisplayNamePolicy"/> rather than <see cref="PiiGuard.EnsureSafeText"/>.
    /// The customer-PII guard rejected ordinary unaccented Vietnamese surnames — Duong, Ngo — and
    /// did it by throwing <see cref="InvalidOperationException"/>, which the error middleware
    /// turned into a 500. A name the policy refuses is now a 422 that says why.
    /// </summary>
    private static string ValidateDisplayName(string displayName)
    {
        string normalized = ConsoleDisplayNamePolicy.Normalize(displayName);
        if (!ConsoleDisplayNamePolicy.IsValid(normalized))
        {
            throw IvrErrors.AccountPolicyViolation(
                "The display name must be 1-128 characters, without control characters or a "
                + "contact number.");
        }

        return normalized;
    }

    private static void ValidateReason(string reason)
    {
        if (string.IsNullOrWhiteSpace(reason) || reason.Length > 512)
        {
            throw IvrErrors.AccountPolicyViolation("An administrative reason is required.");
        }

        PiiGuard.EnsureSafeText(reason);
    }

    private static ConsoleAccountView ToView(ConsoleAccountEntity account, DateTimeOffset now) =>
        new(
            account.Id,
            account.Username,
            account.DisplayName,
            account.Role,
            account.Status,
            account.IsBuiltin,
            account.LockedUntil > now,
            account.LockedUntil,
            account.LastLoginAt,
            account.PasswordChangedAt,
            account.CreatedAt,
            account.UpdatedAt,
            account.DeletedAt,
            account.Version);

    private static string StateJson(ConsoleAccountEntity account) => JsonSerializer.Serialize(
        new
        {
            account.Username,
            account.DisplayName,
            account.Role,
            account.Status,
            account.IsBuiltin,
            account.FailedLoginCount,
            account.LockedUntil,
            account.LastLoginAt,
            account.PasswordChangedAt,
            account.DeletedAt,
            account.Version,
        },
        JsonOptions);

    private static void AppendAudit(
        IvrDbContext dbContext,
        string actor,
        string action,
        string targetId,
        string? reason,
        string correlationId,
        string? beforeStateJson,
        string? afterStateJson,
        DateTimeOffset now)
    {
        dbContext.AuditLog.Add(new AuditLogEntity
        {
            AuditId = Guid.NewGuid(),
            ActorId = actor,
            ActorType = "staff",
            Action = action,
            TargetType = "console_account",
            TargetId = targetId,
            Reason = reason,
            BeforeStateJson = beforeStateJson,
            AfterStateJson = afterStateJson,
            CorrelationId = correlationId,
            DataJson = "{}",
            CreatedAt = now,
            RetentionClass = "audit_log",
        });
    }

    private static string CreateRawToken()
    {
        byte[] bytes = RandomNumberGenerator.GetBytes(32);
        return "ivr_session_" + Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    public static string HashToken(string rawToken) => Convert.ToHexString(
        SHA256.HashData(Encoding.UTF8.GetBytes(rawToken))).ToLowerInvariant();

    private sealed record Mutation<T>(
        T Response,
        string Action,
        string TargetId,
        string Reason,
        string? BeforeStateJson,
        string? AfterStateJson);
}
