// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Tools.SreAgent.Commands.Agents;
using Azure.Mcp.Tools.SreAgent.Commands.ScheduledTasks;
using Azure.Mcp.Tools.SreAgent.Commands.Threads;
using Azure.Mcp.Tools.SreAgent.Models;

namespace Azure.Mcp.Tools.SreAgent.Commands;

[JsonSerializable(typeof(AgentsListCommand.AgentsListCommandResult))]
[JsonSerializable(typeof(SreAgentResource))]
[JsonSerializable(typeof(List<SreAgentResource>))]
// Threads + ScheduledTasks (sub-agent C)
[JsonSerializable(typeof(ThreadsListCommand.ThreadsListCommandResult))]
[JsonSerializable(typeof(ThreadsGetCommand.ThreadsGetCommandResult))]
[JsonSerializable(typeof(SreAgentThreadOperationResult))]
[JsonSerializable(typeof(SreAgentInvestigationResult))]
[JsonSerializable(typeof(ScheduledTasksListCommand.ScheduledTasksListCommandResult))]
[JsonSerializable(typeof(ScheduledTasksGetCommand.ScheduledTasksGetCommandResult))]
[JsonSerializable(typeof(ScheduledTasksDeleteCommand.ScheduledTaskOperationResult))]
[JsonSerializable(typeof(SreAgentThread))]
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
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
internal sealed partial class SreAgentJsonContext : JsonSerializerContext
{
}
