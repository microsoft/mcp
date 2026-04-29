// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Policy.Commands.Assignment;
using Azure.Mcp.Tools.Policy.Models;

namespace Azure.Mcp.Tools.Policy.Commands;

[JsonSerializable(typeof(PolicyAssignmentListCommand.PolicyAssignmentListCommandResult))]
[JsonSerializable(typeof(PolicyAssignment))]
[JsonSerializable(typeof(List<PolicyAssignment>))]
[JsonSerializable(typeof(PolicyDefinition))]
[JsonSerializable(typeof(ManagedIdentityInfo))]
[JsonSerializable(typeof(UserAssignedIdentityDetails))]
[JsonSerializable(typeof(Dictionary<string, UserAssignedIdentityDetails>))]
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
)]
public partial class PolicyJsonContext : JsonSerializerContext;
