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

public class VmGetCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IComputeService _computeService;
    private readonly ILogger<VmGetCommand> _logger;
    private readonly VmGetCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;
    private readonly string _knownSubscription = "sub123";
    private readonly string _knownResourceGroup = "test-rg";
    private readonly string _knownVmName = "test-vm";

    public VmGetCommandTests()
    {
        _computeService = Substitute.For<IComputeService>();
        _logger = Substitute.For<ILogger<VmGetCommand>>();

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
    [InlineData("--subscription sub123", true)] // List all VMs in subscription
    [InlineData("--subscription sub123 --resource-group test-rg", true)] // List VMs in resource group
    [InlineData("--vm-name test-vm --resource-group test-rg --subscription sub123", true)] // Get specific VM
    [InlineData("--vm-name test-vm --resource-group test-rg --subscription sub123 --instance-view", true)] // Get specific VM with instance view
    [InlineData("--vm-name test-vm --subscription sub123", false)] // Missing resource-group (required with vm-name)
    [InlineData("--instance-view --subscription sub123", false)] // instance-view without vm-name
    [InlineData("--instance-view --resource-group test-rg --subscription sub123", false)] // instance-view without vm-name
    [InlineData("", false)] // No parameters
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        // Arrange
        if (shouldSucceed)
        {
            var vmInfo = new VmInfo(
                Id: "/subscriptions/sub123/resourceGroups/test-rg/providers/Microsoft.Compute/virtualMachines/test-vm",
                Name: "test-vm",
                Location: "eastus",
                VmSize: "Standard_D2s_v3",
                ProvisioningState: "Succeeded",
                OsType: "Linux",
                LicenseType: null,
                Zones: new List<string> { "1" },
                Tags: new Dictionary<string, string> { { "env", "test" } }
            );

            var vmList = new List<VmInfo> { vmInfo };

            var instanceView = new VmInstanceView(
                Name: "test-vm",
                PowerState: "running",
                ProvisioningState: "Succeeded",
                VmAgent: new VmAgentInfo("2.7.0", null),
                Disks: new List<DiskInstanceView> { new("Disk0", null) },
                Extensions: new List<ExtensionInstanceView>(),
                Statuses: null
            );

            // Setup mocks based on which scenario
            if (args.Contains("--vm-name") && args.Contains("--instance-view"))
            {
                _computeService.GetVmWithInstanceViewAsync(
                    Arg.Any<string>(),
                    Arg.Any<string>(),
                    Arg.Any<string>(),
                    Arg.Any<string?>(),
                    Arg.Any<RetryPolicyOptions?>(),
                    Arg.Any<CancellationToken>())
                    .Returns((vmInfo, instanceView));
            }
            else if (args.Contains("--vm-name"))
            {
                _computeService.GetVmAsync(
                    Arg.Any<string>(),
                    Arg.Any<string>(),
                    Arg.Any<string>(),
                    Arg.Any<string?>(),
                    Arg.Any<RetryPolicyOptions?>(),
                    Arg.Any<CancellationToken>())
                    .Returns(vmInfo);
            }
            else
            {
                _computeService.ListVmsAsync(
                    Arg.Any<string?>(),
                    Arg.Any<string>(),
                    Arg.Any<string?>(),
                    Arg.Any<RetryPolicyOptions?>(),
                    Arg.Any<CancellationToken>())
                    .Returns(vmList);
            }
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
            // Different error messages depending on validation failure
            // instance-view scenarios have a specific error message
            Assert.False(string.IsNullOrEmpty(response.Message));
            Assert.True(
                response.Message.Contains("required", StringComparison.OrdinalIgnoreCase) ||
                response.Message.Contains("instance-view", StringComparison.OrdinalIgnoreCase),
                $"Expected error message to contain 'required' or 'instance-view', but got: {response.Message}");
        }
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsVmList_WhenListingSubscription()
    {
        // Arrange
        var expectedVms = new List<VmInfo>
        {
            new(
                Id: "/subscriptions/sub123/resourceGroups/rg1/providers/Microsoft.Compute/virtualMachines/vm1",
                Name: "vm1",
                Location: "eastus",
                VmSize: "Standard_D2s_v3",
                ProvisioningState: "Succeeded",
                OsType: "Linux",
                LicenseType: null,
                Zones: null,
                Tags: null
            ),
            new(
                Id: "/subscriptions/sub123/resourceGroups/rg2/providers/Microsoft.Compute/virtualMachines/vm2",
                Name: "vm2",
                Location: "westus",
                VmSize: "Standard_B2s",
                ProvisioningState: "Succeeded",
                OsType: "Windows",
                LicenseType: "Windows_Server",
                Zones: null,
                Tags: null
            )
        };

        _computeService.ListVmsAsync(
            Arg.Is<string?>(x => x == null),
            Arg.Is(_knownSubscription),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(expectedVms);

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
        var result = JsonSerializer.Deserialize(json, ComputeJsonContext.Default.VmGetListResult);

        Assert.NotNull(result);
        Assert.Equal(2, result.Vms.Count);
        Assert.Equal("vm1", result.Vms[0].Name);
        Assert.Equal("vm2", result.Vms[1].Name);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsVmList_WhenListingResourceGroup()
    {
        // Arrange
        var expectedVms = new List<VmInfo>
        {
            new(
                Id: "/subscriptions/sub123/resourceGroups/test-rg/providers/Microsoft.Compute/virtualMachines/vm1",
                Name: "vm1",
                Location: "eastus",
                VmSize: "Standard_D2s_v3",
                ProvisioningState: "Succeeded",
                OsType: "Linux",
                LicenseType: null,
                Zones: new List<string> { "1" },
                Tags: new Dictionary<string, string> { { "env", "prod" } }
            )
        };

        _computeService.ListVmsAsync(
            Arg.Is(_knownResourceGroup),
            Arg.Is(_knownSubscription),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(expectedVms);

        var parseResult = _commandDefinition.Parse([
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
        var result = JsonSerializer.Deserialize(json, ComputeJsonContext.Default.VmGetListResult);

        Assert.NotNull(result);
        Assert.Single(result.Vms);
        Assert.Equal("vm1", result.Vms[0].Name);
        Assert.Equal("eastus", result.Vms[0].Location);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsEmptyList_WhenNoVms()
    {
        // Arrange
        _computeService.ListVmsAsync(
            Arg.Any<string?>(),
            Arg.Is(_knownSubscription),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(new List<VmInfo>());

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
        var result = JsonSerializer.Deserialize(json, ComputeJsonContext.Default.VmGetListResult);

        Assert.NotNull(result);
        Assert.Empty(result.Vms);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsSpecificVm_WithoutInstanceView()
    {
        // Arrange
        var expectedVm = new VmInfo(
            Id: "/subscriptions/sub123/resourceGroups/test-rg/providers/Microsoft.Compute/virtualMachines/test-vm",
            Name: "test-vm",
            Location: "eastus",
            VmSize: "Standard_D2s_v3",
            ProvisioningState: "Succeeded",
            OsType: "Linux",
            LicenseType: null,
            Zones: new List<string> { "1", "2" },
            Tags: new Dictionary<string, string> { { "env", "test" }, { "owner", "team" } }
        );

        _computeService.GetVmAsync(
            Arg.Is(_knownVmName),
            Arg.Is(_knownResourceGroup),
            Arg.Is(_knownSubscription),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(expectedVm);

        var parseResult = _commandDefinition.Parse([
            "--vm-name", _knownVmName,
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
        var result = JsonSerializer.Deserialize(json, ComputeJsonContext.Default.VmGetSingleResult);

        Assert.NotNull(result);
        Assert.NotNull(result.Vm);
        Assert.Null(result.InstanceView);
        Assert.Equal("test-vm", result.Vm.Name);
        Assert.Equal("eastus", result.Vm.Location);
        Assert.Equal(2, result.Vm.Zones?.Count);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsSpecificVm_WithInstanceView()
    {
        // Arrange
        var vmInfo = new VmInfo(
            Id: "/subscriptions/sub123/resourceGroups/test-rg/providers/Microsoft.Compute/virtualMachines/test-vm",
            Name: "test-vm",
            Location: "eastus",
            VmSize: "Standard_D2s_v3",
            ProvisioningState: "Succeeded",
            OsType: "Linux",
            LicenseType: null,
            Zones: new List<string> { "1" },
            Tags: new Dictionary<string, string> { { "env", "test" } }
        );

        var instanceView = new VmInstanceView(
            Name: "test-vm",
            PowerState: "running",
            ProvisioningState: "Succeeded",
            VmAgent: new VmAgentInfo("2.7.0", null),
            Disks: new List<DiskInstanceView>
            {
                new("Disk0", null),
                new("Disk1", null)
            },
            Extensions: new List<ExtensionInstanceView>
            {
                new("AzureMonitorLinuxAgent", "Microsoft.Azure.Monitor", "1.0", null)
            },
            Statuses: null
        );

        _computeService.GetVmWithInstanceViewAsync(
            Arg.Is(_knownVmName),
            Arg.Is(_knownResourceGroup),
            Arg.Is(_knownSubscription),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns((vmInfo, instanceView));

        var parseResult = _commandDefinition.Parse([
            "--vm-name", _knownVmName,
            "--resource-group", _knownResourceGroup,
            "--subscription", _knownSubscription,
            "--instance-view"
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, ComputeJsonContext.Default.VmGetSingleResult);

        Assert.NotNull(result);
        Assert.NotNull(result.Vm);
        Assert.NotNull(result.InstanceView);
        Assert.Equal("test-vm", result.Vm.Name);
        Assert.Equal("running", result.InstanceView.PowerState);
        Assert.Equal("2.7.0", result.InstanceView.VmAgent?.VmAgentVersion);
        Assert.Equal(2, result.InstanceView.Disks?.Count);
        Assert.Single(result.InstanceView.Extensions!);
    }

    [Fact]
    public async Task ExecuteAsync_DeserializationValidation()
    {
        // Arrange
        var vmInfo = new VmInfo(
            Id: "/subscriptions/sub123/resourceGroups/test-rg/providers/Microsoft.Compute/virtualMachines/test-vm",
            Name: "test-vm",
            Location: "eastus",
            VmSize: "Standard_D2s_v3",
            ProvisioningState: "Succeeded",
            OsType: "Linux",
            LicenseType: null,
            Zones: null,
            Tags: null
        );

        _computeService.GetVmAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(vmInfo);

        var parseResult = _commandDefinition.Parse([
            "--vm-name", _knownVmName,
            "--resource-group", _knownResourceGroup,
            "--subscription", _knownSubscription
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response.Results);
        var json = JsonSerializer.Serialize(response.Results);

        // Verify deserialization works
        var result = JsonSerializer.Deserialize(json, ComputeJsonContext.Default.VmGetSingleResult);
        Assert.NotNull(result);
        Assert.NotNull(result.Vm);
        Assert.Equal("test-vm", result.Vm.Name);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesVmNotFoundException()
    {
        // Arrange
        var notFoundException = new RequestFailedException((int)HttpStatusCode.NotFound, "Virtual machine not found");

        _computeService.GetVmAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(notFoundException);

        var parseResult = _commandDefinition.Parse([
            "--vm-name", "nonexistent-vm",
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

        _computeService.ListVmsAsync(
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

        _computeService.GetVmAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(exception);

        var parseResult = _commandDefinition.Parse([
            "--vm-name", _knownVmName,
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
    [InlineData("--vm-name test-vm --subscription sub123")] // Missing resource-group
    [InlineData("--instance-view --resource-group test-rg --subscription sub123")] // instance-view without vm-name
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
            response.Message.Contains("instance-view", StringComparison.OrdinalIgnoreCase) ||
            response.Message.Contains("vm-name", StringComparison.OrdinalIgnoreCase),
            $"Expected error message to contain validation error, but got: {response.Message}");
    }
}
