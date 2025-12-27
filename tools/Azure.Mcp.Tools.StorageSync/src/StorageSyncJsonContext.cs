// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using System.Text.Json.Serialization;
using Azure.Mcp.Tools.StorageSync.Commands.CloudEndpoint;
using Azure.Mcp.Tools.StorageSync.Commands.RegisteredServer;
using Azure.Mcp.Tools.StorageSync.Commands.ServerEndpoint;
using Azure.Mcp.Tools.StorageSync.Commands.StorageSyncService;
using Azure.Mcp.Tools.StorageSync.Commands.SyncGroup;
using Azure.Mcp.Tools.StorageSync.Models;

namespace Azure.Mcp.Tools.StorageSync;

/// <summary>
/// JSON serialization context for Storage Sync commands.
/// Required for AOT (Ahead-of-Time) compilation support.
/// </summary>
[JsonSerializable(typeof(StorageSyncServiceGetCommand.StorageSyncServiceGetCommandResult))]
[JsonSerializable(typeof(StorageSyncServiceCreateCommand.StorageSyncServiceCreateCommandResult))]
[JsonSerializable(typeof(StorageSyncServiceUpdateCommand.StorageSyncServiceUpdateCommandResult))]
[JsonSerializable(typeof(StorageSyncServiceDeleteCommand.StorageSyncServiceDeleteCommandResult))]
[JsonSerializable(typeof(RegisteredServerGetCommand.RegisteredServerGetCommandResult))]
[JsonSerializable(typeof(RegisteredServerUpdateCommand.RegisteredServerUpdateCommandResult))]
[JsonSerializable(typeof(RegisteredServerUnregisterCommand.RegisteredServerUnregisterCommandResult))]
[JsonSerializable(typeof(SyncGroupGetCommand.SyncGroupGetCommandResult))]
[JsonSerializable(typeof(SyncGroupCreateCommand.SyncGroupCreateCommandResult))]
[JsonSerializable(typeof(SyncGroupDeleteCommand.SyncGroupDeleteCommandResult))]
[JsonSerializable(typeof(CloudEndpointGetCommand.CloudEndpointGetCommandResult))]
[JsonSerializable(typeof(CloudEndpointCreateCommand.CloudEndpointCreateCommandResult))]
[JsonSerializable(typeof(CloudEndpointDeleteCommand.CloudEndpointDeleteCommandResult))]
[JsonSerializable(typeof(CloudEndpointTriggerChangeDetectionCommand.CloudEndpointTriggerChangeDetectionCommandResult))]
[JsonSerializable(typeof(ServerEndpointGetCommand.ServerEndpointGetCommandResult))]
[JsonSerializable(typeof(ServerEndpointCreateCommand.ServerEndpointCreateCommandResult))]
[JsonSerializable(typeof(ServerEndpointUpdateCommand.ServerEndpointUpdateCommandResult))]
[JsonSerializable(typeof(ServerEndpointDeleteCommand.ServerEndpointDeleteCommandResult))]
[JsonSerializable(typeof(ServerEndpointSyncStatusSchema))]
[JsonSerializable(typeof(ServerEndpointCloudTieringSchema))]
[JsonSerializable(typeof(ServerEndpointOfflineDataTransferSchema))]
[JsonSerializable(typeof(ServerEndpointSyncPoliciesSchema))]
[JsonSerializable(typeof(ServerEndpointRecallStatusSchema))]
[JsonSerializable(typeof(ServerEndpointRecallErrorSchema))]
[JsonSerializable(typeof(StorageSyncServiceIdentitySchema))]
[JsonSerializable(typeof(StorageSyncServicePropertiesSchema))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
internal partial class StorageSyncJsonContext : JsonSerializerContext
{
}
