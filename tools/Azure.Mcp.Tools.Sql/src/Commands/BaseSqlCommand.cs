// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Tools.Sql.Options;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.Sql.Commands;

public abstract class BaseSqlCommand<
    [DynamicallyAccessedMembers(TrimAnnotations.CommandAnnotations)] TOptions>(ILogger<BaseSqlCommand<TOptions>> logger)
    : SubscriptionCommand<TOptions> where TOptions : BaseSqlOptions, new()
{
    protected readonly Option<string> _serverOption = SqlOptionDefinitions.Server;
    protected readonly ILogger<BaseSqlCommand<TOptions>> _logger = logger;

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        RequireResourceGroup();
        command.AddOption(_serverOption);
    }

    protected override TOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Server = parseResult.GetValueForOption(_serverOption);
        return options;
    }
}
