// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Sql.Options.EntraAdmin;

/// <summary>
/// Options for the SQL Server Entra ID Admin List command.
/// </summary>
public sealed class EntraAdminListOptions : BaseSqlOptions
{
    [Option(Description = SqlOptionDescriptions.Server)]
    public required string Server { get; set; }
}
