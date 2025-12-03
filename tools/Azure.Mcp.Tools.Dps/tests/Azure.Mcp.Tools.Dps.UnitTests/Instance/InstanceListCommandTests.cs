// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Tools.Dps.Commands;
using Azure.Mcp.Tools.Dps.Commands.Instance;
using Azure.Mcp.Tools.Dps.Models;
using Azure.Mcp.Tools.Dps.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System.CommandLine;
using System.Net;
using System.Text.Json;
using Xunit;

namespace Azure.Mcp.Tools.Dps.UnitTests.Instance;

public class InstanceListCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IDpsService _dpsService;
    private readonly ILogger<InstanceListCommand> _logger;
    private readonly InstanceListCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;

    public InstanceListCommandTests()
    {
        _dpsService = Substitute.For<IDpsService>();
        _logger = Substitute.For<ILogger<InstanceListCommand>>();

        var collection = new ServiceCollection().AddSingleton(_dpsService);
        _serviceProvider = collection.BuildServiceProvider();
        _command = new(_logger);
        _context = new(_serviceProvider);
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
    [InlineData("--subscription test-sub", true)]
    [InlineData("--subscription test-sub --resource-group test-rg", true)]
    [InlineData("", false)]
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        // Arrange
        if (shouldSucceed)
        {
            _dpsService
                .ListInstancesAsync(
                    Arg.Any<string>(),
                    Arg.Any<string>(),
                    Arg.Any<string>(),
                    Arg.Any<Azure.Mcp.Core.Options.RetryPolicyOptions>(),
                    Arg.Any<CancellationToken>())
                .Returns([]);
        }

        var parseResult = _commandDefinition.Parse(args);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(shouldSucceed ? HttpStatusCode.OK : HttpStatusCode.BadRequest, response.Status);
        if (shouldSucceed)
        {
            Assert.NotNull(response.Results);
            Assert.Equal("Success", response.Message);
        }
        else
        {
            Assert.Contains("required", response.Message.ToLower());
        }
    }

    [Fact]
    public async Task ExecuteAsync_DeserializationValidation()
    {
        // Arrange
        _dpsService
            .ListInstancesAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<Azure.Mcp.Core.Options.RetryPolicyOptions>(),
                Arg.Any<CancellationToken>())
            .Returns([]);

        var parseResult = _commandDefinition.Parse(["--subscription", "test-sub"]);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, DpsJsonContext.Default.InstanceListCommandResult);

        Assert.NotNull(result);
        Assert.Empty(result.Instances);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        // Arrange
        _dpsService
            .ListInstancesAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<Azure.Mcp.Core.Options.RetryPolicyOptions>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.FromException<List<DpsInstanceInfo>>(new Exception("Test error")));

        var parseResult = _commandDefinition.Parse(["--subscription", "test-sub"]);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Test error", response.Message);
        Assert.Contains("troubleshooting", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsInstancesSuccessfully()
    {
        // Arrange
        var expectedInstances = new List<DpsInstanceInfo>
        {
            new()
            {
                Name = "test-dps",
                Id = "/subscriptions/sub1/resourceGroups/rg1/providers/Microsoft.Devices/provisioningServices/test-dps",
                ResourceGroup = "rg1",
                Location = "eastus",
                ProvisioningState = "Succeeded",
                Sku = "S1",
                IdScope = "0ne00000000"
            }
        };

        _dpsService
            .ListInstancesAsync(
                "test-sub",
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<Azure.Mcp.Core.Options.RetryPolicyOptions>(),
                Arg.Any<CancellationToken>())
            .Returns(expectedInstances);

        var parseResult = _commandDefinition.Parse(["--subscription", "test-sub"]);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, DpsJsonContext.Default.InstanceListCommandResult);

        Assert.NotNull(result);
        Assert.Single(result.Instances);
        Assert.Equal("test-dps", result.Instances[0].Name);
        Assert.Equal("eastus", result.Instances[0].Location);
    }

    [Fact]
    public void BindOptions_BindsOptionsCorrectly()
    {
        // Arrange
        var parseResult = _commandDefinition.Parse([
            "--subscription", "test-subscription",
            "--resource-group", "test-rg"
        ]);

        // Act
        var options = _command.TestBindOptions(parseResult);

        // Assert
        Assert.NotNull(options);
        Assert.Equal("test-subscription", options.Subscription);
        Assert.Equal("test-rg", options.ResourceGroup);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesAuthenticationException()
    {
        // Arrange
        _dpsService
            .ListInstancesAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<Azure.Mcp.Core.Options.RetryPolicyOptions>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.FromException<List<DpsInstanceInfo>>(
                new Azure.Identity.AuthenticationFailedException("Authentication failed")));

        var parseResult = _commandDefinition.Parse(["--subscription", "test-sub"]);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.Status);
        Assert.Contains("az login", response.Message.ToLower());
    }

    [Fact]
    public async Task ExecuteAsync_HandlesForbiddenException()
    {
        // Arrange
        _dpsService
            .ListInstancesAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<Azure.Mcp.Core.Options.RetryPolicyOptions>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.FromException<List<DpsInstanceInfo>>(
                new Azure.RequestFailedException(403, "Forbidden")));

        var parseResult = _commandDefinition.Parse(["--subscription", "test-sub"]);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.Status);
        Assert.Contains("authorization", response.Message.ToLower());
        Assert.Contains("rbac", response.Message.ToLower());
    }

    [Fact]
    public async Task ExecuteAsync_HandlesNotFoundException()
    {
        // Arrange
        _dpsService
            .ListInstancesAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<Azure.Mcp.Core.Options.RetryPolicyOptions>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.FromException<List<DpsInstanceInfo>>(
                new Azure.RequestFailedException(404, "Not Found")));

        var parseResult = _commandDefinition.Parse(["--subscription", "test-sub"]);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.Status);
        Assert.Contains("not found", response.Message.ToLower());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task ExecuteAsync_HandlesOptionalResourceGroup(string? resourceGroup)
    {
        // Arrange
        _dpsService
            .ListInstancesAsync(
                Arg.Any<string>(),
                resourceGroup,
                Arg.Any<string>(),
                Arg.Any<Azure.Mcp.Core.Options.RetryPolicyOptions>(),
                Arg.Any<CancellationToken>())
            .Returns([]);

        var args = resourceGroup == null
            ? new[] { "--subscription", "test-sub" }
            : new[] { "--subscription", "test-sub", "--resource-group", resourceGroup };

        var parseResult = _commandDefinition.Parse(args);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        await _dpsService.Received(1).ListInstancesAsync(
            "test-sub",
            Arg.Is<string?>(rg => rg == resourceGroup),
            Arg.Any<string>(),
            Arg.Any<Azure.Mcp.Core.Options.RetryPolicyOptions>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WithResourceGroupFilter()
    {
        // Arrange
        var expectedInstances = new List<DpsInstanceInfo>
        {
            new()
            {
                Name = "filtered-dps",
                Id = "/subscriptions/sub1/resourceGroups/specific-rg/providers/Microsoft.Devices/provisioningServices/filtered-dps",
                ResourceGroup = "specific-rg",
                Location = "westus",
                ProvisioningState = "Succeeded"
            }
        };

        _dpsService
            .ListInstancesAsync(
                "test-sub",
                "specific-rg",
                Arg.Any<string>(),
                Arg.Any<Azure.Mcp.Core.Options.RetryPolicyOptions>(),
                Arg.Any<CancellationToken>())
            .Returns(expectedInstances);

        var parseResult = _commandDefinition.Parse([
            "--subscription", "test-sub",
            "--resource-group", "specific-rg"
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        
        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, DpsJsonContext.Default.InstanceListCommandResult);

        Assert.NotNull(result);
        Assert.Single(result.Instances);
        Assert.Equal("specific-rg", result.Instances[0].ResourceGroup);
    }
}
