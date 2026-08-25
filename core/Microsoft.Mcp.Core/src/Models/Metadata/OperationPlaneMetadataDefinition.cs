// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Microsoft.Mcp.Core.Commands;

namespace Microsoft.Mcp.Core.Models.Metadata;

public sealed class OperationPlaneMetadataDefinition
{
    [JsonPropertyName("value")]
    public ToolOperationPlane Value { get; init; }

    [JsonPropertyName("description")]
    public string Description { get; init; } = string.Empty;
}
