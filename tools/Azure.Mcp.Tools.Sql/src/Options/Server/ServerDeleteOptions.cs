// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Sql.Options.Server;

/// <summary>
/// Options for the SQL server delete command.
/// </summary>
public class ServerDeleteOptions : BaseSqlOptions
{
    /// <summary>
    /// Gets or sets whether to force delete the server without confirmation.
    /// </summary>
    public bool Force { get; set; }
}
