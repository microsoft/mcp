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
using Azure.Mcp.Tools.Storage.Models;

namespace Azure.Mcp.Tools.Storage.Commands;

[JsonSerializable(typeof(AccountCreateCommand.AccountCreateCommandResult))]
[JsonSerializable(typeof(AccountGetCommand.AccountGetCommandResult))]
[JsonSerializable(typeof(AccountInfo))]
[JsonSerializable(typeof(BatchSetTierCommand.BatchSetTierCommandResult))]
[JsonSerializable(typeof(BlobGetCommand.BlobGetCommandResult))]
[JsonSerializable(typeof(BlobInfo))]
[JsonSerializable(typeof(BlobUploadResult))]
[JsonSerializable(typeof(ContainerCreateCommand.ContainerCreateCommandResult))]
[JsonSerializable(typeof(ContainerGetCommand.ContainerGetCommandResult))]
[JsonSerializable(typeof(ContainerInfo))]
[JsonSerializable(typeof(DataLakePathInfo))]
[JsonSerializable(typeof(DirectoryCreateCommand.DirectoryCreateCommandResult))]
[JsonSerializable(typeof(FileListCommand.FileListCommandResult))]
[JsonSerializable(typeof(FileShareItemInfo))]
[JsonSerializable(typeof(FileSystemListPathsCommand.FileSystemListPathsCommandResult))]
[JsonSerializable(typeof(QueueMessageSendCommand.QueueMessageSendCommandResult))]
[JsonSerializable(typeof(QueueMessageSendResult))]
[JsonSerializable(typeof(TableListCommand.TableListCommandResult))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
internal sealed partial class StorageJsonContext : JsonSerializerContext
{
}
