// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.IoTHub.Models;

public record DeviceCapabilities(
    [property: JsonPropertyName("iotEdge")] bool? IotEdge);
