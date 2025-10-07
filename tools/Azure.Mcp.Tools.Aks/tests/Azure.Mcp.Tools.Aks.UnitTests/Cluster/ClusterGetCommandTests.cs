// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
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

public class ClusterGetCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IAksService _aksService;
    private readonly ILogger<ClusterGetCommand> _logger;
    private readonly ClusterGetCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;

    public ClusterGetCommandTests()
    {
        _aksService = Substitute.For<IAksService>();
        _logger = Substitute.For<ILogger<ClusterGetCommand>>();

        var collection = new ServiceCollection().AddSingleton(_aksService);
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
    [InlineData("--subscription sub1 --resource-group rg1 --cluster cluster1", true)]
    [InlineData("--subscription sub1 --cluster cluster1", false)]  // Missing resource-group
    [InlineData("--resource-group rg1 --cluster cluster1", false)] // Missing subscription
    [InlineData("", false)]                                              // Missing all required options
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        // Arrange
        if (shouldSucceed)
        {
            _aksService.GetClusters(
                Arg.Any<string>(),
                Arg.Any<string?>(),
                Arg.Any<string>(),
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
    public async Task ExecuteAsync_ReturnsClustersList()
    {
        // Arrange
        var expectedClusters = new List<Models.Cluster>
        {
            new() { Name = "cluster1", Location = "eastus", KubernetesVersion = "1.28.0" },
            new() { Name = "cluster2", Location = "westus", KubernetesVersion = "1.29.0" },
            new() { Name = "cluster3", Location = "centralus", KubernetesVersion = "1.28.5" }
        };
        _aksService.GetClusters(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions>())
            .Returns(expectedClusters);

        var context = new CommandContext(_serviceProvider);
        var parseResult = _command.GetCommand().Parse("--subscription sub123");

        // Act
        var response = await _command.ExecuteAsync(context, parseResult);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);

        // Verify the mock was called
        await _aksService.Received(1).GetClusters(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions>());

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, AksJsonContext.Default.ClusterGetCommandResult);

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
            Identity = new() { Type = "SystemAssigned", PrincipalId = Guid.NewGuid().ToString(), TenantId = Guid.NewGuid().ToString() },
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
            NetworkProfile = new()
            {
                NetworkPlugin = "azure",
                NetworkPluginMode = "overlay",
                NetworkPolicy = "cilium",
                NetworkDataplane = "cilium",
                LoadBalancerSku = "standard",
                LoadBalancerProfile = new()
                {
                    ManagedOutboundIPCount = 1,
                    EffectiveOutboundIPs = [new() { Id = "/subscriptions/sub/resourceGroups/rg/providers/Microsoft.Network/publicIPAddresses/pip" }],
                    BackendPoolType = "nodeIPConfiguration"
                },
                PodCidr = "10.244.0.0/16",
                ServiceCidr = "10.0.0.0/16",
                DnsServiceIP = "10.0.0.10",
                OutboundType = "loadBalancer",
                PodCidrs = ["10.244.0.0/16"],
                ServiceCidrs = ["10.0.0.0/16"],
                IpFamilies = ["IPv4"]
            },
            WindowsProfile = new()
            {
                AdminUsername = "azureuser",
                AdminPassword = "P@ssword123!",
                EnableCsiProxy = true,
                GmsaProfile = new() { Enabled = false, DnsServer = string.Empty, RootDomainName = string.Empty },
                LicenseType = "None"
            },
            ServicePrincipalProfile = new() { ClientId = "msi", Secret = null },
            AutoUpgradeProfile = new() { UpgradeChannel = "rapid", NodeOSUpgradeChannel = "NodeImage" },
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
            AgentPoolProfiles = [new() { Name = "np1", Count = 3 }],
            Tags = new Dictionary<string, string> { ["gc_skip"] = "true" }
        };

        _aksService.GetClusters(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions>())
            .Returns([enriched]);

        var context = new CommandContext(_serviceProvider);
        var parseResult = _command.GetCommand().Parse("--subscription sub123");

        // Act
        var response = await _command.ExecuteAsync(context, parseResult);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, AksJsonContext.Default.ClusterGetCommandResult);

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
        _aksService.GetClusters(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions>())
            .Returns([]);

        var context = new CommandContext(_serviceProvider);
        var parseResult = _command.GetCommand().Parse("--subscription sub123");

        // Act
        var response = await _command.ExecuteAsync(context, parseResult);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, AksJsonContext.Default.ClusterGetCommandResult);

        Assert.NotNull(result);
        Assert.Empty(result.Clusters);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        // Arrange
        _aksService.GetClusters(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions>())
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

    [Fact]
    public async Task ExecuteAsync_ReturnsClusterWhenFound()
    {
        // Arrange
        var expectedCluster = new Models.Cluster
        {
            Id = "/subscriptions/s/rg/r/providers/Microsoft.ContainerService/managedClusters/test-cluster",
            Name = "test-cluster",
            SubscriptionId = "test-subscription",
            ResourceGroupName = "test-rg",
            Location = "East US",
            KubernetesVersion = "1.28.0",
            ProvisioningState = "Succeeded",
            EnableRbac = true,
            NetworkProfile = new() { NetworkPlugin = "azure", NetworkPolicy = "cilium" },
            WindowsProfile = new() { AdminUsername = "azureuser", EnableCsiProxy = true },
            ServicePrincipalProfile = new() { ClientId = "msi" },
            AutoUpgradeProfile = new() { UpgradeChannel = "stable" },
            AddonProfiles = new Dictionary<string, IDictionary<string, string>> { ["azurepolicy"] = new Dictionary<string, string> { ["enabled"] = "true" } },
            IdentityProfile = new Dictionary<string, Models.ManagedIdentityReference> { ["kubeletidentity"] = new() { ClientId = Guid.NewGuid().ToString() } },
            AgentPoolProfiles = [new() { Name = "systempool", Count = 3 }]
        };

        _aksService.GetClusters("test-subscription", "test-cluster", "test-rg", null, Arg.Any<RetryPolicyOptions>())
            .Returns([expectedCluster]);

        var parseResult = _commandDefinition.Parse(["--subscription", "test-subscription", "--resource-group", "test-rg", "--cluster", "test-cluster"]);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
        Assert.Equal("Success", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_Handles404NotFound()
    {
        // Arrange
        var notFoundException = new RequestFailedException((int)HttpStatusCode.NotFound, "AKS cluster not found");
        _aksService.GetClusters(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>())
            .Returns(Task.FromException<List<Models.Cluster>>(notFoundException));

        var parseResult = _commandDefinition.Parse(["--subscription", "test-subscription", "--resource-group", "test-rg", "--cluster", "test-cluster"]);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.Status);
        Assert.Contains("AKS cluster not found", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_Handles403Forbidden()
    {
        // Arrange
        var forbiddenException = new RequestFailedException((int)HttpStatusCode.Forbidden, "Authorization failed");
        _aksService.GetClusters(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>())
            .Returns(Task.FromException<List<Models.Cluster>>(forbiddenException));

        var parseResult = _commandDefinition.Parse(["--subscription", "test-subscription", "--resource-group", "test-rg", "--cluster", "test-cluster"]);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.Status);
        Assert.Contains("Authorization failed", response.Message);
    }
}
