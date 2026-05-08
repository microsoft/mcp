// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Tools.SreAgent.Commands.Agents;
using Azure.Mcp.Tools.SreAgent.Commands.Connectors;
using Azure.Mcp.Tools.SreAgent.Commands.Hooks;
using Azure.Mcp.Tools.SreAgent.Models;

namespace Azure.Mcp.Tools.SreAgent.Commands;

[JsonSerializable(typeof(AgentsListCommand.AgentsListCommandResult))]
[JsonSerializable(typeof(SreAgentResource))]
[JsonSerializable(typeof(List<SreAgentResource>))]
// Connectors + Hooks (sub-agent B)
[JsonSerializable(typeof(ConnectorsListCommand.ConnectorsListCommandResult))]
[JsonSerializable(typeof(ConnectorsGetCommand.ConnectorsGetCommandResult))]
[JsonSerializable(typeof(ConnectorsCreateKustoCommand.ConnectorsCreateKustoCommandResult))]
[JsonSerializable(typeof(ConnectorsCreateMcpCommand.ConnectorsCreateMcpCommandResult))]
[JsonSerializable(typeof(ConnectorsDeleteCommand.ConnectorsDeleteCommandResult))]
[JsonSerializable(typeof(ConnectorsTestCommand.ConnectorsTestCommandResult))]
[JsonSerializable(typeof(HooksListCommand.HooksListCommandResult))]
[JsonSerializable(typeof(HooksGetCommand.HooksGetCommandResult))]
[JsonSerializable(typeof(HooksDeleteCommand.HooksDeleteCommandResult))]
[JsonSerializable(typeof(HooksThreadListCommand.HooksThreadListCommandResult))]
[JsonSerializable(typeof(HooksThreadActivateCommand.HooksThreadActivateCommandResult))]
[JsonSerializable(typeof(HooksThreadDeactivateCommand.HooksThreadDeactivateCommandResult))]
[JsonSerializable(typeof(AgentConnector))]
[JsonSerializable(typeof(AgentConnectorEnvelope))]
[JsonSerializable(typeof(ConnectorTestResult))]
[JsonSerializable(typeof(ConnectorToolInfo))]
[JsonSerializable(typeof(HookEnvelope))]
[JsonSerializable(typeof(HookSpec))]
[JsonSerializable(typeof(HookDefinition))]
[JsonSerializable(typeof(ThreadHookInfo))]
[JsonSerializable(typeof(ThreadHooksResponse))]
[JsonSerializable(typeof(List<AgentConnector>))]
[JsonSerializable(typeof(List<AgentConnectorEnvelope>))]
[JsonSerializable(typeof(List<HookEnvelope>))]
[JsonSerializable(typeof(List<ConnectorToolInfo>))]
[JsonSerializable(typeof(List<ThreadHookInfo>))]
[JsonSerializable(typeof(Dictionary<string, object>))]
[JsonSerializable(typeof(Dictionary<string, string>))]
[JsonSerializable(typeof(string[]))]
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
internal sealed partial class SreAgentJsonContext : JsonSerializerContext
{
}


