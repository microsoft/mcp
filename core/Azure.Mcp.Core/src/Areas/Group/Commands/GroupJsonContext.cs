// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Core.Areas.Group.Commands;

[JsonSerializable(typeof(GroupListCommand.Result))]
[JsonSerializable(typeof(GroupResourceListCommand.GroupResourceListCommandResult))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
internal partial class GroupJsonContext : JsonSerializerContext
{

}
