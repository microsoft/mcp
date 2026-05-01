// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Tools.Compute.Commands.PlacementScore;
using Azure.Mcp.Tools.Compute.Models;

namespace Azure.Mcp.Tools.Compute.Commands;

[JsonSerializable(typeof(SpotPlacementMetadataCommand.SpotPlacementMetadataCommandResult))]
[JsonSerializable(typeof(SpotPlacementScoreCommand.SpotPlacementScoreCommandResult))]
[JsonSerializable(typeof(SpotPlacementMetadataInfo))]
[JsonSerializable(typeof(PlacementScoreInfo))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault)]
internal sealed partial class ComputePlacementJsonContext : JsonSerializerContext
{
}
