// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Postgres.Options.Database;

public sealed class DatabaseQueryOptions
{
    [Option(Description = "Query to be executed against a PostgreSQL database.")]
    public required string Query { get; set; }

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
