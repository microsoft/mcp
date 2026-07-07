// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace Fabric.Mcp.Tools.OneLake.Models;

public sealed record TableNamespaceListResult(
    [property: JsonPropertyName("workspace")] string Workspace,
    [property: JsonPropertyName("item")] string Item,
    [property: JsonPropertyName("namespaces")] JsonElement Namespaces,
    [property: JsonPropertyName("rawResponse")] string RawResponse);
