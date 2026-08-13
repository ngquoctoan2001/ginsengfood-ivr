using Ivr.Worker;
using Ivr.Infrastructure.Configuration;
using Ivr.Infrastructure.FeatureFlags;
using Ivr.Infrastructure.Retention;
using Ivr.Worker.Jobs;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddIvrFoundation(builder.Configuration);
builder.Services.AddIvrFeatureFlags(builder.Configuration);
builder.Services.AddIvrRetention(builder.Configuration);
builder.Services.AddHostedService<IvrHeartbeat>();
builder.Services.AddHostedService<RetentionJobHost>();

var host = builder.Build();
host.Run();
