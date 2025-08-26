// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;
using Azure.Mcp.Core.Models.Option;
using Azure.Mcp.Core.Options;

namespace Azure.Mcp.Core.Commands.Subscription;

public abstract class SubscriptionCommand<
    [DynamicallyAccessedMembers(TrimAnnotations.CommandAnnotations)] TOptions> : GlobalCommand<TOptions>
    where TOptions : SubscriptionOptions, new()
{

    protected readonly Option<string> _subscriptionOption = OptionDefinitions.Common.Subscription;

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(_subscriptionOption);
    }

    protected override TOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);

        // Get subscription from command line option or fallback to environment variable
        var subscriptionValue = parseResult.GetValue(_subscriptionOption);
        options.Subscription = (string.IsNullOrEmpty(subscriptionValue)
            || subscriptionValue.Contains("subscription")
            || subscriptionValue.Contains("default"))
            && !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("AZURE_SUBSCRIPTION_ID"))
            ? Environment.GetEnvironmentVariable("AZURE_SUBSCRIPTION_ID")
            : subscriptionValue;

        return options;
    }
}
