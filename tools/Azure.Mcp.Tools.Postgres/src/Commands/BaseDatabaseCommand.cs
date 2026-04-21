// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;
using Azure.Mcp.Tools.Postgres.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Extensions;

namespace Azure.Mcp.Tools.Postgres.Commands;

public abstract class BaseDatabaseCommand<
    [DynamicallyAccessedMembers(TrimAnnotations.CommandAnnotations)] TOptions>(ILogger<BasePostgresCommand<TOptions>> logger)
    : BaseServerCommand<TOptions>(logger) where TOptions : BasePostgresOptions, new()
{

    public override string Name => "database";

    public override string Description =>
        "Retrieves information about a PostgreSQL database.";

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(PostgresOptionDefinitions.Database);
        command.Options.Add(PostgresOptionDefinitions.AuthType);
        command.Options.Add(PostgresOptionDefinitions.Password);
    }

    protected override TOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Database = parseResult.GetValueOrDefault<string>(PostgresOptionDefinitions.Database.Name);
        options.AuthType = parseResult.GetValueOrDefault<string>(PostgresOptionDefinitions.AuthType.Name);
        options.Password = parseResult.GetValueOrDefault<string>(PostgresOptionDefinitions.Password.Name);
        return options;
    }
}
