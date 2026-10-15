// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;

namespace Azure.Mcp.Tools.Advisor.Services.Models;

internal sealed record ServiceGroupIntelligenceDataProperties(
    string? ServiceGroupName,
    string? ServiceGroupId,
    string? Criticality,
    string? LastUpdatedTime,
    List<ServiceGroupIntelligenceKpiData>? IntelligenceDetail,
    JsonElement? Goal);
