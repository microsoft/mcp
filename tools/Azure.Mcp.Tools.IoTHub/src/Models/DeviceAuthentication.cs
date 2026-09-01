// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.IoTHub.Models;

// Only the authentication type is surfaced; symmetric keys and x509 thumbprints
// from the device identity response are intentionally not mapped so they are not returned.
public record DeviceAuthentication(
    [property: JsonPropertyName("type")] string? Type);
