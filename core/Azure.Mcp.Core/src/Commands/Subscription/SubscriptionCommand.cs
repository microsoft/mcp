// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine.Parsing;
using System.Diagnostics.CodeAnalysis;
using Azure.Mcp.Core.Helpers;
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
            if (!HasSubscriptionAvailable(commandResult))
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

        var envSubscription = EnvironmentHelpers.GetAzureSubscriptionId();
        options.Subscription = (string.IsNullOrEmpty(subscriptionValue)
            || IsPlaceholder(subscriptionValue))
            && !string.IsNullOrEmpty(envSubscription)
            ? envSubscription
            : subscriptionValue;

        return options;
    }

    /// <summary>
    /// Checks if a subscription is available either from the command option or AZURE_SUBSCRIPTION_ID environment variable.
    /// </summary>
    /// <param name="commandResult">The command result to check for the subscription option.</param>
    /// <returns>True if a subscription is available, false otherwise.</returns>
    protected static bool HasSubscriptionAvailable(CommandResult commandResult)
    {
        var hasOption = commandResult.HasOptionResult(OptionDefinitions.Common.Subscription);
        var hasEnv = !string.IsNullOrEmpty(EnvironmentHelpers.GetAzureSubscriptionId());
        return hasOption || hasEnv;
    }

    private static bool IsPlaceholder(string value)
        => value.Contains("subscription") || value.Contains("default");
}
