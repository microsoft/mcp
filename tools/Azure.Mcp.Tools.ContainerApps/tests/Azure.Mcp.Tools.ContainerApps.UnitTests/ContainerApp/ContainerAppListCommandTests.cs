// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Net;
using System.Text.Json;
using Azure.Mcp.Core.Helpers;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Tools.ContainerApps.Commands;
using Azure.Mcp.Tools.ContainerApps.Commands.ContainerApp;
using Azure.Mcp.Tools.ContainerApps.Models;
using Azure.Mcp.Tools.ContainerApps.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Models.Command;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.ContainerApps.UnitTests.ContainerApp;

public class ContainerAppListCommandTests
{
    private readonly IContainerAppsService _service;
    private readonly ILogger<ContainerAppListCommand> _logger;
    private readonly ContainerAppListCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;

    public ContainerAppListCommandTests()
    {
        _service = Substitute.For<IContainerAppsService>();
        _logger = Substitute.For<ILogger<ContainerAppListCommand>>();

        _command = new(_logger, _service);
        _context = new(new ServiceCollection().BuildServiceProvider());
        _commandDefinition = _command.GetCommand();
    }

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = _command.GetCommand();
        Assert.Equal("list", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
    }

    [Theory]
    [InlineData("--subscription sub", true)]
    [InlineData("--subscription sub --resource-group rg", true)]
    [InlineData("", false)]
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        var originalSubscriptionId = Environment.GetEnvironmentVariable("AZURE_SUBSCRIPTION_ID");
        try
        {
            // Ensure environment variable fallback does not interfere with validation tests
            EnvironmentHelpers.SetAzureSubscriptionId(null);
            // Arrange
            if (shouldSucceed)
            {
                _service.ListContainerApps(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
                    .Returns(new ResourceQueryResults<ContainerAppInfo>(
                    [
                        new("app1", "eastus", "rg1", "/subscriptions/sub/resourceGroups/rg1/providers/Microsoft.App/managedEnvironments/env1", "Succeeded"),
                        new("app2", "eastus2", "rg2", "/subscriptions/sub/resourceGroups/rg2/providers/Microsoft.App/managedEnvironments/env2", "Succeeded")
                    ], false));
            }

            var parseResult = _commandDefinition.Parse(args);

            // Act
            var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(shouldSucceed ? HttpStatusCode.OK : HttpStatusCode.BadRequest, response.Status);
            if (shouldSucceed)
            {
                Assert.NotNull(response.Results);
            }
            else
            {
                Assert.Contains("required", response.Message.ToLower());
            }
        }
        finally
        {
            EnvironmentHelpers.SetAzureSubscriptionId(originalSubscriptionId);
        }
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        // Arrange
        _service.ListContainerApps(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Test error"));

        var parseResult = _commandDefinition.Parse(["--subscription", "sub"]);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Test error", response.Message);
        Assert.Contains("troubleshooting", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_FiltersByResourceGroup_ReturnsFilteredContainerApps()
    {
        // Arrange
        var expectedApps = new ResourceQueryResults<ContainerAppInfo>([new("app1", null, null, null, null)], false);
        _service.ListContainerApps("sub", "rg", Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .Returns(expectedApps);

        var parseResult = _commandDefinition.Parse(["--subscription", "sub", "--resource-group", "rg"]);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
        await _service.Received(1).ListContainerApps("sub", "rg", Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_EmptyList_ReturnsEmptyResults()
    {
        // Arrange
        _service.ListContainerApps("sub", null, Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .Returns(new ResourceQueryResults<ContainerAppInfo>([], false));

        var parseResult = _commandDefinition.Parse(["--subscription", "sub"]);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, ContainerAppsJsonContext.Default.ContainerAppListCommandResult);

        Assert.NotNull(result);
        Assert.Empty(result.ContainerApps);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsExpectedContainerAppProperties()
    {
        // Arrange
        var containerApp = new ContainerAppInfo("myapp", "eastus", "myrg", "/subscriptions/sub/resourceGroups/myrg/providers/Microsoft.App/managedEnvironments/myenv", "Succeeded");
        _service.ListContainerApps("sub", null, Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .Returns(new ResourceQueryResults<ContainerAppInfo>([containerApp], false));

        var parseResult = _commandDefinition.Parse(["--subscription", "sub"]);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
    }
}
