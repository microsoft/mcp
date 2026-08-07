// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.IoTHub.Models;

public record TwinPatch(
    [property: JsonPropertyName("tags")] object? Tags,
    [property: JsonPropertyName("properties")] TwinPatchProperties? Properties);
