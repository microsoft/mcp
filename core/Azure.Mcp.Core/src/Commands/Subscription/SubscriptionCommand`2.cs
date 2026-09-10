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

    /// <summary>
    /// Determines whether subscription resolution and validation apply to the bound options.
    /// </summary>
    /// <remarks>This method is evaluated during post-binding and must only inspect already-bound option values.</remarks>
    protected virtual bool IsSubscriptionApplicable(TOptions options) => true;

    public override void ValidateOptions(TOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);

        if (IsSubscriptionApplicable(options) && string.IsNullOrEmpty(options.Subscription))
        {
            validationResult.Errors.Add("Missing Required options: --subscription");
        }
    }

    public override void PostBindOptions(TOptions options)
    {
        base.PostBindOptions(options);

        if (!IsSubscriptionApplicable(options))
        {
            return;
        }

        // Always post-process subscription via resolver (env var / CLI profile fallback)
        options.Subscription = _subscriptionResolver.ResolveSubscription(options.Subscription);
        if (!string.IsNullOrEmpty(options.Subscription))
        {
            // Trim any surrounding quotes that may have been included in the input
            options.Subscription = options.Subscription.Trim('"', '\'');
        }
    }
}
