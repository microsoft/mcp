// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Tools.Tables.Options;

namespace Azure.Mcp.Tools.Tables.Commands;

public abstract class BaseTablesCommand<[DynamicallyAccessedMembers(TrimAnnotations.CommandAnnotations)] T> : SubscriptionCommand<T>
    where T : BaseTablesOptions, new()
{
    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(TablesOptionDefinitions.Account);
    }

    protected override T BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Account = parseResult.GetValueOrDefault<string>(TablesOptionDefinitions.Account.Name);
        return options;
    }
}
