// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Core.Areas.Group.Commands;

[JsonSerializable(typeof(GroupListCommand.Result))]
[JsonSerializable(typeof(ResourceListCommand.ResourceListCommandResult))]
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
)]
internal partial class GroupJsonContext : JsonSerializerContext;
