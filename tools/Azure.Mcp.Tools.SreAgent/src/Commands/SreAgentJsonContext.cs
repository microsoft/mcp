// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Tools.SreAgent.Commands.Agents;
using Azure.Mcp.Tools.SreAgent.Commands.Connectors;
using Azure.Mcp.Tools.SreAgent.Commands.Hooks;
using Azure.Mcp.Tools.SreAgent.Commands.ScheduledTasks;
using Azure.Mcp.Tools.SreAgent.Commands.Skills;
using Azure.Mcp.Tools.SreAgent.Commands.Threads;
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
[JsonSerializable(typeof(AgentsToolsListCommand.AgentsToolsListCommandResult))]
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

// Threads + ScheduledTasks (sub-agent C)
[JsonSerializable(typeof(ThreadsListCommand.ThreadsListCommandResult))]
[JsonSerializable(typeof(ThreadsGetCommand.ThreadsGetCommandResult))]
[JsonSerializable(typeof(SreAgentThreadOperationResult))]
[JsonSerializable(typeof(SreAgentInvestigationResult))]
[JsonSerializable(typeof(ScheduledTasksListCommand.ScheduledTasksListCommandResult))]
[JsonSerializable(typeof(ScheduledTasksGetCommand.ScheduledTasksGetCommandResult))]
[JsonSerializable(typeof(ScheduledTasksDeleteCommand.ScheduledTaskOperationResult))]
[JsonSerializable(typeof(SreAgentThread))]
[JsonSerializable(typeof(SreAgentThreadStatus))]
[JsonSerializable(typeof(SreAgentThreadActionsStatus))]
[JsonSerializable(typeof(SreAgentThreadIncidentStatus))]
[JsonSerializable(typeof(SreAgentThreadMessage))]
[JsonSerializable(typeof(SreAgentThreadCreateRequest))]
[JsonSerializable(typeof(SreAgentThreadMessageRequest))]
[JsonSerializable(typeof(SreAgentApprovalRequest))]
[JsonSerializable(typeof(SreAgentScheduledTask))]
[JsonSerializable(typeof(SreAgentScheduledTaskCreateRequest))]
[JsonSerializable(typeof(List<SreAgentThread>))]
[JsonSerializable(typeof(List<SreAgentThreadMessage>))]
[JsonSerializable(typeof(List<SreAgentScheduledTask>))]
[JsonSerializable(typeof(SreAgentPagedResponse<SreAgentThread>))]
[JsonSerializable(typeof(SreAgentPagedResponse<SreAgentThreadMessage>))]
[JsonSerializable(typeof(SreAgentPagedResponse<SreAgentScheduledTask>))]

// Incidents + Workflows + Docs + Architecture (sub-agent D)
[JsonSerializable(typeof(SreAgentTextResult))]
[JsonSerializable(typeof(IncidentFilter))]
[JsonSerializable(typeof(List<IncidentFilter>))]
[JsonSerializable(typeof(IncidentHandler))]
[JsonSerializable(typeof(List<IncidentHandler>))]
[JsonSerializable(typeof(IncidentThreadResponse))]
[JsonSerializable(typeof(ThreadListItem))]
[JsonSerializable(typeof(List<ThreadListItem>))]
[JsonSerializable(typeof(DocumentInfo))]
[JsonSerializable(typeof(List<DocumentInfo>))]
[JsonSerializable(typeof(MemorySearchResult))]
[JsonSerializable(typeof(List<MemorySearchResult>))]
[JsonSerializable(typeof(IncidentFilterPayload))]
[JsonSerializable(typeof(IncidentThreadCreateRequest))]
[JsonSerializable(typeof(CommonPromptEnvelope))]
[JsonSerializable(typeof(List<CommonPromptEnvelope>))]
[JsonSerializable(typeof(CommonPromptProperties))]
[JsonSerializable(typeof(ExtendedAgentResourceEnvelope))]

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
internal sealed partial class SreAgentJsonContext : JsonSerializerContext
{
}
