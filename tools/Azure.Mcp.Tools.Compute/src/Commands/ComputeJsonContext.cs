// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Tools.Compute.Commands.Disk;
using Azure.Mcp.Tools.Compute.Commands.Vm;
using Azure.Mcp.Tools.Compute.Commands.Vmss;
using Azure.Mcp.Tools.Compute.Models;

namespace Azure.Mcp.Tools.Compute.Commands;

/// <summary>
/// JSON serialization context for Compute commands.
/// </summary>
[JsonSerializable(typeof(DiskGetCommand.DiskGetCommandResult))]
[JsonSerializable(typeof(Models.DiskInfo))]
[JsonSerializable(typeof(List<Models.DiskInfo>))]
[JsonSerializable(typeof(VmCreateCommand.VmCreateCommandResult))]
[JsonSerializable(typeof(VmCreateResult))]
[JsonSerializable(typeof(VmUpdateCommand.VmUpdateCommandResult))]
[JsonSerializable(typeof(VmUpdateResult))]
[JsonSerializable(typeof(VmGetCommand.VmGetSingleResult))]
[JsonSerializable(typeof(VmGetCommand.VmGetListResult))]
[JsonSerializable(typeof(VmssGetCommand.VmssGetSingleResult))]
[JsonSerializable(typeof(VmssGetCommand.VmssGetListResult))]
[JsonSerializable(typeof(VmssGetCommand.VmssGetVmInstanceResult))]
[JsonSerializable(typeof(VmssCreateCommand.VmssCreateCommandResult))]
[JsonSerializable(typeof(VmssCreateResult))]
[JsonSerializable(typeof(VmssUpdateCommand.VmssUpdateCommandResult))]
[JsonSerializable(typeof(VmssUpdateResult))]
[JsonSerializable(typeof(VmInfo))]
[JsonSerializable(typeof(VmInstanceView))]
[JsonSerializable(typeof(VmssInfo))]
[JsonSerializable(typeof(VmssVmInfo))]
[JsonSerializable(typeof(WorkloadConfiguration))]
[JsonSerializable(typeof(List<VmInfo>))]
[JsonSerializable(typeof(List<VmssInfo>))]
[JsonSerializable(typeof(List<VmssVmInfo>))]
internal sealed partial class ComputeJsonContext : JsonSerializerContext;
