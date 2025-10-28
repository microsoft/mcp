// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Core.Models.Option;
using Azure.Mcp.Tools.Postgres.Options;
using Microsoft.Extensions.Logging;

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
        command.Options.Add(PostgresOptionDefinitions.Pass.AsOptional());
    }

    protected override TOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Database = parseResult.GetValueOrDefault<string>(PostgresOptionDefinitions.Database.Name);
        options.Password = parseResult.GetValueOrDefault<string>(PostgresOptionDefinitions.Pass.Name);
        return options;
    }
}
