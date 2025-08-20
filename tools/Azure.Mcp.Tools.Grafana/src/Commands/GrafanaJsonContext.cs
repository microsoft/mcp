// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Tools.Grafana.Commands.Workspace;

namespace Azure.Mcp.Tools.Grafana.Commands;

[JsonSerializable(typeof(WorkspaceListCommand.WorkspaceListCommandResult))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault)]
internal sealed partial class GrafanaJsonContext : JsonSerializerContext;
