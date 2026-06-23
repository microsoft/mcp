// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.AppService.Options.Database;

public sealed class DatabaseAddOptions : ISubscriptionOption
{
    [Option(Description = AppServiceOptionDefinitions.App)]
    public required string App { get; set; }

    [Option(Description = "The type of database (e.g., SqlServer, MySQL, PostgreSQL, CosmosDB).")]
    public required string DatabaseType { get; set; }

    [Option(Description = "The server name or endpoint for the database (e.g., myserver.database.windows.net).")]
    public required string DatabaseServer { get; set; }

    [Option(Description = "The name of the database to connect to (e.g., mydb).")]
    public required string Database { get; set; }

    [Option(Description = "The connection string for the database. If not provided, a default will be generated.")]
    public string? ConnectionString { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }

    [Option(Description = OptionDescriptions.Subscription)]
    public string? Subscription { get; set; }

    [Option(Description = OptionDescriptions.ResourceGroup)]
    public required string ResourceGroup { get; set; }

    [OptionContainer(Prefix = "retry")]
    public RetryPolicyOptions? RetryPolicy { get; set; }
}
