// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Tools.Advisor.Commands;

namespace Azure.Mcp.Tools.Advisor.Services.Models;

// Wire models for Azure Resource Graph 'advisorresources' rows of type
// 'microsoft.advisor/servicegroupintelligence'. Property names match the ARG payload through the
// camelCase policy on AdvisorJsonContext.

internal sealed record ServiceGroupIntelligenceData(
    string? Id,
    string? Name,
    string? Type,
    ServiceGroupIntelligenceDataProperties? Properties)
{
    public static ServiceGroupIntelligenceData? FromJson(JsonElement source) =>
        JsonSerializer.Deserialize(source, AdvisorJsonContext.Default.ServiceGroupIntelligenceData);
}
