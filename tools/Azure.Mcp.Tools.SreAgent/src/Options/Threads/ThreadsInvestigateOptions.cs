// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.SreAgent.Options.Threads;

public class ThreadsInvestigateOptions : ISreAgentOption
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

    [Option("The message to send.")]
    public required string Message { get; set; }

    [Option("The maximum number of automatic follow-up iterations.")]
    public int MaxIterations { get; set; } = 20;

    [Option("The investigation timeout in seconds.")]
    public int TimeoutSeconds { get; set; } = 600;
}
