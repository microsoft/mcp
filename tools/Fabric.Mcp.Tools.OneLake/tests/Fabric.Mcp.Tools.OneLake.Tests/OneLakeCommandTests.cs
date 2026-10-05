// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Xml.Linq;
using Microsoft.Mcp.Tests;
using Microsoft.Mcp.Tests.Attributes;
using Microsoft.Mcp.Tests.Client;
using Microsoft.Mcp.Tests.Client.Helpers;
using Xunit;

namespace Fabric.Mcp.Tools.OneLake.Tests;

public class OneLakeCommandTests(ITestOutputHelper output, TestProxyFixture fixture, LiveServerFixture liveServerFixture)
    : RecordedCommandTestsBase(output, fixture, liveServerFixture)
{
    public override async ValueTask InitializeAsync()
    {
        SetArguments("server", "start", "--mode", "all", "--dangerously-disable-elicitation", "--disable-caching");
        LiveServerFixture.ExecutableName = "fabmcp";
        LiveServerFixture.ExecutablePath = Path.Combine(
            PathResolver.RepositoryRoot,
            "servers",
            "Fabric.Mcp.Server",
            "src",
            "bin",
            "Debug",
            OperatingSystem.IsWindows() ? "fabmcp.exe" : "fabmcp");
        await base.InitializeAsync();
    }

    [Fact]
    [LiveTestOnly]
    public async Task Should_list_lifecycle_created_item()
    {
        var expectedWorkspaceId = Environment.GetEnvironmentVariable("ONELAKE_TEST_WORKSPACE_ID");
        var expectedItemName = Environment.GetEnvironmentVariable("ONELAKE_TEST_ITEM_NAME");
        Assert.False(string.IsNullOrWhiteSpace(expectedWorkspaceId));
        Assert.False(string.IsNullOrWhiteSpace(expectedItemName));

        var maxWaitTime = TimeSpan.FromMinutes(2);
        var startTime = DateTime.UtcNow;
        var itemFound = false;

        while (DateTime.UtcNow - startTime < maxWaitTime)
        {
            var result = await CallToolAsync(
                "onelake_list-items",
                new() { { "workspace-id", expectedWorkspaceId } });
            var xmlResponse = result.AssertProperty("xmlResponse").GetString();
            Assert.False(string.IsNullOrWhiteSpace(xmlResponse));

            var document = XDocument.Parse(xmlResponse);
            itemFound = document.Descendants().Any(element =>
                element.Name.LocalName == "Name" && element.Value == expectedItemName);
            if (itemFound)
            {
                break;
            }

            await Task.Delay(PollInterval(10_000), TestContext.Current.CancellationToken);
        }

        Assert.True(itemFound, $"Item '{expectedItemName}' was not visible through OneLake within {maxWaitTime.TotalMinutes} minutes.");
    }
}