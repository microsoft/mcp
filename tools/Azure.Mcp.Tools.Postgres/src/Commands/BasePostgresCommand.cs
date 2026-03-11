// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Core.Models.Option;
using Azure.Mcp.Tools.Postgres.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Models.Option;

namespace Azure.Mcp.Tools.Postgres.Commands;

public abstract class BasePostgresCommand<
    [DynamicallyAccessedMembers(TrimAnnotations.CommandAnnotations)] TOptions>(ILogger<BasePostgresCommand<TOptions>> logger)
    : SubscriptionCommand<TOptions> where TOptions : BasePostgresOptions, new()
{
    protected readonly ILogger<BasePostgresCommand<TOptions>> _logger = logger;

    /// <summary>
    /// Controls whether --resource-group is registered as a required option.
    /// Override to false in commands where the resource group is optional (e.g. list commands
    /// that can operate at the subscription scope).
    /// </summary>
    protected virtual bool ResourceGroupRequired => true;

    /// <summary>
    /// Controls whether --user is registered as a required option.
    /// Override to false in commands where the user is optional (e.g. list commands
    /// that do not need direct database access).
    /// </summary>
    protected virtual bool UserRequired => true;

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(ResourceGroupRequired
            ? OptionDefinitions.Common.ResourceGroup.AsRequired()
            : OptionDefinitions.Common.ResourceGroup.AsOptional());
        command.Options.Add(UserRequired
            ? PostgresOptionDefinitions.User
            : PostgresOptionDefinitions.User.AsOptional());
    }

    protected override TOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.ResourceGroup ??= parseResult.GetValueOrDefault<string>(OptionDefinitions.Common.ResourceGroup.Name);
        options.User = parseResult.GetValueOrDefault<string>(PostgresOptionDefinitions.User.Name);
        return options;
    }
}
