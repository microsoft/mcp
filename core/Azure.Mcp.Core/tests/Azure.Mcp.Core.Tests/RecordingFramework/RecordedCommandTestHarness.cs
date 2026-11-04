// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Microsoft.Mcp.Tests.Client;
using Microsoft.Mcp.Tests.Client.Helpers;
using Microsoft.Mcp.Tests.Helpers;
using Xunit;

namespace Azure.Mcp.Core.Tests.RecordingFramework;

/// <summary>
/// Exercises the shared recording lifecycle without constructing commands, Azure clients, or an MCP host.
/// </summary>
/// <param name="output"></param>
/// <param name="fixture"></param>
internal sealed class RecordedCommandTestHarness(ITestOutputHelper output, TestProxyFixture fixture)
    : RecordedTestsBase(output, fixture)
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

    public void ResetVariables() => TestVariables.Clear();

    public string GetRecordingId() => RecordingId;

    protected override ValueTask InitializeAsyncInternal(TestProxyFixture? proxy = null)
        => ValueTask.CompletedTask;

    protected override Task<JsonElement> InvokeToolAsync(
        string command, Dictionary<string, object?> parameters, CancellationToken cancellationToken)
        => throw new NotSupportedException("The recording lifecycle harness does not execute tools.");
}
