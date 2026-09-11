// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Tools.ResiliencyAgent.Commands.Conversations;
using Azure.Mcp.Tools.ResiliencyAgent.Models;

namespace Azure.Mcp.Tools.ResiliencyAgent.Commands;

[JsonSerializable(typeof(AgentArtifact))]
[JsonSerializable(typeof(ConversationAskCommand.ConversationAskResult))]
[JsonSerializable(typeof(WrittenArtifact))]
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
internal partial class ResiliencyAgentJsonContext : JsonSerializerContext;
