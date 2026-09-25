// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.Kusto.Options;
using Microsoft.Mcp.Core.Commands;

namespace Azure.Mcp.Tools.Kusto.Commands;

public abstract class BaseClusterCommand<
    [DynamicallyAccessedMembers(TrimAnnotations.CommandAnnotations)] TOptions, TResult>(
    ISubscriptionResolver subscriptionResolver)
    : AuthenticatedCommand<TOptions, TResult>
    where TOptions : class, ISubscriptionOption, IClusterOption
{
    protected static bool UseClusterUri(TOptions options) => !string.IsNullOrEmpty(options.ClusterUri);

    public override void PostBindOptions(TOptions options)
    {
        options.Subscription = subscriptionResolver.ResolveSubscription(options.Subscription);
    }

    public override void ValidateOptions(TOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);

        if (!string.IsNullOrEmpty(options.ClusterUri))
        {
            // If clusterUri is provided, subscription becomes optional
            return;
        }

        // clusterUri not provided, require both subscription and clusterName
        if (string.IsNullOrEmpty(options.Cluster) || string.IsNullOrEmpty(options.Subscription))
        {
            validationResult.Errors.Add("Either --cluster-uri must be provided, or both --subscription and --cluster must be provided.");
        }
    }
}
