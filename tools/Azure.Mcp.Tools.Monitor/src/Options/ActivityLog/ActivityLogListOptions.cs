// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.Monitor.Models.ActivityLog;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Monitor.Options.ActivityLog;

public class ActivityLogListOptions : ISubscriptionOption
{
    [Option(Description = "The name of the Azure resource to retrieve activity logs for.")]
    public required string ResourceName { get; set; }

    [Option(Description = "The type of the Azure resource (e.g., 'Microsoft.Storage/storageAccounts'). Only provide this if needed to disambiguate between multiple resources with the same name.")]
    public string? ResourceType { get; set; }

    [Option(Description = "The number of hours prior to now to retrieve activity logs for. Default is 24.0.", DefaultValue = 24.0)]
    public double? Hours { get; set; }

    [Option(Description = "The level of activity logs to retrieve. Valid levels are: Critical, Error, Informational, Verbose, Warning. If not provided, returns all levels.")]
    public ActivityLogEventLevel? EventLevel { get; set; }

    [Option(Description = "The maximum number of activity logs to retrieve. Default is 10.", DefaultValue = 10)]
    public int? Top { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }

    [Option(Description = OptionDescriptions.Subscription)]
    public string? Subscription { get; set; }

    [Option(Description = OptionDescriptions.ResourceGroup)]
    public string? ResourceGroup { get; set; }

    [OptionContainer(Prefix = "retry")]
    public RetryPolicyOptions? RetryPolicy { get; set; }
}
