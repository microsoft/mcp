// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.AI.Projects;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.Foundry.Commands;
using Azure.Mcp.Tools.Foundry.Services;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.Foundry.UnitTests;

public class DeploymentsListCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IFoundryService _foundryService;

    public DeploymentsListCommandTests()
    {
        _foundryService = Substitute.For<IFoundryService>();

        var collection = new ServiceCollection();
        collection.AddSingleton(_foundryService);

        _serviceProvider = collection.BuildServiceProvider();
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsDeployments_WhenDeploymentsExist()
    {
        var endpoint = "https://test-endpoint.com";
        var expectedDeployments = new List<Deployment>
        {
            AIProjectsModelFactory.Deployment("type", "deployment1"),
            AIProjectsModelFactory.Deployment("type", "deployment2"),
        };

        _foundryService.ListDeployments(
                Arg.Is<string>(s => s == endpoint),
                Arg.Any<string>(),
                Arg.Any<RetryPolicyOptions>())
            .Returns(expectedDeployments);

        var command = new DeploymentsListCommand();
        var args = command.GetCommand().Parse(["--endpoint", endpoint]);
        var context = new CommandContext(_serviceProvider);
        var response = await command.ExecuteAsync(context, args);

        Assert.NotNull(response);
        Assert.NotNull(response.Results);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsEmpty_WhenNoDeploymentsExist()
    {
        var endpoint = "https://test-endpoint.com";

        _foundryService.ListDeployments(
                Arg.Is<string>(s => s == endpoint),
                Arg.Any<string>(),
                Arg.Any<RetryPolicyOptions>())
            .Returns([]);

        var command = new DeploymentsListCommand();
        var args = command.GetCommand().Parse(["--endpoint", endpoint]);
        var context = new CommandContext(_serviceProvider);
        var response = await command.ExecuteAsync(context, args);

        Assert.NotNull(response);
        Assert.NotNull(response.Results);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesException()
    {
        var endpoint = "https://test-endpoint.com";
        var expectedError = "Test error";

        _foundryService.ListDeployments(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<RetryPolicyOptions>())
            .ThrowsAsync(new Exception(expectedError));

        var command = new DeploymentsListCommand();
        var args = command.GetCommand().Parse(["--endpoint", endpoint]);
        var context = new CommandContext(_serviceProvider);
        var response = await command.ExecuteAsync(context, args);

        Assert.NotNull(response);
        Assert.Equal(500, response.Status);
        Assert.StartsWith(expectedError, response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsError_WhenMissingEndpoint()
    {
        var endpoint = "https://test-endpoint.com";
        var expectedError = "Test error";

        _foundryService.ListDeployments(
                Arg.Is<string>(s => s == endpoint),
                Arg.Any<string>(),
                Arg.Any<RetryPolicyOptions>())
            .ThrowsAsync(new Exception(expectedError));

        var command = new DeploymentsListCommand();
        var args = command.GetCommand().Parse([]);
        var context = new CommandContext(_serviceProvider);
        var response = await command.ExecuteAsync(context, args);

        Assert.NotNull(response);
        Assert.Equal(400, response.Status);
        Assert.StartsWith("Missing Required options: --endpoint", response.Message);
    }

}
