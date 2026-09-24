using System.Diagnostics;
using System.Globalization;

namespace Ivr.IntegrationTests;

/// <summary>
/// W-0353 / P5-2 §8 <c>CT-OAS-01</c> and <c>CT-OAS-02</c>: the OpenAPI documents parse, and a
/// reference that points nowhere is refused.
/// <para>
/// Both checks already exist as the P0-2 scripts <c>validate-openapi.mjs</c> and
/// <c>selftest-openapi.mjs</c>, and the scripts stay the one implementation. These tests run them
/// and read what they print, so the two IDs are backed by a result in the candidate's test run
/// rather than by a line in a pipeline log.
/// </para>
/// <para>
/// They live in this project because only the job that runs it installs node and
/// <c>deploy/ci/node_modules</c>. <c>globalization_invariant_gate</c> runs the whole unit project
/// and <c>contract_suite</c> the whole contract project, both without node. As with
/// <c>IT-API-MATRIX-38</c>, a missing node fails the test; it never skips.
/// </para>
/// </summary>
public sealed class OpenApiDocumentTests
{
    /// <summary>
    /// The two documents this service owns: the contract Module 3 calls, and the callback contract
    /// IVR calls back on. Named rather than listed from the folder, so a document that is deleted or
    /// renamed fails <c>CT-OAS-01</c> instead of shrinking what "every document" means.
    /// </summary>
    private static readonly string[] Documents =
    [
        "ivr-order-confirmation.v1.yaml",
        "order-core-ivr-callback.target-v1.yaml",
    ];

    [Fact]
    [Trait("TestId", "CT-OAS-01")]
    public async Task EveryOpenApiDocumentParsesAndValidates()
    {
        NodeRun run = await RunNodeAsync("deploy/ci/scripts/validate-openapi.mjs");

        Assert.True(run.ExitCode == 0, run.Diagnostics);
        Assert.Contains(
            string.Concat(
                "OPENAPI_FILES_VALID=",
                Documents.Length.ToString(CultureInfo.InvariantCulture)),
            run.Lines);
        foreach (string document in Documents)
        {
            Assert.Contains(string.Concat("OPENAPI_PARSE_PASS=", document), run.Lines);
        }
    }

    /// <summary>
    /// <c>selftest-openapi.mjs</c> passes when the parser rejects its fixture for any reason at all.
    /// So the fixture is read here too: it must still carry the one reference that cannot resolve,
    /// with nothing defined for it to resolve to. Otherwise a fixture broken some other way, by a
    /// syntax error or a missing field, would keep the self-test green while nothing tested
    /// references.
    /// </summary>
    [Fact]
    [Trait("TestId", "CT-OAS-02")]
    public async Task ADocumentWithADanglingReferenceIsRefused()
    {
        string[] fixture =
        [
            .. File.ReadLines(Path.Combine(
                    FindRepository(),
                    "deploy",
                    "ci",
                    "fixtures",
                    "openapi",
                    "invalid-openapi.yaml"))
                .Select(line => line.Trim()),
        ];

        Assert.Contains(
            fixture,
            line => line.StartsWith("$ref:", StringComparison.Ordinal)
                && line.Contains("#/components/schemas/DoesNotExist", StringComparison.Ordinal));
        Assert.DoesNotContain(
            fixture,
            line => line.StartsWith("DoesNotExist:", StringComparison.Ordinal));

        NodeRun run = await RunNodeAsync("deploy/ci/scripts/selftest-openapi.mjs");

        Assert.True(run.ExitCode == 0, run.Diagnostics);
        Assert.Contains(
            run.Lines,
            line => line.StartsWith("CT-CI-01 PASS", StringComparison.Ordinal));
    }

    private static async Task<NodeRun> RunNodeAsync(string script)
    {
        ProcessStartInfo start = new("node")
        {
            WorkingDirectory = FindRepository(),
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };
        start.ArgumentList.Add(script);
        using Process process = Process.Start(start)!;
        Task<string> output = process.StandardOutput.ReadToEndAsync();
        Task<string> error = process.StandardError.ReadToEndAsync();
        using CancellationTokenSource timeout = new(TimeSpan.FromMinutes(2));
        try
        {
            await process.WaitForExitAsync(timeout.Token);
        }
        catch (OperationCanceledException)
        {
            process.Kill(entireProcessTree: true);
            throw;
        }

        return new NodeRun(process.ExitCode, await output, await error);
    }

    private static string FindRepository()
    {
        DirectoryInfo? directory = new(AppContext.BaseDirectory);
        while (directory is not null
            && !File.Exists(Path.Combine(
                directory.FullName,
                "specs",
                "api",
                "openapi",
                "ivr-order-confirmation.v1.yaml")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName
            ?? throw new DirectoryNotFoundException("IVR checkout not found.");
    }

    private sealed record NodeRun(int ExitCode, string StandardOutput, string StandardError)
    {
        public IReadOnlyList<string> Lines { get; } =
        [
            .. StandardOutput.Split('\n').Select(line => line.TrimEnd('\r')),
        ];

        public string Diagnostics => string.Concat(StandardOutput, StandardError);
    }
}
