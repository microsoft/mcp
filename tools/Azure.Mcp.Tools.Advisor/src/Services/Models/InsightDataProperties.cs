// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Advisor.Services.Models;

internal sealed record InsightDataProperties(
    string? InsightName,
    string? Domain,
    string? LastUpdatedTime,
    List<InsightKpiData>? InsightDetail);
