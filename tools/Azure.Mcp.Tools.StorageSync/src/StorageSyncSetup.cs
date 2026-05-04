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

        storageSyncServiceGroup.AddCommand<StorageSyncServiceGetCommand>(serviceProvider);
        storageSyncServiceGroup.AddCommand<StorageSyncServiceCreateCommand>(serviceProvider);
        storageSyncServiceGroup.AddCommand<StorageSyncServiceUpdateCommand>(serviceProvider);
        storageSyncServiceGroup.AddCommand<StorageSyncServiceDeleteCommand>(serviceProvider);

        // RegisteredServer subgroup
        var registeredServerGroup = new CommandGroup("registeredserver",
            "Registered Server operations - Get, update, and unregister servers in your Storage Sync service.");
        storageSync.AddSubGroup(registeredServerGroup);

        registeredServerGroup.AddCommand<RegisteredServerGetCommand>(serviceProvider);
        registeredServerGroup.AddCommand<RegisteredServerUpdateCommand>(serviceProvider);
        registeredServerGroup.AddCommand<RegisteredServerUnregisterCommand>(serviceProvider);

        // SyncGroup subgroup
        var syncGroupGroup = new CommandGroup("syncgroup",
            "Sync Group operations - Create, get, and delete sync groups in your Storage Sync service.");
        storageSync.AddSubGroup(syncGroupGroup);

        syncGroupGroup.AddCommand<SyncGroupGetCommand>(serviceProvider);
        syncGroupGroup.AddCommand<SyncGroupCreateCommand>(serviceProvider);
        syncGroupGroup.AddCommand<SyncGroupDeleteCommand>(serviceProvider);

        // CloudEndpoint subgroup
        var cloudEndpointGroup = new CommandGroup("cloudendpoint",
            "Cloud Endpoint operations - Create, get, delete, and manage cloud endpoints in your sync groups.");
        storageSync.AddSubGroup(cloudEndpointGroup);

        cloudEndpointGroup.AddCommand<CloudEndpointGetCommand>(serviceProvider);
        cloudEndpointGroup.AddCommand<CloudEndpointCreateCommand>(serviceProvider);
        cloudEndpointGroup.AddCommand<CloudEndpointDeleteCommand>(serviceProvider);
        cloudEndpointGroup.AddCommand<CloudEndpointTriggerChangeDetectionCommand>(serviceProvider);

        // ServerEndpoint subgroup
        var serverEndpointGroup = new CommandGroup("serverendpoint",
            "Server Endpoint operations - Create, get, update, and delete server endpoints in your sync groups.");
        storageSync.AddSubGroup(serverEndpointGroup);

        serverEndpointGroup.AddCommand<ServerEndpointGetCommand>(serviceProvider);
        serverEndpointGroup.AddCommand<ServerEndpointCreateCommand>(serviceProvider);
        serverEndpointGroup.AddCommand<ServerEndpointUpdateCommand>(serviceProvider);
        serverEndpointGroup.AddCommand<ServerEndpointDeleteCommand>(serviceProvider);

        return storageSync;
    }
}
