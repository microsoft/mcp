// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Tools.AzureManagedLustre.Commands.FileSystem;
using Azure.Mcp.Tools.AzureManagedLustre.Models;

namespace Azure.Mcp.Tools.AzureManagedLustre.Commands;

[JsonSerializable(typeof(SubnetSizeAskCommand.FileSystemSubnetSizeResult))]
[JsonSerializable(typeof(SubnetSizeValidateCommand.FileSystemCheckSubnetResult))]
[JsonSerializable(typeof(FileSystemListCommand.FileSystemListResult))]
[JsonSerializable(typeof(SkuGetCommand.SkuGetResult))]
[JsonSerializable(typeof(LustreFileSystem))]
[JsonSerializable(typeof(AzureManagedLustreSkuInfo))]
[JsonSerializable(typeof(AzureManagedLustreSkuCapability))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase, WriteIndented = true, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
internal partial class AzureManagedLustreJsonContext : JsonSerializerContext;
