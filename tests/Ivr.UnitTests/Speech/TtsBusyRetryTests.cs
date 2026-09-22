using System.Diagnostics;
using System.Net;
using Ivr.Domain.Scripts;
using Ivr.Domain.Speech;
using Ivr.Infrastructure.Speech;
using Microsoft.Extensions.Options;

namespace Ivr.UnitTests.Speech;

public sealed class TtsBusyRetryTests : IDisposable
{
    private readonly string media = Path.Combine(Path.GetTempPath(), "ivr-busy-" + Guid.NewGuid().ToString("N"));

    [Fact]
    [Trait("TestId", "UT-TTS-BUSY-01")]
    public async Task BusyResponsesRecoverWithIdenticalRequestAndOneMediaFile()
    {
        var bodies = new List<string>();
        var sent = new List<long>();
        var clock = Stopwatch.StartNew();
        using var handler = new Handler(async (request, token) =>
        {
            bodies.Add(await request.Content!.ReadAsStringAsync(token));
            sent.Add(clock.ElapsedMilliseconds);
            return new HttpResponseMessage(bodies.Count < 3 ? HttpStatusCode.ServiceUnavailable : HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(bodies.Count < 3 ? [] : new byte[16000]),
            };
        });
        RenderedAudio audio = await Provider(handler).SynthesizeAsync(Script(),
            TtsOptions.Create(timeout: TimeSpan.FromSeconds(5)), CancellationToken.None);
        Assert.Equal(3, bodies.Count);
        Assert.All(bodies, b => Assert.Equal(bodies[0], b));
        Assert.True(sent[1] - sent[0] >= 200);
        Assert.True(sent[2] - sent[1] >= 400);
        Assert.Equal(TimeSpan.FromSeconds(1), audio.Duration);
        Assert.Single(Directory.GetFiles(media));
    }

    [Fact]
    [Trait("TestId", "UT-TTS-BUSY-02")]
    public async Task RetryAndResponseBodyShareOneDeadline()
    {
        int calls = 0;
        using var handler = new Handler((_, _) =>
        {
            if (Interlocked.Increment(ref calls) == 1)
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.ServiceUnavailable));
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StreamContent(new PendingBody()),
            });
        });
        var clock = Stopwatch.StartNew();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => Provider(handler).SynthesizeAsync(
            Script(), TtsOptions.Create(timeout: TimeSpan.FromMilliseconds(600)), CancellationToken.None));
        Assert.Equal(2, calls);
        Assert.InRange(clock.ElapsedMilliseconds, 450, 2500);
        Assert.False(Directory.Exists(media));
    }

    [Fact]
    [Trait("TestId", "UT-TTS-BUSY-03")]
    public async Task CallerCancellationStopsBackoffWithoutAnotherRequest()
    {
        int calls = 0;
        using var cancelled = new CancellationTokenSource();
        using var handler = new Handler((_, _) =>
        {
            Interlocked.Increment(ref calls);
            cancelled.Cancel();
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.ServiceUnavailable));
        });
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => Provider(handler).SynthesizeAsync(
            Script(), TtsOptions.Create(), cancelled.Token));
        Assert.Equal(1, calls);
        Assert.False(Directory.Exists(media));
    }

    [Theory]
    [InlineData(HttpStatusCode.BadRequest)]
    [InlineData(HttpStatusCode.TooManyRequests)]
    [InlineData(HttpStatusCode.InternalServerError)]
    [Trait("TestId", "UT-TTS-BUSY-04")]
    public async Task OtherErrorsFailOnceWithoutReadingProviderContent(HttpStatusCode status)
    {
        int calls = 0;
        using var handler = new Handler((_, _) =>
        {
            calls++;
            return Task.FromResult(new HttpResponseMessage(status) { Content = new StringContent("do-not-echo") });
        });
        var error = await Assert.ThrowsAsync<TtsSynthesisException>(() => Provider(handler).SynthesizeAsync(
            Script(), TtsOptions.Create(), CancellationToken.None));
        Assert.Equal("TTS_PROVIDER_HTTP_ERROR", error.TechnicalErrorCode);
        Assert.DoesNotContain("do-not-echo", error.Message);
        Assert.Equal(1, calls);
        Assert.False(Directory.Exists(media));
    }

    [Fact]
    [Trait("TestId", "UT-TTS-BUSY-05")]
    public async Task PersistentBusyStopsAtTheSharedDeadline()
    {
        int calls = 0;
        using var handler = new Handler((_, _) =>
        {
            calls++;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.ServiceUnavailable));
        });
        var clock = Stopwatch.StartNew();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => Provider(handler).SynthesizeAsync(
            Script(), TtsOptions.Create(timeout: TimeSpan.FromMilliseconds(600)), CancellationToken.None));
        Assert.InRange(calls, 1, 2);
        Assert.InRange(clock.ElapsedMilliseconds, 450, 2500);
        Assert.False(Directory.Exists(media));
    }

    [Fact]
    [Trait("TestId", "UT-TTS-BUSY-06")]
    public async Task BusyLongerThanNineSecondsCanRecoverWithinTheOriginalDeadline()
    {
        int calls = 0;
        using var handler = new Handler((_, _) => Task.FromResult(
            new HttpResponseMessage(++calls <= 11 ? HttpStatusCode.ServiceUnavailable : HttpStatusCode.OK)
            { Content = new ByteArrayContent(calls <= 11 ? [] : new byte[16000]) }));
        RenderedAudio audio = await Provider(handler).SynthesizeAsync(Script(),
            TtsOptions.Create(timeout: TimeSpan.FromSeconds(15)), CancellationToken.None);
        Assert.Equal(12, calls);
        Assert.Equal(TimeSpan.FromSeconds(1), audio.Duration);
        Assert.Single(Directory.GetFiles(media));
    }

    private ConfigurableExternalTtsProvider Provider(Handler handler) => new(new Factory(handler),
        Options.Create(new TtsProviderOptions
        {
            Provider = TtsProviderOptions.ExternalProvider,
            External = new ExternalTtsOptions
            {
                Endpoint = "http://127.0.0.1:8090/synthesize",
                RequestBodyTemplate = "{\"text\":\"{{text}}\"}",
                MediaOutputDirectory = media,
            },
        }));

    private static SpeechScript Script() => SpeechScript.Create(TargetV1SpeechPolicy.MockTemplateId,
        TargetV1SpeechPolicy.MockTemplateVersion, "Nội dung kiểm thử.", "test-content", "test-summary");

    public void Dispose()
    {
        if (Directory.Exists(media)) Directory.Delete(media, recursive: true);
    }

    private sealed class Factory(HttpMessageHandler handler) : IHttpClientFactory
    {
        public HttpClient CreateClient(string name) => new(handler, disposeHandler: false);
    }

    private sealed class Handler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> send)
        : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => send(request, cancellationToken);
    }

    private sealed class PendingBody : Stream
    {
        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => throw new NotSupportedException();
        public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
        public override void Flush() => throw new NotSupportedException();
        public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
        public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
            return 0;
        }
    }
}
