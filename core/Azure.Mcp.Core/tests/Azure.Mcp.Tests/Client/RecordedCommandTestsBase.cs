// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.ClientModel;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Azure.Mcp.Tests.Client.Helpers;
using Azure.Mcp.Tests.Helpers;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;
using Xunit;

namespace Azure.Mcp.Tests.Client;

public abstract class RecordedCommandTestsBase(ITestOutputHelper output, TestProxyFixture fixture) : CommandTestsBase(output), IClassFixture<TestProxyFixture>
{
    protected TestProxy? Proxy { get; private set; } = fixture.Proxy;

    // used to resolve a recording "path" given an invoking test
    private static readonly RecordingPathResolver _pathResolver = new();

    protected virtual bool IsAsync => false;

    // todo: use this when we have versioned tests to run this against.
    protected virtual string? VersionQualifier => null;

    public override async ValueTask InitializeAsync()
    {
        await base.InitializeAsyncInternal(fixture);
        await StartRecordOrPlayback();
    }

    public override async ValueTask DisposeAsync()
    {
        await StopRecordOrPlayback();
        await base.DisposeAsync();
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        // On test failure, append proxy stderr for diagnostics.
        if (TestContext.Current?.TestState?.Result == TestResult.Failed && Proxy is not null)
        {
            var stderr = Proxy.SnapshotStdErr();
            if (!string.IsNullOrWhiteSpace(stderr))
            {
                Output.WriteLine("=== Test Proxy stderr (captured) ===");
                Output.WriteLine(stderr);
                Output.WriteLine("=== End Test Proxy stderr ===");
            }
        }
    }

    private async Task StartRecordOrPlayback()
    {
        if (Proxy is null)
        {
            throw new InvalidOperationException("Test proxy is not initialized.");
        }

        var testName = TryGetCurrentTestName();
        var pathToRecording = GetSessionFilePath(testName);
        var assetsPath = _pathResolver.GetAssetsJson(GetType());

        var recordOptions = new Dictionary<string, string>
        {
            { "x-recording-file", pathToRecording },
        };

        if (!string.IsNullOrWhiteSpace(assetsPath))
        {
            recordOptions["x-recording-assets-file"] = assetsPath;
        }
        var bodyContent = BinaryContentHelper.FromObject(recordOptions);

        if (TestingMode is TestMode.Playback)
        {
            Output.WriteLine($"[Playback] Session file: {pathToRecording}");
            try
            {
                await Proxy.Client.StartPlaybackAsync(bodyContent).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                if (Proxy is not null)
                {
                    Output.WriteLine(Proxy.SnapshotStdErr() ?? $"Proxy is null while attempting to snapshot stderr. Facing exception during start playback.{e.ToString()}");
                }
                else
                {
                    Output.WriteLine($"Proxy is null while attempting to snapshot stderr. Facing exception during start playback.{e.ToString()}");
                }
                throw;
            }
        }
        else if (TestingMode is TestMode.Record)
        {
            Output.WriteLine($"[Record] Session file: {pathToRecording}");
            try
            {
                Proxy.Client.StartRecord(bodyContent);
            }
            catch (Exception e)
            {
                if (Proxy is not null)
                {
                    Output.WriteLine(Proxy.SnapshotStdErr() ?? $"Proxy is null while attempting to snapshot stderr. Facing exception during start record.{e.ToString()}");
                }
                else
                {
                    Output.WriteLine($"Proxy is null while attempting to snapshot stderr. Facing exception during start recording.{e.ToString()}");
                }
                throw;
            }
        }

        await Task.CompletedTask;
    }

    private async Task StopRecordOrPlayback()
    {
        if (Proxy is null)
        {
            throw new InvalidOperationException("Test proxy is not initialized.");
        }

        if (TestingMode is TestMode.Playback)
        {
            await Proxy.Client.StopPlaybackAsync("placeholder-ignore").ConfigureAwait(false);
        }
        else if (TestingMode is TestMode.Record)
        {
            Proxy.Client.StopRecord("placeholder-ignore", new Dictionary<string, string>());
        }
        await Task.CompletedTask;
    }

    private static string TryGetCurrentTestName()
    {
        var name = TestContext.Current?.Test?.TestCase.TestCaseDisplayName;
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new InvalidOperationException("Test name is not available. Recording requires a valid test name.");
        }
        return name;
    }

    private string GetSessionFilePath(string displayName)
    {
        var sanitized = RecordingPathResolver.Sanitize(displayName);
        var dir = _pathResolver.GetSessionDirectory(GetType(), variantSuffix: null);
        var fileName = RecordingPathResolver.BuildFileName(sanitized, IsAsync, VersionQualifier);
        var fullPath = Path.Combine(dir, fileName).Replace('\\', '/');
        return fullPath;
    }
}
