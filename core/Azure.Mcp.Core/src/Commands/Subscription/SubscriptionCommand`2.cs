// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Microsoft.Mcp.Core.Commands;

namespace Azure.Mcp.Core.Commands.Subscription;

public abstract class SubscriptionCommand<
    [DynamicallyAccessedMembers(TrimAnnotations.CommandAnnotations)] TOptions, TResult>(ISubscriptionResolver subscriptionResolver)
     : AuthenticatedCommand<TOptions, TResult> where TOptions : class, ISubscriptionOption
{
    private readonly ISubscriptionResolver _subscriptionResolver = subscriptionResolver;

    public override void ValidateOptions(TOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);

        if (string.IsNullOrEmpty(options.Subscription))
        {
            validationResult.Errors.Add("Missing Required options: --subscription");
        }
    }

    public override void PostBindOptions(TOptions options)
    {
        // Always post-process subscription via resolver (env var / CLI profile fallback)
        options.Subscription = _subscriptionResolver.ResolveSubscription(options.Subscription);
    }
}
