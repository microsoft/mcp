// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Tools.ManagedLustre.Commands.FileSystem;
using Azure.Mcp.Tools.ManagedLustre.Commands.FileSystem.AutoexportJob;
using Azure.Mcp.Tools.ManagedLustre.Commands.FileSystem.AutoimportJob;
using Azure.Mcp.Tools.ManagedLustre.Commands.FileSystem.ImportJob;
using Azure.Mcp.Tools.ManagedLustre.Models;

namespace Azure.Mcp.Tools.ManagedLustre.Commands;

[JsonSerializable(typeof(AutoexportJobCreateCommand.AutoexportJobCreateCommandResult))]
[JsonSerializable(typeof(AutoexportJobCancelCommand.AutoexportJobCancelCommandResult))]
[JsonSerializable(typeof(AutoexportJobGetCommand.AutoexportJobGetCommandResult))]
[JsonSerializable(typeof(AutoexportJobDeleteCommand.AutoexportJobDeleteCommandResult))]
[JsonSerializable(typeof(AutoexportJob))]
[JsonSerializable(typeof(AutoexportJobProperties))]
[JsonSerializable(typeof(AutoexportJobStatus))]
[JsonSerializable(typeof(AutoimportJobCreateCommand.AutoimportJobCreateCommandResult))]
[JsonSerializable(typeof(AutoimportJobCancelCommand.AutoimportJobCancelCommandResult))]
[JsonSerializable(typeof(AutoimportJobGetCommand.AutoimportJobGetCommandResult))]
[JsonSerializable(typeof(AutoimportJobDeleteCommand.AutoimportJobDeleteCommandResult))]
[JsonSerializable(typeof(AutoimportJob))]
[JsonSerializable(typeof(AutoimportJobProperties))]
[JsonSerializable(typeof(AutoimportJobStatus))]
[JsonSerializable(typeof(BlobSyncEvents))]
[JsonSerializable(typeof(ImportJobCreateCommand.ImportJobCreateCommandResult))]
[JsonSerializable(typeof(ImportJobCancelCommand.ImportJobCancelCommandResult))]
[JsonSerializable(typeof(ImportJobGetCommand.ImportJobGetCommandResult))]
[JsonSerializable(typeof(ImportJobDeleteCommand.ImportJobDeleteCommandResult))]
[JsonSerializable(typeof(ImportJob))]
[JsonSerializable(typeof(ImportJobProperties))]
[JsonSerializable(typeof(ImportJobStatus))]
[JsonSerializable(typeof(FileSystemCreateCommand.FileSystemCreateCommandResult))]
[JsonSerializable(typeof(FileSystemListCommand.FileSystemListCommandResult))]
[JsonSerializable(typeof(FileSystemUpdateCommand.FileSystemUpdateCommandResult))]
[JsonSerializable(typeof(LustreFileSystem))]
[JsonSerializable(typeof(ManagedLustreSkuCapability))]
[JsonSerializable(typeof(ManagedLustreSkuInfo))]
[JsonSerializable(typeof(SkuGetCommand.SkuGetCommandResult))]
[JsonSerializable(typeof(SubnetSizeAskCommand.FileSystemSubnetSizeCommandResult))]
[JsonSerializable(typeof(SubnetSizeValidateCommand.FileSystemCheckSubnetCommandResult))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase, WriteIndented = true, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
internal partial class ManagedLustreJsonContext : JsonSerializerContext;
