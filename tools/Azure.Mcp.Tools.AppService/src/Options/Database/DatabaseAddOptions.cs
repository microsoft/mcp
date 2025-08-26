// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.AppService.Options.Database;

public class DatabaseAddOptions : BaseAppServiceOptions
{
    [JsonPropertyName(AppServiceOptionDefinitions.DatabaseType)]
    public string? DatabaseType { get; set; }

    [JsonPropertyName(AppServiceOptionDefinitions.DatabaseServer)]
    public string? DatabaseServer { get; set; }

    [JsonPropertyName(AppServiceOptionDefinitions.DatabaseName)]
    public string? DatabaseName { get; set; }

    [JsonPropertyName(AppServiceOptionDefinitions.ConnectionString)]
    public string? ConnectionString { get; set; }
}
