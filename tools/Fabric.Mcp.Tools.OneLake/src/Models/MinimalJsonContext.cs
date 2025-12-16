// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Fabric.Mcp.Tools.OneLake.Commands.File;

namespace Fabric.Mcp.Tools.OneLake.Models;

[JsonSerializable(typeof(BlobListCommand.BlobListCommandResult))]
[JsonSerializable(typeof(PathListCommand.PathListResult))]
[JsonSerializable(typeof(OneLakeFileInfo))]
[JsonSerializable(typeof(FileSystemItem))]
[JsonSerializable(typeof(List<OneLakeFileInfo>))]
[JsonSerializable(typeof(List<FileSystemItem>))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase, WriteIndented = true)]
internal partial class MinimalJsonContext : JsonSerializerContext;
