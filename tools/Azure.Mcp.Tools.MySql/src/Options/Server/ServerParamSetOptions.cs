// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.MySql.Options.Server;

public sealed class ServerParamSetOptions : MySqlServerOptions
{
    [Option(Description = MySqlOptionDescriptions.Param)]
    public required string Param { get; set; }

    [Option(Description = "The value to set for the MySQL parameter.")]
    public required string Value { get; set; }
}
