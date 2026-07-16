// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Sql.Options.ElasticPool;

public sealed class ElasticPoolListOptions : BaseSqlOptions
{
    [Option(Description = SqlOptionDescriptions.Server)]
    public required string Server { get; set; }
}
