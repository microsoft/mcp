// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Microsoft.Mcp.Core.Areas.Server.Options;

/// <summary>
/// Configuration options for publishing plugin telemetry.
/// Inherits from ServiceStartOptions to enable full MCP server service registration.
/// </summary>
public class PluginTelemetryOptions : ServiceStartOptions
{
    /// <summary>
    /// Gets or sets the timestamp of the telemetry event in ISO 8601 format.
    /// </summary>
    [JsonPropertyName("timestamp")]
    public string? Timestamp { get; set; }

    /// <summary>
    /// Gets or sets the type of event being logged (e.g., 'plugin_invocation', 'tool_invocation', 'file_reference').
    /// </summary>
    [JsonPropertyName("eventType")]
    public string? EventType { get; set; }

    /// <summary>
    /// Gets or sets the session identifier for correlating related events.
    /// </summary>
    [JsonPropertyName("sessionId")]
    public string? SessionId { get; set; }

    /// <summary>
    /// Gets or sets the type of client invoking the telemetry (e.g., 'copilot-cli', 'claude-code', 'vscode'). This field is legacy and will be deprecated in favor of clientName when usage in telemetry is sufficiently low. New integrations should prefer clientName but may continue to populate clientType for backward compatibility.
    /// </summary>
    [JsonPropertyName("clientType")]
    public string? ClientType { get; set; }

    /// <summary>
    /// Gets or sets the name of the client invoking the telemetry (e.g., 'copilot-cli', 'claude-code', 'Visual Studio Code', 'Visual Studio Code - Insiders').
    /// </summary>
    [JsonPropertyName("clientName")]
    public string? ClientName { get; set; }

    /// <summary>
    /// Gets or sets the name of the plugin being invoked.
    /// </summary>
    [JsonPropertyName("pluginName")]
    public string? PluginName { get; set; }

    /// <summary>
    /// Gets or sets the version of the plugin being invoked.
    /// </summary>
    [JsonPropertyName("pluginVersion")]
    public string? PluginVersion { get; set; }

    /// <summary>
    /// Gets or sets the name of the skill being invoked.
    /// </summary>
    [JsonPropertyName("skillName")]
    public string? SkillName { get; set; }

    /// <summary>
    /// Gets or sets the version of the skill being invoked.
    /// </summary>
    [JsonPropertyName("skillVersion")]
    public string? SkillVersion { get; set; }

    /// <summary>
    /// Gets or sets the name of the tool being invoked.
    /// </summary>
    [JsonPropertyName("toolName")]
    public string? ToolName { get; set; }

    /// <summary>
    /// Gets or sets the area of the tool being invoked. (e.g., monitor_workspace_list)
    /// </summary>
    [JsonPropertyName("toolArea")]
    public string? ToolArea { get; set; }

    /// <summary>
    /// Gets or sets the file reference being accessed.
    /// This should be a plugin-relative path that will be validated against an allowlist before telemetry is emitted.
    /// </summary>
    [JsonPropertyName("fileReference")]
    public string? FileReference { get; set; }
}
