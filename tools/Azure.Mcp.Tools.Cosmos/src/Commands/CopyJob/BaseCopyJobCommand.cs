// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Tools.Cosmos.Options;

namespace Azure.Mcp.Tools.Cosmos.Commands.CopyJob;

/// <summary>
/// Base command for copy job operations. Extends SubscriptionCommand and adds
/// --account and --job-name options.
/// </summary>
public abstract class BaseCopyJobCommand<
    [DynamicallyAccessedMembers(TrimAnnotations.CommandAnnotations)] TOptions>
    : SubscriptionCommand<TOptions> where TOptions : CopyJobOptions, new()
{
    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(CosmosOptionDefinitions.Account);
        command.Options.Add(CosmosOptionDefinitions.JobName);
    }

    protected override TOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Account = parseResult.GetValueOrDefault<string>(CosmosOptionDefinitions.Account.Name);
        options.JobName = parseResult.GetValueOrDefault<string>(CosmosOptionDefinitions.JobName.Name);
        return options;
    }
}
