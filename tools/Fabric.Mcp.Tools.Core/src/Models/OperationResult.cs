// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace Fabric.Mcp.Tools.Core.Models;

// Undefined is omitted for an empty body; JSON null remains an explicit value.
public sealed record OperationResult(
    Guid? OperationId,
    bool HasBody,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)] JsonElement Value);
