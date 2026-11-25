// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.SreAgent.Commands;
using Microsoft.Mcp.Core.Areas.Server;
using Microsoft.Mcp.Core.Services.Host;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.SreAgent.Tests;

public sealed class SreAgentCommandHelpersTests
{
    [Theory]
    [InlineData("unix-user", "windows-user", "unix-user", "unix-user")]
    [InlineData(null, "windows-user", "windows-user", "windows-user")]
    [InlineData(null, null, "mcp-user", "MCP User")]
    public void GetUser_Local_PreservesEnvironmentPrecedence(string? user, string? username, string expectedId, string expectedName)
    {
        var environment = Substitute.For<IEnvironmentVariables>();
        environment.IsAccessAllowed.Returns(true);
        environment.GetEnvironmentVariable("USER").Returns(user);
        environment.GetEnvironmentVariable("USERNAME").Returns(username);

        Assert.Equal((expectedId, expectedName), SreAgentCommandHelpers.GetUser(environment));
    }

    [Fact]
    public void GetUser_Http_UsesFallbackWithoutReadingEnvironment()
    {
        var environment = new EnvironmentVariables(
            Microsoft.Extensions.Options.Options.Create(new ServerRuntimeConfiguration { Transport = "http" }));

        Assert.Equal(("mcp-user", "MCP User"), SreAgentCommandHelpers.GetUser(environment));
        var thread = SreAgentCommandHelpers.CreateThreadRequest(environment, "Investigate", "agent");
        Assert.Equal("mcp-user", thread.StartMessage.UserId);
        Assert.Equal("MCP User", thread.StartMessage.DisplayName);
        var message = SreAgentCommandHelpers.CreateMessageRequest(environment, "Continue");
        Assert.Equal("mcp-user", message.UserId);
        Assert.Equal("MCP User", message.DisplayName);
    }
}
