// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.ResilienceManagement.Options.Goals.Templates;

public class GoalTemplateGetOptions : ISubscriptionOption
{
    [Option(Description = "The name of the service group.")]
    public required string ServiceGroup { get; set; }

    [Option(Description = "The name of the goal template. Provide this argument to get the details of a particular goal template; omit it to list all goal templates in the service group (id and name only).")]
    public string? Name { get; set; }

    [Option(Description = OptionDescriptions.Subscription)]
    public string? Subscription { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }

    [OptionContainer(Prefix = "retry")]
    public RetryPolicyOptions? RetryPolicy { get; set; }
}
