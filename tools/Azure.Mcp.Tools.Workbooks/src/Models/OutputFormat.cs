// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Workbooks.Models;

/// <summary>
/// Output format options for workbook queries.
/// </summary>
public enum OutputFormat
{
    /// <summary>
    /// Minimal output with id and name only (lowest token consumption).
    /// </summary>
    Summary,

    /// <summary>
    /// Standard metadata without serializedData (default).
    /// </summary>
    Standard,

    /// <summary>
    /// Full output including serializedData.
    /// </summary>
    Full
}
