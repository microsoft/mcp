// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Sql.Options.Server;

public sealed class ServerCreateOptions : BaseSqlOptions
{
    [Option(Description = "The administrator login name for the SQL server.")]
    public required string AdministratorLogin { get; set; }

    [Option(Description = "The administrator password for the SQL server.")]
    public required string AdministratorPassword { get; set; }

    [Option(Description = "The Azure region location where the SQL server will be created.")]
    public required string Location { get; set; }

    [Option(Description = "The version of SQL Server to create (e.g., '12.0').")]
    public string? Version { get; set; }

    [Option(Description = "Whether public network access is enabled for the SQL server ('Enabled' or 'Disabled'). Defaults to 'Disabled'.")]
    public string? PublicNetworkAccess { get; set; }

    [Option(Description = SqlOptionDescriptions.Server)]
    public required string Server { get; set; }
}
