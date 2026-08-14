// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Sql.Options.Server;

/// <summary>
/// Options for the SQL server delete command.
/// </summary>
public sealed class ServerDeleteOptions : BaseSqlOptions
{
    /// <summary>
    /// Gets or sets whether to force delete the server without confirmation.
    /// </summary>
    [Option(Description = "Force delete the server without confirmation prompts.")]
    public bool Force { get; set; }

    [Option(Description = SqlOptionDescriptions.Server)]
    public required string Server { get; set; }
}
