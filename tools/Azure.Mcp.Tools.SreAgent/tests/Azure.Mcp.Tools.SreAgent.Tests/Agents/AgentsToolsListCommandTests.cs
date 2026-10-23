// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tests.Commands;
using Azure.Mcp.Tools.SreAgent.Commands.Agents;
using Azure.Mcp.Tools.SreAgent.Services;
using Xunit;

namespace Azure.Mcp.Tools.SreAgent.Tests.Agents;

public class AgentsToolsListCommandTests : SubscriptionCommandUnitTestsBase<AgentsToolsListCommand, ISreAgentService>
{
    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = Command.GetCommand();
        Assert.NotNull(command.Name);
        Assert.NotEmpty(command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
    }

    [Fact]
    public void RegisterOptions_AddsExpectedOptions()
    {
        var command = Command.GetCommand();
        Assert.NotNull(command.Options);
    }
}
