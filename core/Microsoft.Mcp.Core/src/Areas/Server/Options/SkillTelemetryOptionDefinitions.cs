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
    public const string FileReferenceName = "file-reference";

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

    public static readonly Option<string> FileReference = new(
        $"--{FileReferenceName}"
    )
    {
        Description = "Skill file reference being accessed (will be validated against an allowlist and sanitized to remove PII).",
        Required = false
    };
}
