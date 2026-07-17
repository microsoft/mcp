// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.SreAgent.Options.Incidents;

public sealed class IncidentPlanCreateOptions : BaseSreAgentOptions
{
    [Option(Description = SreAgentOptionDefinitions.NameDescription)]
    public required string Name { get; set; }

    [Option(Description = SreAgentOptionDefinitions.SeverityDescription)]
    public required string Severity { get; set; }

    [Option(Description = "Text that triggers the incident response plan.")]
    public required string TriggerCondition { get; set; }

    [Option(Description = SreAgentOptionDefinitions.ServicesDescription)]
    public required string[] Services { get; set; }

    [Option(Description = "Incident response steps.")]
    public required string[] Steps { get; set; }

    [Option(Description = "Escalation procedure.")]
    public string? Escalation { get; set; }

    [Option(Description = "Runbook URL.")]
    public string? RunbookUrl { get; set; }

    [Option(Description = "Agent mode: autonomous or review.")]
    public string? AgentMode { get; set; }
}
