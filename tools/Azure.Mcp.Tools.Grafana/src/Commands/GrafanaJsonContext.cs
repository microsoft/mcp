// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Tools.Grafana.Commands.Workspace;
using Azure.Mcp.Tools.Grafana.Models;
using Azure.Mcp.Tools.Grafana.Services.Models;

namespace Azure.Mcp.Tools.Grafana.Commands;

[JsonSerializable(typeof(WorkspaceListCommand.WorkspaceListCommandResult))]
[JsonSerializable(typeof(ManagedGrafanaData))]
[JsonSerializable(typeof(GrafanaWorkspace))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault)]
internal sealed partial class GrafanaJsonContext : JsonSerializerContext;
