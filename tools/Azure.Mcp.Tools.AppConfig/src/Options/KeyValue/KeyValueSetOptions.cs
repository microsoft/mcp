// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.AppConfig.Options.KeyValue;

public class KeyValueSetOptions : BaseKeyValueOptions
{
    [JsonPropertyName(AppConfigOptionDefinitions.ValueName)]
    public string? Value { get; set; }

    [JsonPropertyName(AppConfigOptionDefinitions.TagsName)]
    public string[]? Tags { get; set; }
}
