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

namespace Azure.Mcp.Tools.Compute.UnitTests.Vm;

public class VmCreateCommandTests : CommandUnitTestsBase<VmCreateCommand, IComputeService>
{
    private readonly string _knownSubscription = "sub123";
    private readonly string _knownResourceGroup = "test-rg";
    private readonly string _knownVmName = "test-vm";
    private readonly string _knownLocation = "eastus";
    private readonly string _knownAdminUsername = "azureuser";
    private readonly string _knownPassword = "TestPassword123!";
    private readonly string _knownSshKey = "ssh-rsa AAAAB3NzaC1yc2EAAAADAQABAAABgQC...";

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = Command.GetCommand();
        Assert.Equal("create", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
    }

    [Theory]
    [InlineData("--vm-name test-vm --resource-group test-rg --subscription sub123 --location eastus --admin-username azureuser --admin-password TestPassword123!", true)] // All required + password
    [InlineData("--vm-name test-vm --resource-group test-rg --subscription sub123 --location eastus --admin-username azureuser --ssh-public-key ssh-rsa-key", true)] // All required + ssh key
    [InlineData("--vm-name test-vm --resource-group test-rg --subscription sub123 --location eastus --admin-username azureuser", false)] // Missing auth - Linux requires SSH key or password
    [InlineData("--resource-group test-rg --subscription sub123 --location eastus --admin-username azureuser --admin-password TestPassword123!", false)] // Missing vm-name
    [InlineData("--vm-name test-vm --subscription sub123 --location eastus --admin-username azureuser --admin-password TestPassword123!", false)] // Missing resource-group
    [InlineData("--vm-name test-vm --resource-group test-rg --location eastus --admin-username azureuser --admin-password TestPassword123!", false)] // Missing subscription
    [InlineData("--vm-name test-vm --resource-group test-rg --subscription sub123 --admin-username azureuser --admin-password TestPassword123!", false)] // Missing location
    [InlineData("--vm-name test-vm --resource-group test-rg --subscription sub123 --location eastus --admin-password TestPassword123!", false)] // Missing admin-username
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        // Arrange
        if (shouldSucceed)
        {
            var createResult = new VmCreateResult(
                Name: _knownVmName,
                Id: "/subscriptions/sub123/resourceGroups/test-rg/providers/Microsoft.Compute/virtualMachines/test-vm",
                Location: _knownLocation,
                VmSize: "Standard_DS1_v2",
                ProvisioningState: "Succeeded",
                OsType: "linux",
                PublicIpAddress: "40.71.11.2",
                PrivateIpAddress: "10.0.0.4",
                Zones: null,
                Tags: null);

            Service.CreateVmAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string?>(),
                Arg.Any<string?>(),
                Arg.Any<string?>(),
                Arg.Any<string?>(),
                Arg.Any<string?>(),
                Arg.Any<string?>(),
                Arg.Any<string?>(),
                Arg.Any<string?>(),
                Arg.Any<string?>(),
                Arg.Any<bool?>(),
                Arg.Any<string?>(),
                Arg.Any<string?>(),
                Arg.Any<int?>(),
                Arg.Any<string?>(),
                Arg.Any<string?>(),
                Arg.Any<RetryPolicyOptions?>(),
                Arg.Any<CancellationToken>())
                .Returns(createResult);
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
            Assert.False(string.IsNullOrEmpty(response.Message));
        }
    }

    [Fact]
    public async Task ExecuteAsync_CreatesVmWithLinuxSshKey()
    {
        // Arrange
        var expectedResult = new VmCreateResult(
            Name: _knownVmName,
            Id: "/subscriptions/sub123/resourceGroups/test-rg/providers/Microsoft.Compute/virtualMachines/test-vm",
            Location: _knownLocation,
            VmSize: "Standard_DS1_v2",
            ProvisioningState: "Succeeded",
            OsType: "linux",
            PublicIpAddress: "40.71.11.2",
            PrivateIpAddress: "10.0.0.4",
            Zones: ["1"],
            Tags: new Dictionary<string, string> { { "env", "test" } });

        Service.CreateVmAsync(
            Arg.Is(_knownVmName),
            Arg.Is(_knownResourceGroup),
            Arg.Is(_knownSubscription),
            Arg.Is(_knownLocation),
            Arg.Is(_knownAdminUsername),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Is<string?>(x => !string.IsNullOrEmpty(x)), // SSH key
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<bool?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<int?>(),
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
            "--location", _knownLocation,
            "--admin-username", _knownAdminUsername,
            "--ssh-public-key", _knownSshKey);

        // Assert
        var result = ValidateAndDeserializeResponse(response, ComputeJsonContext.Default.VmCreateCommandResult);
        Assert.NotNull(result.Vm);
        Assert.Equal(_knownVmName, result.Vm.Name);
        Assert.Equal("linux", result.Vm.OsType);
        Assert.Equal("40.71.11.2", result.Vm.PublicIpAddress);
    }

    [Fact]
    public async Task ExecuteAsync_RequiresPasswordForWindows()
    {
        // Arrange & Act & Assert
        var response = await ExecuteCommandAsync(
            "--vm-name", _knownVmName,
            "--resource-group", _knownResourceGroup,
            "--subscription", _knownSubscription,
            "--location", _knownLocation,
            "--admin-username", _knownAdminUsername,
            "--image", "Win2022Datacenter");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("password", response.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Windows", response.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesConflictException()
    {
        // Arrange
        var conflictException = new RequestFailedException((int)HttpStatusCode.Conflict, "A VM with this name already exists");

        Service.CreateVmAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<bool?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<int?>(),
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
            "--location", _knownLocation,
            "--admin-username", _knownAdminUsername,
            "--admin-password", _knownPassword);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response.Status);
        Assert.Contains("already exists", response.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesForbiddenException()
    {
        // Arrange
        var forbiddenException = new RequestFailedException((int)HttpStatusCode.Forbidden, "Authorization failed");

        Service.CreateVmAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<bool?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<int?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(forbiddenException);

        // Act
        var response = await ExecuteCommandAsync(
            "--vm-name", _knownVmName,
            "--resource-group", _knownResourceGroup,
            "--subscription", _knownSubscription,
            "--location", _knownLocation,
            "--admin-username", _knownAdminUsername,
            "--admin-password", _knownPassword);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.Status);
        Assert.Contains("Authorization failed", response.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_DeserializationValidation()
    {
        // Arrange
        var expectedResult = new VmCreateResult(
            Name: _knownVmName,
            Id: "/subscriptions/sub123/resourceGroups/test-rg/providers/Microsoft.Compute/virtualMachines/test-vm",
            Location: _knownLocation,
            VmSize: "Standard_DS1_v2",
            ProvisioningState: "Succeeded",
            OsType: "linux",
            PublicIpAddress: "40.71.11.2",
            PrivateIpAddress: "10.0.0.4",
            Zones: null,
            Tags: null);

        Service.CreateVmAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<bool?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<int?>(),
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
            "--location", _knownLocation,
            "--admin-username", _knownAdminUsername,
            "--admin-password", _knownPassword);

        // Assert
        var result = ValidateAndDeserializeResponse(response, ComputeJsonContext.Default.VmCreateCommandResult);
        Assert.NotNull(result.Vm);
        Assert.Equal(_knownVmName, result.Vm.Name);
    }
}
