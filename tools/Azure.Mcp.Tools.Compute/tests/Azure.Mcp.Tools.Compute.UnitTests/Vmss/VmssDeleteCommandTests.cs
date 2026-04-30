// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.Compute.Commands;
using Azure.Mcp.Tools.Compute.Commands.Vmss;
using Azure.Mcp.Tools.Compute.Services;
using Microsoft.Mcp.Core.Options;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.Compute.UnitTests.Vmss;

public class VmssDeleteCommandTests : CommandUnitTestsBase<VmssDeleteCommand, IComputeService>
{
    private readonly string _knownSubscription = "sub123";
    private readonly string _knownResourceGroup = "test-rg";
    private readonly string _knownVmssName = "test-vmss";

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = Command.GetCommand();
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
            Service.DeleteVmssAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<bool?>(),
                Arg.Any<string?>(),
                Arg.Any<RetryPolicyOptions?>(),
                Arg.Any<CancellationToken>())
                .Returns(true);
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

    [Fact]
    public async Task ExecuteAsync_DeletesVmss()
    {
        // Arrange
        Service.DeleteVmssAsync(
            Arg.Is(_knownVmssName),
            Arg.Is(_knownResourceGroup),
            Arg.Is(_knownSubscription),
            Arg.Any<bool?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        var response = await ExecuteCommandAsync(
            "--vmss-name", _knownVmssName,
            "--resource-group", _knownResourceGroup,
            "--subscription", _knownSubscription);

        // Assert
        var result = ValidateAndDeserializeResponse(response, ComputeJsonContext.Default.VmssDeleteCommandResult);
        Assert.True(result.Success);
        Assert.Contains("successfully deleted", result.Message);
        Assert.Contains(_knownVmssName, result.Message);
    }

    [Fact]
    public async Task ExecuteAsync_WithForceDeletion_PassesForceDeletionToService()
    {
        // Arrange
        Service.DeleteVmssAsync(
            Arg.Is(_knownVmssName),
            Arg.Is(_knownResourceGroup),
            Arg.Is(_knownSubscription),
            Arg.Is<bool?>(true),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        var response = await ExecuteCommandAsync(
            "--vmss-name", _knownVmssName,
            "--resource-group", _knownResourceGroup,
            "--subscription", _knownSubscription,
            "--force-deletion");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);

        await Service.Received(1).DeleteVmssAsync(
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
        Service.DeleteVmssAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<bool?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        var response = await ExecuteCommandAsync(
            "--vmss-name", _knownVmssName,
            "--resource-group", _knownResourceGroup,
            "--subscription", _knownSubscription);

        // Assert - idempotent: 404 treated as success
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesForbiddenError()
    {
        // Arrange
        var forbiddenException = new RequestFailedException((int)HttpStatusCode.Forbidden, "Insufficient permissions");

        Service.DeleteVmssAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<bool?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(forbiddenException);

        // Act
        var response = await ExecuteCommandAsync(
            "--vmss-name", _knownVmssName,
            "--resource-group", _knownResourceGroup,
            "--subscription", _knownSubscription);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.Status);
        Assert.Contains("Authorization failed", response.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesConflictError()
    {
        // Arrange
        var conflictException = new RequestFailedException((int)HttpStatusCode.Conflict, "VMSS in state that prevents deletion");

        Service.DeleteVmssAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<bool?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(conflictException);

        // Act
        var response = await ExecuteCommandAsync(
            "--vmss-name", _knownVmssName,
            "--resource-group", _knownResourceGroup,
            "--subscription", _knownSubscription);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response.Status);
        Assert.Contains("--force-deletion", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_DeserializationValidation()
    {
        // Arrange
        Service.DeleteVmssAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<bool?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        var response = await ExecuteCommandAsync(
            "--vmss-name", _knownVmssName,
            "--resource-group", _knownResourceGroup,
            "--subscription", _knownSubscription);

        // Assert
        var result = ValidateAndDeserializeResponse(response, ComputeJsonContext.Default.VmssDeleteCommandResult);
        Assert.True(result.Success);
    }

    [Fact]
    public void BindOptions_BindsOptionsCorrectly()
    {
        // Arrange
        var parseResult = CommandDefinition.Parse(
            $"--vmss-name {_knownVmssName} --resource-group {_knownResourceGroup} --subscription {_knownSubscription} --force-deletion");

        // Assert parse was successful
        Assert.Empty(parseResult.Errors);
    }
}
