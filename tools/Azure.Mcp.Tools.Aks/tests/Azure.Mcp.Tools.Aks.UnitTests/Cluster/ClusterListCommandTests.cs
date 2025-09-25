// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.Aks.Commands;
using Azure.Mcp.Tools.Aks.Commands.Cluster;
using Azure.Mcp.Tools.Aks.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.Aks.UnitTests.Cluster;

public sealed class ClusterListCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IAksService _aksService;
    private readonly ILogger<ClusterListCommand> _logger;
    private readonly ClusterListCommand _command;

    public ClusterListCommandTests()
    {
        _aksService = Substitute.For<IAksService>();
        _logger = Substitute.For<ILogger<ClusterListCommand>>();

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
    [InlineData("--subscription sub123", true)]
    [InlineData("--subscription sub123 --tenant tenant123", true)]
    [InlineData("", false)]
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        // Arrange
        if (shouldSucceed)
        {
            var testClusters = new List<Models.Cluster>
            {
                new() { Name = "cluster1", Location = "eastus" },
                new() { Name = "cluster2", Location = "westus" }
            };
            _aksService.ListClusters(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>())
                .Returns(testClusters);
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
    public async Task ExecuteAsync_ReturnsClustersList()
    {
        // Arrange
        var expectedClusters = new List<Models.Cluster>
        {
            new() { Name = "cluster1", Location = "eastus", KubernetesVersion = "1.28.0" },
            new() { Name = "cluster2", Location = "westus", KubernetesVersion = "1.29.0" },
            new() { Name = "cluster3", Location = "centralus", KubernetesVersion = "1.28.5" }
        };
        _aksService.ListClusters(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>())
            .Returns(expectedClusters);

        var context = new CommandContext(_serviceProvider);
        var parseResult = _command.GetCommand().Parse("--subscription sub123");        // Act
        var response = await _command.ExecuteAsync(context, parseResult);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);

        // Verify the mock was called
        await _aksService.Received(1).ListClusters(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>());

        var json = JsonSerializer.Serialize(response.Results);
        // Debug: Output the actual JSON to understand the structure
        Console.WriteLine($"Actual JSON: {json}");

        var result = JsonSerializer.Deserialize(json, AksJsonContext.Default.ClusterListCommandResult);

        Assert.NotNull(result);
        Assert.Equal(expectedClusters.Count, result.Clusters.Count);
        Assert.Equal(expectedClusters[0].Name, result.Clusters[0].Name);
        Assert.Equal(expectedClusters[0].Location, result.Clusters[0].Location);
        Assert.Equal(expectedClusters[0].KubernetesVersion, result.Clusters[0].KubernetesVersion);
    }

    [Fact]
    public async Task ExecuteAsync_EnrichedClusterFields_SerializeCorrectly()
    {
        // Arrange an enriched cluster with nested fields
        var enriched = new Models.Cluster
        {
            Id = "/subscriptions/sub/rg/cluster/id",
            Name = "c-enriched",
            SubscriptionId = "sub123",
            ResourceGroupName = "rg1",
            Location = "eastus",
            KubernetesVersion = "1.33.2",
            ProvisioningState = "Succeeded",
            PowerState = "Running",
            DnsPrefix = "dns",
            Fqdn = "c-enriched.hcp.eastus.azmk8s.io",
            NodeCount = 3,
            NodeVmSize = "Standard_DS2_v2",
            IdentityType = "SystemAssigned",
            Identity = new Models.ResourceIdentity { Type = "SystemAssigned", PrincipalId = Guid.NewGuid().ToString(), TenantId = Guid.NewGuid().ToString() },
            EnableRbac = true,
            NetworkPlugin = "azure",
            NetworkPolicy = "cilium",
            ServiceCidr = "10.0.0.0/16",
            DnsServiceIP = "10.0.0.10",
            SkuTier = "Standard",
            SkuName = "Base",
            NodeResourceGroup = "MC_rg1_c-enriched_eastus",
            MaxAgentPools = 100,
            SupportPlan = "KubernetesOfficial",
            NetworkProfile = new Models.ClusterNetworkProfile
            {
                NetworkPlugin = "azure",
                NetworkPluginMode = "overlay",
                NetworkPolicy = "cilium",
                NetworkDataplane = "cilium",
                LoadBalancerSku = "standard",
                LoadBalancerProfile = new Models.ClusterNetworkLoadBalancerProfile
                {
                    ManagedOutboundIPCount = 1,
                    EffectiveOutboundIPs = new List<Models.EffectiveOutboundIPReference> { new() { Id = "/subscriptions/sub/resourceGroups/rg/providers/Microsoft.Network/publicIPAddresses/pip" } },
                    BackendPoolType = "nodeIPConfiguration"
                },
                PodCidr = "10.244.0.0/16",
                ServiceCidr = "10.0.0.0/16",
                DnsServiceIP = "10.0.0.10",
                OutboundType = "loadBalancer",
                PodCidrs = new List<string> { "10.244.0.0/16" },
                ServiceCidrs = new List<string> { "10.0.0.0/16" },
                IpFamilies = new List<string> { "IPv4" }
            },
            WindowsProfile = new Models.WindowsProfile
            {
                AdminUsername = "azureuser",
                AdminPassword = "P@ssword123!",
                EnableCsiProxy = true,
                GmsaProfile = new Models.WindowsGmsaProfile { Enabled = false, DnsServer = string.Empty, RootDomainName = string.Empty },
                LicenseType = "None"
            },
            ServicePrincipalProfile = new Models.ServicePrincipalProfile { ClientId = "msi", Secret = null },
            AutoUpgradeProfile = new Models.AutoUpgradeProfile { UpgradeChannel = "rapid", NodeOSUpgradeChannel = "NodeImage" },
            AutoScalerProfile = new Dictionary<string, string> { ["scan-interval"] = "10s", ["max-graceful-termination-sec"] = "600" },
            AddonProfiles = new Dictionary<string, IDictionary<string, string>>
            {
                ["azurepolicy"] = new Dictionary<string, string> { ["enabled"] = "true", ["identity.clientId"] = Guid.NewGuid().ToString() }
            },
            IdentityProfile = new Dictionary<string, Models.ManagedIdentityReference>
            {
                ["kubeletidentity"] = new() { ResourceId = "/subscriptions/sub/rg/providers/Microsoft.ManagedIdentity/userAssignedIdentities/id", ClientId = Guid.NewGuid().ToString(), ObjectId = Guid.NewGuid().ToString() }
            },
            DisableLocalAccounts = false,
            ResourceUid = "abc123",
            AgentPoolProfiles = new List<Models.NodePool> { new() { Name = "np1", Count = 3 } },
            Tags = new Dictionary<string, string> { ["gc_skip"] = "true" }
        };

        _aksService.ListClusters(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>())
            .Returns(new List<Models.Cluster> { enriched });

        var context = new CommandContext(_serviceProvider);
        var parseResult = _command.GetCommand().Parse("--subscription sub123");

        // Act
        var response = await _command.ExecuteAsync(context, parseResult);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, AksJsonContext.Default.ClusterListCommandResult);

        Assert.NotNull(result);
        var c = result.Clusters[0];
        Assert.Equal(enriched.Id, c.Id);
        Assert.Equal(enriched.NetworkProfile?.NetworkPolicy, c.NetworkProfile?.NetworkPolicy);
        Assert.Equal("azureuser", c.WindowsProfile?.AdminUsername);
        Assert.Equal("None", c.WindowsProfile?.LicenseType);
        Assert.Equal("msi", c.ServicePrincipalProfile?.ClientId);
        Assert.Equal("rapid", c.AutoUpgradeProfile?.UpgradeChannel);
        Assert.Equal("true", c.AddonProfiles!["azurepolicy"]["enabled"]);
        Assert.Equal("true", c.Tags!["gc_skip"]);
        Assert.Equal(1, c.AgentPoolProfiles?.Count);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsEmptyWhenNoClusters()
    {
        // Arrange
        _aksService.ListClusters(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>())
            .Returns([]);

        var context = new CommandContext(_serviceProvider);
        var parseResult = _command.GetCommand().Parse("--subscription sub123");

        // Act
        var response = await _command.ExecuteAsync(context, parseResult);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, AksJsonContext.Default.ClusterListCommandResult);

        Assert.NotNull(result);
        Assert.Empty(result.Clusters);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        // Arrange
        _aksService.ListClusters(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>())
            .Returns(Task.FromException<List<Models.Cluster>>(new Exception("Test error")));

        var context = new CommandContext(_serviceProvider);
        var parseResult = _command.GetCommand().Parse("--subscription sub123");

        // Act
        var response = await _command.ExecuteAsync(context, parseResult);

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Test error", response.Message);
        Assert.Contains("troubleshooting", response.Message);
    }
}
