// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Tools.Compute.Commands.Vm;

namespace Azure.Mcp.Core.Tests.Areas.Server.Commands;

[JsonSerializable(typeof(VmGetCommand.VmGetResult))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
internal sealed partial class PerPropertyIgnoreJsonContext : JsonSerializerContext;
