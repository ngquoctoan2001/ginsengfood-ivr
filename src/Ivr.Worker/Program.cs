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

// W-0047 / P7-5. One pass, then exit. A CronJob pod that never terminates is recorded as failed,
// which is what happened when W-0044 first scheduled one: RetentionJobHost completed its pass and
// returned, but the scheduler, normalisation and callback hosts kept the process alive.
//
// The other hosted services are not registered at all in this mode rather than merely stopped
// afterwards. A retention pod that briefly ran the scheduler could dispatch a call, and a job whose
// name says "retention" must not be able to place one.
bool runOnce = builder.Configuration.GetValue<bool>($"{RetentionOptions.SectionName}:RunOnce");
if (runOnce)
{
    builder.Services.AddHostedService<RetentionRunOnceHost>();
}
else
{
    builder.Services.AddIvrCallbackDelivery(builder.Configuration);
    builder.Services.AddHostedService<MockSimChannelProvisioner>();
    builder.Services.AddHostedService<IvrHeartbeat>();
    builder.Services.AddHostedService<RetentionJobHost>();
    builder.Services.AddHostedService<SchedulerJobHost>();
    builder.Services.AddSingleton<ResultNormalizer>();
    builder.Services.AddHostedService<NormalizationJobHost>();
    builder.Services.AddHostedService<CallbackDeliveryJobHost>();
}

var host = builder.Build();
host.Run();
