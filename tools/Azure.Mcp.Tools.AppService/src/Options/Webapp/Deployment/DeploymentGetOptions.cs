// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.AppService.Options.Webapp.Deployment;

public sealed class DeploymentGetOptions : ISubscriptionOption
{
    [Option(Description = AppServiceOptionDefinitions.App)]
    public required string App { get; set; }

    [Option(Description = "The ID of the deployment.")]
    public string? DeploymentId { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }

    [Option(Description = OptionDescriptions.Subscription)]
    public string? Subscription { get; set; }

    [Option(Description = OptionDescriptions.ResourceGroup)]
    public required string ResourceGroup { get; set; }

    [OptionContainer(Prefix = "retry")]
    public RetryPolicyOptions? RetryPolicy { get; set; }
}
