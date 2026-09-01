// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Tools.Compute.Commands.Vm;
using Azure.Mcp.Tools.ResilienceManagement.Commands.Recovery.Plans;

namespace Azure.Mcp.Core.Tests.Areas.Server.Commands;

[JsonSerializable(typeof(VmGetCommand.VmGetResult))]
[JsonSerializable(typeof(RecoveryPlanDeleteCommand.RecoveryPlanDeleteCommandResult))]
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault)]
internal sealed partial class PerPropertyIgnoreJsonContext : JsonSerializerContext;
