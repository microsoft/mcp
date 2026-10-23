// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Sql.Options.Server;

public sealed class ServerGetOptions : BaseSqlOptions
{
    [Option(Description = SqlOptionDescriptions.Server)]
    public string? Server { get; set; }
}
