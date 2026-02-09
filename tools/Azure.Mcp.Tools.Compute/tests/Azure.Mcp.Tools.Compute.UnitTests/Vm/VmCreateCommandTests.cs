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

public class VmCreateCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IComputeService _computeService;
    private readonly ILogger<VmCreateCommand> _logger;
    private readonly VmCreateCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;
    private readonly string _knownSubscription = "sub123";
    private readonly string _knownResourceGroup = "test-rg";
    private readonly string _knownVmName = "test-vm";
    private readonly string _knownLocation = "eastus";
    private readonly string _knownAdminUsername = "azureuser";
    private readonly string _knownPassword = "TestPassword123!";
    private readonly string _knownSshKey = "ssh-rsa AAAAB3NzaC1yc2EAAAADAQABAAABgQC...";

    public VmCreateCommandTests()
    {
        _computeService = Substitute.For<IComputeService>();
        _logger = Substitute.For<ILogger<VmCreateCommand>>();

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
        Assert.Equal("create", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
    }

    [Theory]
    [InlineData("--vm-name test-vm --resource-group test-rg --subscription sub123 --location eastus --admin-username azureuser --admin-password TestPassword123!", true)] // All required + password
    [InlineData("--vm-name test-vm --resource-group test-rg --subscription sub123 --location eastus --admin-username azureuser --ssh-public-key ssh-rsa-key", true)] // All required + ssh key
    [InlineData("--vm-name test-vm --resource-group test-rg --subscription sub123 --location eastus --admin-username azureuser --admin-password TestPassword123! --workload development", true)] // With workload
    [InlineData("--vm-name test-vm --resource-group test-rg --subscription sub123 --location eastus --admin-username azureuser", true)] // No auth - allowed for Linux (AAD SSH login)
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
                VmSize: "Standard_D2s_v3",
                ProvisioningState: "Succeeded",
                OsType: "linux",
                PublicIpAddress: "40.71.11.2",
                PrivateIpAddress: "10.0.0.4",
                Zones: null,
                Tags: null,
                WorkloadConfiguration: new WorkloadConfiguration(
                    WorkloadType: "general",
                    SuggestedVmSize: "Standard_D2s_v3",
                    SuggestedOsDiskType: "StandardSSD_LRS",
                    SuggestedOsDiskSizeGb: 128,
                    Description: "General purpose VM balanced for compute, memory, and storage"));

            _computeService.CreateVmAsync(
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
                Arg.Any<string?>(),
                Arg.Any<bool?>(),
                Arg.Any<string?>(),
                Arg.Any<int?>(),
                Arg.Any<string?>(),
                Arg.Any<string?>(),
                Arg.Any<RetryPolicyOptions?>(),
                Arg.Any<CancellationToken>())
                .Returns(createResult);
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
            VmSize: "Standard_D2s_v3",
            ProvisioningState: "Succeeded",
            OsType: "linux",
            PublicIpAddress: "40.71.11.2",
            PrivateIpAddress: "10.0.0.4",
            Zones: new List<string> { "1" },
            Tags: new Dictionary<string, string> { { "env", "test" } },
            WorkloadConfiguration: new WorkloadConfiguration(
                WorkloadType: "general",
                SuggestedVmSize: "Standard_D2s_v3",
                SuggestedOsDiskType: "StandardSSD_LRS",
                SuggestedOsDiskSizeGb: 128,
                Description: "General purpose VM balanced for compute, memory, and storage"));

        _computeService.CreateVmAsync(
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
            Arg.Any<string?>(),
            Arg.Any<bool?>(),
            Arg.Any<string?>(),
            Arg.Any<int?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        var parseResult = _commandDefinition.Parse([
            "--vm-name", _knownVmName,
            "--resource-group", _knownResourceGroup,
            "--subscription", _knownSubscription,
            "--location", _knownLocation,
            "--admin-username", _knownAdminUsername,
            "--ssh-public-key", _knownSshKey
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, ComputeJsonContext.Default.VmCreateCommandResult);

        Assert.NotNull(result);
        Assert.NotNull(result.Vm);
        Assert.Equal(_knownVmName, result.Vm.Name);
        Assert.Equal("linux", result.Vm.OsType);
        Assert.Equal("40.71.11.2", result.Vm.PublicIpAddress);
    }

    [Fact]
    public async Task ExecuteAsync_CreatesVmWithWorkload()
    {
        // Arrange
        var expectedResult = new VmCreateResult(
            Name: _knownVmName,
            Id: "/subscriptions/sub123/resourceGroups/test-rg/providers/Microsoft.Compute/virtualMachines/test-vm",
            Location: _knownLocation,
            VmSize: "Standard_B2s",
            ProvisioningState: "Succeeded",
            OsType: "linux",
            PublicIpAddress: "40.71.11.2",
            PrivateIpAddress: "10.0.0.4",
            Zones: null,
            Tags: null,
            WorkloadConfiguration: new WorkloadConfiguration(
                WorkloadType: "development",
                SuggestedVmSize: "Standard_B2s",
                SuggestedOsDiskType: "StandardSSD_LRS",
                SuggestedOsDiskSizeGb: 64,
                Description: "Cost-effective burstable VM for development and testing workloads"));

        _computeService.CreateVmAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Is("development"), // Workload
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
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        var parseResult = _commandDefinition.Parse([
            "--vm-name", _knownVmName,
            "--resource-group", _knownResourceGroup,
            "--subscription", _knownSubscription,
            "--location", _knownLocation,
            "--admin-username", _knownAdminUsername,
            "--admin-password", _knownPassword,
            "--workload", "development"
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, ComputeJsonContext.Default.VmCreateCommandResult);

        Assert.NotNull(result);
        Assert.NotNull(result.Vm);
        Assert.NotNull(result.Vm.WorkloadConfiguration);
        Assert.Equal("development", result.Vm.WorkloadConfiguration.WorkloadType);
        Assert.Equal("Standard_B2s", result.Vm.WorkloadConfiguration.SuggestedVmSize);
    }

    [Fact]
    public async Task ExecuteAsync_RequiresPasswordForWindows()
    {
        // Arrange
        var parseResult = _commandDefinition.Parse([
            "--vm-name", _knownVmName,
            "--resource-group", _knownResourceGroup,
            "--subscription", _knownSubscription,
            "--location", _knownLocation,
            "--admin-username", _knownAdminUsername,
            "--image", "Win2022Datacenter" // Windows image
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("password", response.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Windows", response.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesConflictException()
    {
        // Arrange
        var conflictException = new RequestFailedException((int)HttpStatusCode.Conflict, "A VM with this name already exists");

        _computeService.CreateVmAsync(
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
            Arg.Any<string?>(),
            Arg.Any<bool?>(),
            Arg.Any<string?>(),
            Arg.Any<int?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(conflictException);

        var parseResult = _commandDefinition.Parse([
            "--vm-name", _knownVmName,
            "--resource-group", _knownResourceGroup,
            "--subscription", _knownSubscription,
            "--location", _knownLocation,
            "--admin-username", _knownAdminUsername,
            "--admin-password", _knownPassword
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response.Status);
        Assert.Contains("already exists", response.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesForbiddenException()
    {
        // Arrange
        var forbiddenException = new RequestFailedException((int)HttpStatusCode.Forbidden, "Authorization failed");

        _computeService.CreateVmAsync(
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
            Arg.Any<string?>(),
            Arg.Any<bool?>(),
            Arg.Any<string?>(),
            Arg.Any<int?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(forbiddenException);

        var parseResult = _commandDefinition.Parse([
            "--vm-name", _knownVmName,
            "--resource-group", _knownResourceGroup,
            "--subscription", _knownSubscription,
            "--location", _knownLocation,
            "--admin-username", _knownAdminUsername,
            "--admin-password", _knownPassword
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

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
            VmSize: "Standard_D2s_v3",
            ProvisioningState: "Succeeded",
            OsType: "linux",
            PublicIpAddress: "40.71.11.2",
            PrivateIpAddress: "10.0.0.4",
            Zones: null,
            Tags: null,
            WorkloadConfiguration: new WorkloadConfiguration(
                WorkloadType: "general",
                SuggestedVmSize: "Standard_D2s_v3",
                SuggestedOsDiskType: "StandardSSD_LRS",
                SuggestedOsDiskSizeGb: 128,
                Description: "General purpose VM balanced for compute, memory, and storage"));

        _computeService.CreateVmAsync(
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
            Arg.Any<string?>(),
            Arg.Any<bool?>(),
            Arg.Any<string?>(),
            Arg.Any<int?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        var parseResult = _commandDefinition.Parse([
            "--vm-name", _knownVmName,
            "--resource-group", _knownResourceGroup,
            "--subscription", _knownSubscription,
            "--location", _knownLocation,
            "--admin-username", _knownAdminUsername,
            "--admin-password", _knownPassword
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response.Results);
        var json = JsonSerializer.Serialize(response.Results);

        // Verify deserialization works
        var result = JsonSerializer.Deserialize(json, ComputeJsonContext.Default.VmCreateCommandResult);
        Assert.NotNull(result);
        Assert.NotNull(result.Vm);
        Assert.Equal(_knownVmName, result.Vm.Name);
        Assert.NotNull(result.Vm.WorkloadConfiguration);
    }
}
