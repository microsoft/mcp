// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Workbooks.Models;

/// <summary>
/// Result of a list workbooks operation with pagination metadata.
/// </summary>
public sealed record WorkbookListResult(
    IReadOnlyList<WorkbookInfo> Workbooks,
    int? TotalCount,
    string? ContinuationToken
);
