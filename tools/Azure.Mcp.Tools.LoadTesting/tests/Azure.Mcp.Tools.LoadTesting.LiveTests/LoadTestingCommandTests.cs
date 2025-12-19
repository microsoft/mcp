// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Tests;
using Azure.Mcp.Tests.Client;
using Azure.Mcp.Tests.Client.Helpers;
using Azure.Mcp.Tests.Generated.Models;
using Xunit;

namespace Azure.Mcp.Tools.LoadTesting.LiveTests;

public class LoadTestingCommandTests(ITestOutputHelper output, TestProxyFixture fixture) : RecordedCommandTestsBase(output, fixture)
{
    private const string TestResourceName = "TestResourceName";
    private const string TestRunId = "TestRunId";

    public override List<UriRegexSanitizer> UriRegexSanitizers => [
        .. base.UriRegexSanitizers,
         new UriRegexSanitizer(new UriRegexSanitizerBody
         {
             Regex = "resource[Gg]roups/([^?\\/]+)",
             Value = "Sanitized",
             GroupForReplace = "1"
         }),
    ];

    public override List<BodyKeySanitizer> BodyKeySanitizers =>
    [
        ..base.BodyKeySanitizers,
        new BodyKeySanitizer(new BodyKeySanitizerBody("$..displayName") {
             Value = "Sanitized"
        }),
        new BodyKeySanitizer(new BodyKeySanitizerBody("$..value[*].properties.dataPlaneURI") {
             Value = "sanitized.eastus.cnt-prod.loadtesting.azure.com"
        })
    ];

    [Fact]
    public async Task Should_list_loadtests()
    {
        // Arrange
        var result = await CallToolAsync(
            "loadtesting_testresource_list",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "tenant", Settings.TenantId },
                { "resource-group", Settings.ResourceGroupName }
            });

        // Assert
        var items = result.AssertProperty("LoadTest");
        Assert.Equal(JsonValueKind.Array, items.ValueKind);
        Assert.NotEmpty(items.EnumerateArray());
        foreach (var item in items.EnumerateArray())
        {
            Assert.NotNull(item.GetProperty("Id").GetString());
        }
    }
}
