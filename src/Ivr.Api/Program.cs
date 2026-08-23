using Ivr.Api.Application;
using Ivr.Api.Admin;
using Ivr.Api.Foundation;
using Ivr.Api.Health;
using Ivr.Api.Intake;
using Ivr.Api.Internal;
using Ivr.Api.Middleware;
using Ivr.Infrastructure.Configuration;
using Ivr.Infrastructure.FeatureFlags;

var builder = WebApplication.CreateBuilder(args);

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
