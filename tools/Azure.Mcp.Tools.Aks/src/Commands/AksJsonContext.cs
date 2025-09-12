// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Tools.Aks.Commands.Cluster;
using Azure.Mcp.Tools.Aks.Services.Models;

namespace Azure.Mcp.Tools.Aks.Commands;

[JsonSerializable(typeof(ClusterListCommand.ClusterListCommandResult))]
[JsonSerializable(typeof(ClusterGetCommand.ClusterGetCommandResult))]
[JsonSerializable(typeof(Models.Cluster))]
[JsonSerializable(typeof(AksAgentPoolProfile))]
[JsonSerializable(typeof(AksClusterData))]
[JsonSerializable(typeof(AksClusterProperties))]
[JsonSerializable(typeof(AksManagedClusterSku))]
[JsonSerializable(typeof(AksNetworkProfile))]
[JsonSerializable(typeof(AksPowerState))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
internal sealed partial class AksJsonContext : JsonSerializerContext;
