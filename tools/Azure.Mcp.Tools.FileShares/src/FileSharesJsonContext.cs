// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using System.Text.Json.Serialization;
using Azure.Mcp.Tools.FileShares.Commands.FileShare;
using Azure.Mcp.Tools.FileShares.Commands.PrivateEndpointConnection;
using Azure.Mcp.Tools.FileShares.Commands.Snapshot;
using Azure.Mcp.Tools.FileShares.Models;

namespace Azure.Mcp.Tools.FileShares;

[JsonSerializable(typeof(FileShareInfo))]
[JsonSerializable(typeof(List<FileShareInfo>))]
[JsonSerializable(typeof(FileShareGetCommand.FileShareGetCommandResult))]
[JsonSerializable(typeof(FileShareCreateCommand.FileShareCreateCommandResult))]
[JsonSerializable(typeof(FileShareUpdateCommand.FileShareUpdateCommandResult))]
[JsonSerializable(typeof(FileShareDeleteCommand.FileShareDeleteCommandResult))]
[JsonSerializable(typeof(FileShareCheckNameAvailabilityCommand.FileShareCheckNameAvailabilityCommandResult))]
[JsonSerializable(typeof(FileShareSnapshotInfo))]
[JsonSerializable(typeof(List<FileShareSnapshotInfo>))]
[JsonSerializable(typeof(SnapshotCreateCommand.SnapshotCreateCommandResult))]
[JsonSerializable(typeof(SnapshotGetCommand.SnapshotGetCommandResult))]
[JsonSerializable(typeof(SnapshotDeleteCommand.SnapshotDeleteCommandResult))]
[JsonSerializable(typeof(SnapshotUpdateCommand.SnapshotUpdateCommandResult))]
[JsonSerializable(typeof(PrivateEndpointConnectionInfo))]
[JsonSerializable(typeof(List<PrivateEndpointConnectionInfo>))]
[JsonSerializable(typeof(PrivateEndpointConnectionGetCommand.PrivateEndpointConnectionGetCommandResult))]
[JsonSerializable(typeof(PrivateEndpointConnectionUpdateCommand.PrivateEndpointConnectionUpdateCommandResult))]
[JsonSerializable(typeof(FileShareDataSchema))]
[JsonSerializable(typeof(PrivateEndpointConnectionDataSchema))]
[JsonSerializable(typeof(FileShareLimitsResult))]
[JsonSerializable(typeof(FileShareLimits))]
[JsonSerializable(typeof(FileShareProvisioningConstants))]
[JsonSerializable(typeof(FileShareUsageDataResult))]
[JsonSerializable(typeof(LiveSharesUsageData))]
[JsonSerializable(typeof(FileShareProvisioningRecommendationResult))]
[JsonSerializable(typeof(Dictionary<string, string>))]
[JsonSerializable(typeof(JsonElement))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
internal partial class FileSharesJsonContext : JsonSerializerContext
{
}

