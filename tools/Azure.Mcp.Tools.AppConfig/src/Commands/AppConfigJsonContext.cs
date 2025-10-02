// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Tools.AppConfig.Commands.Account;
using Azure.Mcp.Tools.AppConfig.Commands.KeyValue;
using Azure.Mcp.Tools.AppConfig.Commands.KeyValue.Lock;
using Azure.Mcp.Tools.AppConfig.Services.Models;

namespace Azure.Mcp.Tools.AppConfig.Commands;

[JsonSerializable(typeof(AccountListCommand.AccountListCommandResult))]
[JsonSerializable(typeof(KeyValueDeleteCommand.KeyValueDeleteCommandResult))]
[JsonSerializable(typeof(KeyValueGetCommand.KeyValueGetCommandResult))]
[JsonSerializable(typeof(KeyValueLockSetCommand.KeyValueLockSetCommandResult))]
[JsonSerializable(typeof(KeyValueSetCommand.KeyValueSetCommandResult))]
[JsonSerializable(typeof(AppConfigurationStoreData))]
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    WriteIndented = true,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
internal sealed partial class AppConfigJsonContext : JsonSerializerContext
{
}
