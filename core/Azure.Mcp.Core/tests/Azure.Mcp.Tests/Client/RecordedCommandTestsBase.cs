// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.ClientModel;
using System.ClientModel.Primitives;
using System.Text;
using Azure.Mcp.Tests.Client.Attributes;
using Azure.Mcp.Tests.Client.Helpers;
using Azure.Mcp.Tests.Generated.Models;
using Azure.Mcp.Tests.Helpers;
using Xunit;

namespace Azure.Mcp.Tests.Client;

public abstract class RecordedCommandTestsBase(ITestOutputHelper output, TestProxyFixture fixture) : CommandTestsBase(output), IClassFixture<TestProxyFixture>
{
    private const string EmptyGuid = "00000000-0000-0000-0000-000000000000";

    protected TestProxy? Proxy { get; private set; } = fixture.Proxy;

    protected string RecordingId { get; private set; } = string.Empty;

    /// <summary>
    /// When true, a set of default "additional" sanitizers will be registered. Currently includes:
    ///     - Sanitize out value of ResourceBaseName from LiveTestSettings as a GeneralRegexSanitizer
    /// </summary>
    public virtual bool EnableDefaultSanitizerAdditions { get; set; } = true;

    /// <summary>
    /// Sanitizers that will apply generally across all parts (URI, Body, HeaderValues) of the request/response. This sanitization is applied to to recorded data at rest and during recording, and against test requests during playback.
    /// </summary>
    public virtual List<GeneralRegexSanitizer> GeneralRegexSanitizers { get; } = [];

    /// <summary>
    /// Sanitizers that will apply a regex to specific headers. This sanitization is applied to to recorded data at rest and during recording, and against test requests during playback.
    /// </summary>
    public virtual List<HeaderRegexSanitizer> HeaderRegexSanitizers { get; } =
    [
        // Sanitize the WWW-Authenticate header which may contain tenant IDs or resource URLs to "Sanitized"
        // During conversion to recordings, the actual tenant ID is captured in group 1 and replaced with a fixed GUID.
        // REMOVAL of this formatting cause complete failure on tool side when it expects a valid URL with a GUID tenant ID.
        // Hence the more complex replacement rather than a simple static string replace of the entire header value with `Sanitized`
        new HeaderRegexSanitizer(new HeaderRegexSanitizerBody("WWW-Authenticate")
        {
            Regex = "https://login.microsoftonline.com/(.*?)\"",
            GroupForReplace = "1",
            Value = EmptyGuid
        })
    ];

    /// <summary>
    /// Sanitizers that apply a regex replacement to URIs. This sanitization is applied to to recorded data at rest and during recording, and against test requests during playback.
    /// </summary>
    public virtual List<UriRegexSanitizer> UriRegexSanitizers { get; } = [];

    /// <summary>
    /// Sanitizers that will apply a regex replacement to a specific json body key. This sanitization is applied to to recorded data at rest and during recording, and against test requests during playback.
    /// </summary>
    public virtual List<BodyKeySanitizer> BodyKeySanitizers { get; } = [];

    /// <summary>
    /// Sanitizers that will apply regex replacement to the body of requests/responses. This sanitization is applied to to recorded data at rest and during recording, and against test requests during playback.
    /// </summary>
    public virtual List<BodyRegexSanitizer> BodyRegexSanitizers { get; } = [];

    /// <summary>
    /// The test-proxy has a default set of ~90 sanitizers for common sensitive data (GUIDs, tokens, timestamps, etc). This list allows opting out of specific default sanitizers by name.
    /// Grab the names from the test-proxy source at https://github.com/Azure/azure-sdk-tools/blob/main/tools/test-proxy/Azure.Sdk.Tools.TestProxy/Common/SanitizerDictionary.cs#L65)
    /// Default Set:
    ///     - `AZSDK3430`: `$..id`
    /// </summary>
    public virtual List<string> DisabledDefaultSanitizers { get; } = ["AZSDK3430"];

    /// <summary>
    /// During recording, variables saved to this dictionary will be propagated to the test-proxy and saved in the recording file.
    /// During playback, these variables will be available within the test function body, and can be used to ensure that dynamic values from the recording are used where
    /// specific values should be used.
    /// </summary>
    protected readonly Dictionary<string, string> TestVariables = [];

    /// <summary>
    /// When set, applies a custom matcher for _all_ playback tests from this test class. This can be overridden on a per-test basis using the <see cref="CustomMatcherAttribute"/> attribute on test methods.
    /// </summary>
    public virtual CustomDefaultMatcher? TestMatcher { get; set; } = null;

    public virtual void RegisterVariable(string name, string value)
    {
        if (TestMode == TestMode.Playback)
        {
            // no-op in live/playback modes, as during playback the variables will be populated from the recording file automatically.
            return;
        }

        TestVariables[name] = value;
    }

    /// <summary>
    /// Registers a variable to or retrieves it from the session record. This is a convenience equivalent to calling
    /// <see cref="RegisterVariable(string, string)"/> and then retrieving the value from <see cref="TestVariables"/>.
    /// If the test mode is playback, it will load attempt to load the variable and return it. If the test mode is
    /// record, it will store the value and return it.
    /// </summary>
    /// <param name="name">The name of the variable to register or retrieve.</param>
    /// <param name="value">The value reference to register or retrieve.</param>
    /// <returns>The value of the variable.</returns>
    public virtual string RegisterOrRetrieveVariable(string name, string value)
    {
        if (TestMode == TestMode.Record)
        {
            // store the value in the recording
            TestVariables[name] = value;
        }
        else if (TestMode == TestMode.Playback)
        {
            // retrieve the value from the recording
            value = TestVariables[name];
        }

        return value;
    }

    protected TestProxyFixture Fixture => fixture;

    protected IRecordingPathResolver PathResolver => fixture.PathResolver;

    protected virtual bool IsAsync => false;

    // todo: use this when we have versioned tests to run this against.
    protected virtual string? VersionQualifier => null;

    protected override async ValueTask LoadSettingsAsync()
    {
        await base.LoadSettingsAsync();
    }

    public override async ValueTask InitializeAsync()
    {
        // load settings first to determine test mode
        await LoadSettingsAsync();

        if (fixture.Proxy == null)
        {
            // start the proxy if needed
            await StartProxyAsync(fixture);
        }

        // start MCP client with proxy URL available
        await base.InitializeAsyncInternal(fixture);

        // start recording/playback session
        await StartRecordOrPlayback();

        // apply custom matcher if test has attribute
        await ApplyAttributeMatcherSettings();
    }

    private async Task ApplyAttributeMatcherSettings()
    {
        if (Proxy == null || TestMode != TestMode.Playback)
        {
            return;
        }

        var attr = CustomMatcherAttribute.GetActive();
        if (attr == null)
        {
            return;
        }

        var matcher = new CustomDefaultMatcher
        {
            IgnoreQueryOrdering = attr.IgnoreQueryOrdering,
            CompareBodies = attr.CompareBodies,
        };

        await SetMatcher(matcher, RecordingId);
    }

    private async Task SetMatcher(CustomDefaultMatcher matcher, string? recordingId = null)
    {
        if (Proxy == null)
        {
            throw new InvalidOperationException("Test proxy is not initialized. Cannot set a matcher for an uninitialized test proxy.");
        }

        var matcherSb = new StringBuilder();
        matcherSb.Append($"CompareBodies={matcher.CompareBodies}, IgnoreQueryOrdering={matcher.IgnoreQueryOrdering}");
        if (!string.IsNullOrEmpty(matcher.IgnoredHeaders))
        {
            matcherSb.Append($", IgnoredHeaders={matcher.IgnoredHeaders}");
        }
        if (!string.IsNullOrEmpty(matcher.ExcludedHeaders))
        {
            matcherSb.Append($", ExcludedHeaders={matcher.ExcludedHeaders}.");
        }

        // per-test matcher setting
        if (recordingId != null)
        {
            var options = new RequestOptions();
            options.AddHeader("x-recording-id", recordingId);

            Output.WriteLine($"Applying custom matcher to recordingId \"{recordingId}\": {matcherSb}");
            await Proxy.AdminClient.SetMatcherAsync("CustomDefaultMatcher", matcher, options);
        }
        // global matcher setting
        else
        {
            Output.WriteLine($"Applying custom matcher to global settings: {matcherSb}");
            await Proxy.AdminClient.SetMatcherAsync("CustomDefaultMatcher", matcher);
        }
    }

    public async Task StartProxyAsync(TestProxyFixture fixture)
    {
        // we will use the same proxy instance throughout the test class instances, so we only need to start it if not already started.
        if (TestMode is TestMode.Record or TestMode.Playback && fixture.Proxy == null)
        {
            var assetsPath = PathResolver.GetAssetsJson(GetType());
            await fixture.StartProxyAsync(assetsPath);
            Proxy = fixture.Proxy;

            // onetime on starting the proxy, we have initialized the livetest settings so lets add some additional sanitizers by default
            if (EnableDefaultSanitizerAdditions)
            {
                PopulateDefaultSanitizers();
            }

            // onetime registration of default sanitizers
            // and deregistering default sanitizers that we don't want
            if (Proxy != null)
            {
                await DisableSanitizersAsync();
                await ApplySanitizersAsync();

                // set session matcher for this class if specified
                if (TestMatcher != null)
                {
                    await SetMatcher(TestMatcher);
                }
            }
        }
    }

    private void PopulateDefaultSanitizers()
    {
        // Registering a few common sanitizers for values that we know will be universally present and cleaned up
        if (EnableDefaultSanitizerAdditions)
        {
            GeneralRegexSanitizers.Add(new GeneralRegexSanitizer(new GeneralRegexSanitizerBody()
            {
                Regex = Settings.ResourceBaseName,
                Value = "Sanitized",
            }));
            GeneralRegexSanitizers.Add(new GeneralRegexSanitizer(new GeneralRegexSanitizerBody()
            {
                Regex = Settings.SubscriptionId,
                Value = EmptyGuid,
            }));
        }
    }

    private async Task DisableSanitizersAsync()
    {
        if (DisabledDefaultSanitizers.Count > 0)
        {
            var toRemove = new SanitizerList([]);
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
        if (TestContext.Current?.TestState?.Result == TestResult.Failed && Proxy != null)
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
        if (TestMode == TestMode.Live)
        {
            return;
        }

        if (Proxy == null)
        {
            throw new InvalidOperationException("Test proxy is not initialized.");
        }

        var testName = TryGetCurrentTestName();
        var pathToRecording = GetSessionFilePath(testName);
        var assetsPath = PathResolver.GetAssetsJson(GetType());

        var recordOptions = new Dictionary<string, string>
        {
            { "x-recording-file", pathToRecording },
        };

        if (!string.IsNullOrWhiteSpace(assetsPath))
        {
            recordOptions["x-recording-assets-file"] = assetsPath;
        }
        var bodyContent = BinaryContentHelper.FromObject(recordOptions);

        if (TestMode == TestMode.Playback)
        {
            Output.WriteLine($"[Playback] Session file: {pathToRecording}");
            try
            {
                ClientResult<IReadOnlyDictionary<string, string>>? playbackResult = await Proxy.Client.StartPlaybackAsync(new TestProxyStartInformation(pathToRecording, assetsPath, null)).ConfigureAwait(false);

                // Extract recording ID from response header
                if (playbackResult.GetRawResponse().Headers.TryGetValue("x-recording-id", out var recordingId))
                {
                    RecordingId = recordingId ?? string.Empty;
                    Output.WriteLine($"[Playback] Recording ID: {RecordingId}");
                }

                foreach (var key in playbackResult.Value.Keys)
                {
                    Output.WriteLine($"[Playback] Variable from recording: {key} = {playbackResult.Value[key]}");
                    TestVariables[key] = playbackResult.Value[key];
                }
            }
            catch (Exception e)
            {
                Output.WriteLine(Proxy.SnapshotStdErr() ?? $"Proxy is null while attempting to snapshot stderr. Facing exception during start playback.{e.ToString()}");
                throw;
            }
        }
        else if (TestMode == TestMode.Record)
        {
            Output.WriteLine($"[Record] Session file: {pathToRecording}");
            try
            {
                ClientResult result = Proxy.Client.StartRecord(bodyContent);

                // Extract recording ID from response header
                if (result.GetRawResponse().Headers.TryGetValue("x-recording-id", out var recordingId))
                {
                    RecordingId = recordingId ?? string.Empty;
                    Output.WriteLine($"[Record] Recording ID: {RecordingId}");
                }
            }
            catch (Exception e)
            {
                Output.WriteLine(Proxy.SnapshotStdErr() ?? $"Proxy is null while attempting to snapshot stderr. Facing exception during start record.{e.ToString()}");
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

        if (Proxy == null)
        {
            throw new InvalidOperationException("Test proxy is not initialized.");
        }

        if (TestMode == TestMode.Playback)
        {
            await Proxy.Client.StopPlaybackAsync("placeholder-ignore").ConfigureAwait(false);
        }
        else if (TestMode == TestMode.Record)
        {
            Proxy.Client.StopRecord("placeholder-ignore", TestVariables);
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

    public string GetSessionFilePath(string displayName)
    {
        var sanitized = RecordingPathResolver.Sanitize(displayName);
        var dir = PathResolver.GetSessionDirectory(GetType(), variantSuffix: null);
        var fileName = RecordingPathResolver.BuildFileName(sanitized, IsAsync, VersionQualifier);
        var fullPath = Path.Combine(dir, fileName).Replace('\\', '/');
        return fullPath;
    }
}
