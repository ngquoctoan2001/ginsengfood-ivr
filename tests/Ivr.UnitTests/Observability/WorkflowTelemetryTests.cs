using System.Diagnostics;
using Ivr.Infrastructure.Observability;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Logs;

namespace Ivr.UnitTests.Observability;

public sealed class WorkflowTelemetryTests
{
    [Fact]
    [Trait("TestId", "UT-OBS-CONTEXT-06")]
    public void PersistedW3cContextKeepsEveryWorkflowStageOnTheIntakeTrace()
    {
        using var listener = ListenToIvr();
        using Activity? intake = IvrTelemetry.StartSpan(
            "ivr.intake",
            (TelemetryTags.TaskId, "TASK-OBS-06"));
        Assert.NotNull(intake);
        TraceContextSnapshot? snapshot = TraceContextSnapshot.Capture(intake);
        Assert.NotNull(snapshot);
        ActivityTraceId expectedTrace = intake!.TraceId;
        intake.Dispose();

        string[] stages =
        [
            "ivr.eligibility.evaluate",
            "ivr.scheduler.dispatch",
            "ivr.result.normalize",
            "ivr.callback.deliver",
        ];
        foreach (string stage in stages)
        {
            using Activity? activity = IvrTelemetry.StartWorkflowSpan(
                stage,
                ActivityKind.Consumer,
                TraceContextSnapshot.FromPersisted(snapshot!.TraceParent, snapshot.TraceState),
                linkCurrent: false,
                (TelemetryTags.TaskId, "TASK-OBS-06"));
            Assert.NotNull(activity);
            Assert.Equal(expectedTrace, activity!.TraceId);
            Assert.Equal(false, activity.GetTagItem(TelemetryTags.TraceContextMissing));
        }
    }

    [Fact]
    [Trait("TestId", "UT-OBS-CONTEXT-07")]
    public void MissingOrInvalidContextNeverBlocksAWorkflowStage()
    {
        using var listener = ListenToIvr();
        Assert.Null(TraceContextSnapshot.FromPersisted("not-a-traceparent", null));

        using Activity? activity = IvrTelemetry.StartWorkflowSpan(
            "ivr.result.normalize",
            ActivityKind.Consumer,
            parent: null,
            linkCurrent: false,
            (TelemetryTags.TaskId, "TASK-LEGACY-07"));
        Assert.NotNull(activity);
        Assert.Equal(true, activity!.GetTagItem(TelemetryTags.TraceContextMissing));
    }

    [Fact]
    [Trait("TestId", "UT-OBS-CARDINALITY-08")]
    public void WorkflowIdentifiersAreTraceOnlyAndCannotBecomeMetricDimensions()
    {
        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() =>
            IvrTelemetry.RecordResult((TelemetryTags.TaskId, "TASK-OBS-08")));
        Assert.Contains("time series", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    [Trait("TestId", "UT-OBS-CONFIG-09")]
    public void SamplingDefaultsAreEnvironmentSpecificAndInvalidEndpointFailsFast()
    {
        var productionServices = new ServiceCollection();
        productionServices.AddIvrObservability(
            new ConfigurationBuilder().Build(),
            new TestHostEnvironment(Environments.Production),
            "ginsengfood-ivr-test",
            instrumentAspNetCore: false);
        Assert.Equal(
            0.10D,
            Assert.IsType<IvrObservabilityOptions>(productionServices.Single(
                descriptor => descriptor.ServiceType == typeof(IvrObservabilityOptions))
                .ImplementationInstance).TraceSamplingRatio);

        IConfiguration invalid = new ConfigurationBuilder().AddInMemoryCollection(
            new Dictionary<string, string?>
            {
                ["Ivr:Observability:Enabled"] = "true",
                ["OTEL_EXPORTER_OTLP_ENDPOINT"] = "not-an-absolute-uri",
            }).Build();
        Assert.Throws<InvalidOperationException>(() => new ServiceCollection()
            .AddIvrObservability(
                invalid,
                new TestHostEnvironment(Environments.Development),
                "ginsengfood-ivr-test",
                instrumentAspNetCore: false));
    }

    [Fact]
    [Trait("TestId", "UT-OBS-LOG-10")]
    public void OtlpLogBoundaryDropsUnknownFieldsRedactsPiiAndOmitsExceptionMessage()
    {
        using ActivityListener listener = ListenToIvr();
        using Activity activity = Assert.IsType<Activity>(IvrTelemetry.StartSpan(
            "ivr.test.log",
            (TelemetryTags.CorrelationId, "corr-log-safe")));
        Exception exception;
        try
        {
            throw new InvalidOperationException("customer 0912345678");
        }
        catch (Exception caught)
        {
            exception = caught;
        }

        using var capture = new CapturingLogRecordProcessor();
        using ServiceProvider provider = new ServiceCollection()
            .AddLogging(builder => builder.AddOpenTelemetry(options =>
            {
                options.IncludeFormattedMessage = false;
                options.ParseStateValues = true;
                options.AddProcessor(new PiiSafeLogRecordProcessor());
                options.AddProcessor(capture);
            }))
            .BuildServiceProvider();
        ILogger<WorkflowTelemetryTests> logger = provider
            .GetRequiredService<ILogger<WorkflowTelemetryTests>>();
        Action<ILogger, string, string, Exception?> write = LoggerMessage.Define<string, string>(
            LogLevel.Error,
            new EventId(10, "OtlpPiiBoundary"),
            "called 0912345678 {TaskId} {CustomerPhone}");
        write(logger, "0912345678", "0912345678", exception);

        CapturedLogRecord record = Assert.IsType<CapturedLogRecord>(capture.Record);
        Assert.Equal(PiiSafeLogRecordProcessor.Redacted, record.Body);
        Assert.Null(record.Exception);
        Assert.DoesNotContain("CustomerPhone", record.Attributes.Select(item => item.Key));
        Assert.Contains(record.Attributes, item =>
            item.Key == "TaskId"
            && Equals(item.Value, PiiSafeLogRecordProcessor.Redacted));
        Assert.Contains(record.Attributes, item =>
            item.Key == "exception.type"
            && Equals(item.Value, typeof(InvalidOperationException).FullName));
        Assert.Contains(record.Attributes, item =>
            item.Key == TelemetryTags.CorrelationId
            && Equals(item.Value, "corr-log-safe"));
        Assert.Equal(activity.TraceId, record.TraceId);
        Assert.DoesNotContain(record.Attributes, item =>
            item.Value?.ToString()?.Contains("0912345678", StringComparison.Ordinal) == true);
        Assert.DoesNotContain(record.Attributes, item =>
            item.Value?.ToString()?.Contains("customer", StringComparison.OrdinalIgnoreCase) == true);
    }

    private sealed record CapturedLogRecord(
        string? Body,
        Exception? Exception,
        ActivityTraceId TraceId,
        KeyValuePair<string, object?>[] Attributes);

    private sealed class CapturingLogRecordProcessor : BaseProcessor<LogRecord>
    {
        public CapturedLogRecord? Record { get; private set; }

        public override void OnEnd(LogRecord data)
        {
            Record = new CapturedLogRecord(
                data.Body,
                data.Exception,
                data.TraceId,
                data.Attributes?.ToArray() ?? []);
        }
    }

    private static ActivityListener ListenToIvr()
    {
        var listener = new ActivityListener
        {
            ShouldListenTo = source => source.Name == IvrTelemetry.ServiceName,
            Sample = (ref ActivityCreationOptions<ActivityContext> _) =>
                ActivitySamplingResult.AllDataAndRecorded,
        };
        ActivitySource.AddActivityListener(listener);
        return listener;
    }

    private sealed class TestHostEnvironment(string environmentName) : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = environmentName;

        public string ApplicationName { get; set; } = "Ivr.UnitTests";

        public string ContentRootPath { get; set; } = AppContext.BaseDirectory;

        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
