// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using System.Text.Json.Serialization;
using Azure.Mcp.Tools.StorageSync.Commands.CloudEndpoint;
using Azure.Mcp.Tools.StorageSync.Commands.RegisteredServer;
using Azure.Mcp.Tools.StorageSync.Commands.ServerEndpoint;
using Azure.Mcp.Tools.StorageSync.Commands.StorageSyncService;
using Azure.Mcp.Tools.StorageSync.Commands.SyncGroup;

namespace Azure.Mcp.Tools.StorageSync;

/// <summary>
/// JSON serialization context for Storage Sync commands.
/// Required for AOT (Ahead-of-Time) compilation support.
/// </summary>
[JsonSerializable(typeof(StorageSyncServiceListCommand.StorageSyncServiceListCommandResult))]
[JsonSerializable(typeof(StorageSyncServiceGetCommand.StorageSyncServiceGetCommandResult))]
[JsonSerializable(typeof(StorageSyncServiceCreateCommand.StorageSyncServiceCreateCommandResult))]
[JsonSerializable(typeof(StorageSyncServiceUpdateCommand.StorageSyncServiceUpdateCommandResult))]
[JsonSerializable(typeof(StorageSyncServiceDeleteCommand.StorageSyncServiceDeleteCommandResult))]
[JsonSerializable(typeof(RegisteredServerListCommand.RegisteredServerListCommandResult))]
[JsonSerializable(typeof(RegisteredServerGetCommand.RegisteredServerGetCommandResult))]
[JsonSerializable(typeof(RegisteredServerRegisterCommand.RegisteredServerRegisterCommandResult))]
[JsonSerializable(typeof(RegisteredServerUpdateCommand.RegisteredServerUpdateCommandResult))]
[JsonSerializable(typeof(RegisteredServerUnregisterCommand.RegisteredServerUnregisterCommandResult))]
[JsonSerializable(typeof(SyncGroupListCommand.SyncGroupListCommandResult))]
[JsonSerializable(typeof(SyncGroupGetCommand.SyncGroupGetCommandResult))]
[JsonSerializable(typeof(SyncGroupCreateCommand.SyncGroupCreateCommandResult))]
[JsonSerializable(typeof(SyncGroupDeleteCommand.SyncGroupDeleteCommandResult))]
[JsonSerializable(typeof(CloudEndpointListCommand.CloudEndpointListCommandResult))]
[JsonSerializable(typeof(CloudEndpointGetCommand.CloudEndpointGetCommandResult))]
[JsonSerializable(typeof(CloudEndpointCreateCommand.CloudEndpointCreateCommandResult))]
[JsonSerializable(typeof(CloudEndpointDeleteCommand.CloudEndpointDeleteCommandResult))]
[JsonSerializable(typeof(CloudEndpointTriggerChangeDetectionCommand.CloudEndpointTriggerChangeDetectionCommandResult))]
[JsonSerializable(typeof(ServerEndpointListCommand.ServerEndpointListCommandResult))]
[JsonSerializable(typeof(ServerEndpointGetCommand.ServerEndpointGetCommandResult))]
[JsonSerializable(typeof(ServerEndpointCreateCommand.ServerEndpointCreateCommandResult))]
[JsonSerializable(typeof(ServerEndpointUpdateCommand.ServerEndpointUpdateCommandResult))]
[JsonSerializable(typeof(ServerEndpointDeleteCommand.ServerEndpointDeleteCommandResult))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
internal partial class StorageSyncJsonContext : JsonSerializerContext
{
}
