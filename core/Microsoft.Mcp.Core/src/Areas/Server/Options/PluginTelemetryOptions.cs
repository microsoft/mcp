// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Microsoft.Mcp.Core.Areas.Server.Options;

/// <summary>
/// Configuration options for publishing plugin telemetry.
/// Inherits from ServiceStartOptions to enable full MCP server service registration.
/// </summary>
public sealed class PluginTelemetryOptions
{
    /// <summary>
    /// Gets or sets the timestamp of the telemetry event in ISO 8601 format.
    /// </summary>
    [Option(Description = "Timestamp of the telemetry event in ISO 8601 format.")]
    public required string Timestamp { get; set; }

    /// <summary>
    /// Gets or sets the type of event being logged (e.g., 'plugin_invocation', 'tool_invocation', 'file_reference').
    /// </summary>
    [Option(Description = "Type of event being logged (e.g., 'skill_invocation', 'tool_invocation', 'reference_file_read').")]
    public required string EventType { get; set; }

    /// <summary>
    /// Gets or sets the session identifier for correlating related events.
    /// </summary>
    [Option(Description = "Session identifier for correlating related events.")]
    public required string SessionId { get; set; }

    /// <summary>
    /// Gets or sets the type of client invoking the telemetry (e.g., 'copilot-cli', 'claude-code', 'vscode'). This field is legacy and will be deprecated in favor of clientName when usage in telemetry is sufficiently low. New integrations should prefer clientName but may continue to populate clientType for backward compatibility.
    /// </summary>
    [Option(Description = "Type of client invoking the telemetry (e.g., 'copilot-cli', 'claude-code', 'vscode'). Deprecated: prefer --client-name.")]
    public string? ClientType { get; set; }

    /// <summary>
    /// Gets or sets the name of the client invoking the telemetry (e.g., 'copilot-cli', 'claude-code', 'Visual Studio Code', 'Visual Studio Code - Insiders').
    /// </summary>
    [Option(Description = "Name of the client invoking the telemetry (e.g., 'copilot-cli', 'claude-code', 'Visual Studio Code', 'Visual Studio Code - Insiders').")]
    public string? ClientName { get; set; }

    /// <summary>
    /// Gets or sets the name of the plugin being invoked.
    /// </summary>
    [Option(Description = "Name of the plugin being invoked.")]
    public string? PluginName { get; set; }

    /// <summary>
    /// Gets or sets the version of the plugin being invoked.
    /// </summary>
    [Option(Description = "Version of the plugin being invoked.")]
    public string? PluginVersion { get; set; }

    /// <summary>
    /// Gets or sets the name of the skill being invoked.
    /// </summary>
    [Option(Description = "Name of the skill being invoked.")]
    public string? SkillName { get; set; }

    /// <summary>
    /// Gets or sets the version of the skill being invoked.
    /// </summary>
    [Option(Description = "Version of the skill being invoked.")]
    public string? SkillVersion { get; set; }

    /// <summary>
    /// Gets or sets the name of the tool being invoked.
    /// </summary>
    [Option(Description = "Name of the tool being invoked.")]
    public string? ToolName { get; set; }

    /// <summary>
    /// Gets or sets the file reference being accessed.
    /// This should be a plugin-relative path that will be validated against an allowlist before telemetry is emitted.
    /// </summary>
    [Option(Description = "Plugin-relative file reference being accessed (will be validated against an allowlist).")]
    public string? FileReference { get; set; }

    /// <summary>
    /// Gets or sets whether debug mode is enabled.
    /// When true, verbose logging will be sent to stderr.
    /// </summary>
    [Option(Description = ServerOptionDescriptions.Debug)]
    public bool Debug { get; set; } = false;

    /// <summary>
    /// Gets or sets the folder path for support logging.
    /// When specified, detailed debug-level logging is enabled and logs are written to
    /// automatically generated files in this folder with timestamp-based filenames.
    /// Warning: This may include sensitive information in logs.
    /// </summary>
    [Option(Description = ServerOptionDescriptions.DangerouslyWriteSupportLogsToDir)]
    public string? DangerouslyWriteSupportLogsToDir { get; set; } = null;
}
