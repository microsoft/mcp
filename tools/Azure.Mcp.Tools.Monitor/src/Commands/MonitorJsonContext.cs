// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Azure.Mcp.Tools.Monitor.Commands.Metrics;
using Azure.Mcp.Tools.Monitor.Commands.Table;
using Azure.Mcp.Tools.Monitor.Commands.TableType;
using Azure.Mcp.Tools.Monitor.Commands.Workspace;

namespace Azure.Mcp.Tools.Monitor.Commands;

[JsonSerializable(typeof(List<JsonNode>))]
[JsonSerializable(typeof(WorkspaceListCommand.WorkspaceListCommandResult))]
[JsonSerializable(typeof(TableListCommand.TableListCommandResult))]
[JsonSerializable(typeof(TableTypeListCommand.TableTypeListCommandResult))]
[JsonSerializable(typeof(MetricsQueryCommand.MetricsQueryCommandResult))]
[JsonSerializable(typeof(MetricsDefinitionsCommand.MetricsDefinitionsCommandResult))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
internal sealed partial class MonitorJsonContext : JsonSerializerContext
{
}
