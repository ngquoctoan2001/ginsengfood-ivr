using System.Globalization;
using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace Ivr.Worker;

public sealed class WorkerHealthOptions
{
    public const string SectionName = "Ivr:Worker:Health";

    public bool Enabled { get; set; } = true;

    public int Port { get; set; } = 8081;

    public string Path { get; set; } = "/healthz";
}

/// <summary>
/// The smallest thing that lets Kubernetes read <see cref="WorkerLiveness"/> (<c>W-0043</c> §2).
/// <para>
/// A raw <see cref="HttpListener"/> rather than ASP.NET Core, and a separate port rather than one
/// shared with anything: this exists so a probe can ask one question, and pulling a web framework
/// into a worker to answer it would add routing, model binding and middleware that nothing here
/// needs and everything here would then have to be secured.
/// </para>
/// <para>
/// It answers 503 rather than refusing the connection when a loop has stopped, because a refused
/// connection and a stopped process look the same to a probe, and the difference is exactly what
/// the body is for.
/// </para>
/// <para>
/// The body names loops, their last tick and the TYPE of their last fault. Never a fault message:
/// a message can carry a connection string, a row value or a half-masked phone number, and
/// anything that can reach this port can read it.
/// </para>
/// </summary>
public sealed partial class WorkerHealthEndpoint(
    WorkerLiveness liveness,
    IOptions<WorkerHealthOptions> options,
    ILogger<WorkerHealthEndpoint> logger) : BackgroundService
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        WorkerHealthOptions settings = options.Value;
        if (!settings.Enabled)
        {
            // An operator opting out — a port already taken, or an environment that will not have
            // a listening socket on this workload. NOT the run-once retention pod: that path never
            // registers this service at all (Program.cs), so a CronJob cannot reach here to be
            // switched off. Saying otherwise would document a safety this flag does not provide.
            LogDisabled(logger);
            return;
        }

        using var listener = new HttpListener();
        listener.Prefixes.Add(
            string.Create(
                CultureInfo.InvariantCulture,
                $"http://+:{settings.Port}{settings.Path}/"));
        try
        {
            listener.Start();
        }
        catch (HttpListenerException exception)
        {
            // Fail loudly and keep the worker running. A probe that cannot bind is a monitoring
            // outage; taking the worker down over it would turn a monitoring outage into a real one.
            LogBindFailed(logger, settings.Port, exception);
            return;
        }

        LogListening(logger, settings.Port, settings.Path);
        using CancellationTokenRegistration stop = stoppingToken.Register(listener.Stop);
        while (!stoppingToken.IsCancellationRequested)
        {
            HttpListenerContext context;
            try
            {
                context = await listener.GetContextAsync().ConfigureAwait(false);
            }
            catch (Exception exception) when (
                exception is HttpListenerException or ObjectDisposedException
                or InvalidOperationException)
            {
                break;
            }

            try
            {
                await RespondAsync(context).ConfigureAwait(false);
            }
            catch (Exception exception) when (exception is HttpListenerException or IOException)
            {
                // The probe hung up. Not worth a log line every time a kubelet times out.
            }
        }
    }

    private async Task RespondAsync(HttpListenerContext context)
    {
        WorkerLivenessReport report = liveness.Read();
        byte[] body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(
            new
            {
                status = report.Status.ToString().ToLowerInvariant(),
                loops = report.Loops.Select(loop => new
                {
                    name = loop.Loop,
                    enabled = loop.Enabled,
                    last_tick_at = loop.LastTickAt,
                    stale = loop.Stale,
                    consecutive_faults = loop.ConsecutiveFaults,
                    last_fault_kind = loop.LastFaultKind,
                }),
            },
            Json));

        context.Response.StatusCode = report.Live
            ? (int)HttpStatusCode.OK
            : (int)HttpStatusCode.ServiceUnavailable;
        context.Response.ContentType = "application/json";
        context.Response.ContentLength64 = body.Length;
        await context.Response.OutputStream.WriteAsync(body).ConfigureAwait(false);
        context.Response.Close();
    }

    [LoggerMessage(
        EventId = 1010,
        Level = LogLevel.Information,
        Message = "Worker health endpoint listening on port {Port} at {Path}.")]
    private static partial void LogListening(ILogger logger, int port, string path);

    [LoggerMessage(
        EventId = 1011,
        Level = LogLevel.Information,
        Message = "Worker health endpoint is disabled; nothing can probe this worker.")]
    private static partial void LogDisabled(ILogger logger);

    [LoggerMessage(
        EventId = 1012,
        Level = LogLevel.Error,
        Message = "Worker health endpoint could not bind port {Port}; the worker keeps running unprobed.")]
    private static partial void LogBindFailed(ILogger logger, int port, Exception exception);
}
