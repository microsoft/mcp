// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.IoTHub.Models;

public record IoTHubRegistryStatistics(
    [property: JsonPropertyName("disabledDeviceCount")] long DisabledDeviceCount,
    [property: JsonPropertyName("enabledDeviceCount")] long EnabledDeviceCount,
    [property: JsonPropertyName("totalDeviceCount")] long TotalDeviceCount);
