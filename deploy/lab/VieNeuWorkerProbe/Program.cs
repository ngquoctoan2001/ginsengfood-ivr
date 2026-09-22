using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Ivr.Domain.Confirmation;
using Ivr.Domain.Ports;
using Ivr.Domain.Scripts;
using Ivr.Domain.Speech;
using Ivr.Infrastructure.Speech;
using Microsoft.Extensions.Options;

// No scheduler, DB, dial token, SIP or customer endpoint is constructed by this program.
if (Environment.GetEnvironmentVariable("REAL_CUSTOMER_CALL_ALLOWED") != "NO") throw new InvalidOperationException("NO required");
string scope = args[0];
int soakSeconds = int.Parse(args[1], System.Globalization.CultureInfo.InvariantCulture);
if (scope is not ("LOCAL_LAB" or "S5_TARGET") || soakSeconds is < 30 or > 1800) throw new InvalidOperationException("Invalid probe bounds");
const string endpoint = "http://127.0.0.1:8090/synthesize";
using JsonDocument fixtures = JsonDocument.Parse(File.ReadAllText("/base/worker-texts.json"));
using JsonDocument orders = JsonDocument.Parse(File.ReadAllText("/kit/orders.json"));
using JsonDocument accepted = JsonDocument.Parse(File.ReadAllText("/base/voice-acceptance-manifest.json"));
using JsonDocument approved = JsonDocument.Parse(File.ReadAllText("/base/approved-audio.json"));
using JsonDocument expected = JsonDocument.Parse(File.ReadAllText("/kit/expected.json"));
if (!fixtures.RootElement.GetProperty("fixtureOnly").GetBoolean() || fixtures.RootElement.GetProperty("realCustomerCallAllowed").GetString() != "NO") throw new InvalidOperationException("Synthetic fixtures required");
var cases = fixtures.RootElement.GetProperty("cases").EnumerateArray().Where(c => c.GetProperty("variant").GetString() is "MultiItem" or "LongName").ToArray();
if (cases.Length != 6) throw new InvalidOperationException("Six fixtures required");
var definition = ScriptDraftDefinition.Create("SCRIPT-ORDER-CONFIRM", "v3-test-approved", TargetV1SpeechPolicy.CanonicalVietnameseTemplate);
var script = new ApprovedScript(new ScriptVersionSnapshot(definition.Key, ScriptLifecycleStatus.Approved,
    definition.TemplateText, definition.TemplateHash, TargetV1SpeechPolicy.AllowedInputFields, [],
    "LOCAL_MEASUREMENT", "Synthetic fixture", DateTimeOffset.UtcNow, null, null, null, null, null, null), ExecutionMode.LabRealSim);
var renderer = new VietnameseOrderScriptRenderer();
var timer = Stopwatch.StartNew();
var rows = new List<object>();
var parts = new ConcurrentQueue<Part>();
var http = new ConcurrentQueue<object>();
var current = new AsyncLocal<string>();
using var transport = new HttpClientHandler();
using var handler = new CountingHandler(transport, current, http);
var factory = new Factory(handler);
using var direct = new HttpClient { Timeout = TimeSpan.FromSeconds(60) };
for (int i = 0; ; i++)
{
    try { using var ready = await direct.GetAsync("http://127.0.0.1:8090/health/ready"); if (ready.IsSuccessStatusCode) break; }
    catch (HttpRequestException) { }
    if (i >= 120) throw new InvalidOperationException("Readiness timeout");
    await Task.Delay(500);
}
string Voice(string region) => accepted.RootElement.GetProperty("selections").GetProperty(region).GetProperty("voice_id").GetString()!;
FixedSegmentMediaEntry[] Catalog(string region) => JsonSerializer.Deserialize<FixedSegmentMediaEntry[]>(expected.RootElement.GetProperty("catalogs").GetProperty(region).GetRawText())!;
SpeechSynthesisService Service(int segmentTimeout, bool cache)
{
    var configured = Options.Create(new TtsProviderOptions
    {
        Provider = TtsProviderOptions.ExternalProvider, ExecutionMode = "LAB_REAL_SIM", TimeoutMilliseconds = segmentTimeout,
        PreparationQueueLimit = 8, PreparationQueueTimeoutMilliseconds = 90000,
        MaxRequestsPerMinute = 10000, MaxCharactersPerMinute = 12000000,
        Segmentation = new SpeechSegmentationOptions { Enabled = true, FixedSegments = FixedSegmentSource.Catalog },
        RegionalVoices = new RegionalVoiceOptions { Enabled = true,
            North = new RegionalVoiceEntry { VoiceId = Voice("North"), FixedSegments = Catalog("North") },
            Central = new RegionalVoiceEntry { VoiceId = Voice("Central"), FixedSegments = Catalog("Central") },
            South = new RegionalVoiceEntry { VoiceId = Voice("South"), FixedSegments = Catalog("South") } },
        External = new ExternalTtsOptions { Endpoint = endpoint, MediaOutputDirectory = "/tmp/media",
            RequestBodyTemplate = "{\"text\":\"{{text}}\",\"voice_id\":\"{{voice_id}}\",\"locale\":\"{{locale}}\",\"speaking_rate\":{{speaking_rate}},\"output_format\":\"{{output_format}}\",\"sample_rate\":{{sample_rate}}}" },
    });
    var provider = new ObservedProvider(new ConfigurableExternalTtsProvider(factory, configured), current, parts, timer);
    return new SpeechSynthesisService(provider, cache ? new AudioCache(TimeProvider.System) : new NoCache(),
        new TtsRequestBudget(TimeProvider.System), new TtsUsageMeter(), new RegionalVoiceMap(configured), configured, TimeProvider.System);
}
void Save() => File.WriteAllText("/out/worker.json", JsonSerializer.Serialize(new
{
    scope, REAL_CUSTOMER_CALL_ALLOWED = "NO", profile = "DIAGNOSTIC_NOT_PRODUCTION_CONFIG",
    per_segment_comparison_ms = 10000, per_segment_soak_ms = 15000, queue_ms = 90000,
    soak_seconds_requested = soakSeconds, elapsed_ms = timer.ElapsedMilliseconds,
    jobs = rows, parts = parts.ToArray(), http = http.ToArray(),
}, new JsonSerializerOptions { WriteIndented = true }));
async Task Job(SpeechSynthesisService service, JsonElement c, string phase, int id)
{
    string region = c.GetProperty("region").GetString()!, variant = c.GetProperty("variant").GetString()!;
    string job = $"{phase}-{id}-{region}-{variant}";
    current.Value = job;
    long start = timer.ElapsedMilliseconds;
    string? error = null;
    bool pcmEqual = false;
    int segmentCount = 0;
    try
    {
        string area = region switch { "North" => "Phường Cửa Nam, thành phố Hà Nội", "Central" => "Phường Hải Châu, thành phố Đà Nẵng", _ => "Phường Phú Khương, tỉnh Vĩnh Long" };
        JsonElement order = orders.RootElement.GetProperty(variant);
        var summary = PrivacySafeOrderSummary.Create("Khách thử nghiệm", "LAB",
            order.GetProperty("items").EnumerateArray().Select(x => SpeechItem.Create(x.GetProperty("public_name").GetString()!, x.GetProperty("quantity").GetDecimal(), x.GetProperty("unit_label").GetString())),
            Money.Vnd(order.GetProperty("total_amount").GetInt64()), ShortDeliveryArea.Create(area), "Giờ Vàng", null, SpeechSummaryLimits.Create(20, 20));
        ScriptPreview preview = renderer.Render(script, summary);
        if (preview.ContentHash != c.GetProperty("ContentHash").GetString() || preview.ExactText != c.GetProperty("ExactText").GetString()) throw new InvalidOperationException("Renderer fixture drift");
        var rendered = new RenderedSpeech(preview.ScriptReference, preview.ExactText, preview.ContentHash, summary.Locale,
            preview.EstimatedDuration, 0, "TEXT_ONLY", null, preview.Segments);
        RenderedSpeech result = await service.SynthesizeAsync(rendered, summary, definition.Key.TemplateId,
            definition.Key.Version, ExecutionMode.LabRealSim, DateTimeOffset.UtcNow.AddMinutes(3), CancellationToken.None);
        segmentCount = result.Audio!.Segments.Length;
        if (segmentCount != 7) throw new InvalidOperationException("Incomplete playlist");
        using var assembled = new MemoryStream();
        foreach (var (segment, index) in result.Audio.Segments.Select((s, i) => (s, i)))
        {
            bool fixedPart = preview.Segments[index].Kind == SpeechSegmentKind.Fixed;
            string path = fixedPart ? "/kit/fixed/" + segment.ContentRef[6..] + ".sln" : "/tmp/media/" + segment.ContentRef[14..] + ".sln";
            byte[] pcm = await File.ReadAllBytesAsync(path);
            if (!fixedPart && Convert.ToHexStringLower(SHA256.HashData(pcm)) != approved.RootElement.GetProperty($"{region}:{variant}:{index + 1}").GetString()) throw new InvalidOperationException("Dynamic PCM drift");
            assembled.Write(pcm);
        }
        pcmEqual = Convert.ToHexStringLower(SHA256.HashData(assembled.ToArray())) == expected.RootElement.GetProperty("assembled").GetProperty($"{region}:{variant}").GetString();
        if (!pcmEqual) throw new InvalidOperationException("Assembled PCM drift");
    }
    catch (TtsSynthesisException ex) { error = ex.TechnicalErrorCode; }
    catch (Exception ex) { error = ex.GetType().Name; }
    Part[] own = parts.Where(p => p.Job == job).ToArray();
    lock (rows) rows.Add(new { job, phase, region, variant, elapsed_ms = timer.ElapsedMilliseconds - start,
        wait_to_first_provider_ms = own.Length == 0 ? (long?)null : own[0].StartedMs - start,
        provider_calls = own.Length, playlist_segments = segmentCount, pcm_equal = pcmEqual, error });
    Console.WriteLine($"JOB_DONE {job} elapsed_ms={timer.ElapsedMilliseconds-start} result={error ?? "PCM_MATCH"}");
}
var comparison = Service(10000, true);
foreach (var c in cases) { await Job(comparison, c, "cold-cache-10s", rows.Count); Save(); }
foreach (var c in cases) { await Job(comparison, c, "warm-cache-10s", rows.Count); Save(); }
var uncached = Service(15000, false);
foreach (int count in new[] { 2, 4 })
{
    await Task.WhenAll(Enumerable.Range(0, count).Select(i => Job(uncached, cases[i], $"burst-{count}-15s", i)));
    Save();
}
// A disconnected real inference occupies the sidecar while the actual C# client retries.
JsonElement south = cases.First(c => c.GetProperty("region").GetString() == "South");
string text = south.GetProperty("segments")[1].GetProperty("Text").GetString()!;
using (var cancel = new CancellationTokenSource(100))
using (var body = new StringContent(JsonSerializer.Serialize(new { text, voice_id = Voice("South"), locale = "vi-VN", speaking_rate = 1.0, output_format = "audio/L16", sample_rate = 8000 }), Encoding.UTF8, "application/json"))
{
    try { using var response = await direct.PostAsync(endpoint, body, cancel.Token); }
    catch (OperationCanceledException) { }
}
await Job(Service(30000, false), south, "disconnect-recovery-30s", 0);
Save();
var soak = Stopwatch.StartNew();
int batch = 0;
while (soak.Elapsed.TotalSeconds < soakSeconds)
{
    await Task.WhenAll(Enumerable.Range(0, 2).Select(i => Job(uncached, cases[(batch * 2 + i) % cases.Length], "soak-15s", batch * 2 + i)));
    batch++;
    Save();
}
File.WriteAllText("/out/completion.json", JsonSerializer.Serialize(new { completed = true, soak_elapsed_ms = soak.ElapsedMilliseconds, soak_batches = batch, REAL_CUSTOMER_CALL_ALLOWED = "NO" }));
Save();
Console.WriteLine("WORKER_PROBE_COMPLETE_NO_PRODUCTION_APPROVAL");

internal sealed record Part(string Job, long StartedMs, long ElapsedMs, string? Error);
internal sealed class Factory(HttpMessageHandler handler) : IHttpClientFactory
{ public HttpClient CreateClient(string name) => new(handler, false) { Timeout = Timeout.InfiniteTimeSpan }; }
internal sealed class CountingHandler(HttpMessageHandler inner, AsyncLocal<string> job, ConcurrentQueue<object> rows) : DelegatingHandler(inner)
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken token)
    {
        HttpResponseMessage response = await base.SendAsync(request, token);
        rows.Enqueue(new { job = job.Value, status = (int)response.StatusCode });
        return response;
    }
}
internal sealed class ObservedProvider(ITtsProvider inner, AsyncLocal<string> job, ConcurrentQueue<Part> rows, Stopwatch timer) : ITtsProvider
{
    public async Task<RenderedAudio> SynthesizeAsync(SpeechScript script, TtsOptions options, CancellationToken token)
    {
        long started = timer.ElapsedMilliseconds;
        string? error = null;
        try { return await inner.SynthesizeAsync(script, options, token); }
        catch (Exception ex) { error = ex is TtsSynthesisException tts ? tts.TechnicalErrorCode : ex.GetType().Name; throw; }
        finally { rows.Enqueue(new Part(job.Value!, started, timer.ElapsedMilliseconds - started, error)); }
    }
}
internal sealed class NoCache : IAudioCache
{
    public int Count => 0;
    public async Task<AudioCacheResult> GetOrCreateAsync(AudioCacheKey key, DateTimeOffset expiresAt, Func<CancellationToken, Task<RenderedAudio>> factory, CancellationToken token)
        => new(await factory(token), false, expiresAt);
    public Task<int> PurgeExpiredAsync(DateTimeOffset now, bool dryRun, CancellationToken token) => Task.FromResult(0);
}
