// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Mcp.Core.Options;

/// <summary>
/// Options for continuing a tool's paginated results with an opaque token.
/// </summary>
public interface ICursorOption
{
    /// <summary>
    /// Standard cursor parameter description, before any tool-specific guidance.
    /// </summary>
    public const string CursorDescription = "Opaque nextCursor from a previous call. Pass it unchanged to retrieve the next page. Omit for the first page. An empty or missing nextCursor means there are no additional pages to retrieve.";

    /// <summary>
    /// Gets or sets the unchanged nextCursor from a previous call, or null for the first page.
    /// </summary>
    string? Cursor { get; set; }
}
