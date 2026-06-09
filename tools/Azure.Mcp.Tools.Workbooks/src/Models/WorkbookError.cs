// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Workbooks.Models;

/// <summary>
/// Represents an error that occurred during a workbook operation.
/// </summary>
public sealed record WorkbookError(
    [property: JsonPropertyName("WorkbookId")] string WorkbookId,
    [property: JsonPropertyName("StatusCode")] int StatusCode,
    [property: JsonPropertyName("Message")] string Message);
