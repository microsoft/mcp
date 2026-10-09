// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Tools.NetAppFiles.Commands.Account;
using Azure.Mcp.Tools.NetAppFiles.Commands.Pool;
using Azure.Mcp.Tools.NetAppFiles.Models;

namespace Azure.Mcp.Tools.NetAppFiles.Commands;

[JsonSerializable(typeof(AccountCreateCommand.AccountCreateResult))]
[JsonSerializable(typeof(AccountGetCommand.AccountGetResult))]
[JsonSerializable(typeof(AccountUpdateCommand.AccountUpdateResult))]
[JsonSerializable(typeof(Dictionary<string, string>))]
[JsonSerializable(typeof(NetAppFilesAccount))]
[JsonSerializable(typeof(NetAppFilesPool))]
[JsonSerializable(typeof(PoolCreateCommand.PoolCreateResult))]
[JsonSerializable(typeof(PoolGetCommand.PoolGetResult))]
[JsonSerializable(typeof(PoolUpdateCommand.PoolUpdateResult))]
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
internal partial class NetAppFilesJsonContext : JsonSerializerContext;
