// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.AppService.Options.Webapp;

public sealed class WebappChangeStateOptions : ISubscriptionOption
{
    [Option(OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }

    [Option(OptionDescriptions.Subscription)]
    public string? Subscription { get; set; }

    [Option(OptionDescriptions.ResourceGroup)]
    public required string ResourceGroup { get; set; }

    [Option(Name = "retry")]
    public RetryPolicyOptions? RetryPolicy { get; set; }

    [Option(AppServiceOptionDefinitions.App)]
    public required string App { get; set; }

    [Option("The state change action to perform. Valid values are: start, stop, restart.")]
    public required string StateChange { get; set; }

    [Option("When state-change is restart, indicates whether to perform a soft restart.")]
    public bool SoftRestart { get; set; } = false;

    [Option("When state-change is restart, indicates whether to synchronously wait for the state change operation to complete before returning.")]
    public bool WaitForCompletion { get; set; } = false;
}
