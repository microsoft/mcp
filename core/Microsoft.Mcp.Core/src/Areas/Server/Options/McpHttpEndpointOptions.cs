// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Mcp.Core.Areas.Server.Options;

/// <summary>
/// Configuration for the remote HTTP MCP endpoint and its OAuth protected resource.
/// </summary>
public sealed class McpHttpEndpointOptions
{
    /// <summary>
    /// The configuration section containing the remote HTTP endpoint settings.
    /// </summary>
    public const string SectionName = "McpServer";

    /// <summary>
    /// Gets or sets the path at which the MCP endpoint is mapped.
    /// </summary>
    public string? EndpointPath { get; set; }

    /// <summary>
    /// Gets or sets the OAuth protected resource metadata configuration.
    /// </summary>
    public ProtectedResourceOptions ProtectedResource { get; set; } = new();
}
