// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.IoTHub.Models;

public record DeviceIdentity(
    [property: JsonPropertyName("deviceId")] string DeviceId,
    [property: JsonPropertyName("generationId")] string? GenerationId,
    [property: JsonPropertyName("etag")] string? Etag,
    [property: JsonPropertyName("connectionState")] string? ConnectionState,
    [property: JsonPropertyName("status")] string? Status,
    [property: JsonPropertyName("statusReason")] string? StatusReason,
    [property: JsonPropertyName("connectionStateUpdatedTime")] string? ConnectionStateUpdatedTime,
    [property: JsonPropertyName("statusUpdatedTime")] string? StatusUpdatedTime,
    [property: JsonPropertyName("lastActivityTime")] string? LastActivityTime,
    [property: JsonPropertyName("cloudToDeviceMessageCount")] int? CloudToDeviceMessageCount,
    [property: JsonPropertyName("authentication")] DeviceAuthentication? Authentication,
    [property: JsonPropertyName("capabilities")] DeviceCapabilities? Capabilities);
