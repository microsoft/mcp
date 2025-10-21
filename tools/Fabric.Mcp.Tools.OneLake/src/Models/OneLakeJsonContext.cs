// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Fabric.Mcp.Tools.OneLake.Models;
using Fabric.Mcp.Tools.OneLake.Commands.Workspace;
using Fabric.Mcp.Tools.OneLake.Commands.Item;
using Fabric.Mcp.Tools.OneLake.Commands.File;
using System.Text.Json.Serialization;

namespace Fabric.Mcp.Tools.OneLake.Models;

[JsonSerializable(typeof(Models.Workspace))]
[JsonSerializable(typeof(WorkspaceProperties))]
[JsonSerializable(typeof(WorkspaceMetadata))]
[JsonSerializable(typeof(OneLakeItem))]
[JsonSerializable(typeof(OneLakeItemMetadata))]
[JsonSerializable(typeof(Lakehouse))]
[JsonSerializable(typeof(OneLakeShortcut))]
[JsonSerializable(typeof(OneLakeFileInfo))]
[JsonSerializable(typeof(CreateItemRequest))]
[JsonSerializable(typeof(UpdateItemRequest))]
[JsonSerializable(typeof(CreateShortcutRequest))]
[JsonSerializable(typeof(Models.WorkspacesResponse))]
[JsonSerializable(typeof(Models.ItemsResponse))]
[JsonSerializable(typeof(Models.LakehousesResponse))]
[JsonSerializable(typeof(Models.ShortcutsResponse))]
[JsonSerializable(typeof(WorkspaceListCommand.WorkspaceListCommandResult))]
[JsonSerializable(typeof(OneLakeWorkspaceListCommand.OneLakeWorkspaceListCommandResult), TypeInfoPropertyName = "OneLakeWorkspaceListCommandResult")]
[JsonSerializable(typeof(WorkspaceGetCommand.WorkspaceGetCommandResult))]
[JsonSerializable(typeof(ItemListCommand.ItemListCommandResult))]
[JsonSerializable(typeof(Fabric.Mcp.Tools.OneLake.Commands.Item.OneLakeItemListCommand.OneLakeItemListCommandResult))]
[JsonSerializable(typeof(FileReadCommand.FileReadCommandResult))]
[JsonSerializable(typeof(FileWriteCommand.FileWriteCommandResult))]
[JsonSerializable(typeof(IEnumerable<Models.Workspace>), TypeInfoPropertyName = "IEnumerableWorkspace")]
[JsonSerializable(typeof(IEnumerable<OneLakeItem>), TypeInfoPropertyName = "IEnumerableOneLakeItem")]
[JsonSerializable(typeof(IEnumerable<Lakehouse>), TypeInfoPropertyName = "IEnumerableLakehouse")]
[JsonSerializable(typeof(IEnumerable<OneLakeShortcut>), TypeInfoPropertyName = "IEnumerableOneLakeShortcut")]
[JsonSerializable(typeof(IEnumerable<OneLakeFileInfo>), TypeInfoPropertyName = "IEnumerableOneLakeFileInfo")]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase, WriteIndented = true)]
internal partial class OneLakeJsonContext : JsonSerializerContext;