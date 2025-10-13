// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Tools.Authorization.Services.Models;

namespace Azure.Mcp.Tools.Authorization.Commands;

[JsonSerializable(typeof(RoleAssignmentListCommand.RoleAssignmentListCommandResult))]
[JsonSerializable(typeof(RoleAssignmentData))]
[JsonSerializable(typeof(RoleAssignmentProperties))]
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    WriteIndented = true,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
internal partial class AuthorizationJsonContext : JsonSerializerContext;
