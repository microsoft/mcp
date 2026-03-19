// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Net;
using System.Text.Json;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.Compute.Commands;
using Azure.Mcp.Tools.Compute.Commands.Vmss;
using Azure.Mcp.Tools.Compute.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Models.Command;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.Compute.UnitTests.Vmss;

public class VmssDeleteCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IComputeService _computeService;
    private readonly ILogger<VmssDeleteCommand> _logger;
    private readonly VmssDeleteCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;
    private readonly string _knownSubscription = "sub123";
    private readonly string _knownResourceGroup = "test-rg";
    private readonly string _knownVmssName = "test-vmss";

    public VmssDeleteCommandTests()
    {
        _computeService = Substitute.For<IComputeService>();
        _logger = Substitute.For<ILogger<VmssDeleteCommand>>();

        var collection = new ServiceCollection().AddSingleton(_computeService);

        _serviceProvider = collection.BuildServiceProvider();
        _command = new(_logger);
        _context = new(_serviceProvider);
        _commandDefinition = _command.GetCommand();
    }

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = _command.GetCommand();
        Assert.Equal("delete", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
    }

    [Theory]
    [InlineData("--vmss-name test-vmss --resource-group test-rg --subscription sub123", true)]
    [InlineData("--vmss-name test-vmss --resource-group test-rg --subscription sub123 --force-deletion", true)]
    [InlineData("--resource-group test-rg --subscription sub123", false)] // Missing vmss-name
    [InlineData("--vmss-name test-vmss --subscription sub123", false)] // Missing resource-group
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        // Arrange
        if (shouldSucceed)
        {
            _computeService.DeleteVmssAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<bool?>(),
                Arg.Any<string?>(),
                Arg.Any<RetryPolicyOptions?>(),
                Arg.Any<CancellationToken>())
                .Returns(true);
        }

        var parseResult = _commandDefinition.Parse(args);

        // Act & Assert
        if (shouldSucceed)
        {
            var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);
            Assert.Equal(HttpStatusCode.OK, response.Status);
            Assert.NotNull(response.Results);
        }
        else
        {
            try
            {
                var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);
                Assert.Equal(HttpStatusCode.BadRequest, response.Status);
            }
            catch (Microsoft.Mcp.Core.Commands.CommandValidationException)
            {
                // Expected for validation failures
            }
        }
    }

    [Fact]
    public async Task ExecuteAsync_DeletesVmss()
    {
        // Arrange
        _computeService.DeleteVmssAsync(
            Arg.Is(_knownVmssName),
            Arg.Is(_knownResourceGroup),
            Arg.Is(_knownSubscription),
            Arg.Any<bool?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(true);

        var parseResult = _commandDefinition.Parse([
            "--vmss-name", _knownVmssName,
            "--resource-group", _knownResourceGroup,
            "--subscription", _knownSubscription
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, ComputeJsonContext.Default.VmssDeleteCommandResult);

        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Contains("successfully deleted", result.Message);
        Assert.Contains(_knownVmssName, result.Message);
    }

    [Fact]
    public async Task ExecuteAsync_WithForceDeletion_PassesForceDeletionToService()
    {
        // Arrange
        _computeService.DeleteVmssAsync(
            Arg.Is(_knownVmssName),
            Arg.Is(_knownResourceGroup),
            Arg.Is(_knownSubscription),
            Arg.Is<bool?>(true),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(true);

        var parseResult = _commandDefinition.Parse([
            "--vmss-name", _knownVmssName,
            "--resource-group", _knownResourceGroup,
            "--subscription", _knownSubscription,
            "--force-deletion"
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);

        await _computeService.Received(1).DeleteVmssAsync(
            _knownVmssName,
            _knownResourceGroup,
            _knownSubscription,
            true,
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_VmssNotFound_ReturnsSuccess()
    {
        // Arrange - service returns false (VMSS was already gone / 404), but delete is idempotent
        _computeService.DeleteVmssAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<bool?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(false);

        var parseResult = _commandDefinition.Parse([
            "--vmss-name", _knownVmssName,
            "--resource-group", _knownResourceGroup,
            "--subscription", _knownSubscription
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

        // Assert - idempotent: 404 treated as success
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesForbiddenError()
    {
        // Arrange
        var forbiddenException = new RequestFailedException((int)HttpStatusCode.Forbidden, "Insufficient permissions");

        _computeService.DeleteVmssAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<bool?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(forbiddenException);

        var parseResult = _commandDefinition.Parse([
            "--vmss-name", _knownVmssName,
            "--resource-group", _knownResourceGroup,
            "--subscription", _knownSubscription
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.Status);
        Assert.Contains("Authorization failed", response.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesConflictError()
    {
        // Arrange
        var conflictException = new RequestFailedException((int)HttpStatusCode.Conflict, "VMSS in state that prevents deletion");

        _computeService.DeleteVmssAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<bool?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(conflictException);

        var parseResult = _commandDefinition.Parse([
            "--vmss-name", _knownVmssName,
            "--resource-group", _knownResourceGroup,
            "--subscription", _knownSubscription
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response.Status);
        Assert.Contains("--force-deletion", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_DeserializationValidation()
    {
        // Arrange
        _computeService.DeleteVmssAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<bool?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(true);

        var parseResult = _commandDefinition.Parse([
            "--vmss-name", _knownVmssName,
            "--resource-group", _knownResourceGroup,
            "--subscription", _knownSubscription
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response.Results);
        var json = JsonSerializer.Serialize(response.Results);

        var result = JsonSerializer.Deserialize(json, ComputeJsonContext.Default.VmssDeleteCommandResult);
        Assert.NotNull(result);
        Assert.True(result.Success);
    }

    [Fact]
    public void BindOptions_BindsOptionsCorrectly()
    {
        // Arrange
        var parseResult = _commandDefinition.Parse(
            $"--vmss-name {_knownVmssName} --resource-group {_knownResourceGroup} --subscription {_knownSubscription} --force-deletion");

        // Assert parse was successful
        Assert.Empty(parseResult.Errors);
    }
}
