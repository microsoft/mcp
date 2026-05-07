// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.CostManagement.Models;

public record CostQueryResult(
    string? Currency,
    decimal TotalCost,
    string Timeframe,
    DateTime? FromDate,
    DateTime? ToDate,
    string Granularity,
    string? GroupBy,
    IReadOnlyList<CostQueryRow> Rows);
