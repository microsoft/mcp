// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Sql.Options.Server;

/// <summary>
/// Options for the SQL server delete command.
/// </summary>
public sealed class ServerDeleteOptions : BaseSqlOptions
{
    [Option(Description = SqlOptionDescriptions.Server)]
    public required string Server { get; set; }
}
