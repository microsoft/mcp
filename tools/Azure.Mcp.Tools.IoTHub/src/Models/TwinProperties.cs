// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.IoTHub.Models;

public record TwinProperties(
    [property: JsonPropertyName("desired")] JsonElement? Desired,
    [property: JsonPropertyName("reported")] JsonElement? Reported);
