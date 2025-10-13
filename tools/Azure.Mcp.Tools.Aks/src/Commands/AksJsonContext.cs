// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Tools.Aks.Commands.Cluster;
using Azure.Mcp.Tools.Aks.Commands.Nodepool;

namespace Azure.Mcp.Tools.Aks.Commands;

[JsonSerializable(typeof(ClusterGetCommand.ClusterGetCommandResult))]
[JsonSerializable(typeof(Models.Cluster))]
[JsonSerializable(typeof(NodepoolGetCommand.NodepoolGetCommandResult))]
[JsonSerializable(typeof(Models.NodePool))]
[JsonSerializable(typeof(Models.NodePoolPowerState))]
[JsonSerializable(typeof(Models.NodePoolSecurityProfile))]
[JsonSerializable(typeof(Models.NodePoolGpuProfile))]
[JsonSerializable(typeof(Models.NodePoolUpgradeSettings))]
[JsonSerializable(typeof(Models.NodePoolNetworkProfile))]
[JsonSerializable(typeof(Models.PortRange))]
[JsonSerializable(typeof(Models.IPTag))]
[JsonSerializable(typeof(Models.ClusterNetworkProfile))]
[JsonSerializable(typeof(Models.ClusterNetworkLoadBalancerProfile))]
[JsonSerializable(typeof(Models.EffectiveOutboundIPReference))]
[JsonSerializable(typeof(Dictionary<string, Dictionary<string, string>>))]
[JsonSerializable(typeof(Models.ResourceIdentity))]
[JsonSerializable(typeof(Models.ManagedIdentityReference))]
[JsonSerializable(typeof(Models.WindowsProfile))]
[JsonSerializable(typeof(Models.WindowsGmsaProfile))]
[JsonSerializable(typeof(Models.ServicePrincipalProfile))]
[JsonSerializable(typeof(Models.OidcIssuerProfile))]
[JsonSerializable(typeof(Models.AutoUpgradeProfile))]
[JsonSerializable(typeof(Models.ClusterSecurityProfile))]
[JsonSerializable(typeof(Models.AzureKeyVaultKms))]
[JsonSerializable(typeof(Models.DefenderProfile))]
[JsonSerializable(typeof(Models.DefenderSecurityMonitoring))]
[JsonSerializable(typeof(Models.ImageCleanerProfile))]
[JsonSerializable(typeof(Models.WorkloadIdentityProfile))]
[JsonSerializable(typeof(Models.ClusterStorageProfile))]
[JsonSerializable(typeof(Models.CsiDriverProfile))]
[JsonSerializable(typeof(Models.SnapshotControllerProfile))]
[JsonSerializable(typeof(Models.ClusterMetricsProfile))]
[JsonSerializable(typeof(Models.CostAnalysisProfile))]
[JsonSerializable(typeof(Models.ClusterNodeProvisioningProfile))]
[JsonSerializable(typeof(Models.ClusterBootstrapProfile))]
[JsonSerializable(typeof(Models.AiToolchainOperatorProfile))]
[JsonSerializable(typeof(Models.WorkloadAutoScalerProfile))]
[JsonSerializable(typeof(Models.WorkloadAutoScalerKeda))]
[JsonSerializable(typeof(Models.WorkloadAutoScalerVerticalPodAutoscaler))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
internal sealed partial class AksJsonContext : JsonSerializerContext;
