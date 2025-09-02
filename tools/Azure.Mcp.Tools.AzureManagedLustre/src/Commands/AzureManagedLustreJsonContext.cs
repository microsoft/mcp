// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Tools.AzureManagedLustre.Commands.FileSystem;
using Azure.Mcp.Tools.AzureManagedLustre.Models;

namespace Azure.Mcp.Tools.AzureManagedLustre.Commands;

[JsonSerializable(typeof(FileSystemSubnetSizeCommand.FileSystemSubnetSizeResult))]
[JsonSerializable(typeof(FileSystemListCommand.FileSystemListResult))]
[JsonSerializable(typeof(FileSystemCreateCommand.FileSystemCreateResult))]
[JsonSerializable(typeof(FileSystemUpdateCommand.FileSystemUpdateResult))]
[JsonSerializable(typeof(LustreFileSystem))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase, WriteIndented = true, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
internal partial class AzureManagedLustreJsonContext : JsonSerializerContext;
