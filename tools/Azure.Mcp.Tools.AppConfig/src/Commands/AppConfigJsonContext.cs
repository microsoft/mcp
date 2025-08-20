// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Tools.AppConfig.Commands.Account;
using Azure.Mcp.Tools.AppConfig.Commands.KeyValue;

namespace Azure.Mcp.Tools.AppConfig.Commands;

[JsonSerializable(typeof(KeyValueUnlockCommand.KeyValueUnlockResult))]
[JsonSerializable(typeof(KeyValueShowCommand.KeyValueShowResult))]
[JsonSerializable(typeof(KeyValueSetCommand.KeyValueSetCommandResult))]
[JsonSerializable(typeof(KeyValueLockCommand.KeyValueLockCommandResult))]
[JsonSerializable(typeof(KeyValueListCommand.KeyValueListCommandResult))]
[JsonSerializable(typeof(KeyValueDeleteCommand.KeyValueDeleteCommandResult))]
[JsonSerializable(typeof(AccountListCommand.AccountListCommandResult))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
internal sealed partial class AppConfigJsonContext : JsonSerializerContext
{
}
