using Ivr.Worker;
using Ivr.Infrastructure.Callbacks;
using Ivr.Infrastructure.Configuration;
using Ivr.Infrastructure.FeatureFlags;
using Ivr.Infrastructure.Retention;
using Ivr.Worker.Jobs;
using Ivr.Worker.Normalization;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddIvrFoundation(builder.Configuration);
builder.Services.AddIvrFeatureFlags(builder.Configuration);
builder.Services.AddIvrRetention(builder.Configuration);
builder.Services.AddIvrCallbackDelivery(builder.Configuration);
builder.Services.AddHostedService<MockSimChannelProvisioner>();
builder.Services.AddHostedService<IvrHeartbeat>();
builder.Services.AddHostedService<RetentionJobHost>();
builder.Services.AddHostedService<SchedulerJobHost>();
builder.Services.AddSingleton<ResultNormalizer>();
builder.Services.AddHostedService<NormalizationJobHost>();
builder.Services.AddHostedService<CallbackDeliveryJobHost>();

var host = builder.Build();
host.Run();
