// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Core.Areas.Server.Options;

/// <summary>
/// Configuration options for starting the Azure MCP server service.
/// </summary>
public class ServiceStartOptions
{
    /// <summary>
    /// Gets or sets the transport mechanism for the server.
    /// Defaults to standard I/O (stdio).
    /// </summary>
    [JsonPropertyName("transport")]
    public string Transport { get; set; } = TransportTypes.StdIo;

    /// <summary>
    /// Gets or sets the service namespaces to expose through the server.
    /// When null, all available namespaces are exposed.
    /// </summary>
    [JsonPropertyName("namespace")]
    public string[]? Namespace { get; set; } = null;

    /// <summary>
    /// Gets or sets the mode mode for the server.
    /// Defaults to 'namespace' mode which exposes one tool per service namespace.
    /// </summary>
    [JsonPropertyName("mode")]
    public string? Mode { get; set; } = ModeTypes.NamespaceProxy;

    /// <summary>
    /// Gets or sets the specific tool names to expose.
    /// When specified, only these tools will be available.
    /// </summary>
    [JsonPropertyName("tool")]
    public string[]? Tool { get; set; } = null;

    /// <summary>
    /// Gets or sets whether the server should operate in read-only mode.
    /// When true, only tools marked as read-only will be available.
    /// </summary>
    [JsonPropertyName("readOnly")]
    public bool? ReadOnly { get; set; } = null;

    /// <summary>
    /// Gets or sets whether debug mode is enabled.
    /// When true, verbose logging will be sent to stderr.
    /// </summary>
    [JsonPropertyName("debug")]
    public bool Debug { get; set; } = false;

    /// <summary>
    /// Gets or sets whether HTTP incoming authentication is disabled.
    /// When true, the server accepts unauthenticated HTTP requests.
    /// </summary>
    [JsonPropertyName("dangerouslyDisableHttpIncomingAuth")]
    public bool DangerouslyDisableHttpIncomingAuth { get; set; } = false;

    /// <summary>
    /// Gets or sets whether elicitation (user confirmation for high-risk operations like accessing secrets) is disabled (insecure mode).
    /// When true, elicitation will always be treated as accepted without user confirmation.
    /// </summary>
    [JsonPropertyName("insecureDisableElicitation")]
    public bool InsecureDisableElicitation { get; set; } = false;

    /// <summary>
    /// Gets or sets the outgoing authentication strategy for Azure service requests.
    /// Determines whether to use hosting environment identity or on-behalf-of flow.
    /// </summary>
    [JsonPropertyName("outgoingAuthStrategy")]
    public OutgoingAuthStrategy OutgoingAuthStrategy { get; set; } = OutgoingAuthStrategy.NotSet;

    /// <summary>
    /// Gets or sets whether support logging is enabled.
    /// When true, detailed debug-level logging is enabled for troubleshooting purposes.
    /// Warning: This may include sensitive information in logs.
    /// </summary>
    [JsonPropertyName("dangerouslyEnableSupportLogging")]
    public bool DangerouslyEnableSupportLogging { get; set; } = false;

    /// <summary>
    /// Gets or sets the file path where logs should be written.
    /// When support logging is enabled, logs will be written to this file.
    /// If null, logs will only be written to stderr.
    /// </summary>
    [JsonPropertyName("logFilePath")]
    public string? LogFilePath { get; set; } = null;
}
