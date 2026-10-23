// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.IoTHub.Models;

public record DeviceListResult(
    [property: JsonPropertyName("devices")] List<DeviceIdentity> Devices,
    [property: JsonPropertyName("truncated")] bool Truncated);
