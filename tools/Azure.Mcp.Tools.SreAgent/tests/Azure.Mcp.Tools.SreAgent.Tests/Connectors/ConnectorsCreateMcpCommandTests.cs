// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tests.Commands;
using Azure.Mcp.Tools.SreAgent.Commands;
using Azure.Mcp.Tools.SreAgent.Commands.Connectors;
using Azure.Mcp.Tools.SreAgent.Models;
using Azure.Mcp.Tools.SreAgent.Services;
using Microsoft.Mcp.Core.Options;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.SreAgent.Tests.Connectors;

public class ConnectorsCreateMcpCommandTests : SubscriptionCommandUnitTestsBase<ConnectorsCreateMcpCommand, ISreAgentService>
{
    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = Command.GetCommand();
        Assert.Equal("create_mcp", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
    }

    [Fact]
    public void RegisterOptions_AddsExpectedOptions()
    {
        var command = Command.GetCommand();
        Assert.NotNull(command.Options);
        var optionNames = command.Options.Select(o => o.Name).ToList();
        Assert.Contains("--agent", optionNames);
        Assert.Contains("--name", optionNames);
        Assert.Contains("--type", optionNames);
    }

    [Theory]
    [InlineData("--subscription sub --agent agent1 --name connector1 --type stdio --command mycommand", true)]
    [InlineData("--subscription sub --agent agent1 --name connector1 --type http --endpoint https://example.com", true)]
    [InlineData("--subscription sub --agent agent1 --name connector1 --type invalid", false)]
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        if (shouldSucceed)
        {
            Service.GetAgentAsync(
                Arg.Any<string>(),
                Arg.Any<string?>(),
                Arg.Any<string>(),
                Arg.Any<string?>(),
                Arg.Any<RetryPolicyOptions?>(),
                Arg.Any<CancellationToken>())
                .Returns(new SreAgentResource { Name = "agent1", Endpoint = "https://agent1.azuresre.ai" });
            Service.CreateOrUpdateConnectorAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<AgentConnectorEnvelope>(),
                Arg.Any<string?>(),
                Arg.Any<CancellationToken>())
                .Returns(new AgentConnector { Name = "connector1" });
        }

        var response = await ExecuteCommandAsync(args);

        if (shouldSucceed)
        {
            Assert.Equal(HttpStatusCode.OK, response.Status);
        }
        else
        {
            Assert.NotEqual(HttpStatusCode.OK, response.Status);
        }
    }

    [Fact]
    public async Task ExecuteAsync_DeserializationValidation()
    {
        Service.GetAgentAsync(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(new SreAgentResource { Name = "agent1", Endpoint = "https://agent1.azuresre.ai" });
        Service.CreateOrUpdateConnectorAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<AgentConnectorEnvelope>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns(new AgentConnector { Name = "connector1" });

        var response = await ExecuteCommandAsync("--subscription", "sub", "--agent", "agent1", "--name", "connector1", "--type", "stdio", "--command", "mycommand");

        Assert.Equal(HttpStatusCode.OK, response.Status);
        var result = ValidateAndDeserializeResponse(response, SreAgentJsonContext.Default.ConnectorsCreateMcpCommandResult);
        Assert.NotNull(result);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        Service.GetAgentAsync(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(new SreAgentResource { Name = "agent1", Endpoint = "https://agent1.azuresre.ai" });
        Service.CreateOrUpdateConnectorAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<AgentConnectorEnvelope>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Test error"));

        var response = await ExecuteCommandAsync("--subscription", "sub", "--agent", "agent1", "--name", "connector1", "--type", "stdio", "--command", "mycommand");

        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Test error", response.Message);
    }

    [Fact]
    public async Task BindOptions_BindsOptionsCorrectly()
    {
        Service.GetAgentAsync(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(new SreAgentResource { Name = "agent1", Endpoint = "https://agent1.azuresre.ai" });
        Service.CreateOrUpdateConnectorAsync(
            "sub", "rg", "agent1", "connector1", Arg.Any<AgentConnectorEnvelope>(), null, Arg.Any<CancellationToken>())
            .Returns(new AgentConnector { Name = "connector1" });

        var response = await ExecuteCommandAsync("--subscription", "sub", "--agent", "agent1", "--name", "connector1", "--type", "stdio", "--command", "mycommand");

        Assert.Equal(HttpStatusCode.OK, response.Status);
        await Service.Received(1).CreateOrUpdateConnectorAsync("sub", Arg.Any<string>(), "agent1", "connector1", Arg.Any<AgentConnectorEnvelope>(), null, Arg.Any<CancellationToken>());
    }
}
