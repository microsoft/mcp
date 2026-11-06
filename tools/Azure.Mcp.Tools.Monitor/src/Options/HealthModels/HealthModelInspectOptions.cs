// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.Monitor.Models.HealthModels;
using Azure.Mcp.Tools.Monitor.Services;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Monitor.Options.HealthModels;

public sealed class HealthModelInspectOptions : ISubscriptionOption
{
    [Option(Description = "The name of the health model to inspect.")]
    public required string HealthModel { get; set; }

    [Option(Description = OptionDescriptions.ResourceGroup)]
    public required string ResourceGroup { get; set; }

    [Option(Description = "The entity to read, or to start --walk from. Default: every entity; for --walk, the root entity, which is named like the health model.")]
    public string? Entity { get; set; }

    [Option(Description = "Keep only entities in this state: Healthy, Degraded, Unhealthy, Unknown, or NotHealthy (Degraded, Unhealthy or Unknown).")]
    public HealthModelHealthFilter? WhereHealth { get; set; }

    [Option(Description = "Follow the dependency graph from --entity: Causes (down through the children that drive each state: Degraded or Unhealthy and not suppressed, Unknown only where the rollup counts it), Dependencies (everything it depends on), or Dependents (everything that depends on it). Each entity comes back with its path.")]
    public HealthModelWalk? Walk { get; set; }

    [Option(Description = "What to read besides each entity: Signals (keeps its signal groups and returns the signal definitions they name), History (getHistory), SignalHistory (getSignalHistory per signal; needs --entity), Annotations (getDataAnnotations).")]
    public HealthModelInclude[]? Include { get; set; }

    [Option(Description = "The startAt of the History, SignalHistory and Annotations reads (ISO 8601). Default: a day before the end.")]
    public DateTimeOffset? StartAt { get; set; }

    [Option(Description = "The endAt of those reads (ISO 8601). Default: --timestamp, else the service's default (now).")]
    public DateTimeOffset? EndAt { get; set; }

    [Option(Description = "Read entities and relationships as they were at this instant (the API's timestamp, ISO 8601).")]
    public DateTimeOffset? Timestamp { get; set; }

    [Option(Description = $"Field paths to keep, comma-separated or repeated, prefixed by payload: entity, history, signalHistory, annotations or signalDefinitions, for example entity.properties.healthState or entity.properties.signalGroups.*.signals.status. A path keeps everything below it, * as a segment stands for any property, ** for any number of levels, and * alone keeps everything. The result's fields lists every path. The payloads are CloudHealth API {HealthModelApi.Version} objects: {HealthModelApi.Contract}")]
    public string[]? Fields { get; set; }

    [Option(Description = "The most entities to return, 1 through 500. Default: 50.", DefaultValue = 50)]
    public int Top { get; set; }

    [Option(Description = "The result's nextMarker, to read the next page with the same other options. Entities that change in between can move between pages.")]
    public string? NextMarker { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }

    [Option(Description = OptionDescriptions.Subscription)]
    public string? Subscription { get; set; }
}
