// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Tests.Client;
using Microsoft.Mcp.Tests.Client.Helpers;
using Xunit;

namespace Azure.Mcp.Tools.Adme.Tests.Recorded;

/// <summary>Tests ADME health operations through recorded MCP interactions.</summary>
public sealed class HealthRecordedTests(
    ITestOutputHelper output,
    TestProxyFixture fixture,
    LiveServerFixture liveServerFixture)
    : AdmeRecordedTestsBase(output, fixture, liveServerFixture)
{
    private const string HealthCheckTool = "adme_health_check";

    /// <summary>Verifies that ADME authentication and connectivity are healthy.</summary>
    [Fact]
    public async Task Should_check_adme_health()
    {
        var arguments = CreateArguments();
        arguments["include-connectivity"] = true;

        var result = await CallToolResultsAsync(HealthCheckTool, arguments);

        Assert.True(result.GetProperty("authOk").GetBoolean());
        Assert.True(result.GetProperty("connectivityOk").GetBoolean());
    }
}
