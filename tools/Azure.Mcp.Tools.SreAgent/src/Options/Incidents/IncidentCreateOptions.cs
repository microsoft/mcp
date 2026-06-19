// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.SreAgent.Options.Incidents;

public sealed class IncidentCreateOptions : ISubscriptionOption
{
    [Option(SreAgentOptionDefinitions.AgentDescription)]
    public required string Agent { get; set; }

    [Option(SreAgentOptionDefinitions.SeverityDescription)]
    public required string Severity { get; set; }

    [Option("Incident title.")]
    public required string Title { get; set; }

    [Option(SreAgentOptionDefinitions.DescriptionDescription)]
    public required string Description { get; set; }

    [Option(SreAgentOptionDefinitions.ServicesDescription)]
    public required string[] Services { get; set; }

    [Option(OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }

    [Option(OptionDescriptions.Subscription)]
    public string? Subscription { get; set; }

    [Option(OptionDescriptions.ResourceGroup)]
    public string? ResourceGroup { get; set; }

    [Option(Name = "retry")]
    public RetryPolicyOptions? RetryPolicy { get; set; }
}
