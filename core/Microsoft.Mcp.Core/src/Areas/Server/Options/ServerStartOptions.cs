// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Microsoft.Mcp.Core.Options;

namespace Microsoft.Mcp.Core.Areas.Server.Options;

/// <summary>
/// Configuration options for starting the MCP server.
/// </summary>
public sealed class ServerStartOptions
{
    /// <summary>
    /// Gets or sets the transport mechanism for the server.
    /// Defaults to standard I/O (stdio).
    /// </summary>
    [Option(Description = "Transport mechanism to use for MCP Server.", DefaultValue = "stdio")]
    public string Transport { get; set; } = TransportTypes.StdIo;

    /// <summary>
    /// Gets or sets the namespaces to expose through the server.
    /// When null, all available namespaces are exposed.
    /// </summary>
    [Option(Description = "The service namespaces to expose on the MCP server (e.g., storage, keyvault, cosmos).")]
    public string[]? Namespace { get; set; } = null;

    /// <summary>
    /// Gets or sets the mode mode for the server.
    /// Defaults to 'namespace' mode which exposes one tool per namespace.
    /// </summary>
    [Option(Description = "Mode for the MCP server. 'single' exposes one azure tool that routes to all services. 'namespace' (default) exposes one tool per service namespace. 'all' exposes all tools individually.", DefaultValue = "namespace")]
    public string? Mode { get; set; } = ModeTypes.NamespaceProxy;

    /// <summary>
    /// Gets or sets the specific tool names to expose.
    /// When specified, only these tools will be available.
    /// </summary>
    [Option(Description = "Expose only specific tools by name (e.g., 'acr_registry_list'). Repeat this option to include multiple tools, e.g., --tool \"acr_registry_list\" --tool \"group_list\". It automatically switches to \"all\" mode when \"--tool\" is used. It can't be used together with \"--namespace\".")]
    public string[]? Tool { get; set; } = null;

    /// <summary>
    /// Gets or sets whether the server should operate in read-only mode.
    /// When true, only tools marked as read-only will be available.
    /// </summary>
    [Option(Description = "Whether the MCP server should be read-only. If true, no write operations will be allowed.", DefaultValue = false)]
    public bool? ReadOnly { get; set; } = null;

    /// <summary>
    /// Gets or sets whether debug mode is enabled.
    /// When true, verbose logging will be sent to stderr.
    /// </summary>
    [Option(Description = ServerOptionDescriptions.Debug, DefaultValue = false)]
    public bool Debug { get; set; } = false;

    /// <summary>
    /// Gets or sets whether HTTP incoming authentication is disabled.
    /// When true, the server accepts unauthenticated HTTP requests.
    /// </summary>
    [Option(Description = "Dangerously disables HTTP incoming authentication, exposing the server to unauthenticated access over HTTP. Use with extreme caution, this disables all transport security and may expose sensitive data to interception.", DefaultValue = false)]
    public bool DangerouslyDisableHttpIncomingAuth { get; set; } = false;

    /// <summary>
    /// Gets or sets whether elicitation (user confirmation for high-risk operations like accessing secrets) is disabled (dangerous mode).
    /// When true, elicitation will always be treated as accepted without user confirmation.
    /// </summary>
    [Option(Description = "Disable elicitation (user confirmation) before allowing high risk commands to run, such as returning Secrets (passwords) from KeyVault.", DefaultValue = false)]
    public bool DangerouslyDisableElicitation { get; set; } = false;

    /// <summary>
    /// Gets or sets the outgoing authentication strategy for requests.
    /// Determines whether to use hosting environment identity or on-behalf-of flow.
    /// </summary>
    [Option(Description = "Outgoing authentication strategy for service requests. Valid values: NotSet, UseHostingEnvironmentIdentity, UseOnBehalfOf.", DefaultValue = "NotSet")]
    public OutgoingAuthStrategy OutgoingAuthStrategy { get; set; } = OutgoingAuthStrategy.NotSet;

    /// <summary>
    /// Gets or sets the folder path for support logging.
    /// When specified, detailed debug-level logging is enabled and logs are written to
    /// automatically generated files in this folder with timestamp-based filenames.
    /// Warning: This may include sensitive information in logs.
    /// </summary>
    [Option(Description = ServerOptionDescriptions.DangerouslyWriteSupportLogsToDir)]
    public string? DangerouslyWriteSupportLogsToDir { get; set; }

    /// <summary>
    /// Gets or sets whether retry policy bounds checking is disabled.
    /// When true, no upper bounds are enforced on retry delays, max delays, network timeouts, or max retries.
    /// </summary>
    [Option(Description = "Dangerously disables upper bounds on retry delays, max delays, network timeouts, and max retries. This may lead to excessively long waits and should only be used when explicitly needed.", DefaultValue = false)]
    public bool DangerouslyDisableRetryLimits { get; set; } = false;

    /// <summary>
    /// Gets or sets the Azure cloud environment for authentication.
    /// Supports well-known cloud names (AzureCloud, AzureChinaCloud, AzureUSGovernment).
    /// </summary>
    [Option(Description = "Azure cloud environment for authentication. Valid values: AzureCloud (default), AzureChinaCloud, or AzureUSGovernment")]
    public string? Cloud { get; set; }

    /// <summary>
    /// Gets a value indicating whether the server is running in HTTP (remote) mode.
    /// </summary>
    [JsonIgnore]
    public bool IsHttpMode => Transport == TransportTypes.Http;

    /// <summary>
    /// Gets or sets whether caching is disabled.
    /// </summary>
    [Option(Description = "Disable caching of resource responses, requiring repeated requests to fetch fresh data each time.", DefaultValue = false)]
    public bool DisableCaching { get; set; } = false;
}
