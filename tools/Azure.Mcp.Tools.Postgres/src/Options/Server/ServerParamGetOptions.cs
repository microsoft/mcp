// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Postgres.Options.Server;

public class ServerParamGetOptions : BasePostgresOptions
{
    [JsonPropertyName(PostgresOptionDefinitions.ParamName)]
    public string? Param { get; set; }
}
