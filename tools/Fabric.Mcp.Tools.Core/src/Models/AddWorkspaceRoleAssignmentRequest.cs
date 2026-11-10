// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;

namespace Fabric.Mcp.Tools.Core.Models;

public sealed record AddWorkspaceRoleAssignmentRequest(JsonElement Principal, string Role);
