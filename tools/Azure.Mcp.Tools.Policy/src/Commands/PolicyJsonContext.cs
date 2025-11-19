// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Tools.Policy.Commands.Assignment;
using Azure.Mcp.Tools.Policy.Commands.Definition;
using Azure.Mcp.Tools.Policy.Models;

namespace Azure.Mcp.Tools.Policy.Commands;

[JsonSerializable(typeof(PolicyAssignmentGetCommand.PolicyAssignmentGetCommandResult))]
[JsonSerializable(typeof(PolicyAssignmentListCommand.PolicyAssignmentListCommandResult))]
[JsonSerializable(typeof(PolicyDefinitionGetCommand.PolicyDefinitionGetCommandResult))]
[JsonSerializable(typeof(PolicyAssignment))]
[JsonSerializable(typeof(PolicyDefinition))]
[JsonSerializable(typeof(List<PolicyAssignment>))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase, WriteIndented = true, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
public partial class PolicyJsonContext : JsonSerializerContext;
