// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.AzureTerraform.Models;

internal sealed record TerraformSampleEntry(
    [property: JsonPropertyName("ResourceType")] string ResourceType,
    [property: JsonPropertyName("Path")] string Path,
    [property: JsonPropertyName("Description")] string Description);
