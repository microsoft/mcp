// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Tools.AppService.Options;
using System.CommandLine;
using System.CommandLine.Parsing;

namespace Azure.Mcp.Tools.AppService.Commands;

public abstract class BaseAppServiceCommand<
    [DynamicallyAccessedMembers(TrimAnnotations.CommandAnnotations)] TOptions>
    : SubscriptionCommand<TOptions>
    where TOptions : BaseAppServiceOptions, new()
{
    protected override void RegisterOptions(Command command)
    {
    base.RegisterOptions(command);
    command.Options.Add(_resourceGroupOption);
    }

    protected override TOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
    options.ResourceGroup = parseResult.CommandResult.ValueForOption(_resourceGroupOption);
        return options;
    }
}
