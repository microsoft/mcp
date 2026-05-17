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
[JsonSerializable(typeof(DiskCreateCommand.DiskCreateCommandResult))]
[JsonSerializable(typeof(DiskDeleteCommand.DiskDeleteCommandResult))]
[JsonSerializable(typeof(DiskGetCommand.DiskGetCommandResult))]
[JsonSerializable(typeof(DiskUpdateCommand.DiskUpdateCommandResult))]
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
[JsonSerializable(typeof(VmDeleteCommand.VmDeleteCommandResult))]
[JsonSerializable(typeof(VmPowerStateCommand.VmPowerStateCommandResult))]
[JsonSerializable(typeof(VmssDeleteCommand.VmssDeleteCommandResult))]
[JsonSerializable(typeof(VmssUpdateCommand.VmssUpdateCommandResult))]
[JsonSerializable(typeof(VmssUpdateResult))]
[JsonSerializable(typeof(VmInfo))]
[JsonSerializable(typeof(VmInstanceView))]
[JsonSerializable(typeof(VmPowerStateResult))]
[JsonSerializable(typeof(VmssInfo))]
[JsonSerializable(typeof(VmssVmInfo))]
[JsonSerializable(typeof(List<VmInfo>))]
[JsonSerializable(typeof(List<VmssInfo>))]
[JsonSerializable(typeof(List<VmssVmInfo>))]
[JsonSerializable(typeof(VmSkuListCommand.VmSkuListCommandResult))]
[JsonSerializable(typeof(VmImageListCommand.VmImageListCommandResult))]
[JsonSerializable(typeof(VmQuotaCheckCommand.VmQuotaCheckCommandResult))]
[JsonSerializable(typeof(VmRegionRecommendCommand.VmRegionRecommendCommandResult))]
[JsonSerializable(typeof(VmSkuInfo))]
[JsonSerializable(typeof(List<VmSkuInfo>))]
[JsonSerializable(typeof(VmImageInfo))]
[JsonSerializable(typeof(List<VmImageInfo>))]
[JsonSerializable(typeof(VmQuotaInfo))]
[JsonSerializable(typeof(List<VmQuotaInfo>))]
[JsonSerializable(typeof(VmRegionRecommendation))]
[JsonSerializable(typeof(List<VmRegionRecommendation>))]
internal sealed partial class ComputeJsonContext : JsonSerializerContext;
