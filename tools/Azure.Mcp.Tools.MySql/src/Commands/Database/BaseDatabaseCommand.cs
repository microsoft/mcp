// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;
using Azure.Mcp.Tools.MySql.Commands.Server;
using Azure.Mcp.Tools.MySql.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Extensions;

namespace Azure.Mcp.Tools.MySql.Commands.Database;

public abstract class BaseDatabaseCommand<
    [DynamicallyAccessedMembers(TrimAnnotations.CommandAnnotations)] TOptions>(ILogger<BaseDatabaseCommand<TOptions>> logger)
    : BaseServerCommand<TOptions>(logger) where TOptions : MySqlDatabaseOptions, new()
{
    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(MySqlOptionDefinitions.User);
        command.Options.Add(MySqlOptionDefinitions.Database);
    }

    protected override TOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.User = parseResult.GetValueOrDefault<string>(MySqlOptionDefinitions.User.Name);
        options.Database = parseResult.GetValueOrDefault<string>(MySqlOptionDefinitions.Database.Name);
        return options;
    }
}
