// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.Compute.Commands;
using Azure.Mcp.Tools.Compute.Commands.Vmss;
using Azure.Mcp.Tools.Compute.Models;
using Azure.Mcp.Tools.Compute.Services;
using Microsoft.Mcp.Core.Options;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.Compute.UnitTests.Vmss;

public class VmssUpdateCommandTests : CommandUnitTestsBase<VmssUpdateCommand, IComputeService>
{
    private readonly string _knownSubscription = "sub123";
    private readonly string _knownResourceGroup = "test-rg";
    private readonly string _knownVmssName = "test-vmss";

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = Command.GetCommand();
        Assert.Equal("update", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
    }

    [Theory]
    [InlineData("--vmss-name test-vmss --resource-group test-rg --subscription sub123 --upgrade-policy Automatic", true)]
    [InlineData("--vmss-name test-vmss --resource-group test-rg --subscription sub123 --tags env=test", true)]
    [InlineData("--vmss-name test-vmss --resource-group test-rg --subscription sub123 --scale-in-policy OldestVM", true)]
    [InlineData("--vmss-name test-vmss --resource-group test-rg --subscription sub123", false)] // No update property
    [InlineData("--resource-group test-rg --subscription sub123 --tags env=test", false)] // Missing vmss-name
    [InlineData("--vmss-name test-vmss --subscription sub123 --tags env=test", false)] // Missing resource-group
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        // Arrange
        if (shouldSucceed)
        {
            var updateResult = new VmssUpdateResult(
                Name: _knownVmssName,
                Id: "/subscriptions/sub123/resourceGroups/test-rg/providers/Microsoft.Compute/virtualMachineScaleSets/test-vmss",
                Location: "eastus",
                VmSize: "Standard_D2s_v3",
                ProvisioningState: "Succeeded",
                Capacity: 5,
                UpgradePolicy: "Manual",
                Zones: null,
                Tags: null);

            Service.UpdateVmssAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string?>(),
                Arg.Any<int?>(),
                Arg.Any<string?>(),
                Arg.Any<bool?>(),
                Arg.Any<bool?>(),
                Arg.Any<string?>(),
                Arg.Any<string?>(),
                Arg.Any<string?>(),
                Arg.Any<RetryPolicyOptions?>(),
                Arg.Any<CancellationToken>())
                .Returns(updateResult);
        }

        // Act & Assert
        var response = await ExecuteCommandAsync(args);

        if (shouldSucceed)
        {
            Assert.Equal(HttpStatusCode.OK, response.Status);
            Assert.NotNull(response.Results);
            Assert.Equal("Success", response.Message);
        }
        else
        {
            Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        }
    }

    [Fact]
    public async Task ExecuteAsync_UpdatesVmssTags()
    {
        // Arrange
        var expectedResult = new VmssUpdateResult(
            Name: _knownVmssName,
            Id: "/subscriptions/sub123/resourceGroups/test-rg/providers/Microsoft.Compute/virtualMachineScaleSets/test-vmss",
            Location: "eastus",
            VmSize: "Standard_D2s_v3",
            ProvisioningState: "Succeeded",
            Capacity: 5,
            UpgradePolicy: "Manual",
            Zones: null,
            Tags: new Dictionary<string, string> { { "env", "prod" } });

        Service.UpdateVmssAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<int?>(),
            Arg.Any<string?>(),
            Arg.Any<bool?>(),
            Arg.Any<bool?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        var response = await ExecuteCommandAsync(
            "--vmss-name", _knownVmssName,
            "--resource-group", _knownResourceGroup,
            "--subscription", _knownSubscription,
            "--tags", "env=prod");

        // Assert
        var result = ValidateAndDeserializeResponse(response, ComputeJsonContext.Default.VmssUpdateCommandResult);
        Assert.NotNull(result.Vmss);
        Assert.Equal(_knownVmssName, result.Vmss.Name);
        Assert.NotNull(result.Vmss.Tags);
        Assert.Equal("prod", result.Vmss.Tags["env"]);
    }

    [Fact]
    public async Task ExecuteAsync_UpdatesVmssUpgradePolicy()
    {
        // Arrange
        var expectedResult = new VmssUpdateResult(
            Name: _knownVmssName,
            Id: "/subscriptions/sub123/resourceGroups/test-rg/providers/Microsoft.Compute/virtualMachineScaleSets/test-vmss",
            Location: "eastus",
            VmSize: "Standard_D2s_v3",
            ProvisioningState: "Succeeded",
            Capacity: 5,
            UpgradePolicy: "Automatic",
            Zones: null,
            Tags: null);

        Service.UpdateVmssAsync(
            Arg.Is(_knownVmssName),
            Arg.Is(_knownResourceGroup),
            Arg.Is(_knownSubscription),
            Arg.Any<string?>(),
            Arg.Any<int?>(),
            Arg.Is("Automatic"),
            Arg.Any<bool?>(),
            Arg.Any<bool?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        var response = await ExecuteCommandAsync(
            "--vmss-name", _knownVmssName,
            "--resource-group", _knownResourceGroup,
            "--subscription", _knownSubscription,
            "--upgrade-policy", "Automatic");

        // Assert
        var result = ValidateAndDeserializeResponse(response, ComputeJsonContext.Default.VmssUpdateCommandResult);
        Assert.NotNull(result.Vmss);
        Assert.Equal("Automatic", result.Vmss.UpgradePolicy);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesNotFoundError()
    {
        // Arrange
        var notFoundException = new RequestFailedException((int)HttpStatusCode.NotFound, "VMSS not found");

        Service.UpdateVmssAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<int?>(),
            Arg.Any<string?>(),
            Arg.Any<bool?>(),
            Arg.Any<bool?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(notFoundException);

        // Act
        var response = await ExecuteCommandAsync(
            "--vmss-name", _knownVmssName,
            "--resource-group", _knownResourceGroup,
            "--subscription", _knownSubscription,
            "--tags", "env=test");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.Status);
        Assert.Contains("not found", response.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesQuotaExceeded()
    {
        // Arrange
        var quotaException = new RequestFailedException((int)HttpStatusCode.BadRequest, "Quota exceeded for VM size in region");

        Service.UpdateVmssAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<int?>(),
            Arg.Any<string?>(),
            Arg.Any<bool?>(),
            Arg.Any<bool?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(quotaException);

        // Act
        var response = await ExecuteCommandAsync(
            "--vmss-name", _knownVmssName,
            "--resource-group", _knownResourceGroup,
            "--subscription", _knownSubscription,
            "--vm-size", "Standard_D4s_v3");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("quota", response.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_DeserializationValidation()
    {
        // Arrange
        var expectedResult = new VmssUpdateResult(
            Name: _knownVmssName,
            Id: "/subscriptions/sub123/resourceGroups/test-rg/providers/Microsoft.Compute/virtualMachineScaleSets/test-vmss",
            Location: "eastus",
            VmSize: "Standard_D2s_v3",
            ProvisioningState: "Succeeded",
            Capacity: 5,
            UpgradePolicy: "Manual",
            Zones: ["1"],
            Tags: new Dictionary<string, string> { { "env", "test" } });

        Service.UpdateVmssAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<int?>(),
            Arg.Any<string?>(),
            Arg.Any<bool?>(),
            Arg.Any<bool?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        var response = await ExecuteCommandAsync(
            "--vmss-name", _knownVmssName,
            "--resource-group", _knownResourceGroup,
            "--subscription", _knownSubscription,
            "--tags", "env=test");

        // Assert
        var result = ValidateAndDeserializeResponse(response, ComputeJsonContext.Default.VmssUpdateCommandResult);
        Assert.NotNull(result.Vmss);
        Assert.Equal(_knownVmssName, result.Vmss.Name);
    }

    [Fact]
    public void BindOptions_BindsOptionsCorrectly()
    {
        // Arrange
        var parseResult = CommandDefinition.Parse(
            $"--vmss-name {_knownVmssName} --resource-group {_knownResourceGroup} --subscription {_knownSubscription} --capacity 5 --upgrade-policy Automatic --scale-in-policy OldestVM --tags env=test");

        // Assert parse was successful
        Assert.Empty(parseResult.Errors);
    }
}
