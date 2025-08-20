// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Authorization.Commands;

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(RoleAssignmentListCommand.RoleAssignmentListCommandResult))]
internal partial class AuthorizationJsonContext : JsonSerializerContext;
