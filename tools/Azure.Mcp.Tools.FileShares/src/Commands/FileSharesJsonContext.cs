// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Tools.FileShares.Commands.FileShare;
using Azure.Mcp.Tools.FileShares.Commands.Informational;
using Azure.Mcp.Tools.FileShares.Commands.PrivateEndpointConnection;
using Azure.Mcp.Tools.FileShares.Models;
using Azure.Mcp.Tools.FileShares.Services;

namespace Azure.Mcp.Tools.FileShares.Commands;

[JsonSerializable(typeof(FileShareListCommand.FileShareListCommandResult))]
[JsonSerializable(typeof(FileShareSnapshotListCommand.FileShareSnapshotListCommandResult))]
[JsonSerializable(typeof(FileShareGetLimitsCommand.FileShareGetLimitsCommandResult))]
[JsonSerializable(typeof(FileShareGetProvisioningRecommendationCommand.FileShareGetProvisioningRecommendationCommandResult))]
[JsonSerializable(typeof(FileShareGetUsageDataCommand.FileShareGetUsageDataCommandResult))]
[JsonSerializable(typeof(PrivateEndpointConnectionListCommand.PrivateEndpointConnectionListCommandResult))]
[JsonSerializable(typeof(PrivateEndpointConnectionGetCommand.PrivateEndpointConnectionGetCommandResult))]
[JsonSerializable(typeof(PrivateEndpointConnectionUpdateCommand.PrivateEndpointConnectionUpdateCommandResult))]
[JsonSerializable(typeof(PrivateEndpointConnectionDeleteCommand.PrivateEndpointConnectionDeleteCommandResult))]
[JsonSerializable(typeof(FileShareDetail))]
[JsonSerializable(typeof(FileShareSnapshot))]
[JsonSerializable(typeof(PrivateEndpointConnectionData))]
[JsonSerializable(typeof(PrivateEndpointConnectionProperties))]
[JsonSerializable(typeof(PrivateEndpoint))]
[JsonSerializable(typeof(PrivateLinkServiceConnectionState))]
[JsonSerializable(typeof(NameAvailabilityResult))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
internal partial class FileSharesJsonContext : JsonSerializerContext;

