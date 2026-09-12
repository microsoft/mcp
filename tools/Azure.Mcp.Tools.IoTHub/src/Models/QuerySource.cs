// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.IoTHub.Models;

[JsonConverter(typeof(JsonStringEnumConverter<QuerySource>))]
public enum QuerySource
{
    [JsonStringEnumMemberName("devices")]
    Devices,

    [JsonStringEnumMemberName("devices.modules")]
    DeviceModules,

    [JsonStringEnumMemberName("devices.jobs")]
    DeviceJobs
}
