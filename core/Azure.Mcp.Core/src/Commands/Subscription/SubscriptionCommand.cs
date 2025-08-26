// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.CommandLine.Parsing;
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

        // Command-level validation for presence: allow either --subscription or AZURE_SUBSCRIPTION_ID
        // This mirrors the prior behavior that preferred the explicit option but fell back to env var.
        command.Validators.Add(commandResult =>
        {
            // Look for an explicit option result among the command's children
            var optionResult = commandResult.Children.OfType<OptionResult>().FirstOrDefault(r => r.Option == _subscriptionOption);
            var subscriptionValue = optionResult != null && optionResult.Tokens.Count > 0
                ? optionResult.Tokens[0].Value
                : null;

            var envSubscription = Environment.GetEnvironmentVariable("AZURE_SUBSCRIPTION_ID");

            var hasValidSubscription = !string.IsNullOrEmpty(subscriptionValue);
            var hasValidEnvVar = !string.IsNullOrEmpty(envSubscription);

            if (!hasValidSubscription && !hasValidEnvVar)
            {
                commandResult.AddError("Missing Required options: --subscription");
            }
        });
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
