using Ivr.Api.Application;
using Ivr.Api.Admin;
using Ivr.Api.Foundation;
using Ivr.Api.Health;
using Ivr.Api.Intake;
using Ivr.Api.Internal;
using Ivr.Api.Middleware;
using Ivr.Infrastructure.Configuration;
using Ivr.Infrastructure.FeatureFlags;
using Ivr.Infrastructure.Observability;

System.Diagnostics.Activity.DefaultIdFormat = System.Diagnostics.ActivityIdFormat.W3C;
System.Diagnostics.Activity.ForceDefaultIdFormat = true;
var builder = WebApplication.CreateBuilder(args);

// C5 in docs/review/2026-09-09-codebase-audit.md. Seven services in this app are registered as
// singletons and are safe today only because they take IDbContextFactory rather than a scoped
// IvrDbContext -- and nothing enforced that. The next person to inject the context directly gets
// a captured dependency: one context shared by every concurrent request, failing as a race under
// load rather than as a compile error.
//
// The framework already ships the guard; it was simply off where it matters. CreateBuilder turns
// these on only when the environment is Development, and the container sets
// ASPNETCORE_ENVIRONMENT=Production, so neither ran anywhere real. ValidateOnBuild walks the whole
// graph at startup and refuses to boot on a captured dependency, which turns that race into a
// failure at deploy time, for every service, including ones not written yet.
builder.Host.UseDefaultServiceProvider(options =>
{
    options.ValidateScopes = true;
    options.ValidateOnBuild = true;
});

// W-0203. A named profile, layered on top of appsettings.{Environment}.json and then covered
// again by the environment variables and the command line, so a harness can still override one
// key without editing the file. Re-adding those two sources rather than computing an insert
// index: the effect is the same, later wins, and there is no index arithmetic to get wrong.
if (IvrConfigurationProfile.ResolveConfiguredFileName(
        builder.Configuration,
        builder.Environment.EnvironmentName) is { } apiProfileFile)
{
    builder.Configuration.AddJsonFile(apiProfileFile, optional: false, reloadOnChange: false);
    builder.Configuration.AddEnvironmentVariables();
    if (args.Length > 0)
    {
        builder.Configuration.AddCommandLine(args);
    }
}

builder.Services.AddIvrObservability(
    builder.Configuration,
    builder.Environment,
    "ginsengfood-ivr-api",
    instrumentAspNetCore: true);
builder.Services.AddIvrFoundation(builder.Configuration);
builder.Services.AddIvrEligibility(builder.Configuration);
builder.Services.AddIvrFeatureFlags(builder.Configuration);
builder.Services.AddIvrApiFoundation(builder.Configuration);
builder.Services.AddIvrInternalAdminApi(builder.Configuration);

var app = builder.Build();

app.UseRouting();
app.UseIvrApiFoundation();
app.MapIvrHealthEndpoints();
app.MapIvrFeatureFlagEndpoints();
app.MapIvrTaskIntakeEndpoint();
app.MapIvrInternalLifecycleEndpoints();
app.MapIvrAdminEndpoints();

// W-0112. Maps nothing outside a non-production deployment, so production has no such route
// to refuse — see DevToolingEndpoints.
app.MapIvrDevToolingEndpoints();

app.Run();

public partial class Program;
