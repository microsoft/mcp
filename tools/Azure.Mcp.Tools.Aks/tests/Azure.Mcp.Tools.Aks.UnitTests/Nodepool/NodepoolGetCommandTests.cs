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
    [InlineData("--subscription sub123 --resource-group rg1 --cluster c1", false)] // missing nodepool
    [InlineData("--subscription sub123 --resource-group rg1 --nodepool np1", false)] // missing cluster
    [InlineData("--subscription sub123 --cluster c1 --nodepool np1", false)] // missing rg
    [InlineData("--resource-group rg1 --cluster c1 --nodepool np1", false)] // missing subscription
    [InlineData("", false)]
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        // Arrange
        if (shouldSucceed)
        {
            var testNodePool = new Models.NodePool
            {
                Name = "np1",
                Count = 3,
                VmSize = "Standard_DS2_v2",
                Mode = "System"
            };
            _aksService.GetNodePool(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>())
                .Returns(testNodePool);
        }

        var context = new CommandContext(_serviceProvider);
        var parseResult = _command.GetCommand().Parse(args);

        // Act
        var response = await _command.ExecuteAsync(context, parseResult);

        // Assert
        Assert.Equal(shouldSucceed ? HttpStatusCode.OK : HttpStatusCode.BadRequest, response.Status);
        if (shouldSucceed)
        {
            Assert.Equal("Success", response.Message);
        }
        else
        {
            Assert.Contains("required", response.Message.ToLower());
        }
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
            PowerState = new Models.NodePoolPowerState { Code = "Running" },
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
            UpgradeSettings = new Models.NodePoolUpgradeSettings { MaxSurge = "10%", MaxUnavailable = "0" },
            SecurityProfile = new Models.NodePoolSecurityProfile { EnableVTPM = false, EnableSecureBoot = false },
            GpuProfile = new Models.NodePoolGpuProfile { Driver = "Install" },
            NetworkProfile = new Models.NodePoolNetworkProfile
            {
                AllowedHostPorts = new List<Models.PortRange> { new() { StartPort = 8080, EndPort = 8080 } },
                ApplicationSecurityGroups = new List<string> { "/subscriptions/s/rg/r/providers/Microsoft.Network/applicationSecurityGroups/asg1" },
                NodePublicIPTags = new List<Models.IPTag> { new() { IpTagType = "FirstPartyUsage", Tag = "foo" } }
            },
            PodSubnetId = "/subscriptions/s/rg/r/providers/Microsoft.Network/virtualNetworks/vnet/subnets/podsubnet",
            VnetSubnetId = "/subscriptions/s/rg/r/providers/Microsoft.Network/virtualNetworks/vnet/subnets/nodesubnet"
        };
        _aksService.GetNodePool(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>())
            .Returns(expectedNodePool);

        var context = new CommandContext(_serviceProvider);
        var parseResult = _command.GetCommand().Parse("--subscription sub123 --resource-group rg1 --cluster c1 --nodepool userpool");

        // Act
        var response = await _command.ExecuteAsync(context, parseResult);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);

        await _aksService.Received(1).GetNodePool(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>());

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, AksJsonContext.Default.NodepoolGetCommandResult);

        Assert.NotNull(result);
        Assert.Equal(expectedNodePool.Name, result.NodePool.Name);
        Assert.Equal(expectedNodePool.Count, result.NodePool.Count);
        Assert.Equal(expectedNodePool.VmSize, result.NodePool.VmSize);
        Assert.Equal(expectedNodePool.OsDiskSizeGB, result.NodePool.OsDiskSizeGB);
        Assert.Equal(expectedNodePool.OsDiskType, result.NodePool.OsDiskType);
        Assert.Equal(expectedNodePool.KubeletDiskType, result.NodePool.KubeletDiskType);
        Assert.Equal(expectedNodePool.MaxPods, result.NodePool.MaxPods);
        Assert.Equal(expectedNodePool.Type, result.NodePool.Type);
        Assert.Equal(expectedNodePool.MaxCount, result.NodePool.MaxCount);
        Assert.Equal(expectedNodePool.MinCount, result.NodePool.MinCount);
        Assert.Equal(expectedNodePool.EnableAutoScaling, result.NodePool.EnableAutoScaling);
        Assert.Equal(expectedNodePool.ScaleDownMode, result.NodePool.ScaleDownMode);
        Assert.Equal(expectedNodePool.ProvisioningState, result.NodePool.ProvisioningState);
        Assert.Equal(expectedNodePool.PowerState?.Code, result.NodePool.PowerState?.Code);
        Assert.Equal(expectedNodePool.OrchestratorVersion, result.NodePool.OrchestratorVersion);
        Assert.Equal(expectedNodePool.CurrentOrchestratorVersion, result.NodePool.CurrentOrchestratorVersion);
        Assert.Equal(expectedNodePool.EnableNodePublicIP, result.NodePool.EnableNodePublicIP);
        Assert.Equal(expectedNodePool.ScaleSetPriority, result.NodePool.ScaleSetPriority);
        Assert.Equal(expectedNodePool.ScaleSetEvictionPolicy, result.NodePool.ScaleSetEvictionPolicy);
        Assert.Equal(expectedNodePool.NodeLabels!["kubernetes.azure.com/scalesetpriority"], result.NodePool.NodeLabels!["kubernetes.azure.com/scalesetpriority"]);
        Assert.Equal(expectedNodePool.NodeTaints![0], result.NodePool.NodeTaints![0]);
        Assert.Equal(expectedNodePool.Mode, result.NodePool.Mode);
        Assert.Equal(expectedNodePool.OsType, result.NodePool.OsType);
        Assert.Equal(expectedNodePool.OsSKU, result.NodePool.OsSKU);
        Assert.Equal(expectedNodePool.NodeImageVersion, result.NodePool.NodeImageVersion);
        Assert.Equal("true", result.NodePool.Tags!["gc_skip"]);
        Assert.Equal(-1, result.NodePool.SpotMaxPrice);
        Assert.Equal("OCIContainer", result.NodePool.WorkloadRuntime);
        Assert.False(result.NodePool.EnableEncryptionAtHost!.Value);
        Assert.Equal("Install", result.NodePool.GpuProfile?.Driver);
        Assert.Equal(1, result.NodePool.NetworkProfile?.AllowedHostPorts?.Count);
        Assert.Equal("/subscriptions/s/rg/r/providers/Microsoft.Network/virtualNetworks/vnet/subnets/podsubnet", result.NodePool.PodSubnetId);
        Assert.Equal("/subscriptions/s/rg/r/providers/Microsoft.Network/virtualNetworks/vnet/subnets/nodesubnet", result.NodePool.VnetSubnetId);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsNullWhenNodePoolNotFound()
    {
        // Arrange
        _aksService.GetNodePool(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>())
            .Returns((Models.NodePool?)null);

        var context = new CommandContext(_serviceProvider);
        var parseResult = _command.GetCommand().Parse("--subscription sub123 --resource-group rg1 --cluster c1 --nodepool missing");

        // Act
        var response = await _command.ExecuteAsync(context, parseResult);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.Null(response.Results);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        // Arrange
        _aksService.GetNodePool(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>())
            .Returns(Task.FromException<Models.NodePool?>(new Exception("Test error")));

        var context = new CommandContext(_serviceProvider);
        var parseResult = _command.GetCommand().Parse("--subscription sub123 --resource-group rg1 --cluster c1 --nodepool np1");

        // Act
        var response = await _command.ExecuteAsync(context, parseResult);

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Test error", response.Message);
        Assert.Contains("troubleshooting", response.Message);
    }
}
