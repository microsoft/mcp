// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.SreAgent.Options.Incidents;

public class IncidentPlanCreateOptions : BaseSreAgentOptions
{
    public string Name { get; set; } = string.Empty;
    public string? Severity { get; set; }
    public string? TriggerCondition { get; set; }
    public string[]? Services { get; set; }
    public string[]? Steps { get; set; }
    public string? Escalation { get; set; }
    public string? RunbookUrl { get; set; }
    public string? AgentMode { get; set; }
}
