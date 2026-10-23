// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Postgres.Options.Server;

public sealed class ServerParamSetOptions : BaseServerOptions
{
    [Option(Description = PostgresOptionDefinitions.ParamDescription)]
    public required string Param { get; set; }

    [Option(Description = "The value to set for the PostgreSQL parameter.")]
    public required string Value { get; set; }
}
