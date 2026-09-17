// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Areas.Server.Options;

namespace Microsoft.Mcp.Core.Areas.Server;

/// <summary>
/// The server's runtime configurations, such as the mode it is running in, transport type, and other runtime-specific settings.
/// Use this class when dependency injected services or tools need to know information about the server's runtime.
/// </summary>
public sealed class ServerRuntimeConfiguration
{
    /// <summary>
    /// The transport mechanism the server is using to communicate with clients.
    /// </summary>
    public string Transport { get; set; } = TransportTypes.StdIo;

    /// <summary>
    /// The mode the server is running in, which determines how it exposes tools and commands to clients.
    /// </summary>
    public string Mode { get; set; } = ModeTypes.Default;

    /// <summary>
    /// The namespaces that the server is exposing to clients.
    /// </summary>
    public string[]? Namespace { get; set; }

    /// <summary>
    /// The specific tool names that the server is exposing to clients.
    /// </summary>
    public string[]? Tool { get; set; }

    /// <summary>
    /// Indicates whether the server is running in read-only mode, which restricts clients from performing write operations.
    /// </summary>
    public bool ReadOnly { get; set; } = false;

    /// <summary>
    /// Whether elicitation (user confirmation for high-risk operations like accessing secrets) is disabled (dangerous mode).
    /// When true, elicitation will always be treated as accepted without user confirmation.
    /// </summary>
    public bool DangerouslyDisableElicitation { get; set; } = false;

    /// <summary>
    /// The Azure cloud configuration the server is using, which determines how it authenticates and interacts with Azure services.
    /// </summary>
    public string? Cloud { get; set; }

    /// <summary>
    /// Indicates whether the server is running in HTTP transport mode.
    /// </summary>
    public bool IsHttpMode => Transport == TransportTypes.Http;

    /// <summary>
    /// How eligible tools advertise and return structured output, or <see langword="null"/> to disable structured output.
    /// </summary>
    public StructuredOutputMode? StructuredOutputMode { get; set; }
}
