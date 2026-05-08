// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

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
    [JsonPropertyName("incidentFilterId")]
    public string? IncidentFilterId { get; init; }
    [JsonPropertyName("incidentProcessingGuide")]
    public List<string>? IncidentProcessingGuide { get; init; }
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
}

public sealed record MemorySearchResult
{
    [JsonPropertyName("fileName")]
    public string? FileName { get; init; }
    [JsonPropertyName("content")]
    public string? Content { get; init; }
    [JsonPropertyName("score")]
    public double? Score { get; init; }
}
