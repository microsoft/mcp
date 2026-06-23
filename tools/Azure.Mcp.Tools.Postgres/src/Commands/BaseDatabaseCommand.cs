// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;
using Azure.Mcp.Tools.Postgres.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Extensions;

namespace Azure.Mcp.Tools.Postgres.Commands;

// Data-plane commands connect directly to PostgreSQL via Npgsql and therefore do not
// require ARM-scoping options (--subscription / --resource-group). They derive from
// GlobalCommand rather than the subscription-based hierarchy so those options are not
// part of the MCP input schema.
public abstract class BaseDatabaseCommand<
    [DynamicallyAccessedMembers(TrimAnnotations.CommandAnnotations)] TOptions>(ILogger<BaseDatabaseCommand<TOptions>> logger)
    : GlobalCommand<TOptions> where TOptions : BasePostgresOptions, new()
{
    protected readonly ILogger<BaseDatabaseCommand<TOptions>> _logger = logger;

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(PostgresOptionDefinitions.Server);
        command.Options.Add(PostgresOptionDefinitions.User);
        command.Options.Add(PostgresOptionDefinitions.Database);
        command.Options.Add(PostgresOptionDefinitions.AuthType);
        command.Options.Add(PostgresOptionDefinitions.Password);
    }

    protected override TOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Server = parseResult.GetValueOrDefault<string>(PostgresOptionDefinitions.Server.Name);
        options.User = parseResult.GetValueOrDefault<string>(PostgresOptionDefinitions.User.Name);
        options.Database = parseResult.GetValueOrDefault<string>(PostgresOptionDefinitions.Database.Name);
        options.AuthType = parseResult.GetValueOrDefault<string>(PostgresOptionDefinitions.AuthType.Name);
        options.Password = parseResult.GetValueOrDefault<string>(PostgresOptionDefinitions.Password.Name);
        return options;
    }
}
