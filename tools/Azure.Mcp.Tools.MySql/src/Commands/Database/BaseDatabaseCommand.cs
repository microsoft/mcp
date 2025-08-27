// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Tools.MySql.Commands.Server;
using Azure.Mcp.Tools.MySql.Options;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.MySql.Commands.Database;

public abstract class BaseDatabaseCommand<
    [DynamicallyAccessedMembers(TrimAnnotations.CommandAnnotations)] TOptions>(ILogger<BaseMySqlCommand<TOptions>> logger)
    : BaseServerCommand<TOptions>(logger) where TOptions : MySqlDatabaseOptions, new()
{
    private readonly Option<string> _databaseOption = MySqlOptionDefinitions.Database;

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(_databaseOption);
    }

    protected override TOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Database = parseResult.GetValue(_databaseOption);
        return options;
    }
}
