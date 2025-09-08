// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.AppInsights.Models;

public class ApplicationInsightsInfo
{
    public required string Name { get; set; }
    public required string Id { get; set; }
    public required string Location { get; set; }
    public string? AppId { get; set; }
    public string? InstrumentationKey { get; set; }
}
