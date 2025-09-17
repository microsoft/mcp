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
    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(OptionDefinitions.Common.Subscription);

        // Command-level validation for presence: allow either --subscription or AZURE_SUBSCRIPTION_ID
        // This mirrors the prior behavior that preferred the explicit option but fell back to env var.
        command.Validators.Add(commandResult =>
        {
            var hasOption = commandResult.HasOptionResult(OptionDefinitions.Common.Subscription.Name);
            var hasEnv = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("AZURE_SUBSCRIPTION_ID"));
            if (!hasOption && !hasEnv)
            {
                commandResult.AddError("Missing Required options: --subscription");
            }
        });
    }

    protected override TOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);

        // Get subscription from command line option or fallback to environment variable
        var subscriptionValue = parseResult.GetValueOrDefault<string>(OptionDefinitions.Common.Subscription.Name);

        var envSubscription = Environment.GetEnvironmentVariable("AZURE_SUBSCRIPTION_ID");
        options.Subscription = (string.IsNullOrEmpty(subscriptionValue)
            || IsPlaceholder(subscriptionValue))
            && !string.IsNullOrEmpty(envSubscription)
            ? envSubscription
            : subscriptionValue;

        return options;
    }

    private static bool IsPlaceholder(string value)
        => value.Contains("subscription") || value.Contains("default");
}
