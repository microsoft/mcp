// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.AppConfig.Options.KeyValue;

public class KeyValueGetOptions : BaseAppConfigOptions
{
    [JsonPropertyName(AppConfigOptionDefinitions.KeyName)]
    public string? Key { get; set; }

    [JsonPropertyName(AppConfigOptionDefinitions.LabelName)]
    public string? Label { get; set; }

    [JsonPropertyName(AppConfigOptionDefinitions.KeyFilterName)]
    public string? KeyFilter { get; set; }

    [JsonPropertyName(AppConfigOptionDefinitions.LabelFilterName)]
    public string? LabelFilter { get; set; }
}
