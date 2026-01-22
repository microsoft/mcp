// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Net;
using System.Text.Json;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Tests;
using Azure.Mcp.Tools.Compute.Commands;
using Azure.Mcp.Tools.Compute.Commands.Disk;
using Azure.Mcp.Tools.Compute.Models;
using Azure.Mcp.Tools.Compute.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Models.Command;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.Compute.UnitTests.Disk;

/// <summary>
/// Unit tests for the DiskGetCommand.
/// </summary>
public class DiskGetCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IComputeService _computeService;
    private readonly ILogger<DiskGetCommand> _logger;
    private readonly DiskGetCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;

    public DiskGetCommandTests()
    {
        _computeService = Substitute.For<IComputeService>();
        _logger = Substitute.For<ILogger<DiskGetCommand>>();

        // Set up default empty returns to avoid null reference issues
        _computeService.ListDisksAsync(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .ReturnsForAnyArgs(new List<Models.Disk>());

        var collection = new ServiceCollection().AddSingleton(_computeService);

        _serviceProvider = collection.BuildServiceProvider();
        _command = new(_logger, _computeService);
        _context = new(_serviceProvider);
        _commandDefinition = _command.GetCommand();
    }

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        // Arrange & Act
        // Command already created in constructor

        // Assert
        Assert.NotNull(_command);
        Assert.Equal("get", _command.Name);
        Assert.Contains("disk", _command.Description, StringComparison.OrdinalIgnoreCase);
        Assert.NotEqual(Guid.Empty.ToString(), _command.Id.ToString());
    }

    [Fact]
    public async Task ExecuteAsync_ListAllDisks_ReturnsSuccess()
    {
        // Arrange
        var subscription = "test-sub";

        var mockDisks = new List<Models.Disk>
        {
            new()
            {
                Name = "disk1",
                Id = $"/subscriptions/{subscription}/resourceGroups/rg1/providers/Microsoft.Compute/disks/disk1",
                ResourceGroup = "rg1",
                Location = "eastus",
                SkuName = "Premium_LRS",
                DiskSizeGB = 128,
                DiskState = "Unattached"
            },
            new()
            {
                Name = "disk2",
                Id = $"/subscriptions/{subscription}/resourceGroups/rg2/providers/Microsoft.Compute/disks/disk2",
                ResourceGroup = "rg2",
                Location = "westus",
                SkuName = "Standard_LRS",
                DiskSizeGB = 256,
                DiskState = "Attached"
            }
        };

        _computeService.ListDisksAsync(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .ReturnsForAnyArgs(mockDisks);

        var args = _commandDefinition.Parse(["--subscription", subscription]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, ComputeJsonContext.Default.DiskGetCommandResult);

        Assert.NotNull(result);
        Assert.NotNull(result.Disks);
        Assert.Equal(2, result.Disks.Count);
        Assert.Contains(result.Disks, d => d.Name == "disk1");
        Assert.Contains(result.Disks, d => d.Name == "disk2");
    }

    [Fact]
    public async Task ExecuteAsync_ListDisksInResourceGroup_ReturnsSuccess()
    {
        // Arrange
        var subscription = "test-sub";
        var resourceGroup = "testrg";

        var mockDisks = new List<Models.Disk>
        {
            new()
            {
                Name = "disk1",
                Id = $"/subscriptions/{subscription}/resourceGroups/{resourceGroup}/providers/Microsoft.Compute/disks/disk1",
                ResourceGroup = resourceGroup,
                Location = "eastus",
                SkuName = "Premium_LRS",
                DiskSizeGB = 128,
                DiskState = "Unattached"
            }
        };

        _computeService.ListDisksAsync(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .ReturnsForAnyArgs(mockDisks);

        var args = _commandDefinition.Parse(["--subscription", subscription, "--resource-group", resourceGroup]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, ComputeJsonContext.Default.DiskGetCommandResult);

        Assert.NotNull(result);
        Assert.NotNull(result.Disks);
        Assert.Single(result.Disks);
        Assert.Equal("disk1", result.Disks[0].Name);
        Assert.Equal(resourceGroup, result.Disks[0].ResourceGroup);
    }

    [Fact]
    public async Task ExecuteAsync_SpecificDisk_ReturnsSuccess()
    {
        // Arrange
        var subscription = "test-sub";
        var diskName = "testdisk";
        var resourceGroup = "testrg";

        var mockDisk = new Models.Disk
        {
            Name = diskName,
            Id = $"/subscriptions/{subscription}/resourceGroups/{resourceGroup}/providers/Microsoft.Compute/disks/{diskName}",
            ResourceGroup = resourceGroup,
            Location = "eastus",
            SkuName = "Premium_LRS",
            DiskSizeGB = 128,
            DiskState = "Unattached",
            TimeCreated = DateTimeOffset.UtcNow
        };

        _computeService.GetDiskAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .ReturnsForAnyArgs(mockDisk);

        var args = _commandDefinition.Parse(["--subscription", subscription, "--disk", diskName, "--resource-group", resourceGroup]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, ComputeJsonContext.Default.DiskGetCommandResult);

        Assert.NotNull(result);
        Assert.NotNull(result.Disks);
        Assert.Single(result.Disks);
        Assert.Equal(diskName, result.Disks[0].Name);
        Assert.Equal(resourceGroup, result.Disks[0].ResourceGroup);
    }



    [Fact]
    public async Task ExecuteAsync_DeserializationValidation()
    {
        // Arrange
        var subscription = "test-sub";
        var diskName = "testdisk";
        var resourceGroup = "testrg";

        var mockDisk = new Models.Disk
        {
            Name = diskName,
            Id = $"/subscriptions/{subscription}/resourceGroups/{resourceGroup}/providers/Microsoft.Compute/disks/{diskName}",
            ResourceGroup = resourceGroup,
            Location = "westus",
            SkuName = "Standard_LRS",
            DiskSizeGB = 64
        };

        _computeService.GetDiskAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .ReturnsForAnyArgs(mockDisk);

        var args = _commandDefinition.Parse(["--subscription", subscription, "--disk", diskName, "--resource-group", resourceGroup]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, ComputeJsonContext.Default.DiskGetCommandResult);

        Assert.NotNull(result);
        Assert.NotNull(result.Disks);
        Assert.Single(result.Disks);
        Assert.Equal(mockDisk.Name, result.Disks[0].Name);
        Assert.Equal(mockDisk.Location, result.Disks[0].Location);
        Assert.Equal(mockDisk.SkuName, result.Disks[0].SkuName);
        Assert.Equal(mockDisk.DiskSizeGB, result.Disks[0].DiskSizeGB);
    }

    [Fact]
    public async Task ExecuteAsync_WildcardPatternWithoutResourceGroup_ReturnsFilteredDisks()
    {
        // Arrange
        var subscription = "test-sub";
        var diskPattern = "win_OsDisk*";

        var mockDisks = new List<Models.Disk>
        {
            new()
            {
                Name = "win_OsDisk1",
                Id = $"/subscriptions/{subscription}/resourceGroups/rg1/providers/Microsoft.Compute/disks/win_OsDisk1",
                ResourceGroup = "rg1",
                Location = "eastus",
                SkuName = "Premium_LRS",
                DiskSizeGB = 128
            },
            new()
            {
                Name = "win_OsDisk2",
                Id = $"/subscriptions/{subscription}/resourceGroups/rg2/providers/Microsoft.Compute/disks/win_OsDisk2",
                ResourceGroup = "rg2",
                Location = "westus",
                SkuName = "Standard_LRS",
                DiskSizeGB = 256
            },
            new()
            {
                Name = "linux-disk",
                Id = $"/subscriptions/{subscription}/resourceGroups/rg3/providers/Microsoft.Compute/disks/linux-disk",
                ResourceGroup = "rg3",
                Location = "eastus",
                SkuName = "Premium_LRS",
                DiskSizeGB = 64
            }
        };

        _computeService.ListDisksAsync(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .ReturnsForAnyArgs(mockDisks);

        var args = _commandDefinition.Parse(["--subscription", subscription, "--disk", diskPattern]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, ComputeJsonContext.Default.DiskGetCommandResult);

        Assert.NotNull(result);
        Assert.NotNull(result.Disks);
        Assert.Equal(2, result.Disks.Count);
        Assert.Contains(result.Disks, d => d.Name == "win_OsDisk1");
        Assert.Contains(result.Disks, d => d.Name == "win_OsDisk2");
        Assert.DoesNotContain(result.Disks, d => d.Name == "linux-disk");
    }

    [Fact]
    public async Task ExecuteAsync_WildcardPatternWithResourceGroup_ReturnsFilteredDisksInResourceGroup()
    {
        // Arrange
        var subscription = "test-sub";
        var resourceGroup = "testrg";
        var diskPattern = "data*";

        var mockDisks = new List<Models.Disk>
        {
            new()
            {
                Name = "datadisk1",
                Id = $"/subscriptions/{subscription}/resourceGroups/{resourceGroup}/providers/Microsoft.Compute/disks/datadisk1",
                ResourceGroup = resourceGroup,
                Location = "eastus",
                SkuName = "Premium_LRS",
                DiskSizeGB = 128
            },
            new()
            {
                Name = "datadisk2",
                Id = $"/subscriptions/{subscription}/resourceGroups/{resourceGroup}/providers/Microsoft.Compute/disks/datadisk2",
                ResourceGroup = resourceGroup,
                Location = "eastus",
                SkuName = "Standard_LRS",
                DiskSizeGB = 256
            },
            new()
            {
                Name = "osdisk",
                Id = $"/subscriptions/{subscription}/resourceGroups/{resourceGroup}/providers/Microsoft.Compute/disks/osdisk",
                ResourceGroup = resourceGroup,
                Location = "eastus",
                SkuName = "Premium_LRS",
                DiskSizeGB = 64
            }
        };

        _computeService.ListDisksAsync(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .ReturnsForAnyArgs(mockDisks);

        var args = _commandDefinition.Parse(["--subscription", subscription, "--disk", diskPattern, "--resource-group", resourceGroup]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, ComputeJsonContext.Default.DiskGetCommandResult);

        Assert.NotNull(result);
        Assert.NotNull(result.Disks);
        Assert.Equal(2, result.Disks.Count);
        Assert.Contains(result.Disks, d => d.Name == "datadisk1");
        Assert.Contains(result.Disks, d => d.Name == "datadisk2");
        Assert.DoesNotContain(result.Disks, d => d.Name == "osdisk");
    }

    [Fact]
    public async Task ExecuteAsync_ExactDiskNameWithoutResourceGroup_ReturnsFilteredDisks()
    {
        // Arrange - When disk name is exact but no resource group, should list and filter
        var subscription = "test-sub";
        var diskName = "exactdisk";

        var mockDisks = new List<Models.Disk>
        {
            new()
            {
                Name = "exactdisk",
                Id = $"/subscriptions/{subscription}/resourceGroups/rg1/providers/Microsoft.Compute/disks/exactdisk",
                ResourceGroup = "rg1",
                Location = "eastus",
                SkuName = "Premium_LRS",
                DiskSizeGB = 128
            },
            new()
            {
                Name = "otherdisk",
                Id = $"/subscriptions/{subscription}/resourceGroups/rg2/providers/Microsoft.Compute/disks/otherdisk",
                ResourceGroup = "rg2",
                Location = "westus",
                SkuName = "Standard_LRS",
                DiskSizeGB = 256
            }
        };

        _computeService.ListDisksAsync(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .ReturnsForAnyArgs(mockDisks);

        var args = _commandDefinition.Parse(["--subscription", subscription, "--disk", diskName]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, ComputeJsonContext.Default.DiskGetCommandResult);

        Assert.NotNull(result);
        Assert.NotNull(result.Disks);
        Assert.Single(result.Disks);
        Assert.Equal("exactdisk", result.Disks[0].Name);
    }
}

