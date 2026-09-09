// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.EventGrid.Models;

public enum EventSchema
{
    [JsonStringEnumMemberName("CloudEvents")]
    CloudEvents,

    [JsonStringEnumMemberName("EventGrid")]
    EventGrid,

    [JsonStringEnumMemberName("Custom")]
    Custom
}
