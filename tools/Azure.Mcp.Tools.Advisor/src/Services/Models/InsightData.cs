// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Tools.Advisor.Commands;

namespace Azure.Mcp.Tools.Advisor.Services.Models;

// Wire models for Azure Resource Graph 'advisorresources' rows of type
// 'microsoft.advisor/insights'. Property names match the ARG payload through the
// camelCase policy on AdvisorJsonContext.

internal sealed record InsightData(string? Id, string? Name, string? Type, InsightDataProperties? Properties)
{
    public static InsightData? FromJson(JsonElement source) =>
        JsonSerializer.Deserialize(source, AdvisorJsonContext.Default.InsightData);
}
