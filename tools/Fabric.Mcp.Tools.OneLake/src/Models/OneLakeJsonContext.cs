// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Fabric.Mcp.Tools.OneLake.Commands.File;
using Fabric.Mcp.Tools.OneLake.Commands;
using Fabric.Mcp.Tools.OneLake.Commands.Workspace;
using Fabric.Mcp.Tools.OneLake.Commands.Item;
using System.Text.Json.Serialization;

namespace Fabric.Mcp.Tools.OneLake.Models;

[JsonSerializable(typeof(Workspace))]
[JsonSerializable(typeof(WorkspaceProperties))]
[JsonSerializable(typeof(WorkspaceMetadata))]
[JsonSerializable(typeof(OneLakeItem))]
[JsonSerializable(typeof(OneLakeItemMetadata))]
[JsonSerializable(typeof(Lakehouse))]
[JsonSerializable(typeof(OneLakeShortcut))]
[JsonSerializable(typeof(ShortcutTarget))]
[JsonSerializable(typeof(OneLakeShortcutTarget))]
[JsonSerializable(typeof(AdlsGen2ShortcutTarget))]
[JsonSerializable(typeof(S3ShortcutTarget))]
[JsonSerializable(typeof(OneLakeFileInfo))]
[JsonSerializable(typeof(FileSystemItem))]
[JsonSerializable(typeof(CreateItemRequest))]
[JsonSerializable(typeof(UpdateItemRequest))]
[JsonSerializable(typeof(CreateShortcutRequest))]
[JsonSerializable(typeof(WorkspacesResponse))]
[JsonSerializable(typeof(ItemsResponse))]
[JsonSerializable(typeof(LakehousesResponse))]
[JsonSerializable(typeof(ShortcutsResponse))]
[JsonSerializable(typeof(OneLakeEndpoint))]
[JsonSerializable(typeof(OneLakeEnvironmentEndpoints))]
[JsonSerializable(typeof(OneLakeWorkspaceListCommand.OneLakeWorkspaceListCommandResult))]
[JsonSerializable(typeof(OneLakeItemListCommand.OneLakeItemListCommandResult))]
[JsonSerializable(typeof(OneLakeItemListDfsCommand.OneLakeItemListDfsCommandResult))]
[JsonSerializable(typeof(ItemCreateCommand.ItemCreateCommandResult))]
[JsonSerializable(typeof(FileReadCommand.FileReadCommandResult))]
[JsonSerializable(typeof(FileWriteCommand.FileWriteCommandResult))]
[JsonSerializable(typeof(FileDeleteCommand.FileDeleteCommandResult))]
[JsonSerializable(typeof(BlobListCommand.BlobListCommandResult))]
[JsonSerializable(typeof(Commands.PathListCommand.PathListResult))]
[JsonSerializable(typeof(DirectoryCreateCommand.DirectoryCreateCommandResult))]
[JsonSerializable(typeof(DirectoryDeleteCommand.DirectoryDeleteCommandResult))]
[JsonSerializable(typeof(IEnumerable<Workspace>))]
[JsonSerializable(typeof(IEnumerable<OneLakeItem>))]
[JsonSerializable(typeof(IEnumerable<Lakehouse>))]
[JsonSerializable(typeof(IEnumerable<OneLakeShortcut>))]
[JsonSerializable(typeof(IEnumerable<OneLakeFileInfo>))]
[JsonSerializable(typeof(IEnumerable<FileSystemItem>))]
[JsonSerializable(typeof(List<Workspace>))]
[JsonSerializable(typeof(List<OneLakeItem>))]
[JsonSerializable(typeof(List<Lakehouse>))]
[JsonSerializable(typeof(List<OneLakeShortcut>))]
[JsonSerializable(typeof(List<OneLakeFileInfo>))]
[JsonSerializable(typeof(List<FileSystemItem>))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase, WriteIndented = true)]
internal partial class OneLakeJsonContext : JsonSerializerContext;