// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tests.Client;
using Azure.Mcp.Tests.Client.Helpers;
using Azure.Mcp.Tests.Helpers;
using Xunit;

namespace Azure.Mcp.Core.LiveTests.RecordingFramework;

/// <summary>
/// Harness for testing RecordedCommandTestsBase functionality. Intended for proper abstraction of livetest settings etc to allow both record and playback modes in the same test for full roundtrip testing.
/// </summary>
/// <param name="output"></param>
/// <param name="fixture"></param>
internal sealed class RecordedCommandTestHarness(ITestOutputHelper output, TestProxyFixture fixture) : RecordedCommandTestsBase(output, fixture)
{
    public TestMode DesiredMode { get; set; } = TestMode.Record;

    public IReadOnlyDictionary<string, string> Variables => TestVariables;

    protected override ValueTask LoadSettingsAsync()
    {
        Settings = new LiveTestSettings
        {
            SubscriptionId = "00000000-0000-0000-0000-000000000000",
            TenantId = "00000000-0000-0000-0000-000000000000",
            ResourceBaseName = "Sanitized",
            SubscriptionName = "Sanitized",
            TenantName = "Sanitized",
            TestMode = TestMode.Playback
        };

        Settings.TestMode = DesiredMode;
        TestMode = DesiredMode;

        return ValueTask.CompletedTask;
    }

    public void ResetVariables()
    {
        TestVariables.Clear();
    }

    public string GetRecordingId()
    {
        return RecordingId;
    }
}
