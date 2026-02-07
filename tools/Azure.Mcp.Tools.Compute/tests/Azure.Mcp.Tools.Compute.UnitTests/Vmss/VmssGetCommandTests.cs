// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Net;
using System.Text.Json;
using Azure;
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

public class VmssGetCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IComputeService _computeService;
    private readonly ILogger<VmssGetCommand> _logger;
    private readonly VmssGetCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;
    private readonly string _knownSubscription = "sub123";
    private readonly string _knownResourceGroup = "test-rg";
    private readonly string _knownVmssName = "test-vmss";
    private readonly string _knownInstanceId = "0";

    public VmssGetCommandTests()
    {
        _computeService = Substitute.For<IComputeService>();
        _logger = Substitute.For<ILogger<VmssGetCommand>>();

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
        Assert.Equal("get", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
    }

    [Theory]
    [InlineData("--subscription sub123", true)] // List all VMSS in subscription
    [InlineData("--subscription sub123 --resource-group test-rg", true)] // List VMSS in resource group
    [InlineData("--vmss-name test-vmss --resource-group test-rg --subscription sub123", true)] // Get specific VMSS
    [InlineData("--vmss-name test-vmss --resource-group test-rg --subscription sub123 --instance-id 0", true)] // Get specific VM instance in VMSS
    [InlineData("--vmss-name test-vmss --subscription sub123", false)] // Missing resource-group (required with vmss-name)
    [InlineData("--instance-id 0 --subscription sub123", false)] // instance-id without vmss-name
    [InlineData("--instance-id 0 --resource-group test-rg --subscription sub123", false)] // instance-id without vmss-name
    [InlineData("--resource-group test-rg", false)] // Missing subscription
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        // Arrange
        var vmssList = new List<VmssInfo>
        {
            new(
                Name: _knownVmssName,
                Id: "/subscriptions/sub123/resourceGroups/test-rg/providers/Microsoft.Compute/virtualMachineScaleSets/test-vmss",
                Location: "eastus",
                Sku: new VmssSkuInfo("Standard_D2s_v3", "Standard", 3),
                Capacity: 3,
                ProvisioningState: "Succeeded",
                UpgradePolicy: "Manual",
                Overprovision: true,
                Zones: ["1", "2"],
                Tags: new Dictionary<string, string> { { "env", "test" } }
            )
        };

        var vmssInfo = new VmssInfo(
            Name: _knownVmssName,
            Id: "/subscriptions/sub123/resourceGroups/test-rg/providers/Microsoft.Compute/virtualMachineScaleSets/test-vmss",
            Location: "eastus",
            Sku: new VmssSkuInfo("Standard_D2s_v3", "Standard", 3),
            Capacity: 3,
            ProvisioningState: "Succeeded",
            UpgradePolicy: "Manual",
            Overprovision: true,
            Zones: ["1", "2"],
            Tags: new Dictionary<string, string> { { "env", "test" } }
        );

        var vmssVmInfo = new VmssVmInfo(
            InstanceId: _knownInstanceId,
            Name: "test-vmss_0",
            Id: "/subscriptions/sub123/resourceGroups/test-rg/providers/Microsoft.Compute/virtualMachineScaleSets/test-vmss/virtualMachines/0",
            Location: "eastus",
            VmSize: "Standard_D2s_v3",
            ProvisioningState: "Succeeded",
            OsType: "Linux",
            Zones: ["1"],
            Tags: null
        );

        _computeService.ListVmssAsync(
            Arg.Any<string?>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(vmssList);

        _computeService.GetVmssAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(vmssInfo);

        _computeService.GetVmssVmAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(vmssVmInfo);

        var parseResult = _commandDefinition.Parse(args);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

        // Assert
        if (shouldSucceed)
        {
            Assert.Equal(HttpStatusCode.OK, response.Status);
            Assert.NotNull(response.Results);
        }
        else
        {
            Assert.True(
                response.Status == HttpStatusCode.BadRequest ||
                response.Status == HttpStatusCode.InternalServerError,
                $"Expected BadRequest or InternalServerError, got {response.Status}");
        }
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsVmssList_WhenListingSubscription()
    {
        // Arrange
        var vmssList = new List<VmssInfo>
        {
            new(
                Name: "vmss1",
                Id: "/subscriptions/sub123/resourceGroups/rg1/providers/Microsoft.Compute/virtualMachineScaleSets/vmss1",
                Location: "eastus",
                Sku: new VmssSkuInfo("Standard_D2s_v3", "Standard", 3),
                Capacity: 3,
                ProvisioningState: "Succeeded",
                UpgradePolicy: "Manual",
                Overprovision: true,
                Zones: ["1", "2"],
                Tags: new Dictionary<string, string> { { "env", "prod" } }
            ),
            new(
                Name: "vmss2",
                Id: "/subscriptions/sub123/resourceGroups/rg2/providers/Microsoft.Compute/virtualMachineScaleSets/vmss2",
                Location: "westus",
                Sku: new VmssSkuInfo("Standard_D4s_v3", "Standard", 5),
                Capacity: 5,
                ProvisioningState: "Succeeded",
                UpgradePolicy: "Automatic",
                Overprovision: false,
                Zones: null,
                Tags: null
            )
        };

        _computeService.ListVmssAsync(
            Arg.Is((string?)null),
            Arg.Is(_knownSubscription),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(vmssList);

        var parseResult = _commandDefinition.Parse([
            "--subscription", _knownSubscription
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, ComputeJsonContext.Default.VmssGetListResult);

        Assert.NotNull(result);
        Assert.Equal(2, result.VmssList.Count);
        Assert.Equal("vmss1", result.VmssList[0].Name);
        Assert.Equal("vmss2", result.VmssList[1].Name);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsVmssList_WhenListingResourceGroup()
    {
        // Arrange
        var vmssList = new List<VmssInfo>
        {
            new(
                Name: "vmss1",
                Id: "/subscriptions/sub123/resourceGroups/test-rg/providers/Microsoft.Compute/virtualMachineScaleSets/vmss1",
                Location: "eastus",
                Sku: new VmssSkuInfo("Standard_D2s_v3", "Standard", 3),
                Capacity: 3,
                ProvisioningState: "Succeeded",
                UpgradePolicy: "Manual",
                Overprovision: true,
                Zones: ["1", "2"],
                Tags: new Dictionary<string, string> { { "env", "test" } }
            )
        };

        _computeService.ListVmssAsync(
            Arg.Is(_knownResourceGroup),
            Arg.Is(_knownSubscription),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(vmssList);

        var parseResult = _commandDefinition.Parse([
            "--subscription", _knownSubscription,
            "--resource-group", _knownResourceGroup
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, ComputeJsonContext.Default.VmssGetListResult);

        Assert.NotNull(result);
        Assert.Single(result.VmssList);
        Assert.Equal("vmss1", result.VmssList[0].Name);
        Assert.Equal("eastus", result.VmssList[0].Location);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsEmptyList_WhenNoVmss()
    {
        // Arrange
        _computeService.ListVmssAsync(
            Arg.Any<string?>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(new List<VmssInfo>());

        var parseResult = _commandDefinition.Parse([
            "--subscription", _knownSubscription
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, ComputeJsonContext.Default.VmssGetListResult);

        Assert.NotNull(result);
        Assert.Empty(result.VmssList);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsSpecificVmss()
    {
        // Arrange
        var vmssInfo = new VmssInfo(
            Name: _knownVmssName,
            Id: "/subscriptions/sub123/resourceGroups/test-rg/providers/Microsoft.Compute/virtualMachineScaleSets/test-vmss",
            Location: "eastus",
            Sku: new VmssSkuInfo("Standard_D2s_v3", "Standard", 5),
            Capacity: 5,
            ProvisioningState: "Succeeded",
            UpgradePolicy: "Manual",
            Overprovision: true,
            Zones: ["1", "2", "3"],
            Tags: new Dictionary<string, string> { { "env", "test" }, { "owner", "team" } }
        );

        _computeService.GetVmssAsync(
            Arg.Is(_knownVmssName),
            Arg.Is(_knownResourceGroup),
            Arg.Is(_knownSubscription),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(vmssInfo);

        var parseResult = _commandDefinition.Parse([
            "--vmss-name", _knownVmssName,
            "--resource-group", _knownResourceGroup,
            "--subscription", _knownSubscription
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, ComputeJsonContext.Default.VmssGetSingleResult);

        Assert.NotNull(result);
        Assert.NotNull(result.Vmss);
        Assert.Equal("test-vmss", result.Vmss.Name);
        Assert.Equal("eastus", result.Vmss.Location);
        Assert.Equal(5, result.Vmss.Capacity);
        Assert.Equal(3, result.Vmss.Zones?.Count);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsVmssVmInstance()
    {
        // Arrange
        var vmssVmInfo = new VmssVmInfo(
            InstanceId: _knownInstanceId,
            Name: "test-vmss_0",
            Id: "/subscriptions/sub123/resourceGroups/test-rg/providers/Microsoft.Compute/virtualMachineScaleSets/test-vmss/virtualMachines/0",
            Location: "eastus",
            VmSize: "Standard_D2s_v3",
            ProvisioningState: "Succeeded",
            OsType: "Linux",
            Zones: ["1"],
            Tags: null
        );

        _computeService.GetVmssVmAsync(
            Arg.Is(_knownVmssName),
            Arg.Is(_knownInstanceId),
            Arg.Is(_knownResourceGroup),
            Arg.Is(_knownSubscription),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(vmssVmInfo);

        var parseResult = _commandDefinition.Parse([
            "--vmss-name", _knownVmssName,
            "--instance-id", _knownInstanceId,
            "--resource-group", _knownResourceGroup,
            "--subscription", _knownSubscription
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, ComputeJsonContext.Default.VmssGetVmInstanceResult);

        Assert.NotNull(result);
        Assert.NotNull(result.VmInstance);
        Assert.Equal("test-vmss_0", result.VmInstance.Name);
        Assert.Equal("0", result.VmInstance.InstanceId);
        Assert.Equal("Succeeded", result.VmInstance.ProvisioningState);
    }

    [Fact]
    public async Task ExecuteAsync_DeserializationValidation()
    {
        // Arrange
        var vmssInfo = new VmssInfo(
            Name: _knownVmssName,
            Id: "/subscriptions/sub123/resourceGroups/test-rg/providers/Microsoft.Compute/virtualMachineScaleSets/test-vmss",
            Location: "eastus",
            Sku: new VmssSkuInfo("Standard_D2s_v3", "Standard", 3),
            Capacity: 3,
            ProvisioningState: "Succeeded",
            UpgradePolicy: "Manual",
            Overprovision: true,
            Zones: null,
            Tags: null
        );

        _computeService.GetVmssAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(vmssInfo);

        var parseResult = _commandDefinition.Parse([
            "--vmss-name", _knownVmssName,
            "--resource-group", _knownResourceGroup,
            "--subscription", _knownSubscription
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response.Results);
        var json = JsonSerializer.Serialize(response.Results);

        // Verify deserialization works
        var result = JsonSerializer.Deserialize(json, ComputeJsonContext.Default.VmssGetSingleResult);
        Assert.NotNull(result);
        Assert.NotNull(result.Vmss);
        Assert.Equal("test-vmss", result.Vmss.Name);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesVmssNotFoundException()
    {
        // Arrange
        var notFoundException = new RequestFailedException((int)HttpStatusCode.NotFound, "Virtual machine scale set not found");

        _computeService.GetVmssAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(notFoundException);

        var parseResult = _commandDefinition.Parse([
            "--vmss-name", "nonexistent-vmss",
            "--resource-group", _knownResourceGroup,
            "--subscription", _knownSubscription
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.Status);
        Assert.Contains("not found", response.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesVmssVmNotFoundException()
    {
        // Arrange
        var notFoundException = new RequestFailedException((int)HttpStatusCode.NotFound, "Virtual machine instance not found");

        _computeService.GetVmssVmAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(notFoundException);

        var parseResult = _commandDefinition.Parse([
            "--vmss-name", _knownVmssName,
            "--instance-id", "999",
            "--resource-group", _knownResourceGroup,
            "--subscription", _knownSubscription
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.Status);
        Assert.Contains("not found", response.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesForbiddenException()
    {
        // Arrange
        var forbiddenException = new RequestFailedException((int)HttpStatusCode.Forbidden, "Authorization failed");

        _computeService.ListVmssAsync(
            Arg.Any<string?>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(forbiddenException);

        var parseResult = _commandDefinition.Parse([
            "--subscription", _knownSubscription
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.Status);
        Assert.Contains("Authorization failed", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesGenericException()
    {
        // Arrange
        var exception = new Exception("Unexpected error");

        _computeService.GetVmssAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(exception);

        var parseResult = _commandDefinition.Parse([
            "--vmss-name", _knownVmssName,
            "--resource-group", _knownResourceGroup,
            "--subscription", _knownSubscription
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.StartsWith("Unexpected error", response.Message);
    }

    // Note: BindOptions is protected and tested implicitly through ExecuteAsync tests

    [Theory]
    [InlineData("--vmss-name test-vmss --subscription sub123")] // Missing resource-group
    [InlineData("--instance-id 0 --subscription sub123")] // instance-id without vmss-name
    [InlineData("--instance-id 0 --resource-group test-rg --subscription sub123")] // instance-id without vmss-name
    public async Task ExecuteAsync_CustomValidation_ReturnsError(string args)
    {
        // Arrange
        var parseResult = _commandDefinition.Parse(args);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.False(string.IsNullOrEmpty(response.Message));
        Assert.True(
            response.Message.Contains("required", StringComparison.OrdinalIgnoreCase) ||
            response.Message.Contains("instance-id", StringComparison.OrdinalIgnoreCase) ||
            response.Message.Contains("vmss-name", StringComparison.OrdinalIgnoreCase) ||
            response.Message.Contains("resource-group", StringComparison.OrdinalIgnoreCase),
            $"Expected error message to contain validation error, but got: {response.Message}");
    }
}
