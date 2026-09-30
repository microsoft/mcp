// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.Advisor.Options.Recommendation;
using Azure.Mcp.Tools.Advisor.Validation;
using Microsoft.Mcp.Core.Commands;

namespace Azure.Mcp.Tools.Advisor.Commands.Recommendation;

public abstract class RecommendationScopeCommand<
    [DynamicallyAccessedMembers(TrimAnnotations.CommandAnnotations)] TOptions,
    TResult>(ISubscriptionResolver subscriptionResolver)
    : AuthenticatedCommand<TOptions, TResult>
    where TOptions : class, IRecommendationScopeOptions
{
    private readonly ISubscriptionResolver _subscriptionResolver = subscriptionResolver;

    public override void PostBindOptions(TOptions options)
    {
        base.PostBindOptions(options);
        RecommendationScopeValidator.PostBindOptions(options, _subscriptionResolver);
    }

    public override void ValidateOptions(TOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);
        RecommendationScopeValidator.ValidateOptions(options, validationResult);
    }
}
