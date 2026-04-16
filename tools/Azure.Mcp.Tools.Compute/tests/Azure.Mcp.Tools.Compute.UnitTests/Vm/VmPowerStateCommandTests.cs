// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Net;
using System.Text.Json;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.Compute.Commands;
using Azure.Mcp.Tools.Compute.Commands.Vm;
using Azure.Mcp.Tools.Compute.Models;
using Azure.Mcp.Tools.Compute.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Models.Command;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.Compute.UnitTests.Vm;

public class VmPowerStateCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IComputeService _computeService;
    private readonly ILogger<VmPowerStateCommand> _logger;
    private readonly VmPowerStateCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;
    private readonly string _knownSubscription = "sub123";
    private readonly string _knownResourceGroup = "test-rg";
    private readonly string _knownVmName = "test-vm";

    public VmPowerStateCommandTests()
    {
        _computeService = Substitute.For<IComputeService>();
        _logger = Substitute.For<ILogger<VmPowerStateCommand>>();

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
        Assert.Equal("power-state", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
    }

    [Theory]
    [InlineData("--vm-name test-vm --resource-group test-rg --subscription sub123 --state start", true)]
    [InlineData("--vm-name test-vm --resource-group test-rg --subscription sub123 --state stop", true)]
    [InlineData("--vm-name test-vm --resource-group test-rg --subscription sub123 --state deallocate", true)]
    [InlineData("--vm-name test-vm --resource-group test-rg --subscription sub123 --state restart", true)]
    [InlineData("--vm-name test-vm --resource-group test-rg --subscription sub123 --state stop --skip-shutdown", true)]
    [InlineData("--vm-name test-vm --resource-group test-rg --subscription sub123 --state stop --no-wait", true)]
    [InlineData("--resource-group test-rg --subscription sub123 --state start", false)] // Missing vm-name
    [InlineData("--vm-name test-vm --subscription sub123 --state start", false)] // Missing resource-group
    [InlineData("--vm-name test-vm --resource-group test-rg --subscription sub123", false)] // Missing state
    [InlineData("--vm-name test-vm --resource-group test-rg --subscription sub123 --state invalid", false)] // Invalid state
    [InlineData("--vm-name test-vm --resource-group test-rg --subscription sub123 --state start --skip-shutdown", false)] // skip-shutdown with non-stop state
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        // Arrange
        if (shouldSucceed)
        {
            _computeService.ChangeVmPowerStateAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<bool>(),
                Arg.Any<bool>(),
                Arg.Any<string?>(),
                Arg.Any<RetryPolicyOptions?>(),
                Arg.Any<CancellationToken>())
                .Returns(new VmPowerStateResult("test-vm", null, "test-rg", "start", "Operation completed.", true));
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

    [Theory]
    [InlineData("start")]
    [InlineData("stop")]
    [InlineData("deallocate")]
    [InlineData("restart")]
    public async Task ExecuteAsync_ChangesVmPowerState(string state)
    {
        // Arrange
        var expectedResult = new VmPowerStateResult(
            _knownVmName, $"/subscriptions/{_knownSubscription}/resourceGroups/{_knownResourceGroup}/providers/Microsoft.Compute/virtualMachines/{_knownVmName}", _knownResourceGroup, state,
            $"Virtual machine '{_knownVmName}' {state} operation completed successfully.", true);

        _computeService.ChangeVmPowerStateAsync(
            Arg.Is(_knownVmName),
            Arg.Is(_knownResourceGroup),
            Arg.Is(_knownSubscription),
            Arg.Is(state),
            Arg.Any<bool>(),
            Arg.Any<bool>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        var parseResult = _commandDefinition.Parse([
            "--vm-name", _knownVmName,
            "--resource-group", _knownResourceGroup,
            "--subscription", _knownSubscription,
            "--state", state
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, ComputeJsonContext.Default.VmPowerStateCommandResult);

        Assert.NotNull(result);
        Assert.Equal(_knownVmName, result.Result.Name);
        Assert.Equal(state, result.Result.State);
        Assert.True(result.Result.Completed);
    }

    [Fact]
    public async Task ExecuteAsync_WithNoWait_PassesNoWaitToService()
    {
        // Arrange
        var expectedResult = new VmPowerStateResult(
            _knownVmName, $"/subscriptions/{_knownSubscription}/resourceGroups/{_knownResourceGroup}/providers/Microsoft.Compute/virtualMachines/{_knownVmName}", _knownResourceGroup, "start",
            $"Virtual machine '{_knownVmName}' start operation initiated. Use instance view to check status.", false);

        _computeService.ChangeVmPowerStateAsync(
            Arg.Is(_knownVmName),
            Arg.Is(_knownResourceGroup),
            Arg.Is(_knownSubscription),
            Arg.Is("start"),
            Arg.Is(true),
            Arg.Any<bool>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        var parseResult = _commandDefinition.Parse([
            "--vm-name", _knownVmName,
            "--resource-group", _knownResourceGroup,
            "--subscription", _knownSubscription,
            "--state", "start",
            "--no-wait"
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, ComputeJsonContext.Default.VmPowerStateCommandResult);

        Assert.NotNull(result);
        Assert.False(result.Result.Completed);

        await _computeService.Received(1).ChangeVmPowerStateAsync(
            _knownVmName,
            _knownResourceGroup,
            _knownSubscription,
            "start",
            true,
            Arg.Any<bool>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_StopWithSkipShutdown_PassesSkipShutdownToService()
    {
        // Arrange
        var expectedResult = new VmPowerStateResult(
            _knownVmName, $"/subscriptions/{_knownSubscription}/resourceGroups/{_knownResourceGroup}/providers/Microsoft.Compute/virtualMachines/{_knownVmName}", _knownResourceGroup, "stop",
            $"Virtual machine '{_knownVmName}' stop operation completed successfully.", true);

        _computeService.ChangeVmPowerStateAsync(
            Arg.Is(_knownVmName),
            Arg.Is(_knownResourceGroup),
            Arg.Is(_knownSubscription),
            Arg.Is("stop"),
            Arg.Any<bool>(),
            Arg.Is(true),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        var parseResult = _commandDefinition.Parse([
            "--vm-name", _knownVmName,
            "--resource-group", _knownResourceGroup,
            "--subscription", _knownSubscription,
            "--state", "stop",
            "--skip-shutdown"
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);

        await _computeService.Received(1).ChangeVmPowerStateAsync(
            _knownVmName,
            _knownResourceGroup,
            _knownSubscription,
            "stop",
            Arg.Any<bool>(),
            true,
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_HandlesNotFoundError()
    {
        // Arrange
        var notFoundException = new RequestFailedException((int)HttpStatusCode.NotFound, "VM not found");

        _computeService.ChangeVmPowerStateAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<bool>(),
            Arg.Any<bool>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(notFoundException);

        var parseResult = _commandDefinition.Parse([
            "--vm-name", _knownVmName,
            "--resource-group", _knownResourceGroup,
            "--subscription", _knownSubscription,
            "--state", "start"
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.Status);
        Assert.Contains("not found", response.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesForbiddenError()
    {
        // Arrange
        var forbiddenException = new RequestFailedException((int)HttpStatusCode.Forbidden, "Insufficient permissions");

        _computeService.ChangeVmPowerStateAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<bool>(),
            Arg.Any<bool>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(forbiddenException);

        var parseResult = _commandDefinition.Parse([
            "--vm-name", _knownVmName,
            "--resource-group", _knownResourceGroup,
            "--subscription", _knownSubscription,
            "--state", "start"
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
        var conflictException = new RequestFailedException((int)HttpStatusCode.Conflict, "VM in conflicting state");

        _computeService.ChangeVmPowerStateAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<bool>(),
            Arg.Any<bool>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(conflictException);

        var parseResult = _commandDefinition.Parse([
            "--vm-name", _knownVmName,
            "--resource-group", _knownResourceGroup,
            "--subscription", _knownSubscription,
            "--state", "restart"
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response.Status);
        Assert.Contains("conflict", response.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_DeserializationValidation()
    {
        // Arrange
        var expectedResult = new VmPowerStateResult(
            _knownVmName, $"/subscriptions/{_knownSubscription}/resourceGroups/{_knownResourceGroup}/providers/Microsoft.Compute/virtualMachines/{_knownVmName}", _knownResourceGroup, "start",
            $"Virtual machine '{_knownVmName}' start operation completed successfully.", true);

        _computeService.ChangeVmPowerStateAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<bool>(),
            Arg.Any<bool>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        var parseResult = _commandDefinition.Parse([
            "--vm-name", _knownVmName,
            "--resource-group", _knownResourceGroup,
            "--subscription", _knownSubscription,
            "--state", "start"
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response.Results);
        var json = JsonSerializer.Serialize(response.Results);

        var result = JsonSerializer.Deserialize(json, ComputeJsonContext.Default.VmPowerStateCommandResult);
        Assert.NotNull(result);
        Assert.Equal(_knownVmName, result.Result.Name);
        Assert.Equal(_knownResourceGroup, result.Result.ResourceGroup);
        Assert.Equal("start", result.Result.State);
        Assert.True(result.Result.Completed);
    }

    [Fact]
    public void BindOptions_BindsOptionsCorrectly()
    {
        // Arrange
        var parseResult = _commandDefinition.Parse(
            $"--vm-name {_knownVmName} --resource-group {_knownResourceGroup} --subscription {_knownSubscription} --state stop --no-wait --skip-shutdown");

        // Assert parse was successful
        Assert.Empty(parseResult.Errors);
    }
}
