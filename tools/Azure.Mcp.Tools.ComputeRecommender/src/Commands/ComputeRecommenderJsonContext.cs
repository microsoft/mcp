// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Tools.ComputeRecommender.Commands.PlacementScore;
using Azure.Mcp.Tools.ComputeRecommender.Models;

namespace Azure.Mcp.Tools.ComputeRecommender.Commands;

[JsonSerializable(typeof(SpotPlacementMetadataCommand.SpotPlacementMetadataCommandResult))]
[JsonSerializable(typeof(SpotPlacementScoreCommand.SpotPlacementScoreCommandResult))]
[JsonSerializable(typeof(SpotPlacementMetadataInfo))]
[JsonSerializable(typeof(PlacementScoreInfo))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault)]
internal sealed partial class ComputeRecommenderJsonContext : JsonSerializerContext
{
}
