// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Compute.Models;

public enum DiskOperatingSystemType
{
    [JsonStringEnumMemberName("Linux")]
    Linux,

    [JsonStringEnumMemberName("Windows")]
    Windows
}
