// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Skills.Options;

public static class SkillsOptionDefinitions
{
    public const string EventsName = "events";

    public static readonly Option<string> Events = new(
        $"--{EventsName}"
    )
    {
        Description = "A JSON array of telemetry events to publish. Each event should include fields such as 'timestamp', 'event_type', 'tool_name', and 'session_id'.",
        Required = true
    };
}
