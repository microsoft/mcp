// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Sql.Options.Database;

public sealed class DatabaseGetOptions : BaseSqlOptions
{
    [Option(Description = SqlOptionDescriptions.Database)]
    public string? Database { get; set; }

    [Option(Description = SqlOptionDescriptions.Server)]
    public required string Server { get; set; }
}
