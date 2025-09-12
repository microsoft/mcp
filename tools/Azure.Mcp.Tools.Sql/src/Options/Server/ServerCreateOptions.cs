// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Sql.Options.Server;

public class ServerCreateOptions : BaseSqlOptions
{
    [JsonPropertyName(SqlOptionDefinitions.AdministratorLogin)]
    public string? AdministratorLogin { get; set; }

    [JsonPropertyName(SqlOptionDefinitions.AdministratorPassword)]
    public string? AdministratorPassword { get; set; }

    [JsonPropertyName(SqlOptionDefinitions.Location)]
    public string? Location { get; set; }

    [JsonPropertyName(SqlOptionDefinitions.Version)]
    public string? Version { get; set; }

    [JsonPropertyName(SqlOptionDefinitions.PublicNetworkAccess)]
    public string? PublicNetworkAccess { get; set; }
}
