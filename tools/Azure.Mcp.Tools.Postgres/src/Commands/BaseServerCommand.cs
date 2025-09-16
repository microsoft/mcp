// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Tools.Postgres.Options;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.Postgres.Commands;

public abstract class BaseServerCommand<
    [DynamicallyAccessedMembers(TrimAnnotations.CommandAnnotations)] TOptions>(ILogger<BasePostgresCommand<TOptions>> logger)
    : BasePostgresCommand<TOptions>(logger) where TOptions : BasePostgresOptions, new()

{

    public override string Name => "server";

    public override string Description =>
        "Retrieves information about a PostgreSQL server.";

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(PostgresOptionDefinitions.Server);
    }

    protected override TOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Server = parseResult.GetValueOrDefault<string>(PostgresOptionDefinitions.Server.Name);
        return options;
    }
}
