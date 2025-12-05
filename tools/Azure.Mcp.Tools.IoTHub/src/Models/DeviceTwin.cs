// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

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
    [property: JsonPropertyName("tags")] object? Tags);

public record TwinProperties(
    [property: JsonPropertyName("desired")] object? Desired,
    [property: JsonPropertyName("reported")] object? Reported);

public record DeviceCapabilities(
    [property: JsonPropertyName("iotEdge")] bool? IotEdge);

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
    [property: JsonPropertyName("authenticationType")] string? AuthenticationType,
    [property: JsonPropertyName("capabilities")] DeviceCapabilities? Capabilities);

public record TwinPatch(
    [property: JsonPropertyName("tags")] object? Tags,
    [property: JsonPropertyName("properties")] TwinPatchProperties? Properties);

public record TwinPatchProperties(
    [property: JsonPropertyName("desired")] object? Desired);
