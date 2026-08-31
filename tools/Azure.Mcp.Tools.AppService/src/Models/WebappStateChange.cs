// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.AppService.Models;

public enum WebappStateChange
{
    [JsonStringEnumMemberName("start")]
    Start,

    [JsonStringEnumMemberName("stop")]
    Stop,

    [JsonStringEnumMemberName("restart")]
    Restart,
}