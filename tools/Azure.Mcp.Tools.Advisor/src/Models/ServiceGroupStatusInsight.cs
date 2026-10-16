// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Advisor.Models;

public sealed record ServiceGroupStatusInsight(
    string Name,
    string Id,
    string? Criticality,
    string? CriticalityLabel,
    string? Status,
    string? StatusDescription,
    string? Description,
    List<ServiceGroupInsight>? Insights);
