// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;

namespace Fabric.Mcp.Tools.Core.Models;

public sealed record ItemScheduleCreateRequest(bool Enabled, JsonElement Configuration, JsonElement? ExecutionData);
