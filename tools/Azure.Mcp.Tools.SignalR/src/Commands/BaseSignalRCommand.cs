// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Core.Models.Option;
using Azure.Mcp.Tools.SignalR.Options;

namespace Azure.Mcp.Tools.SignalR.Commands;

/// <summary>
/// Base command for all Azure SignalR commands.
/// </summary>
public abstract class BaseSignalRCommand<
    [DynamicallyAccessedMembers(TrimAnnotations.CommandAnnotations)]
TOptions>
    : SubscriptionCommand<TOptions> where TOptions : BaseSignalROptions, new()
{
    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(OptionDefinitions.Common.ResourceGroup.AsOptional());
    }

    protected override TOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.ResourceGroup ??= parseResult.GetValueOrDefault<string>(OptionDefinitions.Common.ResourceGroup.Name);
        return options;
    }
}
