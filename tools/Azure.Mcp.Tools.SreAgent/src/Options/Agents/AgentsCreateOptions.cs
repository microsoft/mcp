// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.SreAgent.Options.Agents;

public sealed class AgentsCreateOptions : ISubscriptionOption
{
    [Option(SreAgentOptionDefinitions.AgentDescription)]
    public required string Agent { get; set; }

    [Option(SreAgentOptionDefinitions.NameDescription)]
    public required string Name { get; set; }

    [Option(SreAgentOptionDefinitions.DescriptionDescription)]
    public string? Description { get; set; }

    [Option("Instructions for the sub-agent.")]
    public string? Instructions { get; set; }

    [Option(SreAgentOptionDefinitions.ToolsDescription)]
    public string[]? Tools { get; set; }

    [Option(SreAgentOptionDefinitions.HandoffsDescription)]
    public string[]? Handoffs { get; set; }

    [Option(OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }

    [Option(OptionDescriptions.Subscription)]
    public string? Subscription { get; set; }

    [Option(OptionDescriptions.ResourceGroup)]
    public string? ResourceGroup { get; set; }

    [Option(Name = "retry")]
    public RetryPolicyOptions? RetryPolicy { get; set; }
}
