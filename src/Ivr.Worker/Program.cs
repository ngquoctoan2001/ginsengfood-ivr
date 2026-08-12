using Ivr.Worker;
using Ivr.Infrastructure.Configuration;
using Ivr.Infrastructure.FeatureFlags;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddIvrFoundation(builder.Configuration);
builder.Services.AddIvrFeatureFlags(builder.Configuration);
builder.Services.AddHostedService<IvrHeartbeat>();

var host = builder.Build();
host.Run();
