// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Tools.Storage.Commands.Account;
using Azure.Mcp.Tools.Storage.Commands.Blob;
using Azure.Mcp.Tools.Storage.Commands.Blob.Batch;
using Azure.Mcp.Tools.Storage.Commands.Blob.Container;
using Azure.Mcp.Tools.Storage.Commands.DataLake.Directory;
using Azure.Mcp.Tools.Storage.Commands.DataLake.FileSystem;
using Azure.Mcp.Tools.Storage.Commands.Queue.Message;
using Azure.Mcp.Tools.Storage.Commands.Share.File;
using Azure.Mcp.Tools.Storage.Commands.Table;

namespace Azure.Mcp.Tools.Storage.Commands;

[JsonSerializable(typeof(BlobListCommand.BlobListCommandResult))]
[JsonSerializable(typeof(BlobDetailsCommand.BlobDetailsCommandResult))]
[JsonSerializable(typeof(BatchSetTierCommand.BatchSetTierCommandResult))]
[JsonSerializable(typeof(AccountListCommand.AccountListCommandResult), TypeInfoPropertyName = "AccountListCommandResult")]
[JsonSerializable(typeof(AccountDetailsCommand.AccountDetailsCommandResult), TypeInfoPropertyName = "AccountDetailsCommandResult")]
[JsonSerializable(typeof(AccountCreateCommand.AccountCreateCommandResult), TypeInfoPropertyName = "AccountCreateCommandResult")]
[JsonSerializable(typeof(TableListCommand.TableListCommandResult))]
[JsonSerializable(typeof(ContainerListCommand.ContainerListCommandResult))]
[JsonSerializable(typeof(ContainerDetailsCommand.ContainerDetailsCommandResult))]
[JsonSerializable(typeof(ContainerCreateCommand.ContainerCreateCommandResult))]
[JsonSerializable(typeof(FileSystemListPathsCommand.FileSystemListPathsCommandResult))]
[JsonSerializable(typeof(DirectoryCreateCommand.DirectoryCreateCommandResult))]
[JsonSerializable(typeof(FileListCommand.FileListCommandResult))]
[JsonSerializable(typeof(QueueMessageSendCommand.QueueMessageSendCommandResult))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
internal sealed partial class StorageJsonContext : JsonSerializerContext
{
}
