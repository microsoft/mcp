// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.WellArchitectedFramework.Options.ServiceGuide;

/// <summary>
/// Defines the output mode for service guide commands.
/// </summary>
public enum ServiceGuideOutputMode
{
    /// <summary>
    /// Returns a summary of the service guide with JSON content.
    /// </summary>
    Summary,

    /// <summary>
    /// Returns the URL to the markdown file for the service guide.
    /// </summary>
    Url
}
