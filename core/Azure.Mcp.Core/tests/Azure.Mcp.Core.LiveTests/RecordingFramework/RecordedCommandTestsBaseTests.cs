// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Reflection;
using System.Text.Json;
using Azure.Mcp.Tests.Client;
using Azure.Mcp.Tests.Client.Helpers;
using Azure.Mcp.Tests.Generated.Models;
using Azure.Mcp.Tests.Helpers;
using Microsoft.Extensions.FileSystemGlobbing;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Core.LiveTests.RecordingFramework;

public sealed class RecordedCommandTestsBaseTests
{
    [Fact]
    public async Task ProxyRecordProducesRecording()
    {
        var fixture = new TestProxyFixture();
        var output = Substitute.For<ITestOutputHelper>();
        var harness = new RecordedCommandTestHarness(output, fixture)
        {
            DesiredMode = TestMode.Record,
            EnableDefaultSanitizerAdditions = true,
        };

        var displayName = TestContext.Current?.Test?.TestCase?.TestCaseDisplayName ?? nameof(ProxyRecordProducesRecording);
        var recordingPath = harness.GetRecordingAbsolutePath(displayName);
        if (File.Exists(recordingPath))
        {
            File.Delete(recordingPath);
        }

        try
        {
            await harness.InitializeAsync();

            Assert.NotNull(fixture.Proxy);
            Assert.False(string.IsNullOrWhiteSpace(fixture.Proxy!.BaseUri));

            harness.RegisterVariable("sampleKey", "sampleValue");
        }
        finally
        {
            await harness.DisposeAsync();
            await fixture.DisposeAsync();
        }

        Assert.True(File.Exists(recordingPath));

        using var document = JsonDocument.Parse(await File.ReadAllTextAsync(recordingPath, CancellationToken.None));
        Assert.True(document.RootElement.TryGetProperty("Variables", out var variablesElement));
        Assert.Equal("sampleValue", variablesElement.GetProperty("sampleKey").GetString());

        File.Delete(recordingPath);
    }

    [Fact]
    public async Task PerTestMatcherAttributeAppliesWhenPresent()
    {
        var fixture = new TestProxyFixture();
        try
        {
            var output = Substitute.For<ITestOutputHelper>();

            var displayName = TestContext.Current?.Test?.TestCase?.TestCaseDisplayName ?? nameof(PerTestMatcherAttributeAppliesWhenPresent);

            var recordHarness = new RecordedCommandTestHarness(output, fixture)
            {
                DesiredMode = TestMode.Record,
                EnableDefaultSanitizerAdditions = false,
            };

            var recordingPath = recordHarness.GetRecordingAbsolutePath(displayName);
            if (File.Exists(recordingPath))
            {
                File.Delete(recordingPath);
            }
            var recordingId = string.Empty;

            await recordHarness.InitializeAsync();
            recordHarness.RegisterVariable("attrKey", "attrValue");
            await recordHarness.DisposeAsync();

            var playbackHarness = new RecordedCommandTestHarness(output, fixture)
            {
                DesiredMode = TestMode.Playback,
                EnableDefaultSanitizerAdditions = false,
            };

            await playbackHarness.InitializeAsync();
            recordingId = GetRecordingId(playbackHarness);
            await playbackHarness.DisposeAsync();

            output.Received().WriteLine(Arg.Is<string>(s => s.Contains($"Applying custom matcher to recordingId \"{recordingId}\"")));

            if (File.Exists(recordingPath))
            {
                File.Delete(recordingPath);
            }
        }
        finally
        {
            await fixture.DisposeAsync();
        }
    }

    [Fact]
    public async Task GlobalMatcherAndSanitizerAppliesWhenPresent()
    {
        var fixture = new TestProxyFixture();
        var output = Substitute.For<ITestOutputHelper>();
        var harness = new RecordedCommandTestHarness(output, fixture)
        {
            DesiredMode = TestMode.Record,
            TestMatcher = new CustomDefaultMatcher
            {
                CompareBodies = true,
                IgnoreQueryOrdering = true,
            }
        };

        harness.GeneralRegexSanitizers.Add(new GeneralRegexSanitizer(new GeneralRegexSanitizerBody
        {
            Regex = "sample",
            Value = "sanitized",
        }));
        harness.DisabledDefaultSanitizers.Add("UriSubscriptionIdSanitizer");

        try
        {
            await harness.InitializeAsync();
            await harness.DisposeAsync();
        }
        finally
        {
            await fixture.DisposeAsync();
        }

        output.Received().WriteLine(Arg.Is<string>(s => s.Contains("Applying custom matcher to global settings")));
    }

    [Fact]
    public async Task VariableSurvivesRecordPlaybackRoundtrip()
    {
        var fixture = new TestProxyFixture();
        var output = Substitute.For<ITestOutputHelper>();
        var displayName = TestContext.Current?.Test?.TestCase?.TestCaseDisplayName ?? nameof(VariableSurvivesRecordPlaybackRoundtrip);

        var recordHarness = new RecordedCommandTestHarness(output, fixture)
        {
            DesiredMode = TestMode.Record,
        };

        var recordingPath = recordHarness.GetRecordingAbsolutePath(displayName);
        if (File.Exists(recordingPath))
        {
            File.Delete(recordingPath);
        }

        try
        {
            await recordHarness.InitializeAsync();
            recordHarness.RegisterVariable("roundtrip", "value");
            await recordHarness.DisposeAsync();

            var playbackHarness = new RecordedCommandTestHarness(output, fixture)
            {
                DesiredMode = TestMode.Playback,
            };

            await playbackHarness.InitializeAsync();

            Assert.True(playbackHarness.Variables.TryGetValue("roundtrip", out var variableValue));
            Assert.Equal("value", variableValue);

            await playbackHarness.DisposeAsync();
        }
        finally
        {
            await fixture.DisposeAsync();
        }

        if (File.Exists(recordingPath))
        {
            File.Delete(recordingPath);
        }
    }

    private string GetRecordingId(RecordedCommandTestsBase harness)
    {
        var property = typeof(RecordedCommandTestsBase).GetProperty(
            "RecordingId",
            BindingFlags.Instance | BindingFlags.NonPublic);

        return property?.GetValue(harness) as string ?? string.Empty;
    }
}
