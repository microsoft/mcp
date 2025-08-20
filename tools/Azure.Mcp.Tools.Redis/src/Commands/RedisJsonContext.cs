// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Tools.Redis.Commands.CacheForRedis;
using Azure.Mcp.Tools.Redis.Commands.ManagedRedis;

namespace Azure.Mcp.Tools.Redis.Commands;

[JsonSerializable(typeof(CacheListCommand.CacheListCommandResult))]
[JsonSerializable(typeof(AccessPolicyListCommand.AccessPolicyListCommandResult))]
[JsonSerializable(typeof(ClusterListCommand.ClusterListCommandResult))]
[JsonSerializable(typeof(DatabaseListCommand.DatabaseListCommandResult))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault)]
internal sealed partial class RedisJsonContext : JsonSerializerContext;
