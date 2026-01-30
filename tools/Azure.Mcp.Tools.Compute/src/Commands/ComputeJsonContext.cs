// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Tools.Compute.Commands.Disk;
using Azure.Mcp.Tools.Compute.Models;

namespace Azure.Mcp.Tools.Compute.Commands;

/// <summary>
/// JSON serialization context for Compute commands.
/// </summary>
[JsonSerializable(typeof(DiskGetCommand.DiskGetCommandResult))]
[JsonSerializable(typeof(Models.Disk))]
[JsonSerializable(typeof(List<Models.Disk>))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
public partial class ComputeJsonContext : JsonSerializerContext;
