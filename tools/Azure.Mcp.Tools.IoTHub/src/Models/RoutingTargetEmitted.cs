// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;

namespace Azure.Mcp.Tools.IoTHub.Models;

public record RoutingTargetEmitted(
    string QueryStatus,
    string MetricScope,
    string? MetricNamespace,
    Dictionary<string, JsonElement> WindowAggregates,
    List<Dictionary<string, JsonElement>> Buckets,
    List<RoutingDiagnosticError> Errors)
{
    public Dictionary<string, string> MetricAvailability { get; init; } = [];
}
