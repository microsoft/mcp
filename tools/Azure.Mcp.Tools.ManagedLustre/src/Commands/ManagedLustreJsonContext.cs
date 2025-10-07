// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Tools.ManagedLustre.Commands.FileSystem;
using Azure.Mcp.Tools.ManagedLustre.Models;

namespace Azure.Mcp.Tools.ManagedLustre.Commands;

[JsonSerializable(typeof(SubnetSizeAskCommand.FileSystemSubnetSizeResult))]
[JsonSerializable(typeof(SubnetSizeValidateCommand.FileSystemCheckSubnetResult))]
[JsonSerializable(typeof(FileSystemListCommand.FileSystemListResult))]
[JsonSerializable(typeof(SkuGetCommand.SkuGetResult))]
[JsonSerializable(typeof(FileSystemCreateCommand.FileSystemCreateResult))]
[JsonSerializable(typeof(FileSystemUpdateCommand.FileSystemUpdateResult))]
[JsonSerializable(typeof(LustreFileSystem))]
[JsonSerializable(typeof(ManagedLustreSkuInfo))]
[JsonSerializable(typeof(ManagedLustreSkuCapability))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase, WriteIndented = true, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
internal partial class ManagedLustreJsonContext : JsonSerializerContext;
