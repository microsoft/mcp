// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Tools.Storage.Commands.Account;
using Azure.Mcp.Tools.Storage.Commands.Blob;
using Azure.Mcp.Tools.Storage.Commands.Blob.Container;
using Azure.Mcp.Tools.Storage.Models;

namespace Azure.Mcp.Tools.Storage.Commands;

[JsonSerializable(typeof(AccountCreateCommand.AccountCreateCommandResult))]
[JsonSerializable(typeof(AccountGetCommand.AccountGetCommandResult))]
[JsonSerializable(typeof(AccountInfo))]
[JsonSerializable(typeof(BlobGetCommand.BlobGetCommandResult))]
[JsonSerializable(typeof(BlobInfo))]
[JsonSerializable(typeof(BlobUploadResult))]
[JsonSerializable(typeof(ContainerCreateCommand.ContainerCreateCommandResult))]
[JsonSerializable(typeof(ContainerGetCommand.ContainerGetCommandResult))]
[JsonSerializable(typeof(ContainerInfo))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
internal sealed partial class StorageJsonContext : JsonSerializerContext
{
}
