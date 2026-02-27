// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.AppService.Options.Webapp.Settings;

public sealed class AppSettingsUpdateCommandOptions : BaseAppServiceOptions
{
    [JsonPropertyName(AppServiceOptionDefinitions.AppSettingNameName)]
    public string? SettingName { get; set; }

    [JsonPropertyName(AppServiceOptionDefinitions.AppSettingValueName)]
    public string? SettingValue { get; set; }

    [JsonPropertyName(AppServiceOptionDefinitions.AppSettingUpdateTypeName)]
    public string? SettingUpdateType { get; set; }
}
