// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Tools.MySql.Options;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.MySql.Commands.Server;

public abstract class BaseServerCommand<
    [DynamicallyAccessedMembers(TrimAnnotations.CommandAnnotations)] TOptions>(ILogger<BaseMySqlCommand<TOptions>> logger)
    : BaseMySqlCommand<TOptions>(logger) where TOptions : MySqlServerOptions, new()
{
    private readonly Option<string> _serverOption = MySqlOptionDefinitions.Server;

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(_serverOption);
    }

    protected override TOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Server = parseResult.GetValue(_serverOption);
        return options;
    }
}
