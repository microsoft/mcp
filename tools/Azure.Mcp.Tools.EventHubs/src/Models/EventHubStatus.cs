// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.EventHubs.Models;

public enum EventHubStatus
{
    [JsonStringEnumMemberName("Active")]
    Active,

    [JsonStringEnumMemberName("Disabled")]
    Disabled,

    [JsonStringEnumMemberName("Restoring")]
    Restoring,

    [JsonStringEnumMemberName("SendDisabled")]
    SendDisabled,

    [JsonStringEnumMemberName("ReceiveDisabled")]
    ReceiveDisabled,

    [JsonStringEnumMemberName("Creating")]
    Creating,

    [JsonStringEnumMemberName("Deleting")]
    Deleting,

    [JsonStringEnumMemberName("Renaming")]
    Renaming,

    [JsonStringEnumMemberName("Unknown")]
    Unknown
}
