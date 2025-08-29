// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Tools.MySql.Options;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.MySql.Commands;

public abstract class BaseMySqlCommand<
    [DynamicallyAccessedMembers(TrimAnnotations.CommandAnnotations)] TOptions>(ILogger<BaseMySqlCommand<TOptions>> logger)
    : SubscriptionCommand<TOptions> where TOptions : BaseMySqlOptions, new()
{
    protected readonly Option<string> _userOption = MySqlOptionDefinitions.User;

    protected readonly ILogger<BaseMySqlCommand<TOptions>> _logger = logger;

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        RequireResourceGroup();
        command.Options.Add(_userOption);
    }

    protected override TOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.User = parseResult.GetValue(_userOption);
        return options;
    }
}
