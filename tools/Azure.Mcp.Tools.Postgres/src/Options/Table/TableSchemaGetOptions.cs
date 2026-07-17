// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Postgres.Options.Table;

public sealed class TableSchemaGetOptions
{
    [Option(Description = "The PostgreSQL table to be accessed.")]
    public required string Table { get; set; }

    [Option(Description = PostgresOptionDefinitions.AuthTypeDescription)]
    public required string AuthType { get; set; }

    [Option(Description = PostgresOptionDefinitions.UserDescription)]
    public required string User { get; set; }

    [Option(Description = PostgresOptionDefinitions.PasswordDescription)]
    public string? Password { get; set; }

    [Option(Description = PostgresOptionDefinitions.ServerDescription)]
    public required string Server { get; set; }

    [Option(Description = PostgresOptionDefinitions.DatabaseDescription)]
    public required string Database { get; set; }
}
