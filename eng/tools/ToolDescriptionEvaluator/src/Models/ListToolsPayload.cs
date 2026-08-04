// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace ToolSelection.Models;

public sealed record ListToolsPayload(
    [property: JsonPropertyName("commands")] List<Tool>? Commands);
