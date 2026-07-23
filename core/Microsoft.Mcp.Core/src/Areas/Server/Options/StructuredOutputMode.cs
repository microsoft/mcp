// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Mcp.Core.Areas.Server.Options;

/// <summary>
/// Controls whether MCP tools advertise and return structured output.
/// </summary>
public enum StructuredOutputMode
{
    /// <summary>
    /// Preserve the historical content-only response shape.
    /// </summary>
    Legacy = 0,

    /// <summary>
    /// Return the complete response in both content and structuredContent.
    /// </summary>
    Duplicated = 1,

    /// <summary>
    /// Return the complete response in structuredContent and concise text in content.
    /// </summary>
    Compact = 2
}
