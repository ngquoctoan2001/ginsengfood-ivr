using Microsoft.AspNetCore.Authorization;

namespace Ivr.Api.Auth;

public sealed record PermissionRequirement(string Permission) : IAuthorizationRequirement;
