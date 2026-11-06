// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Tools.Authorization.Services.Models;

namespace Azure.Mcp.Tools.Authorization.Commands;

[JsonSerializable(typeof(RoleAssignmentListCommand.RoleAssignmentListCommandResult))]
[JsonSerializable(typeof(RoleAssignmentApprovalListCommand.RoleAssignmentApprovalListCommandResult))]
[JsonSerializable(typeof(RoleAssignmentApprovalApproveCommand.RoleAssignmentApprovalApproveCommandResult))]
[JsonSerializable(typeof(RoleAssignmentData))]
[JsonSerializable(typeof(RoleAssignmentProperties))]
[JsonSerializable(typeof(Models.RoleAssignmentApproval))]
[JsonSerializable(typeof(Models.RoleAssignmentApprovalStage))]
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
internal partial class AuthorizationJsonContext : JsonSerializerContext;
