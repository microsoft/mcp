// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Reflection;
using System.Text.Json;
using Microsoft.Mcp.Tests;
using Microsoft.Mcp.Tests.Attributes;
using Microsoft.Mcp.Tests.Client;
using Microsoft.Mcp.Tests.Client.Helpers;
using Microsoft.Mcp.Tests.Generated.Models;
using Microsoft.Mcp.Tests.Helpers;
using NSubstitute;
using Xunit;
using Xunit.v3;

namespace Azure.Mcp.Core.Tests.RecordingFramework;

[Trait("TestType", "Live")]
public sealed class RecordedCommandTestsBaseTest() : IAsyncLifetime
{
    private string RecordingFileLocation = string.Empty;
    private readonly TemporaryAssetsPathResolver Resolver = new();
    private readonly TestProxyFixture Fixture = new();
    private readonly ITestOutputHelper CollectedOutput = Substitute.For<ITestOutputHelper>();
    private RecordedCommandTestHarness? DefaultHarness;

    [Fact]
    public void Harness_DoesNotRequireMcpServer()
    {
        Assert.IsAssignableFrom<RecordedTestsBase>(DefaultHarness);
        Assert.IsNotAssignableFrom<RecordedCommandTestsBase>(DefaultHarness);
        Assert.DoesNotContain(typeof(IClassFixture<LiveServerFixture>),
            typeof(RecordedCommandTestHarness).GetInterfaces());
    }

    [Fact]
    public async Task ProxyRecordProducesRecording()
    {
        await DefaultHarness!.InitializeAsync();

        Assert.NotNull(Fixture.Proxy);
        Assert.False(string.IsNullOrWhiteSpace(Fixture.Proxy!.BaseUri));

        DefaultHarness!.RegisterVariable("sampleKey", "sampleValue");
        await DefaultHarness!.DisposeAsync();

        var recordingPath = Assert.Single(Directory.EnumerateFiles(
            Resolver.RepositoryRoot, Path.GetFileName(RecordingFileLocation), SearchOption.AllDirectories));

        using var document = JsonDocument.Parse(await File.ReadAllTextAsync(recordingPath, TestContext.Current.CancellationToken));
        var variablesElement = document.RootElement.AssertProperty("Variables");
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

        await using var playbackHarness = new RecordedCommandTestHarness(CollectedOutput, Fixture)
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
        var attribute = new CustomMatcherAttribute(compareBody: true, ignoreQueryOrdering: true);
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
        var method = typeof(CustomMatcherAttribute).GetMethod("GetActive", BindingFlags.NonPublic | BindingFlags.Static, Type.EmptyTypes);
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

        await using var playbackHarness = new RecordedCommandTestHarness(CollectedOutput, Fixture)
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
        var testDisplayName = TestContext.Current?.Test?.TestCase?.TestCaseDisplayName
            ?? throw new InvalidDataException("Test case display name is not available.");
        Fixture.ConfigurePathResolver(Resolver);

        var harness = new RecordedCommandTestHarness(CollectedOutput, Fixture)
        {
            DesiredMode = TestMode.Record
        };

        RecordingFileLocation = harness.GetSessionFilePath(testDisplayName);

        DefaultHarness = harness;
        return ValueTask.CompletedTask;
    }

    public async ValueTask DisposeAsync()
    {
        try
        {
            if (DefaultHarness is not null)
            {
                await DefaultHarness.DisposeAsync();
            }
        }
        finally
        {
            try
            {
                await Fixture.DisposeAsync();
            }
            finally
            {
                Resolver.Dispose();
            }
        }
    }
}
