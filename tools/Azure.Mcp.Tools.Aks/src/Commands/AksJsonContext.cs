// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Tools.Aks.Commands.Cluster;
using Azure.Mcp.Tools.Aks.Commands.Nodepool;

namespace Azure.Mcp.Tools.Aks.Commands;

[JsonSerializable(typeof(ClusterListCommand.ClusterListCommandResult))]
[JsonSerializable(typeof(ClusterGetCommand.ClusterGetCommandResult))]
[JsonSerializable(typeof(Models.Cluster))]
[JsonSerializable(typeof(NodepoolListCommand.NodepoolListCommandResult))]
[JsonSerializable(typeof(NodepoolGetCommand.NodepoolGetCommandResult))]
[JsonSerializable(typeof(Models.NodePool))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
internal sealed partial class AksJsonContext : JsonSerializerContext;
