// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Tools.SreAgent.Commands.Agents;
using Azure.Mcp.Tools.SreAgent.Models;

namespace Azure.Mcp.Tools.SreAgent.Commands;

[JsonSerializable(typeof(AgentsListCommand.AgentsListCommandResult))]
[JsonSerializable(typeof(SreAgentResource))]
[JsonSerializable(typeof(List<SreAgentResource>))]
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
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
internal sealed partial class SreAgentJsonContext : JsonSerializerContext
{
}
