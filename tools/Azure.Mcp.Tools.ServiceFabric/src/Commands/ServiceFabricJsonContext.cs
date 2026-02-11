// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Tools.ServiceFabric.Commands.ManagedCluster;
using Azure.Mcp.Tools.ServiceFabric.Models;

namespace Azure.Mcp.Tools.ServiceFabric.Commands;

[JsonSerializable(typeof(ManagedClusterNodeListCommand.ManagedClusterNodeListCommandResult))]
[JsonSerializable(typeof(ManagedClusterNodeTypeRestartCommand.ManagedClusterNodeTypeRestartCommandResult))]
[JsonSerializable(typeof(ManagedClusterNode))]
[JsonSerializable(typeof(ManagedClusterNodeProperties))]
[JsonSerializable(typeof(NodeIdentifier))]
[JsonSerializable(typeof(ListNodesResponse))]
[JsonSerializable(typeof(RestartNodeRequest))]
[JsonSerializable(typeof(RestartNodeResponse))]
[JsonSourceGenerationOptions(DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
internal sealed partial class ServiceFabricJsonContext : JsonSerializerContext;
