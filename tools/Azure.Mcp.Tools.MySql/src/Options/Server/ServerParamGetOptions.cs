// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.MySql.Options.Server;

public sealed class ServerParamGetOptions : MySqlServerOptions
{
    [Option(Description = MySqlOptionDescriptions.Param)]
    public required string Param { get; set; }
}
