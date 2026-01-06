// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using System.Text.Json.Serialization;
using Azure.Mcp.Tools.FileShares.Commands.FileShare;
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
[JsonSerializable(typeof(FileShareDataSchema))]
[JsonSerializable(typeof(FileShareSnapshotDataSchema))]
[JsonSerializable(typeof(PrivateEndpointConnectionDataSchema))]
[JsonSerializable(typeof(PrivateEndpointConnectionInfo))]
[JsonSerializable(typeof(JsonElement))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
internal partial class FileSharesJsonContext : JsonSerializerContext
{
}

