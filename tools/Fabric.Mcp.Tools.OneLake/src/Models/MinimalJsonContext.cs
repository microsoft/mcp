// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Fabric.Mcp.Tools.OneLake.Commands.File;
using Fabric.Mcp.Tools.OneLake.Commands;
using System.Text.Json.Serialization;

namespace Fabric.Mcp.Tools.OneLake.Models;

[JsonSerializable(typeof(BlobListCommand.BlobListCommandResult))]
[JsonSerializable(typeof(Commands.PathListCommand.PathListResult))]
[JsonSerializable(typeof(OneLakeFileInfo))]
[JsonSerializable(typeof(FileSystemItem))]
[JsonSerializable(typeof(List<OneLakeFileInfo>))]
[JsonSerializable(typeof(List<FileSystemItem>))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase, WriteIndented = true)]
internal partial class MinimalJsonContext : JsonSerializerContext;