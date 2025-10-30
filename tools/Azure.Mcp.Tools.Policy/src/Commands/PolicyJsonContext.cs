// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Policy.Commands.Assignment;
using Azure.Mcp.Tools.Policy.Models;

namespace Azure.Mcp.Tools.Policy.Commands;

[JsonSerializable(typeof(AssignmentGetCommand.AssignmentGetCommandResult))]
[JsonSerializable(typeof(PolicyAssignment))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase, WriteIndented = true, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
internal partial class PolicyJsonContext : JsonSerializerContext;
