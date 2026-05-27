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

public class VmUpdateCommandTests : CommandUnitTestsBase<VmUpdateCommand, IComputeService>
{
    private readonly string _knownSubscription = "sub123";
    private readonly string _knownResourceGroup = "test-rg";
    private readonly string _knownVmName = "test-vm";

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = Command.GetCommand();
        Assert.Equal("update", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
    }

    [Theory]
    [InlineData("--vm-name test-vm --resource-group test-rg --subscription sub123 --tags env=test", true)]
    [InlineData("--vm-name test-vm --resource-group test-rg --subscription sub123 --boot-diagnostics true", true)]
    [InlineData("--vm-name test-vm --resource-group test-rg --subscription sub123 --license-type Windows_Server", true)]
    [InlineData("--vm-name test-vm --resource-group test-rg --subscription sub123 --vm-size Standard_D4s_v3", true)]
    [InlineData("--vm-name test-vm --resource-group test-rg --subscription sub123", false)] // No update property
    [InlineData("--resource-group test-rg --subscription sub123 --tags env=test", false)] // Missing vm-name
    [InlineData("--vm-name test-vm --subscription sub123 --tags env=test", false)] // Missing resource-group
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        // Arrange
        if (shouldSucceed)
        {
            var updateResult = new VmUpdateResult(
                Name: _knownVmName,
                Id: "/subscriptions/sub123/resourceGroups/test-rg/providers/Microsoft.Compute/virtualMachines/test-vm",
                Location: "eastus",
                VmSize: "Standard_D2s_v3",
                ProvisioningState: "Succeeded",
                PowerState: "VM running",
                OsType: "linux",
                LicenseType: null,
                Zones: null,
                Tags: new Dictionary<string, string> { { "env", "test" } });

            Service.UpdateVmAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string?>(),
                Arg.Any<string?>(),
                Arg.Any<string?>(),
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
    public async Task ExecuteAsync_UpdatesVmWithTags()
    {
        // Arrange
        var expectedResult = new VmUpdateResult(
            Name: _knownVmName,
            Id: "/subscriptions/sub123/resourceGroups/test-rg/providers/Microsoft.Compute/virtualMachines/test-vm",
            Location: "eastus",
            VmSize: "Standard_D2s_v3",
            ProvisioningState: "Succeeded",
            PowerState: "VM running",
            OsType: "linux",
            LicenseType: null,
            Zones: null,
            Tags: new Dictionary<string, string> { { "env", "prod" }, { "team", "compute" } });

        Service.UpdateVmAsync(
            Arg.Is(_knownVmName),
            Arg.Is(_knownResourceGroup),
            Arg.Is(_knownSubscription),
            Arg.Any<string?>(),
            Arg.Is("env=prod,team=compute"),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        var response = await ExecuteCommandAsync(
            "--vm-name", _knownVmName,
            "--resource-group", _knownResourceGroup,
            "--subscription", _knownSubscription,
            "--tags", "env=prod,team=compute");

        // Assert
        var result = ValidateAndDeserializeResponse(response, ComputeJsonContext.Default.VmUpdateCommandResult);
        Assert.NotNull(result.Vm);
        Assert.Equal(_knownVmName, result.Vm.Name);
        Assert.NotNull(result.Vm.Tags);
        Assert.Equal(2, result.Vm.Tags.Count);
    }

    [Fact]
    public async Task ExecuteAsync_UpdatesVmWithLicenseType()
    {
        // Arrange
        var expectedResult = new VmUpdateResult(
            Name: _knownVmName,
            Id: "/subscriptions/sub123/resourceGroups/test-rg/providers/Microsoft.Compute/virtualMachines/test-vm",
            Location: "eastus",
            VmSize: "Standard_D2s_v3",
            ProvisioningState: "Succeeded",
            PowerState: "VM running",
            OsType: "windows",
            LicenseType: "Windows_Server",
            Zones: null,
            Tags: null);

        Service.UpdateVmAsync(
            Arg.Is(_knownVmName),
            Arg.Is(_knownResourceGroup),
            Arg.Is(_knownSubscription),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Is("Windows_Server"),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        var response = await ExecuteCommandAsync(
            "--vm-name", _knownVmName,
            "--resource-group", _knownResourceGroup,
            "--subscription", _knownSubscription,
            "--license-type", "Windows_Server");

        // Assert
        var result = ValidateAndDeserializeResponse(response, ComputeJsonContext.Default.VmUpdateCommandResult);
        Assert.NotNull(result.Vm);
        Assert.Equal("Windows_Server", result.Vm.LicenseType);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesNotFoundError()
    {
        // Arrange
        var notFoundException = new RequestFailedException((int)HttpStatusCode.NotFound, "VM not found");

        Service.UpdateVmAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(notFoundException);

        // Act
        var response = await ExecuteCommandAsync(
            "--vm-name", _knownVmName,
            "--resource-group", _knownResourceGroup,
            "--subscription", _knownSubscription,
            "--tags", "env=test");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.Status);
        Assert.Contains("not found", response.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesConflictError()
    {
        // Arrange
        var conflictException = new RequestFailedException((int)HttpStatusCode.Conflict, "VM must be deallocated to change size");

        Service.UpdateVmAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(conflictException);

        // Act
        var response = await ExecuteCommandAsync(
            "--vm-name", _knownVmName,
            "--resource-group", _knownResourceGroup,
            "--subscription", _knownSubscription,
            "--vm-size", "Standard_D4s_v3");

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response.Status);
        Assert.Contains("deallocated", response.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_DeserializationValidation()
    {
        // Arrange
        var expectedResult = new VmUpdateResult(
            Name: _knownVmName,
            Id: "/subscriptions/sub123/resourceGroups/test-rg/providers/Microsoft.Compute/virtualMachines/test-vm",
            Location: "eastus",
            VmSize: "Standard_D2s_v3",
            ProvisioningState: "Succeeded",
            PowerState: "VM running",
            OsType: "linux",
            LicenseType: null,
            Zones: ["1"],
            Tags: new Dictionary<string, string> { { "env", "test" } });

        Service.UpdateVmAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        var response = await ExecuteCommandAsync(
            "--vm-name", _knownVmName,
            "--resource-group", _knownResourceGroup,
            "--subscription", _knownSubscription,
            "--tags", "env=test");

        // Assert
        var result = ValidateAndDeserializeResponse(response, ComputeJsonContext.Default.VmUpdateCommandResult);
        Assert.NotNull(result.Vm);
        Assert.Equal(_knownVmName, result.Vm.Name);
    }

    [Fact]
    public void BindOptions_BindsOptionsCorrectly()
    {
        // Arrange
        var parseResult = CommandDefinition.Parse(
            $"--vm-name {_knownVmName} --resource-group {_knownResourceGroup} --subscription {_knownSubscription} --vm-size Standard_D4s_v3 --tags env=test --license-type Windows_Server --boot-diagnostics true --user-data dGVzdA==");

        // Assert parse was successful
        Assert.Empty(parseResult.Errors);
    }

    [Fact]
    public async Task ExecuteAsync_UpdatesVmWithUserData_PassesValueToService()
    {
        // The command passes the user-supplied value (expected to be Base64-encoded by the caller)
        // directly to the service without modification (COMP-03: ARM requires Base64).
        const string base64UserData = "IyEvYmluL2Jhc2gKZWNobyBoZWxsbw=="; // base64 of "#!/bin/bash\necho hello"

        var expectedResult = new VmUpdateResult(
            Name: _knownVmName,
            Id: "/subscriptions/sub123/resourceGroups/test-rg/providers/Microsoft.Compute/virtualMachines/test-vm",
            Location: "eastus",
            VmSize: "Standard_D2s_v5",
            ProvisioningState: "Succeeded",
            PowerState: "VM running",
            OsType: "linux",
            LicenseType: null,
            Zones: null,
            Tags: null);

        Service.UpdateVmAsync(
            Arg.Is(_knownVmName),
            Arg.Is(_knownResourceGroup),
            Arg.Is(_knownSubscription),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Is(base64UserData),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        var response = await ExecuteCommandAsync(
            "--vm-name", _knownVmName,
            "--resource-group", _knownResourceGroup,
            "--subscription", _knownSubscription,
            "--user-data", base64UserData);

        // Assert - command passes the value through as-is; caller is responsible for Base64 encoding
        Assert.Equal(HttpStatusCode.OK, response.Status);
        await Service.Received(1).UpdateVmAsync(
            _knownVmName, _knownResourceGroup, _knownSubscription,
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(),
            Arg.Any<string?>(), base64UserData, Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_ClearTagsWithEmptyString_PassesEmptyTagsToService()
    {
        // COMP-04: passing --tags "" should clear existing tags via the service
        var expectedResult = new VmUpdateResult(
            Name: _knownVmName,
            Id: "/subscriptions/sub123/resourceGroups/test-rg/providers/Microsoft.Compute/virtualMachines/test-vm",
            Location: "eastus",
            VmSize: "Standard_D2s_v5",
            ProvisioningState: "Succeeded",
            PowerState: "VM running",
            OsType: "linux",
            LicenseType: null,
            Zones: null,
            Tags: new Dictionary<string, string>());

        Service.UpdateVmAsync(
            Arg.Is(_knownVmName),
            Arg.Is(_knownResourceGroup),
            Arg.Is(_knownSubscription),
            Arg.Any<string?>(),
            Arg.Is(string.Empty),  // empty string triggers tag clearing in service
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        var response = await ExecuteCommandAsync(
            "--vm-name", _knownVmName,
            "--resource-group", _knownResourceGroup,
            "--subscription", _knownSubscription,
            "--tags", "");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        await Service.Received(1).UpdateVmAsync(
            _knownVmName,
            _knownResourceGroup,
            _knownSubscription,
            Arg.Any<string?>(),
            string.Empty,
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_ClearTagsWithBareOption_PassesEmptyTagsToService()
    {
        var expectedResult = new VmUpdateResult(
            Name: _knownVmName,
            Id: "/subscriptions/sub123/resourceGroups/test-rg/providers/Microsoft.Compute/virtualMachines/test-vm",
            Location: "eastus",
            VmSize: "Standard_D2s_v5",
            ProvisioningState: "Succeeded",
            PowerState: "VM running",
            OsType: "linux",
            LicenseType: null,
            Zones: null,
            Tags: new Dictionary<string, string>());

        Service.UpdateVmAsync(
            Arg.Is(_knownVmName),
            Arg.Is(_knownResourceGroup),
            Arg.Is(_knownSubscription),
            Arg.Any<string?>(),
            Arg.Is(string.Empty),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        var response = await ExecuteCommandAsync(
            "--vm-name", _knownVmName,
            "--resource-group", _knownResourceGroup,
            "--subscription", _knownSubscription,
            "--tags");

        Assert.Equal(HttpStatusCode.OK, response.Status);
        await Service.Received(1).UpdateVmAsync(
            _knownVmName,
            _knownResourceGroup,
            _knownSubscription,
            Arg.Any<string?>(),
            string.Empty,
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>());
    }
}
