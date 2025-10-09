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
        command.Options.Add(TablesOptionDefinitions.CosmosDbAccount);
        command.Validators.Add(commandResult =>
        {
            var storageAccount = commandResult.GetValueOrDefault<string>(TablesOptionDefinitions.Account.Name);
            var cosmosDbAccount = commandResult.GetValueOrDefault<string>(TablesOptionDefinitions.CosmosDbAccount.Name);
            if (string.IsNullOrWhiteSpace(storageAccount) && string.IsNullOrWhiteSpace(cosmosDbAccount))
            {
                commandResult.AddError($"One of --{TablesOptionDefinitions.StorageAccountName} or --{TablesOptionDefinitions.CosmosDbAccountName} must be provided.");
            } else if (!string.IsNullOrWhiteSpace(storageAccount) && !string.IsNullOrWhiteSpace(cosmosDbAccount))
            {
                commandResult.AddError($"Only one of --{TablesOptionDefinitions.StorageAccountName} or --{TablesOptionDefinitions.CosmosDbAccountName} can be provided.");
            }
        });
    }

    protected override T BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.StorageAccount = parseResult.GetValueOrDefault<string>(TablesOptionDefinitions.Account.Name);
        options.CosmosDbAccount = parseResult.GetValueOrDefault<string>(TablesOptionDefinitions.CosmosDbAccount.Name);
        return options;
    }
}
