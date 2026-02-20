// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Net;
using System.Text.Json;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.Compute.Commands;
using Azure.Mcp.Tools.Compute.Commands.Vmss;
using Azure.Mcp.Tools.Compute.Models;
using Azure.Mcp.Tools.Compute.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Models.Command;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.Compute.UnitTests.Vmss;

public class VmssCreateCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IComputeService _computeService;
    private readonly ILogger<VmssCreateCommand> _logger;
    private readonly VmssCreateCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;
    private readonly string _knownSubscription = "sub123";
    private readonly string _knownResourceGroup = "test-rg";
    private readonly string _knownVmssName = "test-vmss";
    private readonly string _knownLocation = "eastus";
    private readonly string _knownAdminUsername = "azureuser";
    private readonly string _knownPassword = "TestPassword123!";
    private readonly string _knownSshKey = "ssh-rsa AAAAB3NzaC1yc2EAAAADAQABAAABgQC...";

    public VmssCreateCommandTests()
    {
        _computeService = Substitute.For<IComputeService>();
        _logger = Substitute.For<ILogger<VmssCreateCommand>>();

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
    [InlineData("--vmss-name test-vmss --resource-group test-rg --subscription sub123 --location eastus --admin-username azureuser --admin-password TestPassword123!", true)]
    [InlineData("--vmss-name test-vmss --resource-group test-rg --subscription sub123 --location eastus --admin-username azureuser --ssh-public-key ssh-rsa-key", true)]
    [InlineData("--vmss-name test-vmss --resource-group test-rg --subscription sub123 --location eastus --admin-username azureuser --admin-password TestPassword123! --instance-count 3", true)]
    [InlineData("--resource-group test-rg --subscription sub123 --location eastus --admin-username azureuser --admin-password TestPassword123!", false)] // Missing vmss-name
    [InlineData("--vmss-name test-vmss --subscription sub123 --location eastus --admin-username azureuser --admin-password TestPassword123!", false)] // Missing resource-group
    [InlineData("--vmss-name test-vmss --resource-group test-rg --location eastus --admin-username azureuser --admin-password TestPassword123!", false)] // Missing subscription
    [InlineData("--vmss-name test-vmss --resource-group test-rg --subscription sub123 --admin-username azureuser --admin-password TestPassword123!", false)] // Missing location
    [InlineData("--vmss-name test-vmss --resource-group test-rg --subscription sub123 --location eastus --admin-password TestPassword123!", false)] // Missing admin-username
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        // Arrange
        if (shouldSucceed)
        {
            var createResult = new VmssCreateResult(
                Name: _knownVmssName,
                Id: "/subscriptions/sub123/resourceGroups/test-rg/providers/Microsoft.Compute/virtualMachineScaleSets/test-vmss",
                Location: _knownLocation,
                VmSize: "Standard_D2s_v3",
                ProvisioningState: "Succeeded",
                OsType: "linux",
                Capacity: 2,
                UpgradePolicy: "Manual",
                Zones: null,
                Tags: null,
                WorkloadConfiguration: new WorkloadConfiguration(
                    WorkloadType: "general",
                    SuggestedVmSize: "Standard_D2s_v3",
                    SuggestedOsDiskType: "StandardSSD_LRS",
                    SuggestedOsDiskSizeGb: 128,
                    Description: "General purpose VM balanced for compute, memory, and storage"));

            _computeService.CreateVmssAsync(
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
    public async Task ExecuteAsync_CreatesVmssWithLinuxSshKey()
    {
        // Arrange
        var expectedResult = new VmssCreateResult(
            Name: _knownVmssName,
            Id: "/subscriptions/sub123/resourceGroups/test-rg/providers/Microsoft.Compute/virtualMachineScaleSets/test-vmss",
            Location: _knownLocation,
            VmSize: "Standard_D2s_v3",
            ProvisioningState: "Succeeded",
            OsType: "linux",
            Capacity: 3,
            UpgradePolicy: "Manual",
            Zones: ["1"],
            Tags: new Dictionary<string, string> { { "env", "test" } },
            WorkloadConfiguration: new WorkloadConfiguration(
                WorkloadType: "general",
                SuggestedVmSize: "Standard_D2s_v3",
                SuggestedOsDiskType: "StandardSSD_LRS",
                SuggestedOsDiskSizeGb: 128,
                Description: "General purpose VM balanced for compute, memory, and storage"));

        _computeService.CreateVmssAsync(
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
            Arg.Any<int?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<int?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        var parseResult = _commandDefinition.Parse([
            "--vmss-name", _knownVmssName,
            "--resource-group", _knownResourceGroup,
            "--subscription", _knownSubscription,
            "--location", _knownLocation,
            "--admin-username", _knownAdminUsername,
            "--ssh-public-key", _knownSshKey,
            "--instance-count", "3"
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, ComputeJsonContext.Default.VmssCreateCommandResult);

        Assert.NotNull(result);
        Assert.NotNull(result.Vmss);
        Assert.Equal(_knownVmssName, result.Vmss.Name);
        Assert.Equal("linux", result.Vmss.OsType);
        Assert.Equal(3, result.Vmss.Capacity);
    }

    [Fact]
    public async Task ExecuteAsync_RequiresPasswordForWindows()
    {
        // Arrange
        var parseResult = _commandDefinition.Parse([
            "--vmss-name", _knownVmssName,
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
    }

    [Fact]
    public async Task ExecuteAsync_HandlesConflictException()
    {
        // Arrange
        var conflictException = new RequestFailedException((int)HttpStatusCode.Conflict, "A VMSS with this name already exists");

        _computeService.CreateVmssAsync(
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
            Arg.Any<int?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<int?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(conflictException);

        var parseResult = _commandDefinition.Parse([
            "--vmss-name", _knownVmssName,
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
    public async Task ExecuteAsync_DeserializationValidation()
    {
        // Arrange
        var expectedResult = new VmssCreateResult(
            Name: _knownVmssName,
            Id: "/subscriptions/sub123/resourceGroups/test-rg/providers/Microsoft.Compute/virtualMachineScaleSets/test-vmss",
            Location: _knownLocation,
            VmSize: "Standard_D2s_v3",
            ProvisioningState: "Succeeded",
            OsType: "linux",
            Capacity: 2,
            UpgradePolicy: "Manual",
            Zones: null,
            Tags: null,
            WorkloadConfiguration: new WorkloadConfiguration(
                WorkloadType: "general",
                SuggestedVmSize: "Standard_D2s_v3",
                SuggestedOsDiskType: "StandardSSD_LRS",
                SuggestedOsDiskSizeGb: 128,
                Description: "General purpose VM balanced for compute, memory, and storage"));

        _computeService.CreateVmssAsync(
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
            Arg.Any<int?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<int?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        var parseResult = _commandDefinition.Parse([
            "--vmss-name", _knownVmssName,
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

        var result = JsonSerializer.Deserialize(json, ComputeJsonContext.Default.VmssCreateCommandResult);
        Assert.NotNull(result);
        Assert.NotNull(result.Vmss);
        Assert.Equal(_knownVmssName, result.Vmss.Name);
        Assert.NotNull(result.Vmss.WorkloadConfiguration);
    }
}
