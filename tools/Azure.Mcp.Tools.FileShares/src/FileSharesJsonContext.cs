// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using System.Text.Json.Serialization;
using Azure.Mcp.Tools.FileShares.Commands.FileShare;
using Azure.Mcp.Tools.FileShares.Models;

namespace Azure.Mcp.Tools.FileShares;

[JsonSerializable(typeof(FileShareInfo))]
[JsonSerializable(typeof(List<FileShareInfo>))]
[JsonSerializable(typeof(FileShareListCommand.FileShareListCommandResult))]
[JsonSerializable(typeof(FileShareDataSchema))]
[JsonSerializable(typeof(FileShareSnapshotSchema))]
[JsonSerializable(typeof(FileShareSnapshotInfo))]
[JsonSerializable(typeof(PrivateEndpointConnectionDataSchema))]
[JsonSerializable(typeof(PrivateEndpointConnectionInfo))]
[JsonSerializable(typeof(NameAvailabilityResult))]
[JsonSerializable(typeof(JsonElement))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
internal partial class FileSharesJsonContext : JsonSerializerContext
{
}
