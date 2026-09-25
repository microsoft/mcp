// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.IoTHub.Models;

public record DeviceTwin(
    [property: JsonPropertyName("deviceId")] string DeviceId,
    [property: JsonPropertyName("etag")] string? Etag,
    [property: JsonPropertyName("deviceEtag")] string? DeviceEtag,
    [property: JsonPropertyName("status")] string? Status,
    [property: JsonPropertyName("statusUpdateTime")] string? StatusUpdateTime,
    [property: JsonPropertyName("connectionState")] string? ConnectionState,
    [property: JsonPropertyName("lastActivityTime")] string? LastActivityTime,
    [property: JsonPropertyName("cloudToDeviceMessageCount")] int? CloudToDeviceMessageCount,
    [property: JsonPropertyName("authenticationType")] string? AuthenticationType,
    [property: JsonPropertyName("version")] long? Version,
    [property: JsonPropertyName("properties")] TwinProperties? Properties,
    [property: JsonPropertyName("capabilities")] DeviceCapabilities? Capabilities,
    [property: JsonPropertyName("tags")] JsonElement? Tags);
