// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Tools.Postgres.Options;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.Postgres.Commands;

public abstract class BasePostgresCommand<
    [DynamicallyAccessedMembers(TrimAnnotations.CommandAnnotations)] TOptions>(ILogger<BasePostgresCommand<TOptions>> logger)
    : SubscriptionCommand<TOptions> where TOptions : BasePostgresOptions, new()
{
    protected readonly Option<string> _userOption = PostgresOptionDefinitions.User;

    protected readonly ILogger<BasePostgresCommand<TOptions>> _logger = logger;

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        RequireResourceGroup();
        command.AddOption(_userOption);
    }

    protected override TOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.User = parseResult.GetValueForOption(_userOption);
        return options;
    }
}
