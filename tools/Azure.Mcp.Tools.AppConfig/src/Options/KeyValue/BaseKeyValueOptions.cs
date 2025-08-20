// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.AppConfig.Options.KeyValue;

public class BaseKeyValueOptions : BaseAppConfigOptions
{
    [JsonPropertyName(AppConfigOptionDefinitions.KeyName)]
    public string? Key { get; set; }

    [JsonPropertyName(AppConfigOptionDefinitions.LabelName)]
    public string? Label { get; set; }

    [JsonPropertyName(AppConfigOptionDefinitions.ContentTypeName)]
    public string? ContentType { get; set; }
}
