// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Tools.Redis.Models.ManagedRedis;

namespace Azure.Mcp.Tools.Redis.Commands;

[JsonSerializable(typeof(ResourceListCommand.ResourceListCommandResult))]
[JsonSerializable(typeof(ResourceCreateCommand.ResourceCreateCommandResult))]
[JsonSerializable(typeof(RedisCreateParameters))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault)]
internal sealed partial class RedisJsonContext : JsonSerializerContext;
