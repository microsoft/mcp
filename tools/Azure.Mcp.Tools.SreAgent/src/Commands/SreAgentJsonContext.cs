// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Tools.SreAgent.Commands.Agents;
using Azure.Mcp.Tools.SreAgent.Commands.Skills;
using Azure.Mcp.Tools.SreAgent.Models;

namespace Azure.Mcp.Tools.SreAgent.Commands;

[JsonSerializable(typeof(AgentsListCommand.AgentsListCommandResult))]
[JsonSerializable(typeof(SreAgentResource))]
[JsonSerializable(typeof(List<SreAgentResource>))]
// Agents + Skills (sub-agent A)
[JsonSerializable(typeof(AgentsGetCommand.AgentsGetCommandResult))]
[JsonSerializable(typeof(AgentsCreateCommand.AgentsCreateCommandResult))]
[JsonSerializable(typeof(AgentsDeleteCommand.AgentsDeleteCommandResult))]
[JsonSerializable(typeof(AgentsToolsGetCommand.AgentsToolsGetCommandResult))]
[JsonSerializable(typeof(AgentsToolsCreateCommand.AgentsToolsCreateCommandResult))]
[JsonSerializable(typeof(AgentToolsListCommand.AgentToolsListCommandResult))]
[JsonSerializable(typeof(SkillsDeleteCommand.SkillsDeleteCommandResult))]
[JsonSerializable(typeof(SkillsListCommand.SkillsListCommandResult))]
[JsonSerializable(typeof(SkillsCreateCommand.SkillsCreateCommandResult))]
[JsonSerializable(typeof(SreSubAgent))]
[JsonSerializable(typeof(SreSubAgentProperties))]
[JsonSerializable(typeof(SreSubAgentCreateRequest))]
[JsonSerializable(typeof(SreAgentTool))]
[JsonSerializable(typeof(SreAgentToolProperties))]
[JsonSerializable(typeof(SreAgentToolParameter))]
[JsonSerializable(typeof(List<SreAgentToolParameter>))]
[JsonSerializable(typeof(SreAgentToolCreateRequest))]
[JsonSerializable(typeof(SreSkill))]
[JsonSerializable(typeof(SreSkillProperties))]
[JsonSerializable(typeof(SreSkillCreateRequest))]
[JsonSerializable(typeof(SreAgentDeleteResult))]
[JsonSerializable(typeof(List<SreSubAgent>))]
[JsonSerializable(typeof(List<SreAgentTool>))]
[JsonSerializable(typeof(List<SreSkill>))]
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
internal sealed partial class SreAgentJsonContext : JsonSerializerContext
{
}
