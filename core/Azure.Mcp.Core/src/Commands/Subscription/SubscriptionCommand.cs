// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

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
        command.Validators.Add(commandResult =>
        {
            // Command-level validation for presence: allow either --subscription or AZURE_SUBSCRIPTION_ID
            // This mirrors the prior behavior that preferred the explicit option but fell back to env var.
            if (!CommandHelper.HasSubscriptionAvailable(commandResult))
            {
                commandResult.AddError("Missing Required options: --subscription");
            }
        });
    }

    protected override TOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Subscription = CommandHelper.GetSubscription(parseResult);
        if (!string.IsNullOrEmpty(options.Subscription))
        {
            // Trim any surrounding quotes that may have been included in the input
            options.Subscription = options.Subscription.Trim('"', '\'');
        }

        return options;
    }
}
