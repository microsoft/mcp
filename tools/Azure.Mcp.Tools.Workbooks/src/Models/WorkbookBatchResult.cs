// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Workbooks.Models;

/// <summary>
/// Result of a batch operation with partial success handling.
/// </summary>
public sealed record WorkbookBatchResult(
    IReadOnlyList<WorkbookInfo> Succeeded,
    IReadOnlyList<WorkbookError> Failed
);

/// <summary>
/// Result of a batch delete operation.
/// </summary>
public sealed record WorkbookDeleteBatchResult(
    IReadOnlyList<string> Succeeded,
    IReadOnlyList<WorkbookError> Failed
);
