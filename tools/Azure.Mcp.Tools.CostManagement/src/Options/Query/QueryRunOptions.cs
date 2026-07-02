// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.CostManagement.Models;

namespace Azure.Mcp.Tools.CostManagement.Options.Query;

public class QueryRunOptions : BaseCostManagementOptions
{
    public QueryTimeframe? Timeframe { get; set; }
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public QueryGranularity? Granularity { get; set; }
    public string? GroupBy { get; set; }
}
