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

public class IncidentsCreateCommandTests : SubscriptionCommandUnitTestsBase<IncidentsCreateCommand, ISreAgentService>
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
        Assert.Contains(command.Options, o => o.Name == $"--{SreAgentOptionDefinitions.SeverityName}");
        Assert.Contains(command.Options, o => o.Name == $"--{SreAgentOptionDefinitions.TitleName}");
        Assert.Contains(command.Options, o => o.Name == $"--{SreAgentOptionDefinitions.DescriptionName}");
        Assert.Contains(command.Options, o => o.Name == $"--{SreAgentOptionDefinitions.ServicesName}");
    }

    [Theory]
    [InlineData("--subscription sub --agent agent1 --severity critical --title \"Test\" --description \"Test desc\" --services svc1", true)]
    [InlineData("--subscription sub --agent agent1", false)]
    [InlineData("", false)]
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

            Service.CreateIncidentThreadAsync(
                Arg.Any<string>(),
                Arg.Any<IncidentThreadCreateRequest>(),
                Arg.Any<string?>(),
                Arg.Any<CancellationToken>())
                .Returns(new IncidentThreadResponse("thread-1", "created"));
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
        Service.GetAgentAsync(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(new SreAgentResource { Name = "agent1", Endpoint = "https://agent1.azuresre.ai" });

        Service.CreateIncidentThreadAsync(
            Arg.Any<string>(),
            Arg.Any<IncidentThreadCreateRequest>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Test error"));

        var response = await ExecuteCommandAsync("--subscription", "sub", "--agent", "agent1", "--severity", "critical", "--title", "Test", "--description", "Desc", "--services", "svc1");

        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Test error", response.Message);
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

        Service.CreateIncidentThreadAsync(
            Arg.Any<string>(),
            Arg.Any<IncidentThreadCreateRequest>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns(new IncidentThreadResponse("thread-1", "created"));

        var response = await ExecuteCommandAsync("--subscription", "sub", "--agent", "agent1", "--severity", "critical", "--title", "Test", "--description", "Desc", "--services", "svc1");

        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
    }

    [Fact]
    public async Task BindOptions_BindsOptionsCorrectly()
    {
        Service.GetAgentAsync(
            "sub",
            null,
            "agent1",
            "tenant1",
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(new SreAgentResource { Name = "agent1", Endpoint = "https://agent1.azuresre.ai" });

        Service.CreateIncidentThreadAsync(
            Arg.Any<string>(),
            Arg.Any<IncidentThreadCreateRequest>(),
            "tenant1",
            Arg.Any<CancellationToken>())
            .Returns(new IncidentThreadResponse("thread-1", "created"));

        var response = await ExecuteCommandAsync("--subscription", "sub", "--agent", "agent1", "--severity", "critical", "--title", "Test", "--description", "Desc", "--services", "svc1", "--tenant", "tenant1");

        Assert.Equal(HttpStatusCode.OK, response.Status);
        await Service.Received(1).CreateIncidentThreadAsync(Arg.Any<string>(), Arg.Any<IncidentThreadCreateRequest>(), "tenant1", Arg.Any<CancellationToken>());
    }
}
