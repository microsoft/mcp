// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.Aks.Commands;
using Azure.Mcp.Tools.Aks.Commands.Nodepool;
using Azure.Mcp.Tools.Aks.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.Aks.UnitTests.Nodepool;

public sealed class NodepoolListCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IAksService _aksService;
    private readonly ILogger<NodepoolListCommand> _logger;
    private readonly NodepoolListCommand _command;

    public NodepoolListCommandTests()
    {
        _aksService = Substitute.For<IAksService>();
        _logger = Substitute.For<ILogger<NodepoolListCommand>>();

        var collection = new ServiceCollection();
        collection.AddSingleton(_aksService);
        _serviceProvider = collection.BuildServiceProvider();

        _command = new(_logger);
    }

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = _command.GetCommand();
        Assert.Equal("list", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
    }

    [Theory]
    [InlineData("--subscription sub123 --resource-group rg1 --cluster c1", true)]
    [InlineData("--subscription sub123 --resource-group rg1 --cluster c1 --tenant t1", true)]
    [InlineData("--subscription sub123 --resource-group rg1", false)] // missing cluster
    [InlineData("--subscription sub123 --cluster c1", false)] // missing rg
    [InlineData("--resource-group rg1 --cluster c1", false)] // missing subscription
    [InlineData("", false)]
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        // Arrange
        if (shouldSucceed)
        {
            var testNodePools = new List<Models.NodePool>
            {
                new() { Name = "np1", Count = 3, VmSize = "Standard_DS2_v2" },
                new() { Name = "np2", Count = 5, VmSize = "Standard_D4s_v5" }
            };
            _aksService.ListNodePools(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>())
                .Returns(testNodePools);
        }

        var context = new CommandContext(_serviceProvider);
        var parseResult = _command.GetCommand().Parse(args);

        // Act
        var response = await _command.ExecuteAsync(context, parseResult);

        // Assert
        Assert.Equal(shouldSucceed ? 200 : 400, response.Status);
        if (shouldSucceed)
        {
            Assert.NotNull(response.Results);
            Assert.Equal("Success", response.Message);
        }
        else
        {
            Assert.Contains("required", response.Message.ToLower());
        }
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsNodePoolsList()
    {
        // Arrange
        var expectedNodePools = new List<Models.NodePool>
        {
            new()
            {
                Name = "systempool",
                Count = 3,
                VmSize = "Standard_DS2_v2",
                OsDiskSizeGB = 128,
                OsDiskType = "Managed",
                KubeletDiskType = "OS",
                MaxPods = 110,
                Type = "VirtualMachineScaleSets",
                MaxCount = 5,
                MinCount = 1,
                EnableAutoScaling = true,
                ScaleDownMode = "Delete",
                ProvisioningState = "Succeeded",
                PowerState = new Models.NodePoolPowerState { Code = "Running" },
                OrchestratorVersion = "1.33.1",
                CurrentOrchestratorVersion = "1.33.1",
                EnableNodePublicIP = false,
                ScaleSetPriority = "Regular",
                ScaleSetEvictionPolicy = null,
                NodeLabels = new Dictionary<string, string> { { "pool", "system" } },
                NodeTaints = new List<string>(),
                Mode = "System",
                OsType = "Linux",
                OsSKU = "Ubuntu",
                NodeImageVersion = "AKSUbuntu-2204gen2containerd-202508.20.1"
            },
            new()
            {
                Name = "userpool",
                Count = 5,
                VmSize = "Standard_D4s_v5",
                OsDiskSizeGB = 256,
                OsDiskType = "Ephemeral",
                KubeletDiskType = "OS",
                MaxPods = 110,
                Type = "VirtualMachineScaleSets",
                MaxCount = 10,
                MinCount = 1,
                EnableAutoScaling = true,
                ScaleDownMode = "Delete",
                ProvisioningState = "Succeeded",
                PowerState = new Models.NodePoolPowerState { Code = "Running" },
                OrchestratorVersion = "1.33.2",
                CurrentOrchestratorVersion = "1.33.2",
                EnableNodePublicIP = false,
                ScaleSetPriority = "Spot",
                ScaleSetEvictionPolicy = "Delete",
                NodeLabels = new Dictionary<string, string> { { "kubernetes.azure.com/scalesetpriority", "spot" } },
                NodeTaints = new List<string> { "kubernetes.azure.com/scalesetpriority=spot:NoSchedule" },
                Mode = "User",
                OsType = "Linux",
                OsSKU = "Ubuntu",
                NodeImageVersion = "AKSUbuntu-2204gen2containerd-202508.20.1"
            }
        };
        _aksService.ListNodePools(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>())
            .Returns(expectedNodePools);

        var context = new CommandContext(_serviceProvider);
        var parseResult = _command.GetCommand().Parse("--subscription sub123 --resource-group rg1 --cluster c1");

        // Act
        var response = await _command.ExecuteAsync(context, parseResult);

        // Assert
        Assert.Equal(200, response.Status);
        Assert.NotNull(response.Results);

        // Verify the mock was called
        await _aksService.Received(1).ListNodePools(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>());

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, AksJsonContext.Default.NodepoolListCommandResult);

        Assert.NotNull(result);
        Assert.Equal(expectedNodePools.Count, result.NodePools.Count);

        // Validate enriched fields for first pool
        Assert.Equal(expectedNodePools[0].Name, result.NodePools[0].Name);
        Assert.Equal(expectedNodePools[0].Count, result.NodePools[0].Count);
        Assert.Equal(expectedNodePools[0].VmSize, result.NodePools[0].VmSize);
        Assert.Equal(expectedNodePools[0].OsDiskSizeGB, result.NodePools[0].OsDiskSizeGB);
        Assert.Equal(expectedNodePools[0].OsDiskType, result.NodePools[0].OsDiskType);
        Assert.Equal(expectedNodePools[0].KubeletDiskType, result.NodePools[0].KubeletDiskType);
        Assert.Equal(expectedNodePools[0].MaxPods, result.NodePools[0].MaxPods);
        Assert.Equal(expectedNodePools[0].Type, result.NodePools[0].Type);
        Assert.Equal(expectedNodePools[0].MaxCount, result.NodePools[0].MaxCount);
        Assert.Equal(expectedNodePools[0].MinCount, result.NodePools[0].MinCount);
        Assert.Equal(expectedNodePools[0].EnableAutoScaling, result.NodePools[0].EnableAutoScaling);
        Assert.Equal(expectedNodePools[0].ScaleDownMode, result.NodePools[0].ScaleDownMode);
        Assert.Equal(expectedNodePools[0].ProvisioningState, result.NodePools[0].ProvisioningState);
        Assert.Equal(expectedNodePools[0].PowerState?.Code, result.NodePools[0].PowerState?.Code);
        Assert.Equal(expectedNodePools[0].OrchestratorVersion, result.NodePools[0].OrchestratorVersion);
        Assert.Equal(expectedNodePools[0].CurrentOrchestratorVersion, result.NodePools[0].CurrentOrchestratorVersion);
        Assert.Equal(expectedNodePools[0].EnableNodePublicIP, result.NodePools[0].EnableNodePublicIP);
        Assert.Equal(expectedNodePools[0].ScaleSetPriority, result.NodePools[0].ScaleSetPriority);
        Assert.Equal(expectedNodePools[0].ScaleSetEvictionPolicy, result.NodePools[0].ScaleSetEvictionPolicy);
        Assert.Equal(expectedNodePools[0].NodeLabels!["pool"], result.NodePools[0].NodeLabels!["pool"]);
        Assert.Equal(expectedNodePools[0].NodeTaints!.Count, result.NodePools[0].NodeTaints!.Count);
        Assert.Equal(expectedNodePools[0].Mode, result.NodePools[0].Mode);
        Assert.Equal(expectedNodePools[0].OsType, result.NodePools[0].OsType);
        Assert.Equal(expectedNodePools[0].OsSKU, result.NodePools[0].OsSKU);
        Assert.Equal(expectedNodePools[0].NodeImageVersion, result.NodePools[0].NodeImageVersion);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsEmptyWhenNoNodePools()
    {
        // Arrange
        _aksService.ListNodePools(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>())
            .Returns([]);

        var context = new CommandContext(_serviceProvider);
        var parseResult = _command.GetCommand().Parse("--subscription sub123 --resource-group rg1 --cluster c1");

        // Act
        var response = await _command.ExecuteAsync(context, parseResult);

        // Assert
        Assert.Equal(200, response.Status);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, AksJsonContext.Default.NodepoolListCommandResult);

        Assert.NotNull(result);
        Assert.Empty(result.NodePools);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        // Arrange
        _aksService.ListNodePools(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>())
            .Returns(Task.FromException<List<Models.NodePool>>(new Exception("Test error")));

        var context = new CommandContext(_serviceProvider);
        var parseResult = _command.GetCommand().Parse("--subscription sub123 --resource-group rg1 --cluster c1");

        // Act
        var response = await _command.ExecuteAsync(context, parseResult);

        // Assert
        Assert.Equal(500, response.Status);
        Assert.Contains("Test error", response.Message);
        Assert.Contains("troubleshooting", response.Message);
    }
}
