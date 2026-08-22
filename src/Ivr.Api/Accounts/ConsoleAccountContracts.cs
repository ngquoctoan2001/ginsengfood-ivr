using System.Text.Json.Serialization;

namespace Ivr.Api.Accounts;

public sealed record ConsoleSignInRequest(
    [property: JsonPropertyName("username")] string Username,
    [property: JsonPropertyName("password")] string Password);

public sealed record ConsoleAccountView(
    [property: JsonPropertyName("account_id")] Guid AccountId,
    [property: JsonPropertyName("username")] string Username,
    [property: JsonPropertyName("display_name")] string DisplayName,
    [property: JsonPropertyName("role")] string Role,
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("is_builtin")] bool IsBuiltin,
    [property: JsonPropertyName("is_locked")] bool IsLocked,
    [property: JsonPropertyName("locked_until")] DateTimeOffset? LockedUntil,
    [property: JsonPropertyName("last_login_at")] DateTimeOffset? LastLoginAt,
    [property: JsonPropertyName("password_changed_at")] DateTimeOffset PasswordChangedAt,
    [property: JsonPropertyName("created_at")] DateTimeOffset CreatedAt,
    [property: JsonPropertyName("updated_at")] DateTimeOffset UpdatedAt,
    [property: JsonPropertyName("deleted_at")] DateTimeOffset? DeletedAt,
    [property: JsonPropertyName("version")] long Version);

public sealed record ConsoleSessionView(
    [property: JsonPropertyName("account")] ConsoleAccountView Account,
    [property: JsonPropertyName("permissions")] IReadOnlyList<string> Permissions,
    [property: JsonPropertyName("expires_at")] DateTimeOffset ExpiresAt);

public sealed record ConsoleSignInApiResult(
    [property: JsonPropertyName("access_token")] string AccessToken,
    [property: JsonPropertyName("token_type")] string TokenType,
    [property: JsonPropertyName("session")] ConsoleSessionView Session);

public sealed record ConsoleAccountPageApiResult(
    [property: JsonPropertyName("page")] int Page,
    [property: JsonPropertyName("page_size")] int PageSize,
    [property: JsonPropertyName("total_count")] int TotalCount,
    [property: JsonPropertyName("items")] IReadOnlyList<ConsoleAccountView> Items);

public sealed record CreateConsoleAccountRequest(
    [property: JsonPropertyName("username")] string Username,
    [property: JsonPropertyName("display_name")] string DisplayName,
    [property: JsonPropertyName("role")] string Role,
    [property: JsonPropertyName("password")] string Password,
    [property: JsonPropertyName("reason")] string Reason);

public sealed record UpdateConsoleAccountRequest(
    [property: JsonPropertyName("display_name")] string? DisplayName,
    [property: JsonPropertyName("role")] string? Role,
    [property: JsonPropertyName("status")] string? Status,
    [property: JsonPropertyName("version")] long Version,
    [property: JsonPropertyName("reason")] string Reason);

public sealed record ResetConsolePasswordRequest(
    [property: JsonPropertyName("new_password")] string NewPassword,
    [property: JsonPropertyName("version")] long Version,
    [property: JsonPropertyName("reason")] string Reason);

public sealed record DeleteConsoleAccountRequest(
    [property: JsonPropertyName("version")] long Version,
    [property: JsonPropertyName("reason")] string Reason);

public sealed record ConsoleRoleView(
    [property: JsonPropertyName("role")] string Role,
    [property: JsonPropertyName("label")] string Label,
    [property: JsonPropertyName("permissions")] IReadOnlyList<string> Permissions);

public sealed record ConsoleRoleMatrixApiResult(
    [property: JsonPropertyName("roles")] IReadOnlyList<ConsoleRoleView> Roles);

public sealed record ConsoleSignOutApiResult(
    [property: JsonPropertyName("revoked")] bool Revoked);

public sealed record AuthenticatedConsoleSession(
    Guid SessionId,
    Guid AccountId,
    string Username,
    string DisplayName,
    string Role,
    IReadOnlyList<string> Permissions,
    DateTimeOffset ExpiresAt);
