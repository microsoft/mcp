// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.StorageSync.Commands.CloudEndpoint;
using Azure.Mcp.Tools.StorageSync.Commands.RegisteredServer;
using Azure.Mcp.Tools.StorageSync.Commands.ServerEndpoint;
using Azure.Mcp.Tools.StorageSync.Commands.StorageSyncService;
using Azure.Mcp.Tools.StorageSync.Commands.SyncGroup;
using Azure.Mcp.Tools.StorageSync.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Areas;
using Microsoft.Mcp.Core.Commands;

namespace Azure.Mcp.Tools.StorageSync;

/// <summary>
/// Setup configuration for Azure Storage Sync MCP tools.
/// </summary>
public class StorageSyncSetup : IAreaSetup
{
    /// <summary>
    /// Gets the namespace name for Storage Sync commands.
    /// </summary>
    public string Name => "storagesync";

    /// <summary>
    /// Gets the display title for the Storage Sync area.
    /// </summary>
    public string Title => "Manage Azure Storage Sync Services";

    /// <summary>
    /// Configures services for Storage Sync operations.
    /// </summary>
    public void ConfigureServices(IServiceCollection services)
    {
        // Register the service implementation
        services.AddSingleton<IStorageSyncService, StorageSyncService>();

        // Register StorageSyncService commands
        services.AddSingleton<StorageSyncServiceGetCommand>();
        services.AddSingleton<StorageSyncServiceCreateCommand>();
        services.AddSingleton<StorageSyncServiceUpdateCommand>();
        services.AddSingleton<StorageSyncServiceDeleteCommand>();

        // Register RegisteredServer commands
        services.AddSingleton<RegisteredServerGetCommand>();
        services.AddSingleton<RegisteredServerUpdateCommand>();
        services.AddSingleton<RegisteredServerUnregisterCommand>();

        // Register SyncGroup commands
        services.AddSingleton<SyncGroupGetCommand>();
        services.AddSingleton<SyncGroupCreateCommand>();
        services.AddSingleton<SyncGroupDeleteCommand>();

        // Register CloudEndpoint commands
        services.AddSingleton<CloudEndpointGetCommand>();
        services.AddSingleton<CloudEndpointCreateCommand>();
        services.AddSingleton<CloudEndpointDeleteCommand>();
        services.AddSingleton<CloudEndpointTriggerChangeDetectionCommand>();

        // Register ServerEndpoint commands
        services.AddSingleton<ServerEndpointGetCommand>();
        services.AddSingleton<ServerEndpointCreateCommand>();
        services.AddSingleton<ServerEndpointUpdateCommand>();
        services.AddSingleton<ServerEndpointDeleteCommand>();
    }

    /// <summary>
    /// Registers all Storage Sync commands into the command hierarchy.
    /// </summary>
    public CommandGroup RegisterCommands(IServiceProvider serviceProvider)
    {
        var storageSync = new CommandGroup(Name,
            """
            Azure Storage Sync Operations - Commands to manage Azure File Sync resources, including Storage Sync services, sync groups, cloud and server
            endpoints, and registered servers. Use this tool to deploy and maintain hybrid cloud file synchronization.
            """,
            Title);

        // StorageSyncService subgroup
        var storageSyncServiceGroup = new CommandGroup("service",
            "Storage Sync Service operations - Create, get, update, and delete Storage Sync services in your Azure subscription.");
        storageSync.AddSubGroup(storageSyncServiceGroup);

        var serviceGet = serviceProvider.GetRequiredService<StorageSyncServiceGetCommand>();
        storageSyncServiceGroup.AddCommand(serviceGet.Name, serviceGet);

        var serviceCreate = serviceProvider.GetRequiredService<StorageSyncServiceCreateCommand>();
        storageSyncServiceGroup.AddCommand(serviceCreate.Name, serviceCreate);

        var serviceUpdate = serviceProvider.GetRequiredService<StorageSyncServiceUpdateCommand>();
        storageSyncServiceGroup.AddCommand(serviceUpdate.Name, serviceUpdate);

        var serviceDelete = serviceProvider.GetRequiredService<StorageSyncServiceDeleteCommand>();
        storageSyncServiceGroup.AddCommand(serviceDelete.Name, serviceDelete);

        // RegisteredServer subgroup
        var registeredServerGroup = new CommandGroup("registeredserver",
            "Registered Server operations - Get, update, and unregister servers in your Storage Sync service.");
        storageSync.AddSubGroup(registeredServerGroup);

        var registeredServerGet = serviceProvider.GetRequiredService<RegisteredServerGetCommand>();
        registeredServerGroup.AddCommand(registeredServerGet.Name, registeredServerGet);

        var registeredServerUpdate = serviceProvider.GetRequiredService<RegisteredServerUpdateCommand>();
        registeredServerGroup.AddCommand(registeredServerUpdate.Name, registeredServerUpdate);

        var registeredServerUnregister = serviceProvider.GetRequiredService<RegisteredServerUnregisterCommand>();
        registeredServerGroup.AddCommand(registeredServerUnregister.Name, registeredServerUnregister);

        // SyncGroup subgroup
        var syncGroupGroup = new CommandGroup("syncgroup",
            "Sync Group operations - Create, get, and delete sync groups in your Storage Sync service.");
        storageSync.AddSubGroup(syncGroupGroup);

        var syncGroupGet = serviceProvider.GetRequiredService<SyncGroupGetCommand>();
        syncGroupGroup.AddCommand(syncGroupGet.Name, syncGroupGet);

        var syncGroupCreate = serviceProvider.GetRequiredService<SyncGroupCreateCommand>();
        syncGroupGroup.AddCommand(syncGroupCreate.Name, syncGroupCreate);

        var syncGroupDelete = serviceProvider.GetRequiredService<SyncGroupDeleteCommand>();
        syncGroupGroup.AddCommand(syncGroupDelete.Name, syncGroupDelete);

        // CloudEndpoint subgroup
        var cloudEndpointGroup = new CommandGroup("cloudendpoint",
            "Cloud Endpoint operations - Create, get, delete, and manage cloud endpoints in your sync groups.");
        storageSync.AddSubGroup(cloudEndpointGroup);

        var cloudEndpointGet = serviceProvider.GetRequiredService<CloudEndpointGetCommand>();
        cloudEndpointGroup.AddCommand(cloudEndpointGet.Name, cloudEndpointGet);

        var cloudEndpointCreate = serviceProvider.GetRequiredService<CloudEndpointCreateCommand>();
        cloudEndpointGroup.AddCommand(cloudEndpointCreate.Name, cloudEndpointCreate);

        var cloudEndpointDelete = serviceProvider.GetRequiredService<CloudEndpointDeleteCommand>();
        cloudEndpointGroup.AddCommand(cloudEndpointDelete.Name, cloudEndpointDelete);

        var cloudEndpointTriggerChangeDetection = serviceProvider.GetRequiredService<CloudEndpointTriggerChangeDetectionCommand>();
        cloudEndpointGroup.AddCommand(cloudEndpointTriggerChangeDetection.Name, cloudEndpointTriggerChangeDetection);

        // ServerEndpoint subgroup
        var serverEndpointGroup = new CommandGroup("serverendpoint",
            "Server Endpoint operations - Create, get, update, and delete server endpoints in your sync groups.");
        storageSync.AddSubGroup(serverEndpointGroup);

        var serverEndpointGet = serviceProvider.GetRequiredService<ServerEndpointGetCommand>();
        serverEndpointGroup.AddCommand(serverEndpointGet.Name, serverEndpointGet);

        var serverEndpointCreate = serviceProvider.GetRequiredService<ServerEndpointCreateCommand>();
        serverEndpointGroup.AddCommand(serverEndpointCreate.Name, serverEndpointCreate);

        var serverEndpointUpdate = serviceProvider.GetRequiredService<ServerEndpointUpdateCommand>();
        serverEndpointGroup.AddCommand(serverEndpointUpdate.Name, serverEndpointUpdate);

        var serverEndpointDelete = serviceProvider.GetRequiredService<ServerEndpointDeleteCommand>();
        serverEndpointGroup.AddCommand(serverEndpointDelete.Name, serverEndpointDelete);

        return storageSync;
    }
}
