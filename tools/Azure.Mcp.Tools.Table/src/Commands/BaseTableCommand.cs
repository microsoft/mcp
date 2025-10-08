// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Tools.Table.Options;

namespace Azure.Mcp.Tools.Table.Commands;

public abstract class BaseTableCommand<[DynamicallyAccessedMembers(TrimAnnotations.CommandAnnotations)] T> : SubscriptionCommand<T>
    where T : BaseTableOptions, new()
{
    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(TableOptionDefinitions.Account);
    }

    protected override T BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Account = parseResult.GetValueOrDefault<string>(TableOptionDefinitions.Account.Name);
        return options;
    }
}
