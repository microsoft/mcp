// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Tools.Compute.Commands.Vm;
using Azure.Mcp.Tools.Compute.Commands.Vmss;
using Azure.Mcp.Tools.Compute.Models;

namespace Azure.Mcp.Tools.Compute.Commands;

[JsonSerializable(typeof(VmGetCommand.VmGetCommandResult))]
[JsonSerializable(typeof(VmListCommand.VmListCommandResult))]
[JsonSerializable(typeof(VmInstanceViewCommand.VmInstanceViewCommandResult))]
[JsonSerializable(typeof(VmSizesListCommand.VmSizesListCommandResult))]
[JsonSerializable(typeof(VmssGetCommand.VmssGetCommandResult))]
[JsonSerializable(typeof(VmssListCommand.VmssListCommandResult))]
[JsonSerializable(typeof(VmssVmsListCommand.VmssVmsListCommandResult))]
[JsonSerializable(typeof(VmssVmGetCommand.VmssVmGetCommandResult))]
[JsonSerializable(typeof(VmssRollingUpgradeStatusCommand.VmssRollingUpgradeStatusCommandResult))]
[JsonSerializable(typeof(VmInfo))]
[JsonSerializable(typeof(VmInstanceView))]
[JsonSerializable(typeof(VmSizeInfo))]
[JsonSerializable(typeof(VmssInfo))]
[JsonSerializable(typeof(VmssVmInfo))]
[JsonSerializable(typeof(VmssRollingUpgradeStatus))]
[JsonSerializable(typeof(List<VmInfo>))]
[JsonSerializable(typeof(List<VmSizeInfo>))]
[JsonSerializable(typeof(List<VmssInfo>))]
[JsonSerializable(typeof(List<VmssVmInfo>))]
internal sealed partial class ComputeJsonContext : JsonSerializerContext;
