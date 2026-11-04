// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Tests.Client;
using Microsoft.Mcp.Tests.Client.Helpers;
using Xunit;

namespace Azure.Mcp.Tools.Adme.Tests.Recorded;

/// <summary>Tests ADME health operations through recorded direct command execution.</summary>
public sealed class HealthRecordedTests(
    ITestOutputHelper output,
    TestProxyFixture fixture)
    : AdmeRecordedTestsBase(output, fixture)
{
    private const string HealthCheckTool = "adme_health_check";

    /// <summary>Verifies that the ADME storage info endpoint responds successfully.</summary>
    [Fact]
    public async Task Should_check_adme_health()
    {
        var arguments = CreateArguments();

        var result = await CallToolResultsAsync(HealthCheckTool, arguments);

        Assert.Equal(200, result.GetProperty("statusCode").GetInt32());
    }
}
