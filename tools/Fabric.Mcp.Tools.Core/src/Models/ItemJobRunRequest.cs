// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;

namespace Fabric.Mcp.Tools.Core.Models;

public sealed record ItemJobRunRequest(JsonElement? ExecutionData, JsonElement? Parameters);
