// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization.Metadata;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Helpers;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Models.Option;
using Microsoft.Mcp.Core.Options;

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
            // Command-level validation for presence: allow either --subscription,
            // Azure CLI profile default, or AZURE_SUBSCRIPTION_ID env var.
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

/// <summary>
/// Two-generic variant of <see cref="SubscriptionCommand{TOptions}"/> that additionally declares the
/// strongly-typed success-payload type via <typeparamref name="TResult"/>. Mirrors
/// <see cref="BaseCommand{TOptions, TResult}"/>.
/// </summary>
public abstract class SubscriptionCommand<
    [DynamicallyAccessedMembers(TrimAnnotations.CommandAnnotations)] TOptions,
    TResult> : SubscriptionCommand<TOptions>, IBaseCommand where TOptions : SubscriptionOptions, new()
{
    /// <inheritdoc cref="BaseCommand{TOptions, TResult}.ResultTypeInfo"/>
    protected abstract JsonTypeInfo<TResult> ResultTypeInfo { get; }

    JsonTypeInfo IBaseCommand.ResultTypeInfo => ResultTypeInfo;

    /// <inheritdoc cref="BaseCommand{TOptions, TResult}.SetResult"/>
    protected void SetResult(CommandContext context, TResult value)
    {
        ArgumentNullException.ThrowIfNull(context);
        context.Response.Results = ResponseResult.Create(value, ResultTypeInfo);
    }
}
