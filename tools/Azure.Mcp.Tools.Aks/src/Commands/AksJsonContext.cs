// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Tools.Aks.Commands.Cluster;

namespace Azure.Mcp.Tools.Aks.Commands;

[JsonSerializable(typeof(ClusterListCommand.ClusterListCommandResult))]
[JsonSerializable(typeof(ClusterGetCommand.ClusterGetCommandResult))]
[JsonSerializable(typeof(Models.Cluster))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
internal sealed partial class AksJsonContext : JsonSerializerContext;
