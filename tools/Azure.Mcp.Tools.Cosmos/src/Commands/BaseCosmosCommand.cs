// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Tools.Cosmos.Options;
using Microsoft.Azure.Cosmos;

namespace Azure.Mcp.Tools.Cosmos.Commands;

public abstract class BaseCosmosCommand<
    [DynamicallyAccessedMembers(TrimAnnotations.CommandAnnotations)] TOptions>
    : SubscriptionCommand<TOptions> where TOptions : BaseCosmosOptions, new()
{
    protected readonly Option<string> _accountOption = CosmosOptionDefinitions.Account;

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.AddOption(_accountOption);
    }

    protected override TOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Account = parseResult.GetValueForOption(_accountOption);
        return options;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        CosmosException cosmosEx => cosmosEx.Message,
        _ => base.GetErrorMessage(ex)
    };

    protected override int GetStatusCode(Exception ex) => ex switch
    {
        CosmosException cosmosEx => (int)cosmosEx.StatusCode,
        _ => base.GetStatusCode(ex)
    };
}
