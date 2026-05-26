// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.SreAgent.Options.Incidents;

public class IncidentPlanCreateOptions : ISreAgentOption
{
    [Option("The name of the Azure SRE Agent resource to target.")]
    public string? Agent { get; set; }

    [Option(OptionDescriptions.ResourceGroup)]
    public string? ResourceGroup { get; set; }

    [Option(OptionDescriptions.Subscription)]
    public string? Subscription { get; set; }

    [Option(OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }

    [Option(Name = "retry")]
    public RetryPolicyOptions? RetryPolicy { get; set; }

    [Option("The name of the SRE Agent item.")]
    public required string Name { get; set; }

    [Option("Incident severity: critical, high, medium, or low.")]
    public required string Severity { get; set; }

    [Option("Text that triggers the incident response plan.")]
    public required string TriggerCondition { get; set; }

    [Option("Affected service names.")]
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
