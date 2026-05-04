// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.Aks.Commands;
using Azure.Mcp.Tools.Aks.Commands.Cluster;
using Azure.Mcp.Tools.Aks.Services;
using Microsoft.Mcp.Core.Options;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.Aks.UnitTests.Cluster;

public class ClusterGetCommandTests : CommandUnitTestsBase<ClusterGetCommand, IAksService>
{
    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = Command.GetCommand();
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
            Service.GetClusters(
                Arg.Any<string>(),
                Arg.Any<string?>(),
                Arg.Any<string>(),
                Arg.Any<string?>(),
                Arg.Any<RetryPolicyOptions>(),
                Arg.Any<CancellationToken>())
                .Returns([]);
        }

        // Act
        var response = await ExecuteCommandAsync(args);

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
        Service.GetClusters(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns(expectedClusters);

        // Act
        var response = await ExecuteCommandAsync("--subscription", "sub123");

        // Assert
        var result = ValidateAndDeserializeResponse(response, AksJsonContext.Default.ClusterGetCommandResult);

        Assert.Equal(expectedClusters.Count, result.Clusters.Count);
        Assert.Equal(expectedClusters[0].Name, result.Clusters[0].Name);
        Assert.Equal(expectedClusters[0].Location, result.Clusters[0].Location);
        Assert.Equal(expectedClusters[0].KubernetesVersion, result.Clusters[0].KubernetesVersion);

        // Verify the mock was called
        await Service.Received(1).GetClusters(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>());
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

        Service.GetClusters(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns([enriched]);

        // Act
        var response = await ExecuteCommandAsync("--subscription", "sub123");

        // Assert
        var result = ValidateAndDeserializeResponse(response, AksJsonContext.Default.ClusterGetCommandResult);

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
        Service.GetClusters(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns([]);

        // Act
        var response = await ExecuteCommandAsync("--subscription", "sub123");

        // Assert
        var result = ValidateAndDeserializeResponse(response, AksJsonContext.Default.ClusterGetCommandResult);

        Assert.Empty(result.Clusters);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        // Arrange
        Service.GetClusters(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Test error"));

        // Act
        var response = await ExecuteCommandAsync("--subscription", "sub123");

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

        Service.GetClusters("test-subscription", "test-cluster", "test-rg", null, Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .Returns([expectedCluster]);

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "test-subscription",
            "--resource-group", "test-rg",
            "--cluster", "test-cluster");

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
        Service.GetClusters(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(notFoundException);

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "test-subscription",
            "--resource-group", "test-rg",
            "--cluster", "test-cluster");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.Status);
        Assert.Contains("AKS cluster not found", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_Handles403Forbidden()
    {
        // Arrange
        var forbiddenException = new RequestFailedException((int)HttpStatusCode.Forbidden, "Authorization failed");
        Service.GetClusters(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(forbiddenException);

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "test-subscription",
            "--resource-group", "test-rg",
            "--cluster", "test-cluster");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.Status);
        Assert.Contains("Authorization failed", response.Message);
    }
}
