// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Workbooks.Models;

/// <summary>
/// Represents an error that occurred during a workbook operation.
/// </summary>
public sealed record WorkbookError(
    string WorkbookId,
    int StatusCode,
    string Message
);
