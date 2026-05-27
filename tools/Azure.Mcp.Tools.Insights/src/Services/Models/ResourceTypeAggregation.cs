// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Nodes;

namespace Azure.Mcp.Tools.Insights.Services.Models;

// Property value frequencies for a single ARM resource type. PropertyAggregations mirrors the
// resource's property shape; each scalar leaf is replaced by an object mapping the top observed
// values to their counts.
public sealed record ResourceTypeAggregation(
    string ArmResourceType,
    int TotalCount,
    JsonObject PropertyAggregations);
