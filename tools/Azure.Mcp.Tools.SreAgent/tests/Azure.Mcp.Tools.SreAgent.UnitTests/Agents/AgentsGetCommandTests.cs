// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.SreAgent.Commands;
using Azure.Mcp.Tools.SreAgent.Commands.Agents;
using Azure.Mcp.Tools.SreAgent.Models;
using Azure.Mcp.Tools.SreAgent.Services;
using Microsoft.Mcp.Core.Options;
using Microsoft.Mcp.Tests.Client;
using Microsoft.Mcp.Tests.Helpers;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.SreAgent.UnitTests.Agents;

public class AgentsGetCommandTests : CommandUnitTestsBase<AgentsGetCommand, ISreAgentService>
{
    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = Command.GetCommand();
        Assert.Equal("get", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
    }

    [Fact]
    public void RegisterOptions_AddsExpectedOptions()
    {
        var command = Command.GetCommand();
        Assert.NotNull(command.Options);
        Assert.True(command.Options.Any(o => o.Name == "--agent"), "Missing --agent option");
        Assert.True(command.Options.Any(o => o.Name == "--name"), "Missing --name option");
    }

    [Theory]
    [InlineData("--subscription sub --agent myagent --name mysubagent", true)]
    [InlineData("--subscription sub --agent myagent", false)]
    [InlineData("--subscription sub --name mysubagent", false)]
    [InlineData("", false)]
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        TestEnvironment.ClearAzureSubscriptionId();
        if (shouldSucceed)
        {
            Service.ListAgentsAsync(Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
                .Returns(new List<SreAgentResource> { new() { Name = "myagent", Endpoint = "https://test.azuresre.ai" } });

            Service.GetSubAgentAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string?>(),
                Arg.Any<CancellationToken>())
                .Returns(new SreSubAgent { Name = "mysubagent" });
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
        Service.ListAgentsAsync(Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns(new List<SreAgentResource> { new() { Name = "myagent", Endpoint = "https://test.azuresre.ai" } });

        var testAgent = new SreSubAgent { Name = "testsubagent", Properties = new SreSubAgentProperties { Instructions = "test instructions" } };
        Service.GetSubAgentAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns(testAgent);

        var response = await ExecuteCommandAsync("--subscription", "sub", "--agent", "myagent", "--name", "testsubagent");

        Assert.Equal(HttpStatusCode.OK, response.Status);
        var result = ValidateAndDeserializeResponse(response, SreAgentJsonContext.Default.AgentsGetCommandResult);
        Assert.NotNull(result.Agent);
        Assert.Equal("testsubagent", result.Agent.Name);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        Service.ListAgentsAsync(Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns(new List<SreAgentResource> { new() { Name = "myagent", Endpoint = "https://test.azuresre.ai" } });

        Service.GetSubAgentAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Test error"));

        var response = await ExecuteCommandAsync("--subscription", "sub", "--agent", "myagent", "--name", "testsubagent");

        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Test error", response.Message);
    }

    [Fact]
    public async Task BindOptions_BindsOptionsCorrectly()
    {
        Service.ListAgentsAsync(Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns(new List<SreAgentResource> { new() { Name = "myagent", Endpoint = "https://test.azuresre.ai" } });

        Service.GetSubAgentAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns(new SreSubAgent { Name = "testsubagent" });

        var response = await ExecuteCommandAsync("--subscription", "sub", "--agent", "myagent", "--name", "testsubagent");

        Assert.Equal(HttpStatusCode.OK, response.Status);
        await Service.Received(1).GetSubAgentAsync(
            Arg.Any<string>(),
            "testsubagent",
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>());
    }
}
