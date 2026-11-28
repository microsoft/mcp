// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Sql.Options.SensitivityLabel;

/// <summary>
/// Options for the SQL Database Sensitivity Label List command.
/// </summary>
public sealed class SensitivityLabelListOptions : BaseSqlOptions
{
    [Option(Description = SqlOptionDescriptions.Database)]
    public required string Database { get; set; }

    [Option(Description = SqlOptionDescriptions.Server)]
    public required string Server { get; set; }
}
