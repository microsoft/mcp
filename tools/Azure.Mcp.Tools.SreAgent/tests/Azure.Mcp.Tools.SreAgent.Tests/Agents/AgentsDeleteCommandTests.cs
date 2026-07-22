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

public class AgentsDeleteCommandTests : SubscriptionCommandUnitTestsBase<AgentsDeleteCommand, ISreAgentService>
{
    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = Command.GetCommand();
        Assert.Equal("delete", command.Name);
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
        Assert.True(command.Options.Any(o => o.Name == "--confirm"), "Missing --confirm option");
    }

    [Theory]
    [InlineData("--subscription sub --agent myagent --name mysubagent --confirm true", true)]
    [InlineData("--subscription sub --agent myagent --name mysubagent --confirm false", false)]
    [InlineData("--subscription sub --agent myagent --name mysubagent", false)]
    [InlineData("--subscription sub --agent myagent", false)]
    [InlineData("", false)]
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        if (shouldSucceed)
        {
            Service.GetAgentAsync(Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
                .Returns(new SreAgentResource { Name = "myagent", Endpoint = "https://test.azuresre.ai" });

            Service.DeleteSubAgentAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string?>(),
                Arg.Any<CancellationToken>())
                .Returns(new SreAgentDeleteResult("mysubagent", "Agent", true));
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

        var deleteResult = new SreAgentDeleteResult("testsubagent", "Agent", true);
        Service.DeleteSubAgentAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns(deleteResult);

        var response = await ExecuteCommandAsync("--subscription", "sub", "--agent", "myagent", "--name", "testsubagent", "--confirm", "true");

        Assert.Equal(HttpStatusCode.OK, response.Status);
        var result = ValidateAndDeserializeResponse(response, SreAgentJsonContext.Default.AgentsDeleteCommandResult);
        Assert.NotNull(result.Agent);
        Assert.Equal("testsubagent", result.Agent.Name);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        Service.GetAgentAsync(Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns(new SreAgentResource { Name = "myagent", Endpoint = "https://test.azuresre.ai" });

        Service.DeleteSubAgentAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Test error"));

        var response = await ExecuteCommandAsync("--subscription", "sub", "--agent", "myagent", "--name", "testsubagent", "--confirm", "true");

        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Test error", response.Message);
    }

    [Fact]
    public async Task BindOptions_BindsOptionsCorrectly()
    {
        Service.GetAgentAsync(Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns(new SreAgentResource { Name = "myagent", Endpoint = "https://test.azuresre.ai" });

        Service.DeleteSubAgentAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns(new SreAgentDeleteResult("testsubagent", "Agent", true));

        var response = await ExecuteCommandAsync("--subscription", "sub", "--agent", "myagent", "--name", "testsubagent", "--confirm", "true");

        Assert.Equal(HttpStatusCode.OK, response.Status);
        await Service.Received(1).DeleteSubAgentAsync(
            Arg.Any<string>(),
            "testsubagent",
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>());
    }
}
