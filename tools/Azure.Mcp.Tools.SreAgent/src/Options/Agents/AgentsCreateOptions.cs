// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.SreAgent.Options.Agents;

public class AgentsCreateOptions : ISreAgentOption
{
    [Option("The name of the Azure SRE Agent resource to target.")]
    public required string? Agent { get; set; }

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

    [Option("A description for the SRE Agent item.")]
    public string? Description { get; set; }

    [Option("Instructions for the sub-agent.")]
    public string? Instructions { get; set; }

    [Option("Tool names to attach. Multiple values are supported.")]
    public string[]? Tools { get; set; }

    [Option("Sub-agent handoff names. Multiple values are supported.")]
    public string[]? Handoffs { get; set; }
}
