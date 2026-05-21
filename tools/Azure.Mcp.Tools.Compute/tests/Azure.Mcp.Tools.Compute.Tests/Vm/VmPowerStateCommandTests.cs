// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.Compute.Commands;
using Azure.Mcp.Tools.Compute.Commands.Vm;
using Azure.Mcp.Tools.Compute.Models;
using Azure.Mcp.Tools.Compute.Services;
using Microsoft.Mcp.Core.Options;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.Compute.Tests.Vm;

public class VmPowerStateCommandTests : CommandUnitTestsBase<VmPowerStateCommand, IComputeService>
{
    private readonly string _knownSubscription = "sub123";
    private readonly string _knownResourceGroup = "test-rg";
    private readonly string _knownVmName = "test-vm";

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = Command.GetCommand();
        Assert.Equal("power-state", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
    }

    [Theory]
    [InlineData("--vm-name test-vm --resource-group test-rg --subscription sub123 --power-action start", true)]
    [InlineData("--vm-name test-vm --resource-group test-rg --subscription sub123 --power-action stop", true)]
    [InlineData("--vm-name test-vm --resource-group test-rg --subscription sub123 --power-action deallocate", true)]
    [InlineData("--vm-name test-vm --resource-group test-rg --subscription sub123 --power-action restart", true)]
    [InlineData("--vm-name test-vm --resource-group test-rg --subscription sub123 --power-action stop --skip-shutdown", true)]
    [InlineData("--vm-name test-vm --resource-group test-rg --subscription sub123 --power-action stop --no-wait", true)]
    [InlineData("--resource-group test-rg --subscription sub123 --power-action start", false)] // Missing vm-name
    [InlineData("--vm-name test-vm --subscription sub123 --power-action start", false)] // Missing resource-group
    [InlineData("--vm-name test-vm --resource-group test-rg --subscription sub123", false)] // Missing power-action
    [InlineData("--vm-name test-vm --resource-group test-rg --subscription sub123 --power-action invalid", false)] // Invalid action
    [InlineData("--vm-name test-vm --resource-group test-rg --subscription sub123 --power-action start --skip-shutdown", false)] // skip-shutdown with non-stop action
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        // Arrange
        if (shouldSucceed)
        {
            Service.ChangeVmPowerStateAsync(
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

        // Act & Assert
        var response = await ExecuteCommandAsync(args);

        if (shouldSucceed)
        {
            Assert.Equal(HttpStatusCode.OK, response.Status);
            Assert.NotNull(response.Results);
        }
        else
        {
            Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        }
    }

    [Theory]
    [InlineData("start")]
    [InlineData("stop")]
    [InlineData("deallocate")]
    [InlineData("restart")]
    public async Task ExecuteAsync_ChangesVmPowerState(string powerAction)
    {
        // Arrange
        var expectedResult = new VmPowerStateResult(
            _knownVmName, $"/subscriptions/{_knownSubscription}/resourceGroups/{_knownResourceGroup}/providers/Microsoft.Compute/virtualMachines/{_knownVmName}", _knownResourceGroup,
            powerAction, $"Virtual machine '{_knownVmName}' {powerAction} operation completed successfully.", true);

        Service.ChangeVmPowerStateAsync(
            Arg.Is(_knownVmName),
            Arg.Is(_knownResourceGroup),
            Arg.Is(_knownSubscription),
            Arg.Is(powerAction),
            Arg.Any<bool>(),
            Arg.Any<bool>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        var response = await ExecuteCommandAsync(
            "--vm-name", _knownVmName,
            "--resource-group", _knownResourceGroup,
            "--subscription", _knownSubscription,
            "--power-action", powerAction);

        var result = ValidateAndDeserializeResponse(response, ComputeJsonContext.Default.VmPowerStateCommandResult);
        Assert.Equal(_knownVmName, result.PowerState.Name);
        Assert.True(result.PowerState.Completed);
    }

    [Fact]
    public async Task ExecuteAsync_WithNoWait_PassesNoWaitToService()
    {
        // Arrange
        var expectedResult = new VmPowerStateResult(
            _knownVmName, $"/subscriptions/{_knownSubscription}/resourceGroups/{_knownResourceGroup}/providers/Microsoft.Compute/virtualMachines/{_knownVmName}", _knownResourceGroup,
            "start", $"Virtual machine '{_knownVmName}' start operation initiated. Use instance view to check status.", false);

        Service.ChangeVmPowerStateAsync(
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

        // Act
        var response = await ExecuteCommandAsync(
            "--vm-name", _knownVmName,
            "--resource-group", _knownResourceGroup,
            "--subscription", _knownSubscription,
            "--power-action", "start",
            "--no-wait");

        // Assert
        var result = ValidateAndDeserializeResponse(response, ComputeJsonContext.Default.VmPowerStateCommandResult);
        Assert.False(result.PowerState.Completed);

        await Service.Received(1).ChangeVmPowerStateAsync(
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
            _knownVmName, $"/subscriptions/{_knownSubscription}/resourceGroups/{_knownResourceGroup}/providers/Microsoft.Compute/virtualMachines/{_knownVmName}", _knownResourceGroup,
            "stop", $"Virtual machine '{_knownVmName}' stop operation completed successfully.", true);

        Service.ChangeVmPowerStateAsync(
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

        // Act
        var response = await ExecuteCommandAsync(
            "--vm-name", _knownVmName,
            "--resource-group", _knownResourceGroup,
            "--subscription", _knownSubscription,
            "--power-action", "stop",
            "--skip-shutdown");

        // Assert
        var result = ValidateAndDeserializeResponse(response, ComputeJsonContext.Default.VmPowerStateCommandResult);

        await Service.Received(1).ChangeVmPowerStateAsync(
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

        Service.ChangeVmPowerStateAsync(
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

        //Act
        var response = await ExecuteCommandAsync(
            "--vm-name", _knownVmName,
            "--resource-group", _knownResourceGroup,
            "--subscription", _knownSubscription,
            "--power-action", "start");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.Status);
        Assert.Contains("not found", response.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesForbiddenError()
    {
        // Arrange
        var forbiddenException = new RequestFailedException((int)HttpStatusCode.Forbidden, "Insufficient permissions");

        Service.ChangeVmPowerStateAsync(
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

        // Act
        var response = await ExecuteCommandAsync(
            "--vm-name", _knownVmName,
            "--resource-group", _knownResourceGroup,
            "--subscription", _knownSubscription,
            "--power-action", "start");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.Status);
        Assert.Contains("Authorization failed", response.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesConflictError()
    {
        // Arrange
        var conflictException = new RequestFailedException((int)HttpStatusCode.Conflict, "VM in conflicting state");

        Service.ChangeVmPowerStateAsync(
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

        var response = await ExecuteCommandAsync(
            "--vm-name", _knownVmName,
            "--resource-group", _knownResourceGroup,
            "--subscription", _knownSubscription,
            "--power-action", "restart");

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response.Status);
        Assert.Contains("conflict", response.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_DeserializationValidation()
    {
        // Arrange
        var expectedResult = new VmPowerStateResult(
            _knownVmName, $"/subscriptions/{_knownSubscription}/resourceGroups/{_knownResourceGroup}/providers/Microsoft.Compute/virtualMachines/{_knownVmName}", _knownResourceGroup,
            "start", $"Virtual machine '{_knownVmName}' start operation completed successfully.", true);

        Service.ChangeVmPowerStateAsync(
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

        // Act
        var response = await ExecuteCommandAsync(
            "--vm-name", _knownVmName,
            "--resource-group", _knownResourceGroup,
            "--subscription", _knownSubscription,
            "--power-action", "start");

        // Assert
        var result = ValidateAndDeserializeResponse(response, ComputeJsonContext.Default.VmPowerStateCommandResult);
        Assert.Equal(_knownVmName, result.PowerState.Name);
        Assert.Equal(_knownResourceGroup, result.PowerState.ResourceGroup);
        Assert.True(result.PowerState.Completed);
    }

    [Fact]
    public void BindOptions_BindsOptionsCorrectly()
    {
        // Arrange
        var parseResult = CommandDefinition.Parse(
            $"--vm-name {_knownVmName} --resource-group {_knownResourceGroup} --subscription {_knownSubscription} --power-action stop --no-wait --skip-shutdown");

        // Assert parse was successful
        Assert.Empty(parseResult.Errors);
    }
}
