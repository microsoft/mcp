// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.SreAgent.Options.Incidents;

public sealed class IncidentPlanCreateOptions : BaseSreAgentOptions
{
    [Option(SreAgentOptionDefinitions.NameDescription)]
    public required string Name { get; set; }

    [Option(SreAgentOptionDefinitions.SeverityDescription)]
    public required string Severity { get; set; }

    [Option("Text that triggers the incident response plan.")]
    public required string TriggerCondition { get; set; }

    [Option(SreAgentOptionDefinitions.ServicesDescription)]
    public required string[] Services { get; set; }

    [Option("Incident response steps.")]
    public required string[] Steps { get; set; }

    [Option("Escalation procedure.")]
    public string? Escalation { get; set; }

    [Option("Runbook URL.")]
    public string? RunbookUrl { get; set; }

    [Option("Agent mode: autonomous or review.")]
    public string? AgentMode { get; set; }
}
