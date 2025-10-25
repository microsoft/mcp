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

    // Recording path support (lightweight) ----------------------------------
    private static readonly RecordingPathResolver _pathResolver = new();

    // TODO: grab asyncncess of the test. Given that it is good practice to separate sync and async tests, this may
    // not be necessary?
    protected virtual bool IsAsync => false;

    // TODO: do I need to worry about service version? Adding a versionQualifier here just in case. Feedback on PR will clean it out possibly.
    protected virtual string? VersionQualifier => null;

    public override async ValueTask InitializeAsync()
    {
        await base.InitializeAsyncInternal(Proxy);

        await StartRecordOrPlayback();
    }

    public new async ValueTask DisposeAsync()
    {
        await StopRecordOrPlayback();

        await base.DisposeAsync();
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
        // todo: replace after regenerating using Azure.Core instead of System.ClientModel
        var bodyContent = BinaryContentHelper.FromObject(recordOptions);

        if (TestingMode is TestMode.Playback)
        {
            Output.WriteLine($"[Playback] Session file: {pathToRecording}");
            await Proxy.Client.StartPlaybackAsync(bodyContent).ConfigureAwait(false);
        }
        else if (TestingMode is TestMode.Record)
        {
            Output.WriteLine($"[Record] Session file: {pathToRecording}");
            Proxy.Client.StartRecord(bodyContent);
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
            // TODO: feed variables / metadata to proxy stop.
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
