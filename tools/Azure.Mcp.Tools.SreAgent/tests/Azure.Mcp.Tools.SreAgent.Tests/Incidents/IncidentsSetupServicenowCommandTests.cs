// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tests.Commands;
using Azure.Mcp.Tools.SreAgent.Commands.Incidents;
using Azure.Mcp.Tools.SreAgent.Models;
using Azure.Mcp.Tools.SreAgent.Options;
using Azure.Mcp.Tools.SreAgent.Services;
using Microsoft.Mcp.Core.Options;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.SreAgent.Tests.Incidents;

public class IncidentsSetupServicenowCommandTests : SubscriptionCommandUnitTestsBase<IncidentsSetupServicenowCommand, ISreAgentService>
{
    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = Command.GetCommand();
        Assert.Equal("setup_servicenow", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
    }

    [Fact]
    public void RegisterOptions_AddsExpectedOptions()
    {
        var command = Command.GetCommand();
        Assert.Contains(command.Options, o => o.Name == $"--{SreAgentOptionDefinitions.NameName}");
        Assert.Contains(command.Options, o => o.Name == $"--{SreAgentOptionDefinitions.InstanceUrlName}");
        Assert.Contains(command.Options, o => o.Name == $"--{SreAgentOptionDefinitions.AuthTypeName}");
    }

    [Theory]
    [InlineData("--subscription sub --agent agent1 --name connector1 --instance-url https://test.service-now.com --auth-type BearerToken --token-env TOKEN_VAR", true)]
    [InlineData("--subscription sub --agent agent1 --name connector1", false)]
    [InlineData("", false)]
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        if (shouldSucceed)
        {
            Environment.SetEnvironmentVariable("TOKEN_VAR", "test-token");
            Service.GetAgentAsync(
                Arg.Any<string>(),
                Arg.Any<string?>(),
                Arg.Any<string>(),
                Arg.Any<string?>(),
                Arg.Any<RetryPolicyOptions?>(),
                Arg.Any<CancellationToken>())
                .Returns(new SreAgentResource { Name = "agent1", Endpoint = "https://agent1.azuresre.ai" });
            Service.ResolveAgentResourceGroupAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
                .Returns("rg");
            Service.GetConnectorAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
                .ThrowsAsync<HttpRequestException>();
            Service.CreateOrUpdateConnectorAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<AgentConnectorEnvelope>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(new AgentConnector()));
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
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        Environment.SetEnvironmentVariable("TOKEN_VAR", "test-token");
        Service.GetAgentAsync(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(new SreAgentResource { Name = "agent1", Endpoint = "https://agent1.azuresre.ai" });
        Service.ResolveAgentResourceGroupAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns("rg");
        Service.GetConnectorAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync<HttpRequestException>();
        Service.CreateOrUpdateConnectorAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<AgentConnectorEnvelope>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Test error"));

        var response = await ExecuteCommandAsync("--subscription", "sub", "--agent", "agent1", "--name", "connector1", "--instance-url", "https://test.service-now.com", "--auth-type", "BearerToken", "--token-env", "TOKEN_VAR");

        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Test error", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_DeserializationValidation()
    {
        Environment.SetEnvironmentVariable("TOKEN_VAR", "test-token");
        Service.GetAgentAsync(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(new SreAgentResource { Name = "agent1", Endpoint = "https://agent1.azuresre.ai" });
        Service.ResolveAgentResourceGroupAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns("rg");
        Service.GetConnectorAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync<HttpRequestException>();
        Service.CreateOrUpdateConnectorAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<AgentConnectorEnvelope>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new AgentConnector()));

        var response = await ExecuteCommandAsync("--subscription", "sub", "--agent", "agent1", "--name", "connector1", "--instance-url", "https://test.service-now.com", "--auth-type", "BearerToken", "--token-env", "TOKEN_VAR");

        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
    }

    [Fact]
    public async Task BindOptions_BindsOptionsCorrectly()
    {
        Environment.SetEnvironmentVariable("TOKEN_VAR", "test-token");
        Service.GetAgentAsync(
            "sub",
            null,
            "agent1",
            "tenant1",
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(new SreAgentResource { Name = "agent1", Endpoint = "https://agent1.azuresre.ai" });
        Service.ResolveAgentResourceGroupAsync("sub", "agent1", "tenant1", Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns("rg");
        Service.GetConnectorAsync("sub", "rg", "agent1", Arg.Any<string>(), "tenant1", Arg.Any<CancellationToken>())
            .ThrowsAsync<HttpRequestException>();
        Service.CreateOrUpdateConnectorAsync("sub", "rg", "agent1", Arg.Any<string>(), Arg.Any<AgentConnectorEnvelope>(), "tenant1", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new AgentConnector()));

        var response = await ExecuteCommandAsync("--subscription", "sub", "--agent", "agent1", "--name", "connector1", "--instance-url", "https://test.service-now.com", "--auth-type", "BearerToken", "--token-env", "TOKEN_VAR", "--tenant", "tenant1");

        Assert.Equal(HttpStatusCode.OK, response.Status);
        await Service.Received(1).CreateOrUpdateConnectorAsync("sub", "rg", "agent1", Arg.Any<string>(), Arg.Any<AgentConnectorEnvelope>(), "tenant1", Arg.Any<CancellationToken>());
    }
}
