// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tests.Commands;
using Azure.Mcp.Tools.SreAgent.Commands;
using Azure.Mcp.Tools.SreAgent.Commands.Agents;
using Azure.Mcp.Tools.SreAgent.Models;
using Azure.Mcp.Tools.SreAgent.Services;
using Microsoft.Mcp.Core.Options;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.SreAgent.Tests.Agents;

public class AgentsCreateCommandTests : SubscriptionCommandUnitTestsBase<AgentsCreateCommand, ISreAgentService>
{
    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = Command.GetCommand();
        Assert.Equal("create", command.Name);
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
        Assert.True(command.Options.Any(o => o.Name == "--description"), "Missing --description option");
        Assert.True(command.Options.Any(o => o.Name == "--instructions"), "Missing --instructions option");
        Assert.True(command.Options.Any(o => o.Name == "--tools"), "Missing --tools option");
        Assert.True(command.Options.Any(o => o.Name == "--handoffs"), "Missing --handoffs option");
    }

    [Theory]
    [InlineData("--subscription sub --agent myagent --name mysubagent", true)]
    [InlineData("--subscription sub --agent myagent --name mysubagent --description test", true)]
    [InlineData("--subscription sub --agent myagent", false)]
    [InlineData("--subscription sub --name mysubagent", false)]
    [InlineData("", false)]
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        if (shouldSucceed)
        {
            Service.GetAgentAsync(Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
                .Returns(new SreAgentResource { Name = "myagent", Endpoint = "https://test.azuresre.ai" });

            Service.CreateSubAgentAsync(
                Arg.Any<string>(),
                Arg.Any<SreSubAgentCreateRequest>(),
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
        Service.GetAgentAsync(Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns(new SreAgentResource { Name = "myagent", Endpoint = "https://test.azuresre.ai" });

        var testAgent = new SreSubAgent { Name = "testsubagent", Properties = new SreSubAgentProperties { Instructions = "test" } };
        Service.CreateSubAgentAsync(
            Arg.Any<string>(),
            Arg.Any<SreSubAgentCreateRequest>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns(testAgent);

        var response = await ExecuteCommandAsync("--subscription", "sub", "--agent", "myagent", "--name", "testsubagent");

        Assert.Equal(HttpStatusCode.OK, response.Status);
        var result = ValidateAndDeserializeResponse(response, SreAgentJsonContext.Default.AgentsCreateCommandResult);
        Assert.NotNull(result.Agent);
        Assert.Equal("testsubagent", result.Agent.Name);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        Service.GetAgentAsync(Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns(new SreAgentResource { Name = "myagent", Endpoint = "https://test.azuresre.ai" });

        Service.CreateSubAgentAsync(Arg.Any<string>(), Arg.Any<SreSubAgentCreateRequest>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Test error"));

        var response = await ExecuteCommandAsync("--subscription", "sub", "--agent", "myagent", "--name", "testsubagent");

        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Test error", response.Message);
    }

    [Fact]
    public async Task BindOptions_BindsOptionsCorrectly()
    {
        Service.GetAgentAsync(Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns(new SreAgentResource { Name = "myagent", Endpoint = "https://test.azuresre.ai" });

        Service.CreateSubAgentAsync(
            Arg.Any<string>(),
            Arg.Any<SreSubAgentCreateRequest>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns(new SreSubAgent { Name = "testsubagent" });

        var response = await ExecuteCommandAsync("--subscription", "sub", "--agent", "myagent", "--name", "testsubagent", "--description", "test description", "--instructions", "test instructions", "--tools", "tool1", "tool2", "--handoffs", "agent1");

        Assert.Equal(HttpStatusCode.OK, response.Status);
        await Service.Received(1).CreateSubAgentAsync(
            Arg.Any<string>(),
            Arg.Is<SreSubAgentCreateRequest>(r =>
                r.Name == "testsubagent" &&
                r.Properties != null &&
                r.Properties.HandoffDescription == "test description" &&
                r.Properties.Instructions == "test instructions" &&
                r.Properties.Tools != null &&
                r.Properties.Tools.Count == 2 &&
                r.Properties.Handoffs != null &&
                r.Properties.Handoffs.Count == 1),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>());
    }
}
