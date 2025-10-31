// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.ClientModel;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using Azure.Mcp.Tests.Client.Helpers;
using Azure.Mcp.Tests.Generated.Models;
using Azure.Mcp.Tests.Helpers;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;
using Xunit;

namespace Azure.Mcp.Tests.Client;

public abstract class RecordedCommandTestsBase(ITestOutputHelper output, TestProxyFixture fixture) : CommandTestsBase(output), IClassFixture<TestProxyFixture>
{
    protected TestProxy? Proxy { get; private set; } = fixture.Proxy;

    /// <summary>
    /// Sanitizers that will apply generally across all parts (URI, Body, HeaderValues) of the request/response. This sanitization is applied to to recorded data at rest and during recording, and against test requests during playback.
    /// </summary>
    public virtual List<GeneralRegexSanitizer> GeneralRegexSanitizers { get; } = new();

    /// <summary>
    /// Sanitizers that will apply a regex to specific headers. This sanitization is applied to to recorded data at rest and during recording, and against test requests during playback.
    /// </summary>
    public virtual List<HeaderRegexSanitizer> HeaderRegexSanitizers { get; } = new()
    {
        // Sanitize the WWW-Authenticate header which may contain tenant IDs or resource URLs to "Sanitized"
        // During conversion to recordings, the actual tenant ID is captured in group 1 and replaced with a fixed GUID.
        // REMOVAL of this formatting cause complete failure on tool side when it expects a valid URL with a GUID tenant ID.
        // Hence the more complex replacement rather than a simple static string replace of the entire header value with `Sanitized`
        new HeaderRegexSanitizer(new HeaderRegexSanitizerBody("WWW-Authenticate")
        {
            Regex = "https://login.microsoftonline.com/(.*?)\"",
            GroupForReplace = "1",
            Value = "00000000-0000-0000-0000-000000000000"
        })
    };

    /// <summary>
    /// Sanitizers that apply a regex replacement to URIs. This sanitization is applied to to recorded data at rest and during recording, and against test requests during playback.
    /// </summary>
    public virtual List<UriRegexSanitizer> UriRegexSanitizers { get; } = new();

    /// <summary>
    /// Sanitizers that will apply a regex replacement to a specific json body key. This sanitization is applied to to recorded data at rest and during recording, and against test requests during playback.
    /// </summary>
    public virtual List<BodyKeySanitizer> BodyKeySanitizers { get; } = new();

    /// <summary>
    /// Sanitizers that will apply regex replacement to the body of requests/responses. This sanitization is applied to to recorded data at rest and during recording, and against test requests during playback.
    /// </summary>
    public virtual List<BodyRegexSanitizer> BodyRegexSanitizers { get; } = new();

    /// <summary>
    /// The test-proxy has a default set of ~90 sanitizers for common sensitive data (GUIDs, tokens, timestamps, etc). This list allows opting out of specific default sanitizers by name.
    /// Grab the names from the test-proxy source at https://github.com/Azure/azure-sdk-tools/blob/main/tools/test-proxy/Azure.Sdk.Tools.TestProxy/Common/SanitizerDictionary.cs#L65)
    /// </summary>
    public virtual List<string> DisabledDefaultSanitizers { get; } = new();

    // used to resolve a recording "path" given an invoking test
    private static readonly RecordingPathResolver _pathResolver = new();

    protected virtual bool IsAsync => false;

    // todo: use this when we have versioned tests to run this against.
    protected virtual string? VersionQualifier => null;

    public override async ValueTask InitializeAsync()
    {
        // load settings first to determine test mode
        await base.LoadSettingsAsync();

        if (fixture.Proxy == null)
        {
            // start the proxy if needed
            await StartProxyAsync(fixture);
        }

        // start MCP client with proxy URL available
        await base.InitializeAsyncInternal(fixture);

        // start recording/playback session
        await StartRecordOrPlayback();
    }

    public async Task StartProxyAsync(TestProxyFixture fixture)
    {
        // we will use the same proxy instance throughout the test class instances, so we only need to start it if not already started.
        if (TestMode is TestMode.Record or TestMode.Playback && fixture.Proxy == null)
        {
            await fixture.StartProxyAsync();
            Proxy = fixture.Proxy;

            // onetime registration of default sanitizers
            // and deregistering default sanitizers that we don't want
            if (Proxy != null)
            {
                await DisableSanitizersAsync();
                await ApplySanitizersAsync();
            }
        }
    }

    private async Task DisableSanitizersAsync()
    {
        if (DisabledDefaultSanitizers.Count > 0)
        {
            var toRemove = new SanitizerList(new List<string>());
            foreach (var sanitizer in DisabledDefaultSanitizers)
            {
                toRemove.Sanitizers.Add(sanitizer);
            }
            await Proxy!.AdminClient.RemoveSanitizersAsync(toRemove);
        }
    }

    private async Task ApplySanitizersAsync()
    {
        List<SanitizerAddition> sanitizers = new();

        sanitizers.AddRange(GeneralRegexSanitizers);
        sanitizers.AddRange(BodyRegexSanitizers);
        sanitizers.AddRange(HeaderRegexSanitizers);
        sanitizers.AddRange(UriRegexSanitizers);
        sanitizers.AddRange(BodyKeySanitizers);

        if (sanitizers.Count > 0)
        {
            await Proxy!.AdminClient.AddSanitizersAsync(sanitizers);
        }
    }

    public override async ValueTask DisposeAsync()
    {
        await StopRecordOrPlayback();

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

        await base.DisposeAsync();
    }

    private async Task StartRecordOrPlayback()
    {
        if (TestMode is TestMode.Live)
        {
            return;
        }

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

        if (TestMode is TestMode.Playback)
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
        else if (TestMode is TestMode.Record)
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
        if (TestMode is TestMode.Live)
        {
            return;
        }

        if (Proxy is null)
        {
            throw new InvalidOperationException("Test proxy is not initialized.");
        }

        if (TestMode is TestMode.Playback)
        {
            await Proxy.Client.StopPlaybackAsync("placeholder-ignore").ConfigureAwait(false);
        }
        else if (TestMode is TestMode.Record)
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
