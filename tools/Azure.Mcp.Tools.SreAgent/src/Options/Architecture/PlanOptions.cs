// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.SreAgent.Options.Architecture;

public class PlanOptions
{
    [Option("Architecture requirements.")]
    public required string Requirements { get; set; }

    [Option("Trigger type, such as manual or scheduled.")]
    public string? TriggerType { get; set; }

    [Option("Kusto connector name.")]
    public string? KustoConnector { get; set; }

    [Option(OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }

    [Option(Name = "retry")]
    public RetryPolicyOptions? RetryPolicy { get; set; }
}
