// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace Fabric.Mcp.Tools.OneLake.Models;

public sealed record TableListResult(
    [property: JsonPropertyName("workspace")] string Workspace,
    [property: JsonPropertyName("item")] string Item,
    [property: JsonPropertyName("namespace")] string Namespace,
    [property: JsonPropertyName("tables")] JsonElement Tables,
    [property: JsonPropertyName("rawResponse")] string RawResponse);
