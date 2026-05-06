// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Tools.Sql.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Extensions;
using Microsoft.Mcp.Core.Models.Option;

namespace Azure.Mcp.Tools.Sql.Commands;

public abstract class BaseSqlCommand<
    [DynamicallyAccessedMembers(TrimAnnotations.CommandAnnotations)] TOptions, TResult>(ILogger<BaseSqlCommand<TOptions, TResult>> logger)
    : SubscriptionCommand<TOptions, TResult> where TOptions : BaseSqlOptions, new()
{
    protected readonly ILogger<BaseSqlCommand<TOptions, TResult>> _logger = logger;

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(OptionDefinitions.Common.ResourceGroup.AsRequired());
        command.Options.Add(SqlOptionDefinitions.Server);
    }

    protected override TOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.ResourceGroup ??= parseResult.GetValueOrDefault<string>(OptionDefinitions.Common.ResourceGroup.Name);
        options.Server = parseResult.GetValueOrDefault<string>(SqlOptionDefinitions.Server.Name);
        return options;
    }
}
