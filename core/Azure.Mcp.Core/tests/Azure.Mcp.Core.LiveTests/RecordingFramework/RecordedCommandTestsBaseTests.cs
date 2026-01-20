// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.Json;
using Azure.Mcp.Tests.Client.Attributes;
using Azure.Mcp.Tests.Client.Helpers;
using Azure.Mcp.Tests.Generated.Models;
using Azure.Mcp.Tests.Helpers;
using NSubstitute;
using Xunit;
using Xunit.v3;

namespace Azure.Mcp.Core.LiveTests.RecordingFramework;

internal sealed class TemporaryAssetsPathResolver : IRecordingPathResolver, IDisposable
{
    private readonly RecordingPathResolver _inner = new();
    private readonly string _repositoryRoot;
    private readonly string _tempDirectory;
    private readonly string _assetsPath;
    private bool _disposed;

    public TemporaryAssetsPathResolver()
    {
        _repositoryRoot = Path.Combine(Path.GetTempPath(), "mcp-recordings-harness-tests");

        if (Directory.Exists(_repositoryRoot))
        {
            DeleteGitDirectory(_repositoryRoot);
        }
        Directory.CreateDirectory(_repositoryRoot);

        // write an empty file named .git to simulate a repository root
        var gitMarkerPath = Path.Combine(_repositoryRoot, ".git");
        using (File.Create(gitMarkerPath))
        { }

        _tempDirectory = Path.Combine(_repositoryRoot, "tools", "fake-tool");
        Directory.CreateDirectory(_tempDirectory);
        _assetsPath = Path.Combine(_tempDirectory, "assets.json");
    }

    /// <summary>
    /// Recursively delete a git directory. Calling Directory.Delete(path, true), to recursiverly delete a directory
    /// that was populated from sparse-checkout, will fail. This is because the git files under .git\objects\pack
    /// have file attributes on them that will cause an UnauthorizedAccessException when trying to delete them. In order
    /// to delete it, the file attributes need to be set to Normal.
    /// </summary>
    /// <param name="directory">The git directory to delete</param>
    public static void DeleteGitDirectory(string directory)
    {
        File.SetAttributes(directory, FileAttributes.Normal);

        string[] files = Directory.GetFiles(directory);
        string[] dirs = Directory.GetDirectories(directory);

        foreach (string file in files)
        {
            File.SetAttributes(file, FileAttributes.Normal);
            File.Delete(file);
        }

        foreach (string dir in dirs)
        {
            DeleteGitDirectory(dir);
        }

        Directory.Delete(directory, false);
    }

    public string RepositoryRoot => _repositoryRoot;

    public string GetSessionDirectory(Type testType, string? variantSuffix = null)
    {
        return _inner.GetSessionDirectory(testType, variantSuffix);
    }

    public string GetAssetsJson(Type testType)
    {
        var tagPrefix = testType.Assembly.GetName().Name ?? testType.Name;
        var json = $@"
    {{
        ""AssetsRepo"": ""Azure/azure-sdk-assets"",
        ""AssetsRepoPrefixPath"": """",
        ""TagPrefix"": ""{tagPrefix}"",
        ""Tag"": """"
    }}
    ";
        File.WriteAllText(_assetsPath, json, Encoding.UTF8);
        return _assetsPath;
    }

    /// <summary>
    /// Cleanup temp assets file. Not strictly necessary but keeps things tidy.
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }
        _disposed = true;

        try
        {
            if (File.Exists(_assetsPath))
            {
                File.Delete(_assetsPath);
            }
            if (Directory.Exists(_repositoryRoot))
            {
                DeleteGitDirectory(_repositoryRoot);
            }
        }
        catch
        {
            // ignore cleanup failures
        }
    }
}



public sealed class RecordedCommandTestsBaseTest : IAsyncLifetime
{
    private string RecordingFileLocation = string.Empty;
    private string TestDisplayName = string.Empty;
    private readonly TemporaryAssetsPathResolver Resolver = new();
    private readonly TestProxyFixture Fixture;
    private ITestOutputHelper CollectedOutput = Substitute.For<ITestOutputHelper>();
    private RecordedCommandTestHarness? DefaultHarness;

    public RecordedCommandTestsBaseTest()
    {
        Fixture = new TestProxyFixture();
        Fixture.ConfigurePathResolver(Resolver);
    }

    [Fact]
    public async Task ProxyRecordProducesRecording()
    {
        await DefaultHarness!.InitializeAsync();

        Assert.NotNull(Fixture.Proxy);
        Assert.False(string.IsNullOrWhiteSpace(Fixture.Proxy!.BaseUri));

        DefaultHarness!.RegisterVariable("sampleKey", "sampleValue");
        await DefaultHarness!.DisposeAsync();

        var recordingPath = Path.Combine(Fixture.PathResolver.RepositoryRoot, ".assets", "437w6mqk5i", RecordingFileLocation);

        Assert.True(File.Exists(recordingPath));

        using var document = JsonDocument.Parse(await File.ReadAllTextAsync(recordingPath, TestContext.Current.CancellationToken));
        Assert.True(document.RootElement.TryGetProperty("Variables", out var variablesElement));
        Assert.Equal("sampleValue", variablesElement.GetProperty("sampleKey").GetString());
    }

    [CustomMatcher(IgnoreQueryOrdering = true, CompareBodies = true)]
    [Fact]
    public async Task PerTestMatcherAttributeAppliesWhenPresent()
    {
        var activeMatcher = GetActiveMatcher();
        Assert.NotNull(activeMatcher);
        Assert.True(activeMatcher!.CompareBodies);
        Assert.True(activeMatcher.IgnoreQueryOrdering);

        DefaultHarness = new RecordedCommandTestHarness(CollectedOutput, Fixture)
        {
            DesiredMode = TestMode.Record,
            EnableDefaultSanitizerAdditions = false,
        };
        var recordingId = string.Empty;

        await DefaultHarness.InitializeAsync();
        DefaultHarness.RegisterVariable("attrKey", "attrValue");
        await DefaultHarness.DisposeAsync();

        var playbackHarness = new RecordedCommandTestHarness(CollectedOutput, Fixture)
        {
            DesiredMode = TestMode.Playback,
            EnableDefaultSanitizerAdditions = false,
        };

        await playbackHarness.InitializeAsync();
        recordingId = playbackHarness.GetRecordingId();
        await playbackHarness.DisposeAsync();

        CollectedOutput.Received().WriteLine(Arg.Is<string>(s => s.Contains($"Applying custom matcher to recordingId \"{recordingId}\"")));
    }

    [Fact]
    public void CustomMatcherAttributeClearsAfterExecution()
    {
        var attribute = new CustomMatcherAttribute(compareBody: true, ignoreQueryordering: true);
        var xunitTest = Substitute.For<IXunitTest>();
        var methodInfo = typeof(RecordedCommandTestsBaseTest).GetMethod(nameof(CustomMatcherAttributeClearsAfterExecution))
            ?? throw new InvalidOperationException("Unable to locate test method for CustomMatcherAttribute verification.");

        attribute.Before(methodInfo, xunitTest);
        try
        {
            var active = GetActiveMatcher();
            Assert.Same(attribute, active);
            Assert.True(active!.CompareBodies);
            Assert.True(active.IgnoreQueryOrdering);
        }
        finally
        {
            attribute.After(methodInfo, xunitTest);
        }

        Assert.Null(GetActiveMatcher());
    }

    private static CustomMatcherAttribute? GetActiveMatcher()
    {
        var method = typeof(CustomMatcherAttribute).GetMethod("GetActive", BindingFlags.NonPublic | BindingFlags.Static);
        return (CustomMatcherAttribute?)method?.Invoke(null, null);
    }

    [Fact]
    public async Task GlobalMatcherAndSanitizerAppliesWhenPresent()
    {
        DefaultHarness = new RecordedCommandTestHarness(CollectedOutput, Fixture)
        {
            DesiredMode = TestMode.Record,
            TestMatcher = new CustomDefaultMatcher
            {
                CompareBodies = true,
                IgnoreQueryOrdering = true,
            }
        };

        DefaultHarness.GeneralRegexSanitizers.Add(new GeneralRegexSanitizer(new GeneralRegexSanitizerBody
        {
            Regex = "sample",
            Value = "sanitized",
        }));
        DefaultHarness.DisabledDefaultSanitizers.Add("UriSubscriptionIdSanitizer");

        await DefaultHarness.InitializeAsync();
        await DefaultHarness.DisposeAsync();

        CollectedOutput.Received().WriteLine(Arg.Is<string>(s => s.Contains("Applying custom matcher to global settings")));
    }

    [Fact]
    public async Task VariableSurvivesRecordPlaybackRoundtrip()
    {
        await DefaultHarness!.InitializeAsync();
        DefaultHarness.RegisterVariable("roundtrip", "value");
        await DefaultHarness.DisposeAsync();

        var playbackHarness = new RecordedCommandTestHarness(CollectedOutput, Fixture)
        {
            DesiredMode = TestMode.Playback,
        };
        await playbackHarness.InitializeAsync();
        Assert.True(playbackHarness.Variables.TryGetValue("roundtrip", out var variableValue));
        Assert.Equal("value", variableValue);
        await playbackHarness.DisposeAsync();
    }

    public ValueTask InitializeAsync()
    {
        TestDisplayName = TestContext.Current?.Test?.TestCase?.TestCaseDisplayName ?? throw new InvalidDataException("Test case display name is not available.");

        var harness = new RecordedCommandTestHarness(CollectedOutput, Fixture)
        {
            DesiredMode = TestMode.Record
        };

        RecordingFileLocation = harness.GetSessionFilePath(TestDisplayName);

        if (File.Exists(RecordingFileLocation))
        {
            File.Delete(RecordingFileLocation);
        }

        DefaultHarness = harness;
        return ValueTask.CompletedTask;
    }

    public async ValueTask DisposeAsync()
    {
        // always clean up this recording file on our way out of the test if it exists
        if (File.Exists(RecordingFileLocation))
        {
            File.Delete(RecordingFileLocation);
        }

        // automatically collect the proxy fixture so that writers of tests don't need to remember to do so and the proxy process doesn't run forever
        await Fixture.DisposeAsync();
        Resolver.Dispose();
    }
}

