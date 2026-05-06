// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Tools.Deploy.Commands.Architecture;
using Azure.Mcp.Tools.Deploy.Commands.Infrastructure;
using Azure.Mcp.Tools.Deploy.Commands.Plan;
using Azure.Mcp.Tools.Deploy.Models;
using Azure.Mcp.Tools.Deploy.Options;

namespace Azure.Mcp.Tools.Deploy.Commands;

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    PropertyNameCaseInsensitive = true,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
)]
[JsonSerializable(typeof(AppTopology))]
[JsonSerializable(typeof(MermaidData))]
[JsonSerializable(typeof(MermaidConfig))]
[JsonSerializable(typeof(List<string>))]
[JsonSerializable(typeof(DiagramGenerateCommand.DiagramGenerateCommandResult))]
[JsonSerializable(typeof(RulesGetCommand.RulesGetCommandResult))]
[JsonSerializable(typeof(GetCommand.GetCommandResult))]
internal sealed partial class DeployJsonContext : JsonSerializerContext
{
}
