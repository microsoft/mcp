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

public class IncidentsPlansCreateCommandTests : SubscriptionCommandUnitTestsBase<IncidentsPlansCreateCommand, ISreAgentService>
{
    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = Command.GetCommand();
        Assert.Equal("plans_create", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
    }

    [Fact]
    public void RegisterOptions_AddsExpectedOptions()
    {
        var command = Command.GetCommand();
        Assert.Contains(command.Options, o => o.Name == $"--{SreAgentOptionDefinitions.NameName}");
        Assert.Contains(command.Options, o => o.Name == $"--{SreAgentOptionDefinitions.SeverityName}");
        Assert.Contains(command.Options, o => o.Name == $"--{SreAgentOptionDefinitions.TriggerConditionName}");
        Assert.Contains(command.Options, o => o.Name == $"--{SreAgentOptionDefinitions.ServicesName}");
        Assert.Contains(command.Options, o => o.Name == $"--{SreAgentOptionDefinitions.StepsName}");
    }

    [Theory]
    [InlineData("--subscription sub --agent agent1 --name plan1 --severity critical --trigger-condition \"test\" --services svc1 --steps \"step1\" --steps \"step2\"", true)]
    [InlineData("--subscription sub --agent agent1 --name plan1", false)]
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

            Service.CreateOrUpdateIncidentFilterAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<IncidentFilterPayload>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
                .Returns(Task.CompletedTask);
            Service.CreateOrUpdateIncidentHandlerAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<IncidentHandler>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
                .Returns(Task.CompletedTask);
            Service.EnableIncidentFilterAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
                .Returns(Task.CompletedTask);
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

        Service.CreateOrUpdateIncidentFilterAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<IncidentFilterPayload>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Test error"));

        var response = await ExecuteCommandAsync("--subscription", "sub", "--agent", "agent1", "--name", "plan1", "--severity", "critical", "--trigger-condition", "test", "--services", "svc1", "--steps", "step1", "--steps", "step2");

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

        Service.CreateOrUpdateIncidentFilterAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<IncidentFilterPayload>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        Service.CreateOrUpdateIncidentHandlerAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<IncidentHandler>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        Service.EnableIncidentFilterAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        var response = await ExecuteCommandAsync("--subscription", "sub", "--agent", "agent1", "--name", "plan1", "--severity", "critical", "--trigger-condition", "test", "--services", "svc1", "--steps", "step1", "--steps", "step2");

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

        Service.CreateOrUpdateIncidentFilterAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<IncidentFilterPayload>(), "tenant1", Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        Service.CreateOrUpdateIncidentHandlerAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<IncidentHandler>(), "tenant1", Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        Service.EnableIncidentFilterAsync(Arg.Any<string>(), Arg.Any<string>(), "tenant1", Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        var response = await ExecuteCommandAsync("--subscription", "sub", "--agent", "agent1", "--name", "plan1", "--severity", "critical", "--trigger-condition", "test", "--services", "svc1", "--steps", "step1", "--steps", "step2", "--tenant", "tenant1");

        Assert.Equal(HttpStatusCode.OK, response.Status);
        await Service.Received(1).CreateOrUpdateIncidentFilterAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<IncidentFilterPayload>(), "tenant1", Arg.Any<CancellationToken>());
    }
}
