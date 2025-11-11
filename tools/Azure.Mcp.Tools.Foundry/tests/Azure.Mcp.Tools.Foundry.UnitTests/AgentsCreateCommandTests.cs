// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.Foundry.Commands;
using Azure.Mcp.Tools.Foundry.Models;
using Azure.Mcp.Tools.Foundry.Options;
using Azure.Mcp.Tools.Foundry.Services;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.Foundry.UnitTests;

public class AgentsCreateCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IFoundryService _foundryService;

    public AgentsCreateCommandTests()
    {
        _foundryService = Substitute.For<IFoundryService>();

        var collection = new ServiceCollection();
        collection.AddSingleton(_foundryService);

        _serviceProvider = collection.BuildServiceProvider();
    }

    [Theory]
    [InlineData("", FoundryOptionDefinitions.Endpoint)]
    [InlineData("--endpoint https://test-endpoint.com", FoundryOptionDefinitions.ModelDeploymentName)]
    [InlineData("--endpoint https://test-endpoint.com --model-deployment modeldeployment", FoundryOptionDefinitions.AgentName)]
    [InlineData("--endpoint https://test-endpoint.com --model-deployment modeldeployment --agent-name agentname", FoundryOptionDefinitions.SystemInstruction)]
    public async Task ExecuteAsync_Fails_WhenMissingRequiredParameter(string argsString, string missingArgName)
    {
        var command = new AgentsCreateCommand();
        var args = command.GetCommand().Parse(argsString);
        var context = new CommandContext(_serviceProvider);
        var response = await command.ExecuteAsync(context, args, TestContext.Current.CancellationToken);

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains(missingArgName, response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsCreatedAgent()
    {
        var endpoint = "https://test-endpoint.com";
        var modelDeploymentName = "model-deployment";
        var agentName = "agent-name";
        var systemInstruction = "system-instruction";

        var expectedResult = new AgentsCreateResult()
        {
            AgentId = "agent-id",
            AgentName = agentName,
            ProjectEndpoint = endpoint,
            ModelDeploymentName = modelDeploymentName
        };

        _foundryService.CreateAgent(
            Arg.Is(endpoint),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        var command = new AgentsCreateCommand();
        var args = command.GetCommand().Parse(["--endpoint", endpoint, "--model-deployment", modelDeploymentName, "--agent-name", agentName, "--system-instruction", systemInstruction]);
        var context = new CommandContext(_serviceProvider);
        var response = await command.ExecuteAsync(context, args, TestContext.Current.CancellationToken);

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
    }
}
