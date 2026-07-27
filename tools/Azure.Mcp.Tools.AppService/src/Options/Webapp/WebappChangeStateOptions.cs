// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.AppService.Options.Webapp;

public sealed class WebappChangeStateOptions : ISubscriptionOption
{
    [Option(Description = AppServiceOptionDefinitions.App)]
    public required string App { get; set; }

    [Option(Description = "The state change action to perform. Valid values are: start, stop, restart.")]
    public required string StateChange { get; set; }

    [Option(Description = "When state-change is restart, indicates whether to perform a soft restart.")]
    public bool SoftRestart { get; set; } = false;

    [Option(Description = "When state-change is restart, indicates whether to synchronously wait for the state change operation to complete before returning.")]
    public bool WaitForCompletion { get; set; } = false;

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }

    [Option(Description = OptionDescriptions.Subscription)]
    public string? Subscription { get; set; }

    [Option(Description = OptionDescriptions.ResourceGroup)]
    public required string ResourceGroup { get; set; }

    [OptionContainer(Prefix = "retry")]
    public RetryPolicyOptions? RetryPolicy { get; set; }
}
