// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.AppService.Models;

public enum AppSettingUpdateType
{
    [JsonStringEnumMemberName("add")]
    Add,

    [JsonStringEnumMemberName("set")]
    Set,

    [JsonStringEnumMemberName("delete")]
    Delete,
}