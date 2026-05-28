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

namespace Azure.Mcp.Tools.Compute.Tests.Vmss;

public class VmssCreateCommandTests : CommandUnitTestsBase<VmssCreateCommand, IComputeService>
{
    private readonly string _knownSubscription = "sub123";
    private readonly string _knownResourceGroup = "test-rg";
    private readonly string _knownVmssName = "test-vmss";
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
    [InlineData("--vmss-name test-vmss --resource-group test-rg --subscription sub123 --location eastus --admin-username azureuser --image Ubuntu2404 --admin-password TestPassword123!", true)]
    [InlineData("--vmss-name test-vmss --resource-group test-rg --subscription sub123 --location eastus --admin-username azureuser --image Ubuntu2404 --ssh-public-key ssh-rsa-key", true)]
    [InlineData("--vmss-name test-vmss --resource-group test-rg --subscription sub123 --location eastus --admin-username azureuser --image Ubuntu2404 --admin-password TestPassword123! --instance-count 3", true)]
    [InlineData("--resource-group test-rg --subscription sub123 --location eastus --admin-username azureuser --image Ubuntu2404 --admin-password TestPassword123!", false)] // Missing vmss-name
    [InlineData("--vmss-name test-vmss --subscription sub123 --location eastus --admin-username azureuser --image Ubuntu2404 --admin-password TestPassword123!", false)] // Missing resource-group
    [InlineData("--vmss-name test-vmss --resource-group test-rg --location eastus --admin-username azureuser --image Ubuntu2404 --admin-password TestPassword123!", false)] // Missing subscription
    [InlineData("--vmss-name test-vmss --resource-group test-rg --subscription sub123 --admin-username azureuser --image Ubuntu2404 --admin-password TestPassword123!", false)] // Missing location
    [InlineData("--vmss-name test-vmss --resource-group test-rg --subscription sub123 --location eastus --image Ubuntu2404 --admin-password TestPassword123!", false)] // Missing admin-username
    [InlineData("--vmss-name test-vmss --resource-group test-rg --subscription sub123 --location eastus --admin-username azureuser --admin-password TestPassword123!", false)] // Missing image
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        // Arrange
        if (shouldSucceed)
        {
            var createResult = new VmssCreateResult(
                Name: _knownVmssName,
                Id: "/subscriptions/sub123/resourceGroups/test-rg/providers/Microsoft.Compute/virtualMachineScaleSets/test-vmss",
                Location: _knownLocation,
                VmSize: "Standard_D2s_v5",
                ProvisioningState: "Succeeded",
                OsType: "linux",
                Capacity: 2,
                UpgradePolicy: "Manual",
                Zones: null,
                Tags: null);

            Service.CreateVmssAsync(
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
                Arg.Any<int?>(),
                Arg.Any<string?>(),
                Arg.Any<string?>(),
                Arg.Any<int?>(),
                Arg.Any<string?>(),
                Arg.Any<string?>(),
                Arg.Any<RetryPolicyOptions?>(),
                Arg.Any<CancellationToken>())
                .Returns(createResult);
        }

        // Act
        var response = await ExecuteCommandAsync(args);

        // Assert
        Assert.Equal(shouldSucceed ? HttpStatusCode.OK : HttpStatusCode.BadRequest, response.Status);
        if (shouldSucceed)
        {
            Assert.NotNull(response.Results);
            Assert.Equal("Success", response.Message);
        }
        else
        {
            Assert.False(string.IsNullOrEmpty(response.Message));
        }
    }

    [Fact]
    public async Task ExecuteAsync_CreatesVmssWithLinuxSshKey()
    {
        // Arrange
        var expectedResult = new VmssCreateResult(
            Name: _knownVmssName,
            Id: "/subscriptions/sub123/resourceGroups/test-rg/providers/Microsoft.Compute/virtualMachineScaleSets/test-vmss",
            Location: _knownLocation,
            VmSize: "Standard_D2s_v5",
            ProvisioningState: "Succeeded",
            OsType: "linux",
            Capacity: 3,
            UpgradePolicy: "Manual",
            Zones: ["1"],
            Tags: new Dictionary<string, string> { { "env", "test" } });

        Service.CreateVmssAsync(
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
            Arg.Any<int?>(),
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
            "--vmss-name", _knownVmssName,
            "--resource-group", _knownResourceGroup,
            "--subscription", _knownSubscription,
            "--location", _knownLocation,
            "--admin-username", _knownAdminUsername,
            "--image", "Ubuntu2404",
            "--ssh-public-key", _knownSshKey,
            "--instance-count", "3");

        // Assert
        var result = ValidateAndDeserializeResponse(response, ComputeJsonContext.Default.VmssCreateCommandResult);
        Assert.NotNull(result.Vmss);
        Assert.Equal(_knownVmssName, result.Vmss.Name);
        Assert.Equal("linux", result.Vmss.OsType);
        Assert.Equal(3, result.Vmss.Capacity);
    }

    [Fact]
    public async Task ExecuteAsync_RequiresPasswordForWindows()
    {
        // Arrange & Act & Assert
        var response = await ExecuteCommandAsync(
            "--vmss-name", _knownVmssName,
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
        var conflictException = new RequestFailedException((int)HttpStatusCode.Conflict, "A VMSS with this name already exists");

        Service.CreateVmssAsync(
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
            Arg.Any<int?>(),
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
            "--vmss-name", _knownVmssName,
            "--resource-group", _knownResourceGroup,
            "--subscription", _knownSubscription,
            "--location", _knownLocation,
            "--admin-username", _knownAdminUsername,
            "--image", "Ubuntu2404",
            "--admin-password", _knownPassword);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response.Status);
        Assert.Contains("already exists", response.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_DeserializationValidation()
    {
        // Arrange
        var expectedResult = new VmssCreateResult(
            Name: _knownVmssName,
            Id: "/subscriptions/sub123/resourceGroups/test-rg/providers/Microsoft.Compute/virtualMachineScaleSets/test-vmss",
            Location: _knownLocation,
            VmSize: "Standard_D2s_v5",
            ProvisioningState: "Succeeded",
            OsType: "linux",
            Capacity: 2,
            UpgradePolicy: "Manual",
            Zones: null,
            Tags: null);

        Service.CreateVmssAsync(
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
            Arg.Any<int?>(),
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
            "--vmss-name", _knownVmssName,
            "--resource-group", _knownResourceGroup,
            "--subscription", _knownSubscription,
            "--location", _knownLocation,
            "--admin-username", _knownAdminUsername,
            "--image", "Ubuntu2404",
            "--admin-password", _knownPassword);

        // Assert
        var result = ValidateAndDeserializeResponse(response, ComputeJsonContext.Default.VmssCreateCommandResult);
        Assert.NotNull(result.Vmss);
        Assert.Equal(_knownVmssName, result.Vmss.Name);
    }

    private VmssCreateResult MakeResult() => new(
        Name: _knownVmssName,
        Id: "/subscriptions/sub123/resourceGroups/test-rg/providers/Microsoft.Compute/virtualMachineScaleSets/test-vmss",
        Location: _knownLocation,
        VmSize: "Standard_D2s_v5",
        ProvisioningState: "Succeeded",
        OsType: "linux",
        Capacity: 2,
        UpgradePolicy: "Manual",
        Zones: null,
        Tags: null);

    private void SetupMock(VmssCreateResult result)
    {
        Service.CreateVmssAsync(
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
            Arg.Any<int?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<int?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(result);
    }

    [Fact]
    public async Task ExecuteAsync_PassesPublicIpAddress()
    {
        // Arrange
        const string publicIp = "my-vmss-pip";
        SetupMock(MakeResult());

        // Act
        var response = await ExecuteCommandAsync(
            "--vmss-name", _knownVmssName,
            "--resource-group", _knownResourceGroup,
            "--subscription", _knownSubscription,
            "--location", _knownLocation,
            "--admin-username", _knownAdminUsername,
            "--image", "Ubuntu2404",
            "--admin-password", _knownPassword,
            "--public-ip-address", publicIp);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        await Service.Received(1).CreateVmssAsync(
            Arg.Any<string>(),                  // name
            Arg.Any<string>(),                  // resourceGroup
            Arg.Any<string>(),                  // subscription
            Arg.Any<string>(),                  // location
            Arg.Any<string>(),                  // adminUsername
            Arg.Any<string?>(),                 // vmSize
            Arg.Any<string?>(),                 // image
            Arg.Any<string?>(),                 // adminPassword
            Arg.Any<string?>(),                 // sshPublicKey
            Arg.Any<string?>(),                 // osType
            Arg.Any<string?>(),                 // virtualNetwork
            Arg.Any<string?>(),                 // subnet
            Arg.Is<string?>(publicIp),          // publicIpAddress
            Arg.Any<string?>(),                 // networkSecurityGroup
            Arg.Any<bool?>(),                   // noPublicIp
            Arg.Any<string?>(),                 // sourceAddressPrefix
            Arg.Any<int?>(),                    // instanceCount
            Arg.Any<string?>(),                 // upgradePolicy
            Arg.Any<string?>(),                 // zone
            Arg.Any<int?>(),                    // osDiskSizeGb
            Arg.Any<string?>(),                 // osDiskType
            Arg.Any<string?>(),                 // tenant
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_PassesNetworkSecurityGroup()
    {
        // Arrange
        const string nsgName = "my-vmss-nsg";
        SetupMock(MakeResult());

        // Act
        var response = await ExecuteCommandAsync(
            "--vmss-name", _knownVmssName,
            "--resource-group", _knownResourceGroup,
            "--subscription", _knownSubscription,
            "--location", _knownLocation,
            "--admin-username", _knownAdminUsername,
            "--image", "Ubuntu2404",
            "--admin-password", _knownPassword,
            "--network-security-group", nsgName);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        await Service.Received(1).CreateVmssAsync(
            Arg.Any<string>(),                  // name
            Arg.Any<string>(),                  // resourceGroup
            Arg.Any<string>(),                  // subscription
            Arg.Any<string>(),                  // location
            Arg.Any<string>(),                  // adminUsername
            Arg.Any<string?>(),                 // vmSize
            Arg.Any<string?>(),                 // image
            Arg.Any<string?>(),                 // adminPassword
            Arg.Any<string?>(),                 // sshPublicKey
            Arg.Any<string?>(),                 // osType
            Arg.Any<string?>(),                 // virtualNetwork
            Arg.Any<string?>(),                 // subnet
            Arg.Any<string?>(),                 // publicIpAddress
            Arg.Is<string?>(nsgName),           // networkSecurityGroup
            Arg.Any<bool?>(),                   // noPublicIp
            Arg.Any<string?>(),                 // sourceAddressPrefix
            Arg.Any<int?>(),                    // instanceCount
            Arg.Any<string?>(),                 // upgradePolicy
            Arg.Any<string?>(),                 // zone
            Arg.Any<int?>(),                    // osDiskSizeGb
            Arg.Any<string?>(),                 // osDiskType
            Arg.Any<string?>(),                 // tenant
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_PassesNoPublicIp()
    {
        // Arrange
        SetupMock(MakeResult());

        // Act
        var response = await ExecuteCommandAsync(
            "--vmss-name", _knownVmssName,
            "--resource-group", _knownResourceGroup,
            "--subscription", _knownSubscription,
            "--location", _knownLocation,
            "--admin-username", _knownAdminUsername,
            "--image", "Ubuntu2404",
            "--admin-password", _knownPassword,
            "--no-public-ip");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        await Service.Received(1).CreateVmssAsync(
            Arg.Any<string>(),                  // name
            Arg.Any<string>(),                  // resourceGroup
            Arg.Any<string>(),                  // subscription
            Arg.Any<string>(),                  // location
            Arg.Any<string>(),                  // adminUsername
            Arg.Any<string?>(),                 // vmSize
            Arg.Any<string?>(),                 // image
            Arg.Any<string?>(),                 // adminPassword
            Arg.Any<string?>(),                 // sshPublicKey
            Arg.Any<string?>(),                 // osType
            Arg.Any<string?>(),                 // virtualNetwork
            Arg.Any<string?>(),                 // subnet
            Arg.Any<string?>(),                 // publicIpAddress
            Arg.Any<string?>(),                 // networkSecurityGroup
            Arg.Is<bool?>(true),                // noPublicIp
            Arg.Any<string?>(),                 // sourceAddressPrefix
            Arg.Any<int?>(),                    // instanceCount
            Arg.Any<string?>(),                 // upgradePolicy
            Arg.Any<string?>(),                 // zone
            Arg.Any<int?>(),                    // osDiskSizeGb
            Arg.Any<string?>(),                 // osDiskType
            Arg.Any<string?>(),                 // tenant
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_PassesSourceAddressPrefix()
    {
        // Arrange
        const string sourcePrefix = "10.0.0.0/24";
        SetupMock(MakeResult());

        // Act
        var response = await ExecuteCommandAsync(
            "--vmss-name", _knownVmssName,
            "--resource-group", _knownResourceGroup,
            "--subscription", _knownSubscription,
            "--location", _knownLocation,
            "--admin-username", _knownAdminUsername,
            "--image", "Ubuntu2404",
            "--admin-password", _knownPassword,
            "--source-address-prefix", sourcePrefix);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        await Service.Received(1).CreateVmssAsync(
            Arg.Any<string>(),                  // name
            Arg.Any<string>(),                  // resourceGroup
            Arg.Any<string>(),                  // subscription
            Arg.Any<string>(),                  // location
            Arg.Any<string>(),                  // adminUsername
            Arg.Any<string?>(),                 // vmSize
            Arg.Any<string?>(),                 // image
            Arg.Any<string?>(),                 // adminPassword
            Arg.Any<string?>(),                 // sshPublicKey
            Arg.Any<string?>(),                 // osType
            Arg.Any<string?>(),                 // virtualNetwork
            Arg.Any<string?>(),                 // subnet
            Arg.Any<string?>(),                 // publicIpAddress
            Arg.Any<string?>(),                 // networkSecurityGroup
            Arg.Any<bool?>(),                   // noPublicIp
            Arg.Is<string?>(sourcePrefix),      // sourceAddressPrefix
            Arg.Any<int?>(),                    // instanceCount
            Arg.Any<string?>(),                 // upgradePolicy
            Arg.Any<string?>(),                 // zone
            Arg.Any<int?>(),                    // osDiskSizeGb
            Arg.Any<string?>(),                 // osDiskType
            Arg.Any<string?>(),                 // tenant
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>());
    }
}
