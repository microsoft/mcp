// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Compute.Models;

public enum VmPowerAction
{
    [JsonStringEnumMemberName("start")]
    Start,

    [JsonStringEnumMemberName("stop")]
    Stop,

    [JsonStringEnumMemberName("deallocate")]
    Deallocate,

    [JsonStringEnumMemberName("restart")]
    Restart
}
