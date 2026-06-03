// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.SreAgent.Options.Incidents;

public class IncidentCreateOptions : ISreAgentOption
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

    [Option("Incident severity: critical, high, medium, or low.")]
    public required string Severity { get; set; }

    [Option("Incident title.")]
    public required string Title { get; set; }

    [Option("A description for the SRE Agent item.")]
    public string? Description { get; set; }

    [Option("Affected service names.")]
    public string[]? Services { get; set; }
}
