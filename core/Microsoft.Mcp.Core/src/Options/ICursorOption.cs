// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Mcp.Core.Options;

/// <summary>
/// Options for continuing a tool's paginated results with an opaque token.
/// </summary>
public interface ICursorOption
{
    /// <summary>
    /// Gets or sets the unchanged nextCursor from a previous call, or null for the first page.
    /// </summary>
    string? Cursor { get; set; }
}
