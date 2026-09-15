// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
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
    public async Task Should_list_workspaces()
    {
        var expectedWorkspaceId = Environment.GetEnvironmentVariable("ONELAKE_TEST_WORKSPACE_ID");
        var expectedWorkspaceName = Environment.GetEnvironmentVariable("ONELAKE_TEST_WORKSPACE_NAME");
        Assert.False(string.IsNullOrWhiteSpace(expectedWorkspaceId));
        Assert.False(string.IsNullOrWhiteSpace(expectedWorkspaceName));

        var result = await CallToolAsync("onelake_list-workspaces", []);

        var workspaces = result.AssertProperty("workspaces");
        Assert.Equal(JsonValueKind.Array, workspaces.ValueKind);
        Assert.Contains(workspaces.EnumerateArray(), workspace =>
            workspace.GetProperty("id").GetString() == expectedWorkspaceId &&
            workspace.GetProperty("displayName").GetString() == expectedWorkspaceName);
    }
}