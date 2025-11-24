// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Tools.ManagedLustre.Commands.FileSystem;
using Azure.Mcp.Tools.ManagedLustre.Commands.FileSystem.AutoexportJob;
using Azure.Mcp.Tools.ManagedLustre.Commands.FileSystem.AutoimportJob;
using Azure.Mcp.Tools.ManagedLustre.Models;

namespace Azure.Mcp.Tools.ManagedLustre.Commands;

[JsonSerializable(typeof(AutoexportJobCreateCommand.AutoexportJobCreateResult))]
[JsonSerializable(typeof(AutoexportJobCancelCommand.AutoexportJobCancelResult))]
[JsonSerializable(typeof(AutoexportJobGetCommand.AutoexportJobGetResult))]
[JsonSerializable(typeof(AutoexportJobListCommand.AutoexportJobListResult))]
[JsonSerializable(typeof(AutoexportJobDeleteCommand.AutoexportJobDeleteResult))]
[JsonSerializable(typeof(AutoexportJob))]
[JsonSerializable(typeof(AutoimportJobCreateCommand.AutoimportJobCreateResult))]
[JsonSerializable(typeof(AutoimportJobCancelCommand.AutoimportJobCancelResult))]
[JsonSerializable(typeof(AutoimportJobGetCommand.AutoimportJobGetResult))]
[JsonSerializable(typeof(AutoimportJobListCommand.AutoimportJobListResult))]
[JsonSerializable(typeof(AutoimportJobDeleteCommand.AutoimportJobDeleteResult))]
[JsonSerializable(typeof(AutoimportJob))]
[JsonSerializable(typeof(FileSystemCreateCommand.FileSystemCreateResult))]
[JsonSerializable(typeof(FileSystemListCommand.FileSystemListResult))]
[JsonSerializable(typeof(FileSystemUpdateCommand.FileSystemUpdateResult))]
[JsonSerializable(typeof(LustreFileSystem))]
[JsonSerializable(typeof(ManagedLustreSkuCapability))]
[JsonSerializable(typeof(ManagedLustreSkuInfo))]
[JsonSerializable(typeof(SkuGetCommand.SkuGetResult))]
[JsonSerializable(typeof(SubnetSizeAskCommand.FileSystemSubnetSizeResult))]
[JsonSerializable(typeof(SubnetSizeValidateCommand.FileSystemCheckSubnetResult))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase, WriteIndented = true, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
internal partial class ManagedLustreJsonContext : JsonSerializerContext;
