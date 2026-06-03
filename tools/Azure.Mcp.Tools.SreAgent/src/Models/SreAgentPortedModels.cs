// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.SreAgent.Models;

public sealed record SreAgentTextResult(string Message);

public sealed record IncidentFilter
{
    [JsonPropertyName("id")]
    public string? Id { get; init; }
    [JsonPropertyName("impactedService")]
    public string? ImpactedService { get; init; }
    [JsonPropertyName("priorities")]
    public List<string>? Priorities { get; init; }
    [JsonPropertyName("titleContains")]
    public string? TitleContains { get; init; }
    [JsonPropertyName("agentMode")]
    public string? AgentMode { get; init; }
    [JsonPropertyName("handlingAgent")]
    public string? HandlingAgent { get; init; }
    [JsonPropertyName("isEnabled")]
    public bool? IsEnabled { get; init; }
    [JsonPropertyName("isDeleted")]
    public bool? IsDeleted { get; init; }
}

public sealed record IncidentHandler
{
    [JsonPropertyName("id")]
    public string? Id { get; init; }
    [JsonPropertyName("name")]
    public string? Name { get; init; }
    [JsonPropertyName("description")]
    public string? Description { get; init; }
    [JsonPropertyName("incidentFilterId")]
    public string? IncidentFilterId { get; init; }
    [JsonPropertyName("incidentProcessingGuide")]
    public List<string>? IncidentProcessingGuide { get; init; }
    [JsonPropertyName("tools")]
    public List<string>? Tools { get; init; }
    [JsonPropertyName("incidents")]
    public List<string>? Incidents { get; init; }
    [JsonPropertyName("customInstructions")]
    public string? CustomInstructions { get; init; }
}

// Filter PUT body intentionally uses Pascal-case property names. The data-plane
// handler reads the body as a raw JsonNode using nameof(...) lookups, so
// camelCase keys silently miss and the filter is created with empty values.
public sealed record IncidentFilterPayload
{
    [JsonPropertyName("Id")]
    public string? Id { get; init; }
    [JsonPropertyName("ImpactedService")]
    public string? ImpactedService { get; init; }
    [JsonPropertyName("Priorities")]
    public List<string>? Priorities { get; init; }
    [JsonPropertyName("TitleContains")]
    public string? TitleContains { get; init; }
    [JsonPropertyName("AgentMode")]
    public string? AgentMode { get; init; }
    [JsonPropertyName("HandlingAgent")]
    public string? HandlingAgent { get; init; }
}

public sealed record IncidentThreadCreateRequest
{
    [JsonPropertyName("startMessage")]
    public IncidentThreadStartMessage StartMessage { get; init; } = new();
}

public sealed record IncidentThreadStartMessage
{
    [JsonPropertyName("text")]
    public string? Text { get; init; }
    [JsonPropertyName("userId")]
    public string? UserId { get; init; }
    [JsonPropertyName("displayName")]
    public string? DisplayName { get; init; }
    [JsonPropertyName("agent")]
    public string? Agent { get; init; }
}

public sealed record CommonPromptEnvelope
{
    [JsonPropertyName("name")]
    public string? Name { get; init; }
    [JsonPropertyName("type")]
    public string? Type { get; init; }
    [JsonPropertyName("properties")]
    public CommonPromptProperties? Properties { get; init; }
}

public sealed record CommonPromptProperties
{
    [JsonPropertyName("prompt")]
    public string? Prompt { get; init; }
}

public sealed record ExtendedAgentResourceEnvelope
{
    [JsonPropertyName("name")]
    public string? Name { get; init; }
    [JsonPropertyName("type")]
    public string? Type { get; init; }
    [JsonPropertyName("tags")]
    public List<string>? Tags { get; init; }
    [JsonPropertyName("owner")]
    public string? Owner { get; init; }
    [JsonPropertyName("properties")]
    public Dictionary<string, JsonElement>? Properties { get; init; }
}

public sealed record IncidentThreadResponse([property: JsonPropertyName("id")] string? Id, [property: JsonPropertyName("status")] string? Status);

public sealed record ThreadListItem
{
    [JsonPropertyName("id")]
    public string? Id { get; init; }
    [JsonPropertyName("title")]
    public string? Title { get; init; }
    [JsonPropertyName("status")]
    public ThreadStatus? Status { get; init; }
    [JsonPropertyName("startMessage")]
    public ThreadStartMessage? StartMessage { get; init; }
    [JsonPropertyName("modifiedTimestamp")]
    public string? ModifiedTimestamp { get; init; }
}

public sealed record ThreadStatus([property: JsonPropertyName("actionsStatus")] ThreadActionsStatus? ActionsStatus, [property: JsonPropertyName("incidentStatus")] ThreadIncidentStatus? IncidentStatus);
public sealed record ThreadActionsStatus([property: JsonPropertyName("hasCriticalActions")] bool? HasCriticalActions, [property: JsonPropertyName("hasWarningActions")] bool? HasWarningActions);
public sealed record ThreadIncidentStatus([property: JsonPropertyName("incidentId")] string? IncidentId, [property: JsonPropertyName("status")] string? Status);
public sealed record ThreadStartMessage([property: JsonPropertyName("author")] ThreadAuthor? Author, [property: JsonPropertyName("text")] string? Text);
public sealed record ThreadAuthor([property: JsonPropertyName("role")] string? Role, [property: JsonPropertyName("displayName")] string? DisplayName);

public sealed record DocumentInfo
{
    [JsonPropertyName("name")]
    public string? Name { get; init; }
    [JsonPropertyName("fileName")]
    public string? FileName { get; init; }
    [JsonPropertyName("size")]
    public long? Size { get; init; }
    [JsonPropertyName("lastModified")]
    public string? LastModified { get; init; }
    [JsonPropertyName("isIndexed")]
    public bool? IsIndexed { get; init; }
    [JsonPropertyName("errorReason")]
    public string? ErrorReason { get; init; }
}

public sealed record MemorySearchResult
{
    [JsonPropertyName("Id")]
    public string? Id { get; init; }
    [JsonPropertyName("Title")]
    public string? Title { get; init; }
    [JsonPropertyName("Type")]
    public string? Type { get; init; }
    [JsonPropertyName("Filter")]
    public string? Filter { get; init; }
    [JsonPropertyName("Contents")]
    public string? Contents { get; init; }
    [JsonPropertyName("DerivedTypeName")]
    public string? DerivedTypeName { get; init; }
}
