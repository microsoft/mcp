// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Reflection;
using System.Text.Json;
using Azure.Mcp.Tests.Client;
using Azure.Mcp.Tests.Client.Attributes;
using Azure.Mcp.Tests.Client.Helpers;
using Azure.Mcp.Tests.Generated.Models;
using Azure.Mcp.Tests.Helpers;
using Microsoft.Extensions.FileSystemGlobbing;
using NSubstitute;
using Xunit;
using Xunit.v3;

namespace Azure.Mcp.Core.LiveTests.RecordingFramework;

public sealed class RecordedCommandTestsBaseTest : IAsyncLifetime
{
    private string RecordingFileLocation = string.Empty;
    private string TestDisplayName = string.Empty;
    private TestProxyFixture Fixture = new TestProxyFixture();
    private ITestOutputHelper CollectedOutput = Substitute.For<ITestOutputHelper>();
    private RecordedCommandTestHarness? DefaultHarness;

    [Fact]
    public async Task ProxyRecordProducesRecording()
    {
        await DefaultHarness!.InitializeAsync();

        Assert.NotNull(Fixture.Proxy);
        Assert.False(string.IsNullOrWhiteSpace(Fixture.Proxy!.BaseUri));

        DefaultHarness!.RegisterVariable("sampleKey", "sampleValue");
        await DefaultHarness!.DisposeAsync();

        Assert.True(File.Exists(RecordingFileLocation));

        using var document = JsonDocument.Parse(await File.ReadAllTextAsync(RecordingFileLocation, TestContext.Current.CancellationToken));
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

        RecordingFileLocation = harness.GetRecordingAbsolutePath(TestDisplayName);

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
    }
}
