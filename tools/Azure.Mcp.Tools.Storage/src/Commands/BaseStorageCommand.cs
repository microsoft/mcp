// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Tools.Storage.Options;

namespace Azure.Mcp.Tools.Storage.Commands;

public abstract class BaseStorageCommand<
    [DynamicallyAccessedMembers(TrimAnnotations.CommandAnnotations)] T>
    : SubscriptionCommand<T>
    where T : BaseStorageOptions, new()
{
    protected readonly Option<string> _accountOption = StorageOptionDefinitions.Account;

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(_accountOption);
    }

    protected override T BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Account = parseResult.GetValueOrDefault(_accountOption);
        return options;
    }
}
