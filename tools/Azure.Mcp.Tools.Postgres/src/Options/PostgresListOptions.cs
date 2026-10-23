// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Postgres.Options;

public sealed class PostgresListOptions : ISubscriptionOption
{
    [Option(Description = PostgresOptionDefinitions.AuthTypeDescription)]
    public string? AuthType { get; set; }

    [Option(Description = PostgresOptionDefinitions.UserDescription)]
    public string? User { get; set; }

    [Option(Description = PostgresOptionDefinitions.PasswordDescription)]
    public string? Password { get; set; }

    [Option(Description = "The PostgreSQL server to list databases from.")]
    public string? Server { get; set; }

    [Option(Description = "The PostgreSQL database to list tables from (requires --server).")]
    public string? Database { get; set; }

    [Option(Description = "The PostgreSQL schema to list tables from when listing tables (defaults to 'public').", DefaultValue = "public")]
    public string? Schema { get; set; }

    [Option(Description = OptionDescriptions.ResourceGroup)]
    public string? ResourceGroup { get; set; }

    [Option(Description = OptionDescriptions.Subscription)]
    public string? Subscription { get; set; }
}
