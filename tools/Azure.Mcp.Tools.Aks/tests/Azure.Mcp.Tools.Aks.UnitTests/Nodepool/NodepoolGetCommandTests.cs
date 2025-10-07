// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
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

public sealed class NodepoolGetCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IAksService _aksService;
    private readonly ILogger<NodepoolGetCommand> _logger;
    private readonly NodepoolGetCommand _command;

    public NodepoolGetCommandTests()
    {
        _aksService = Substitute.For<IAksService>();
        _logger = Substitute.For<ILogger<NodepoolGetCommand>>();

        var collection = new ServiceCollection();
        collection.AddSingleton(_aksService);
        _serviceProvider = collection.BuildServiceProvider();

        _command = new(_logger);
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
    [InlineData("--subscription sub123 --resource-group rg1 --cluster c1 --nodepool np1", true)]
    [InlineData("--subscription sub123 --resource-group rg1 --cluster c1 --nodepool np1 --tenant t1", true)]
    [InlineData("--subscription sub123 --resource-group rg1 --nodepool np1", false)] // missing cluster
    [InlineData("--subscription sub123 --cluster c1 --nodepool np1", false)] // missing rg
    [InlineData("--resource-group rg1 --cluster c1 --nodepool np1", false)] // missing subscription
    [InlineData("", false)]
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        // Arrange
        if (shouldSucceed)
        {
            _aksService.GetNodePools(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string?>(),
                Arg.Any<string?>(),
                Arg.Any<RetryPolicyOptions>())
                .Returns([]);
        }

        var context = new CommandContext(_serviceProvider);
        var parseResult = _command.GetCommand().Parse(args);

        // Act
        var response = await _command.ExecuteAsync(context, parseResult);

        // Assert
        Assert.Equal(shouldSucceed ? HttpStatusCode.OK : HttpStatusCode.BadRequest, response.Status);
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
                PowerState = new() { Code = "Running" },
                OrchestratorVersion = "1.33.1",
                CurrentOrchestratorVersion = "1.33.1",
                EnableNodePublicIP = false,
                ScaleSetPriority = "Regular",
                ScaleSetEvictionPolicy = null,
                NodeLabels = new Dictionary<string, string> { { "pool", "system" } },
                NodeTaints = [],
                Mode = "System",
                OsType = "Linux",
                OsSKU = "Ubuntu",
                NodeImageVersion = "AKSUbuntu-2204gen2containerd-202508.20.1",
                Tags = new Dictionary<string,string> { ["env"] = "test" },
                SpotMaxPrice = -1,
                WorkloadRuntime = "OCIContainer",
                EnableEncryptionAtHost = true,
                UpgradeSettings = new() { MaxSurge = "10%", MaxUnavailable = "0" },
                SecurityProfile = new() { EnableVTPM = false, EnableSecureBoot = false },
                GpuProfile = new() { Driver = "Install" },
                NetworkProfile = new()
                {
                    AllowedHostPorts = [new() { StartPort = 80, EndPort = 80 }],
                    ApplicationSecurityGroups = ["/subscriptions/s/rg/r/providers/Microsoft.Network/applicationSecurityGroups/asg1"],
                    NodePublicIPTags = [new() { IpTagType = "FirstPartyUsage", Tag = "foo" }]
                },
                PodSubnetId = "/subscriptions/s/rg/r/providers/Microsoft.Network/virtualNetworks/vnet/subnets/podsubnet",
                VnetSubnetId = "/subscriptions/s/rg/r/providers/Microsoft.Network/virtualNetworks/vnet/subnets/nodesubnet"
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
                PowerState = new() { Code = "Running" },
                OrchestratorVersion = "1.33.2",
                CurrentOrchestratorVersion = "1.33.2",
                EnableNodePublicIP = false,
                ScaleSetPriority = "Spot",
                ScaleSetEvictionPolicy = "Delete",
                NodeLabels = new Dictionary<string, string> { { "kubernetes.azure.com/scalesetpriority", "spot" } },
                NodeTaints = ["kubernetes.azure.com/scalesetpriority=spot:NoSchedule"],
                Mode = "User",
                OsType = "Linux",
                OsSKU = "Ubuntu",
                NodeImageVersion = "AKSUbuntu-2204gen2containerd-202508.20.1"
            }
        };
        _aksService.GetNodePools(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions>())
            .Returns(expectedNodePools);

        var context = new CommandContext(_serviceProvider);
        var parseResult = _command.GetCommand().Parse("--subscription sub123 --resource-group rg1 --cluster c1");

        // Act
        var response = await _command.ExecuteAsync(context, parseResult);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);

        // Verify the mock was called
        await _aksService.Received(1).GetNodePools(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions>());

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, AksJsonContext.Default.NodepoolGetCommandResult);

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
        Assert.Equal("test", result.NodePools[0].Tags!["env"]);
        Assert.Equal(-1, result.NodePools[0].SpotMaxPrice);
        Assert.True(result.NodePools[0].EnableEncryptionAtHost);
        Assert.Equal("Install", result.NodePools[0].GpuProfile?.Driver);
        Assert.Equal(1, result.NodePools[0].NetworkProfile?.AllowedHostPorts?.Count);
        Assert.Equal("/subscriptions/s/rg/r/providers/Microsoft.Network/virtualNetworks/vnet/subnets/podsubnet", result.NodePools[0].PodSubnetId);
        Assert.Equal("/subscriptions/s/rg/r/providers/Microsoft.Network/virtualNetworks/vnet/subnets/nodesubnet", result.NodePools[0].VnetSubnetId);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsEmptyWhenNoNodePools()
    {
        // Arrange
        _aksService.GetNodePools(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions>())
            .Returns([]);

        var context = new CommandContext(_serviceProvider);
        var parseResult = _command.GetCommand().Parse("--subscription sub123 --resource-group rg1 --cluster c1");

        // Act
        var response = await _command.ExecuteAsync(context, parseResult);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, AksJsonContext.Default.NodepoolGetCommandResult);

        Assert.NotNull(result);
        Assert.Empty(result.NodePools);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        // Arrange
        _aksService.GetNodePools(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions>())
            .Returns(Task.FromException<List<Models.NodePool>>(new Exception("Test error")));

        var context = new CommandContext(_serviceProvider);
        var parseResult = _command.GetCommand().Parse("--subscription sub123 --resource-group rg1 --cluster c1");

        // Act
        var response = await _command.ExecuteAsync(context, parseResult);

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Test error", response.Message);
        Assert.Contains("troubleshooting", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsNodePool()
    {
        // Arrange
        var expectedNodePool = new Models.NodePool
        {
            Name = "userpool",
            Count = 1,
            VmSize = "Standard_NC24ads_A100_v4",
            OsDiskSizeGB = 256,
            OsDiskType = "Ephemeral",
            KubeletDiskType = "OS",
            MaxPods = 110,
            Type = "VirtualMachineScaleSets",
            MaxCount = 5,
            MinCount = 1,
            EnableAutoScaling = true,
            ScaleDownMode = "Delete",
            ProvisioningState = "Succeeded",
            PowerState = new() { Code = "Running" },
            OrchestratorVersion = "1.33.2",
            CurrentOrchestratorVersion = "1.33.2",
            EnableNodePublicIP = false,
            ScaleSetPriority = "Spot",
            ScaleSetEvictionPolicy = "Delete",
            NodeLabels = new Dictionary<string, string> { { "kubernetes.azure.com/scalesetpriority", "spot" } },
            NodeTaints = ["kubernetes.azure.com/scalesetpriority=spot:NoSchedule"],
            Mode = "User",
            OsType = "Linux",
            OsSKU = "Ubuntu",
            NodeImageVersion = "AKSUbuntu-2204gen2containerd-202508.20.1",
            Tags = new Dictionary<string, string> { ["gc_skip"] = "true" },
            SpotMaxPrice = -1,
            WorkloadRuntime = "OCIContainer",
            EnableEncryptionAtHost = false,
            UpgradeSettings = new() { MaxSurge = "10%", MaxUnavailable = "0" },
            SecurityProfile = new() { EnableVTPM = false, EnableSecureBoot = false },
            GpuProfile = new() { Driver = "Install" },
            NetworkProfile = new()
            {
                AllowedHostPorts = [new() { StartPort = 8080, EndPort = 8080 }],
                ApplicationSecurityGroups = ["/subscriptions/s/rg/r/providers/Microsoft.Network/applicationSecurityGroups/asg1"],
                NodePublicIPTags = [new() { IpTagType = "FirstPartyUsage", Tag = "foo" }]
            },
            PodSubnetId = "/subscriptions/s/rg/r/providers/Microsoft.Network/virtualNetworks/vnet/subnets/podsubnet",
            VnetSubnetId = "/subscriptions/s/rg/r/providers/Microsoft.Network/virtualNetworks/vnet/subnets/nodesubnet"
        };
        _aksService.GetNodePools(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>())
            .Returns([expectedNodePool]);

        var context = new CommandContext(_serviceProvider);
        var parseResult = _command.GetCommand().Parse("--subscription sub123 --resource-group rg1 --cluster c1 --nodepool userpool");

        // Act
        var response = await _command.ExecuteAsync(context, parseResult);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);

        await _aksService.Received(1).GetNodePools(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>());

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, AksJsonContext.Default.NodepoolGetCommandResult);

        Assert.NotNull(result);
        Assert.Single(result.NodePools);
        Assert.Equal(expectedNodePool.Name, result.NodePools[0].Name);
        Assert.Equal(expectedNodePool.Count, result.NodePools[0].Count);
        Assert.Equal(expectedNodePool.VmSize, result.NodePools[0].VmSize);
        Assert.Equal(expectedNodePool.OsDiskSizeGB, result.NodePools[0].OsDiskSizeGB);
        Assert.Equal(expectedNodePool.OsDiskType, result.NodePools[0].OsDiskType);
        Assert.Equal(expectedNodePool.KubeletDiskType, result.NodePools[0].KubeletDiskType);
        Assert.Equal(expectedNodePool.MaxPods, result.NodePools[0].MaxPods);
        Assert.Equal(expectedNodePool.Type, result.NodePools[0].Type);
        Assert.Equal(expectedNodePool.MaxCount, result.NodePools[0].MaxCount);
        Assert.Equal(expectedNodePool.MinCount, result.NodePools[0].MinCount);
        Assert.Equal(expectedNodePool.EnableAutoScaling, result.NodePools[0].EnableAutoScaling);
        Assert.Equal(expectedNodePool.ScaleDownMode, result.NodePools[0].ScaleDownMode);
        Assert.Equal(expectedNodePool.ProvisioningState, result.NodePools[0].ProvisioningState);
        Assert.Equal(expectedNodePool.PowerState?.Code, result.NodePools[0].PowerState?.Code);
        Assert.Equal(expectedNodePool.OrchestratorVersion, result.NodePools[0].OrchestratorVersion);
        Assert.Equal(expectedNodePool.CurrentOrchestratorVersion, result.NodePools[0].CurrentOrchestratorVersion);
        Assert.Equal(expectedNodePool.EnableNodePublicIP, result.NodePools[0].EnableNodePublicIP);
        Assert.Equal(expectedNodePool.ScaleSetPriority, result.NodePools[0].ScaleSetPriority);
        Assert.Equal(expectedNodePool.ScaleSetEvictionPolicy, result.NodePools[0].ScaleSetEvictionPolicy);
        Assert.Equal(expectedNodePool.NodeLabels!["kubernetes.azure.com/scalesetpriority"], result.NodePools[0].NodeLabels!["kubernetes.azure.com/scalesetpriority"]);
        Assert.Equal(expectedNodePool.NodeTaints![0], result.NodePools[0].NodeTaints![0]);
        Assert.Equal(expectedNodePool.Mode, result.NodePools[0].Mode);
        Assert.Equal(expectedNodePool.OsType, result.NodePools[0].OsType);
        Assert.Equal(expectedNodePool.OsSKU, result.NodePools[0].OsSKU);
        Assert.Equal(expectedNodePool.NodeImageVersion, result.NodePools[0].NodeImageVersion);
        Assert.Equal("true", result.NodePools[0].Tags!["gc_skip"]);
        Assert.Equal(-1, result.NodePools[0].SpotMaxPrice);
        Assert.Equal("OCIContainer", result.NodePools[0].WorkloadRuntime);
        Assert.False(result.NodePools[0].EnableEncryptionAtHost!.Value);
        Assert.Equal("Install", result.NodePools[0].GpuProfile?.Driver);
        Assert.Equal(1, result.NodePools[0].NetworkProfile?.AllowedHostPorts?.Count);
        Assert.Equal("/subscriptions/s/rg/r/providers/Microsoft.Network/virtualNetworks/vnet/subnets/podsubnet", result.NodePools[0].PodSubnetId);
        Assert.Equal("/subscriptions/s/rg/r/providers/Microsoft.Network/virtualNetworks/vnet/subnets/nodesubnet", result.NodePools[0].VnetSubnetId);
    }
}
