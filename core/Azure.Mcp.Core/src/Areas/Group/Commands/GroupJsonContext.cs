// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Core.Areas.Group.Commands;

[JsonSerializable(typeof(GroupListCommand.Result), TypeInfoPropertyName = "GroupListCommandResult")]
[JsonSerializable(typeof(GroupResourceListCommand.Result), TypeInfoPropertyName = "GroupResourceListCommandResult")]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
internal partial class GroupJsonContext : JsonSerializerContext
{

}
