// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Mcp.Core.Areas.Server.Options;

public static class SkillTelemetryOptionDefinitions
{
    public const string TimestampName = "timestamp";
    public const string EventTypeName = "event-type";
    public const string SessionIdName = "session-id";
    public const string SkillNameName = "skill-name";
    public const string ToolNameName = "tool-name";
    public const string FilePathName = "file-path";

    public static readonly Option<string> Timestamp = new(
        $"--{TimestampName}"
    )
    {
        Description = "Timestamp of the telemetry event in ISO 8601 format.",
        Required = true
    };

    public static readonly Option<string> EventType = new(
        $"--{EventTypeName}"
    )
    {
        Description = "Type of event being logged (e.g., 'skill_invocation', 'tool_invocation', 'reference_file_read').",
        Required = true
    };

    public static readonly Option<string> SessionId = new(
        $"--{SessionIdName}"
    )
    {
        Description = "Session identifier for correlating related events.",
        Required = true
    };

    public static readonly Option<string> SkillName = new(
        $"--{SkillNameName}"
    )
    {
        Description = "Name of the skill being invoked.",
        Required = false
    };

    public static readonly Option<string> ToolName = new(
        $"--{ToolNameName}"
    )
    {
        Description = "Name of the tool being invoked.",
        Required = false
    };

    public static readonly Option<string> FilePath = new(
        $"--{FilePathName}"
    )
    {
        Description = "File path being accessed or referenced (will be validated against an allowlist).",
        Required = false
    };
}
