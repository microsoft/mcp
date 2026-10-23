// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.VirtualDesktop.Options.Hostpool;
using Microsoft.Mcp.Core.Commands;

namespace Azure.Mcp.Tools.VirtualDesktop.Commands.Hostpool;

public abstract class BaseHostPoolCommand<[DynamicallyAccessedMembers(TrimAnnotations.CommandAnnotations)] TOptions, TResult>(ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<TOptions, TResult>(subscriptionResolver) where TOptions : BaseHostPoolOptions
{
    public override void ValidateOptions(TOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);

        var hasHostPool = !string.IsNullOrWhiteSpace(options.Hostpool);
        var hasHostPoolResourceId = !string.IsNullOrWhiteSpace(options.HostpoolResourceId);

        // Validate that either hostpool or hostpool-resource-id is provided, but not both
        if (!hasHostPool && !hasHostPoolResourceId)
        {
            validationResult.Errors.Add("Either --hostpool or --hostpool-resource-id must be provided.");
        }

        if (hasHostPool && hasHostPoolResourceId)
        {
            validationResult.Errors.Add("Cannot specify both --hostpool and --hostpool-resource-id. Use only one.");
        }
    }
}
