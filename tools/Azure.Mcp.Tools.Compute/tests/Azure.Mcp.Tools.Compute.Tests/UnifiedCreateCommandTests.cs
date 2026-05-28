// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.Compute.Commands;
using Azure.Mcp.Tools.Compute.Models;
using Azure.Mcp.Tools.Compute.Services;
using Microsoft.Mcp.Core.Options;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.Compute.UnitTests;

public class UnifiedCreateCommandTests : CommandUnitTestsBase<UnifiedCreateCommand, IComputeService>
{
    private readonly string _knownSubscription = "sub123";
    private readonly string _knownResourceGroup = "test-rg";
    private readonly string _knownName = "test-name";
    private readonly string _knownLocation = "eastus";
    private readonly string _knownAdminUsername = "azureuser";
    private readonly string _knownPassword = "TestPassword123!";
    private readonly string _knownSshKey = "ssh-rsa AAAAB3NzaC1yc2EAAAADAQABAAABgQC...";

    private VmssCreateResult MakeVmssResult(int capacity = 2, string osType = "linux") => new(
        Name: _knownName,
        Id: $"/subscriptions/{_knownSubscription}/resourceGroups/{_knownResourceGroup}/providers/Microsoft.Compute/virtualMachineScaleSets/{_knownName}",
        Location: _knownLocation,
        VmSize: "Standard_D2s_v5",
        ProvisioningState: "Succeeded",
        OsType: osType,
        Capacity: capacity,
        UpgradePolicy: "Manual",
        Zones: null,
        Tags: null);

    private VmCreateResult MakeVmResult(string osType = "linux") => new(
        Name: _knownName,
        Id: $"/subscriptions/{_knownSubscription}/resourceGroups/{_knownResourceGroup}/providers/Microsoft.Compute/virtualMachines/{_knownName}",
        Location: _knownLocation,
        VmSize: "Standard_D2s_v5",
        ProvisioningState: "Succeeded",
        OsType: osType,
        PublicIpAddress: "40.71.11.2",
        PrivateIpAddress: "10.0.0.4",
        Zones: null,
        Tags: null);

    private void SetupVmssMock(VmssCreateResult result)
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

    private void SetupVmMock(VmCreateResult result)
    {
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
            .Returns(result);
    }

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = Command.GetCommand();
        Assert.Equal("create", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
    }

    [Theory]
    [InlineData("--vmss-name test-name --resource-group test-rg --subscription sub123 --location eastus --admin-username azureuser --image Ubuntu2404 --admin-password TestPassword123!", true)]
    [InlineData("--vmss-name test-name --resource-group test-rg --subscription sub123 --location eastus --admin-username azureuser --image Ubuntu2404 --ssh-public-key ssh-rsa-key", true)]
    [InlineData("--resource-group test-rg --subscription sub123 --location eastus --admin-username azureuser --image Ubuntu2404 --admin-password TestPassword123!", false)] // Missing name
    [InlineData("--vmss-name test-name --subscription sub123 --location eastus --admin-username azureuser --image Ubuntu2404 --admin-password TestPassword123!", false)] // Missing resource-group
    [InlineData("--vmss-name test-name --resource-group test-rg --location eastus --admin-username azureuser --image Ubuntu2404 --admin-password TestPassword123!", false)] // Missing subscription
    [InlineData("--vmss-name test-name --resource-group test-rg --subscription sub123 --admin-username azureuser --image Ubuntu2404 --admin-password TestPassword123!", false)] // Missing location
    [InlineData("--vmss-name test-name --resource-group test-rg --subscription sub123 --location eastus --image Ubuntu2404 --admin-password TestPassword123!", false)] // Missing admin-username
    [InlineData("--vmss-name test-name --resource-group test-rg --subscription sub123 --location eastus --admin-username azureuser --admin-password TestPassword123!", false)] // Missing image
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        // Arrange
        if (shouldSucceed)
        {
            SetupVmssMock(MakeVmssResult());
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
    public async Task ExecuteAsync_Default_DispatchesToVmssFlex()
    {
        // Arrange
        SetupVmssMock(MakeVmssResult(capacity: 2));

        // Act — no --single-instance => VMSS Flex path
        var response = await ExecuteCommandAsync(
            "--vmss-name", _knownName,
            "--resource-group", _knownResourceGroup,
            "--subscription", _knownSubscription,
            "--location", _knownLocation,
            "--admin-username", _knownAdminUsername,
            "--image", "Ubuntu2404",
            "--admin-password", _knownPassword);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        var result = ValidateAndDeserializeResponse(response, ComputeJsonContext.Default.UnifiedCreateCommandResult);
        Assert.Equal("vmss-flex", result.Mode);
        Assert.NotNull(result.Vmss);
        Assert.Null(result.Vm);
        Assert.Equal(_knownName, result.Vmss.Name);
        Assert.Equal(2, result.Vmss.Capacity);

        // Verify the VM path was NOT invoked.
        await Service.DidNotReceiveWithAnyArgs().CreateVmAsync(
            default!, default!, default!, default!, default!,
            default, default, default, default, default,
            default, default, default, default, default,
            default, default, default, default, default,
            default, TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task ExecuteAsync_SingleInstance_DispatchesToVm()
    {
        // Arrange
        SetupVmMock(MakeVmResult());

        // Act
        var response = await ExecuteCommandAsync(
            "--vmss-name", _knownName,
            "--resource-group", _knownResourceGroup,
            "--subscription", _knownSubscription,
            "--location", _knownLocation,
            "--admin-username", _knownAdminUsername,
            "--image", "Ubuntu2404",
            "--admin-password", _knownPassword,
            "--single-instance");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        var result = ValidateAndDeserializeResponse(response, ComputeJsonContext.Default.UnifiedCreateCommandResult);
        Assert.Equal("single-vm", result.Mode);
        Assert.NotNull(result.Vm);
        Assert.Null(result.Vmss);
        Assert.Equal(_knownName, result.Vm.Name);

        // Verify the VMSS path was NOT invoked.
        await Service.DidNotReceiveWithAnyArgs().CreateVmssAsync(
            default!, default!, default!, default!, default!,
            default, default, default, default, default,
            default, default, default, default, default,
            default, default, default, default, default,
            default, default, default, TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task ExecuteAsync_DefaultPath_PassesInstanceCountAndUpgradePolicy()
    {
        // Arrange
        SetupVmssMock(MakeVmssResult(capacity: 5));

        // Act
        var response = await ExecuteCommandAsync(
            "--vmss-name", _knownName,
            "--resource-group", _knownResourceGroup,
            "--subscription", _knownSubscription,
            "--location", _knownLocation,
            "--admin-username", _knownAdminUsername,
            "--image", "Ubuntu2404",
            "--admin-password", _knownPassword,
            "--instance-count", "5",
            "--upgrade-policy", "Rolling");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        await Service.Received(1).CreateVmssAsync(
            Arg.Is(_knownName),
            Arg.Is(_knownResourceGroup),
            Arg.Is(_knownSubscription),
            Arg.Is(_knownLocation),
            Arg.Is(_knownAdminUsername),
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
            Arg.Is<int?>(5),
            Arg.Is<string?>("Rolling"),
            Arg.Any<string?>(),
            Arg.Any<int?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_SingleInstance_IgnoresVmssOnlyOptions()
    {
        // Arrange
        SetupVmMock(MakeVmResult());

        // Act — --instance-count and --upgrade-policy are accepted but ignored on the single-VM path.
        var response = await ExecuteCommandAsync(
            "--vmss-name", _knownName,
            "--resource-group", _knownResourceGroup,
            "--subscription", _knownSubscription,
            "--location", _knownLocation,
            "--admin-username", _knownAdminUsername,
            "--image", "Ubuntu2404",
            "--admin-password", _knownPassword,
            "--instance-count", "7",
            "--upgrade-policy", "Rolling",
            "--single-instance");

        // Assert — VM path was used and the VMSS service was never called regardless of --instance-count.
        Assert.Equal(HttpStatusCode.OK, response.Status);
        var result = ValidateAndDeserializeResponse(response, ComputeJsonContext.Default.UnifiedCreateCommandResult);
        Assert.Equal("single-vm", result.Mode);
        await Service.DidNotReceiveWithAnyArgs().CreateVmssAsync(
            default!, default!, default!, default!, default!,
            default, default, default, default, default,
            default, default, default, default, default,
            default, default, default, default, default,
            default, default, default, TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task ExecuteAsync_Linux_RequiresSshKeyOrPassword()
    {
        // Act — Linux image with neither --ssh-public-key nor --admin-password.
        var response = await ExecuteCommandAsync(
            "--vmss-name", _knownName,
            "--resource-group", _knownResourceGroup,
            "--subscription", _knownSubscription,
            "--location", _knownLocation,
            "--admin-username", _knownAdminUsername,
            "--image", "Ubuntu2404");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("ssh-public-key", response.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("admin-password", response.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_Linux_AcceptsSshKey()
    {
        // Arrange
        SetupVmssMock(MakeVmssResult());

        // Act
        var response = await ExecuteCommandAsync(
            "--vmss-name", _knownName,
            "--resource-group", _knownResourceGroup,
            "--subscription", _knownSubscription,
            "--location", _knownLocation,
            "--admin-username", _knownAdminUsername,
            "--image", "Ubuntu2404",
            "--ssh-public-key", _knownSshKey);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        var result = ValidateAndDeserializeResponse(response, ComputeJsonContext.Default.UnifiedCreateCommandResult);
        Assert.Equal("vmss-flex", result.Mode);
        Assert.NotNull(result.Vmss);
    }

    [Fact]
    public async Task ExecuteAsync_Windows_RequiresPassword_OnVmssPath()
    {
        // Act
        var response = await ExecuteCommandAsync(
            "--vmss-name", "winapp",
            "--resource-group", _knownResourceGroup,
            "--subscription", _knownSubscription,
            "--location", _knownLocation,
            "--admin-username", _knownAdminUsername,
            "--image", "Win2022Datacenter");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("password", response.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Windows", response.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_Windows_RequiresPassword_OnSingleInstancePath()
    {
        // Act
        var response = await ExecuteCommandAsync(
            "--vmss-name", "winapp",
            "--resource-group", _knownResourceGroup,
            "--subscription", _knownSubscription,
            "--location", _knownLocation,
            "--admin-username", _knownAdminUsername,
            "--image", "Win2022Datacenter",
            "--single-instance");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("password", response.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Windows", response.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_Windows_VmssNameLimitedTo9Chars()
    {
        // Act — 10-char Windows VMSS name on default path should fail (9-char limit because Azure
        // appends a 6-char suffix => 15-char Windows computer name ceiling).
        var response = await ExecuteCommandAsync(
            "--vmss-name", "win-app-10",
            "--resource-group", _knownResourceGroup,
            "--subscription", _knownSubscription,
            "--location", _knownLocation,
            "--admin-username", _knownAdminUsername,
            "--image", "Win2022Datacenter",
            "--admin-password", _knownPassword);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("9 characters", response.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("single-instance", response.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_Windows_SingleInstanceAllowsUpTo15Chars()
    {
        // Arrange — 10-char name is fine for the single-VM path (15-char Windows computer name limit).
        SetupVmMock(MakeVmResult(osType: "windows"));

        // Act
        var response = await ExecuteCommandAsync(
            "--vmss-name", "win-app-10",
            "--resource-group", _knownResourceGroup,
            "--subscription", _knownSubscription,
            "--location", _knownLocation,
            "--admin-username", _knownAdminUsername,
            "--image", "Win2022Datacenter",
            "--admin-password", _knownPassword,
            "--single-instance");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        var result = ValidateAndDeserializeResponse(response, ComputeJsonContext.Default.UnifiedCreateCommandResult);
        Assert.Equal("single-vm", result.Mode);
    }

    [Fact]
    public async Task ExecuteAsync_Windows_SingleInstance_RejectsNamesOver15Chars()
    {
        // Act — 16-char Windows name violates the standalone-VM computer-name limit.
        var response = await ExecuteCommandAsync(
            "--vmss-name", "winapp-1234567890",
            "--resource-group", _knownResourceGroup,
            "--subscription", _knownSubscription,
            "--location", _knownLocation,
            "--admin-username", _knownAdminUsername,
            "--image", "Win2022Datacenter",
            "--admin-password", _knownPassword,
            "--single-instance");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("Windows computer name", response.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_VmssPath_HandlesConflictException()
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
            "--vmss-name", _knownName,
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
    public async Task ExecuteAsync_SingleInstance_HandlesConflictException()
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
            "--vmss-name", _knownName,
            "--resource-group", _knownResourceGroup,
            "--subscription", _knownSubscription,
            "--location", _knownLocation,
            "--admin-username", _knownAdminUsername,
            "--image", "Ubuntu2404",
            "--admin-password", _knownPassword,
            "--single-instance");

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response.Status);
        Assert.Contains("already exists", response.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_DeserializationValidation_VmssBranch()
    {
        // Arrange
        SetupVmssMock(MakeVmssResult());

        // Act
        var response = await ExecuteCommandAsync(
            "--vmss-name", _knownName,
            "--resource-group", _knownResourceGroup,
            "--subscription", _knownSubscription,
            "--location", _knownLocation,
            "--admin-username", _knownAdminUsername,
            "--image", "Ubuntu2404",
            "--admin-password", _knownPassword);

        // Assert
        var result = ValidateAndDeserializeResponse(response, ComputeJsonContext.Default.UnifiedCreateCommandResult);
        Assert.Equal("vmss-flex", result.Mode);
        Assert.NotNull(result.Vmss);
        Assert.Null(result.Vm);
        Assert.Equal(_knownName, result.Vmss.Name);
    }

    [Fact]
    public async Task ExecuteAsync_DeserializationValidation_VmBranch()
    {
        // Arrange
        SetupVmMock(MakeVmResult());

        // Act
        var response = await ExecuteCommandAsync(
            "--vmss-name", _knownName,
            "--resource-group", _knownResourceGroup,
            "--subscription", _knownSubscription,
            "--location", _knownLocation,
            "--admin-username", _knownAdminUsername,
            "--image", "Ubuntu2404",
            "--admin-password", _knownPassword,
            "--single-instance");

        // Assert
        var result = ValidateAndDeserializeResponse(response, ComputeJsonContext.Default.UnifiedCreateCommandResult);
        Assert.Equal("single-vm", result.Mode);
        Assert.NotNull(result.Vm);
        Assert.Null(result.Vmss);
        Assert.Equal(_knownName, result.Vm.Name);
    }
}
