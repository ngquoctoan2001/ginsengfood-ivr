using System.Collections;
using System.Globalization;
using System.Diagnostics;
using Ivr.Domain.Privacy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Ivr.Infrastructure.Observability;

public sealed class IvrObservabilityOptions
{
    public const string SectionName = "Ivr:Observability";

    public bool Enabled { get; set; }

    public bool ExportTraces { get; set; } = true;

    public bool ExportMetrics { get; set; } = true;

    public bool ExportLogs { get; set; } = true;

    public double? TraceSamplingRatio { get; set; }

    public string? DeploymentEnvironmentName { get; set; }
}

public static class IvrObservabilityServiceCollectionExtensions
{
    public static IServiceCollection AddIvrObservability(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment,
        string serviceName,
        bool instrumentAspNetCore)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(environment);
        ArgumentException.ThrowIfNullOrWhiteSpace(serviceName);

        IvrObservabilityOptions settings = configuration
            .GetSection(IvrObservabilityOptions.SectionName)
            .Get<IvrObservabilityOptions>() ?? new IvrObservabilityOptions();
        settings.TraceSamplingRatio ??= environment.IsProduction() ? 0.10D : 1.00D;
        settings.DeploymentEnvironmentName = string.IsNullOrWhiteSpace(
            settings.DeploymentEnvironmentName)
                ? environment.EnvironmentName
                : settings.DeploymentEnvironmentName;
        if (settings.TraceSamplingRatio is < 0D or > 1D)
        {
            throw new InvalidOperationException(
                "Ivr:Observability:TraceSamplingRatio must be between 0 and 1.");
        }

        if (!PiiGuard.IsSafeText(settings.DeploymentEnvironmentName))
        {
            throw new InvalidOperationException(
                "Ivr:Observability:DeploymentEnvironmentName failed the privacy guard.");
        }

        services.AddSingleton(settings);
        if (!settings.Enabled)
        {
            return services;
        }

        ValidateOtlpConfiguration(configuration);
        string serviceVersion = typeof(IvrTelemetry).Assembly.GetName().Version?.ToString(3)
            ?? IvrTelemetry.Version;
        ResourceBuilder resource = ResourceBuilder.CreateDefault()
            .AddService(serviceName, serviceVersion: serviceVersion)
            .AddAttributes(new Dictionary<string, object>
            {
                ["deployment.environment.name"] = settings.DeploymentEnvironmentName,
            });

        OpenTelemetryBuilder telemetry = services.AddOpenTelemetry()
            .ConfigureResource(builder => builder
                .AddService(serviceName, serviceVersion: serviceVersion)
                .AddAttributes(new Dictionary<string, object>
                {
                    ["deployment.environment.name"] = settings.DeploymentEnvironmentName,
                }));

        if (settings.ExportTraces)
        {
            telemetry.WithTracing(builder =>
            {
                builder
                    .SetSampler(new ParentBasedSampler(
                        new TraceIdRatioBasedSampler(settings.TraceSamplingRatio.Value)))
                    .AddSource(IvrTelemetry.ServiceName)
                    .AddHttpClientInstrumentation(options =>
                    {
                        // The default exception event contains exception.message. B-06 exports
                        // error status without putting customer/provider text into a span.
                        options.RecordException = false;
                    });
                if (instrumentAspNetCore)
                {
                    builder.AddAspNetCoreInstrumentation(options =>
                    {
                        options.RecordException = false;
                    });
                }

                builder.AddOtlpExporter();
            });
        }

        if (settings.ExportMetrics)
        {
            telemetry.WithMetrics(builder => builder
                .AddMeter(IvrTelemetry.ServiceName)
                .AddOtlpExporter());
        }

        if (settings.ExportLogs)
        {
            services.AddLogging(builder => builder.AddOpenTelemetry(options =>
            {
                options.SetResourceBuilder(resource);
                options.IncludeFormattedMessage = false;
                options.IncludeScopes = false;
                options.ParseStateValues = true;
                options.AddProcessor(new PiiSafeLogRecordProcessor());
                options.AddOtlpExporter();
            }));
        }

        return services;
    }

    private static void ValidateOtlpConfiguration(IConfiguration configuration)
    {
        string? endpoint = configuration["OTEL_EXPORTER_OTLP_ENDPOINT"];
        if (string.IsNullOrWhiteSpace(endpoint)
            || !Uri.TryCreate(endpoint, UriKind.Absolute, out Uri? uri)
            || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            throw new InvalidOperationException(
                "Observability is enabled but OTEL_EXPORTER_OTLP_ENDPOINT is not an absolute HTTP(S) URI.");
        }

        string protocol = configuration["OTEL_EXPORTER_OTLP_PROTOCOL"] ?? "grpc";
        if (!string.Equals(protocol, "grpc", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(protocol, "http/protobuf", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "OTEL_EXPORTER_OTLP_PROTOCOL must be 'grpc' or 'http/protobuf'.");
        }
    }
}

/// <summary>
/// Last fail-closed boundary before an ILogger record reaches OTLP. Unknown fields are removed;
/// unsafe values are replaced without echoing them; exception messages are never exported.
/// </summary>
public sealed class PiiSafeLogRecordProcessor : BaseProcessor<LogRecord>
{
    public const string Redacted = "[REDACTED]";

    private const int StackTraceMaxLength = 8192;

    private static readonly HashSet<string> AllowedAttributeNames = new(
        [
            "{OriginalFormat}",
            "OriginalFormat",
            "CorrelationId",
            "TaskId",
            "JobId",
            "AttemptId",
            "CallbackId",
            "ReasonCode",
            "Outcome",
            "Status",
            "WorkerId",
            "Provider",
            "EventName",
            "EventId",
            TelemetryTags.CorrelationId,
            TelemetryTags.TaskId,
            TelemetryTags.JobId,
            TelemetryTags.AttemptId,
            TelemetryTags.CallbackId,
            TelemetryTags.ReasonCode,
            TelemetryTags.Outcome,
            "exception.type",
            "exception.stacktrace",
        ],
        StringComparer.OrdinalIgnoreCase);

    public override void OnEnd(LogRecord data)
    {
        ArgumentNullException.ThrowIfNull(data);
        data.Body = SafeString(data.Body);
        data.FormattedMessage = null;

        var attributes = new List<KeyValuePair<string, object?>>();
        if (data.Attributes is not null)
        {
            foreach (KeyValuePair<string, object?> attribute in data.Attributes)
            {
                if (!AllowedAttributeNames.Contains(attribute.Key))
                {
                    continue;
                }

                attributes.Add(new KeyValuePair<string, object?>(
                    attribute.Key,
                    SafeValue(attribute.Value)));
            }
        }

        object? activityCorrelation = Activity.Current?.GetTagItem(TelemetryTags.CorrelationId);
        if (activityCorrelation is not null
            && !attributes.Any(attribute => string.Equals(
                attribute.Key,
                TelemetryTags.CorrelationId,
                StringComparison.OrdinalIgnoreCase)))
        {
            attributes.Add(new KeyValuePair<string, object?>(
                TelemetryTags.CorrelationId,
                SafeValue(activityCorrelation)));
        }

        if (data.Exception is Exception exception)
        {
            attributes.Add(new KeyValuePair<string, object?>(
                "exception.type",
                SafeString(exception.GetType().FullName)));
            if (!string.IsNullOrWhiteSpace(exception.StackTrace))
            {
                string stack = exception.StackTrace.Length <= StackTraceMaxLength
                    ? exception.StackTrace
                    : exception.StackTrace[..StackTraceMaxLength];
                attributes.Add(new KeyValuePair<string, object?>(
                    "exception.stacktrace",
                    SafeString(stack)));
            }
        }

        data.Exception = null;
        data.Attributes = attributes;
    }

    private static object? SafeValue(object? value) => value switch
    {
        null => null,
        string text => SafeString(text),
        bool or byte or sbyte or short or ushort or int or uint or long or ulong
            or float or double or decimal => value,
        Guid guid => guid.ToString("D", CultureInfo.InvariantCulture),
        DateTime valueDate => valueDate.ToString("O", CultureInfo.InvariantCulture),
        DateTimeOffset valueDateOffset => valueDateOffset.ToString("O", CultureInfo.InvariantCulture),
        IEnumerable => Redacted,
        _ => SafeString(Convert.ToString(value, CultureInfo.InvariantCulture)),
    };

    private static string SafeString(string? value) =>
        string.IsNullOrEmpty(value) || PiiGuard.IsSafeText(value)
            ? value ?? string.Empty
            : Redacted;
}
