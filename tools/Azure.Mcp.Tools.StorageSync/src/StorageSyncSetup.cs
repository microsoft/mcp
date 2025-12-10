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
        services.AddSingleton<StorageSyncServiceListCommand>();
        services.AddSingleton<StorageSyncServiceGetCommand>();
        services.AddSingleton<StorageSyncServiceCreateCommand>();
        services.AddSingleton<StorageSyncServiceUpdateCommand>();
        services.AddSingleton<StorageSyncServiceDeleteCommand>();

        // Register RegisteredServer commands
        services.AddSingleton<RegisteredServerListCommand>();
        services.AddSingleton<RegisteredServerGetCommand>();
        services.AddSingleton<RegisteredServerRegisterCommand>();
        services.AddSingleton<RegisteredServerUpdateCommand>();
        services.AddSingleton<RegisteredServerUnregisterCommand>();

        // Register SyncGroup commands
        services.AddSingleton<SyncGroupListCommand>();
        services.AddSingleton<SyncGroupGetCommand>();
        services.AddSingleton<SyncGroupCreateCommand>();
        services.AddSingleton<SyncGroupDeleteCommand>();

        // Register CloudEndpoint commands
        services.AddSingleton<CloudEndpointListCommand>();
        services.AddSingleton<CloudEndpointGetCommand>();
        services.AddSingleton<CloudEndpointCreateCommand>();
        services.AddSingleton<CloudEndpointDeleteCommand>();
        services.AddSingleton<CloudEndpointTriggerChangeDetectionCommand>();

        // Register ServerEndpoint commands
        services.AddSingleton<ServerEndpointListCommand>();
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
            Azure Storage Sync operations - Commands for managing Azure File Sync services, sync groups, cloud endpoints,
            server endpoints, and registered servers. Use this tool to deploy, configure, and manage File Sync infrastructure
            for hybrid cloud file synchronization scenarios. The tool supports listing, creating, updating, and deleting
            resources across the Storage Sync service hierarchy. Each command requires appropriate Azure permissions and
            subscription access.
            """,
            Title);

        // StorageSyncService subgroup
        var storageSyncServiceGroup = new CommandGroup("storageSyncService",
            "Storage Sync Service operations - Create, list, get, update, and delete Storage Sync services in your Azure subscription.");
        storageSync.AddSubGroup(storageSyncServiceGroup);

        storageSyncServiceGroup.AddCommand("list", serviceProvider.GetRequiredService<StorageSyncServiceListCommand>());
        storageSyncServiceGroup.AddCommand("get", serviceProvider.GetRequiredService<StorageSyncServiceGetCommand>());
        storageSyncServiceGroup.AddCommand("create", serviceProvider.GetRequiredService<StorageSyncServiceCreateCommand>());
        storageSyncServiceGroup.AddCommand("update", serviceProvider.GetRequiredService<StorageSyncServiceUpdateCommand>());
        storageSyncServiceGroup.AddCommand("delete", serviceProvider.GetRequiredService<StorageSyncServiceDeleteCommand>());

        // RegisteredServer subgroup
        var registeredServerGroup = new CommandGroup("registeredServer",
            "Registered Server operations - Register, list, get, update, and unregister servers in your Storage Sync service.");
        storageSync.AddSubGroup(registeredServerGroup);

        registeredServerGroup.AddCommand("list", serviceProvider.GetRequiredService<RegisteredServerListCommand>());
        registeredServerGroup.AddCommand("get", serviceProvider.GetRequiredService<RegisteredServerGetCommand>());
        registeredServerGroup.AddCommand("register", serviceProvider.GetRequiredService<RegisteredServerRegisterCommand>());
        registeredServerGroup.AddCommand("update", serviceProvider.GetRequiredService<RegisteredServerUpdateCommand>());
        registeredServerGroup.AddCommand("unregister", serviceProvider.GetRequiredService<RegisteredServerUnregisterCommand>());

        // SyncGroup subgroup
        var syncGroupGroup = new CommandGroup("syncGroup",
            "Sync Group operations - Create, list, get, and delete sync groups in your Storage Sync service.");
        storageSync.AddSubGroup(syncGroupGroup);

        syncGroupGroup.AddCommand("list", serviceProvider.GetRequiredService<SyncGroupListCommand>());
        syncGroupGroup.AddCommand("get", serviceProvider.GetRequiredService<SyncGroupGetCommand>());
        syncGroupGroup.AddCommand("create", serviceProvider.GetRequiredService<SyncGroupCreateCommand>());
        syncGroupGroup.AddCommand("delete", serviceProvider.GetRequiredService<SyncGroupDeleteCommand>());

        // CloudEndpoint subgroup
        var cloudEndpointGroup = new CommandGroup("cloudEndpoint",
            "Cloud Endpoint operations - Create, list, get, delete, and manage cloud endpoints in your sync groups.");
        storageSync.AddSubGroup(cloudEndpointGroup);

        cloudEndpointGroup.AddCommand("list", serviceProvider.GetRequiredService<CloudEndpointListCommand>());
        cloudEndpointGroup.AddCommand("get", serviceProvider.GetRequiredService<CloudEndpointGetCommand>());
        cloudEndpointGroup.AddCommand("create", serviceProvider.GetRequiredService<CloudEndpointCreateCommand>());
        cloudEndpointGroup.AddCommand("delete", serviceProvider.GetRequiredService<CloudEndpointDeleteCommand>());
        cloudEndpointGroup.AddCommand("triggerChangeDetection", serviceProvider.GetRequiredService<CloudEndpointTriggerChangeDetectionCommand>());

        // ServerEndpoint subgroup
        var serverEndpointGroup = new CommandGroup("serverEndpoint",
            "Server Endpoint operations - Create, list, get, update, and delete server endpoints in your sync groups.");
        storageSync.AddSubGroup(serverEndpointGroup);

        serverEndpointGroup.AddCommand("list", serviceProvider.GetRequiredService<ServerEndpointListCommand>());
        serverEndpointGroup.AddCommand("get", serviceProvider.GetRequiredService<ServerEndpointGetCommand>());
        serverEndpointGroup.AddCommand("create", serviceProvider.GetRequiredService<ServerEndpointCreateCommand>());
        serverEndpointGroup.AddCommand("update", serviceProvider.GetRequiredService<ServerEndpointUpdateCommand>());
        serverEndpointGroup.AddCommand("delete", serviceProvider.GetRequiredService<ServerEndpointDeleteCommand>());

        return storageSync;
    }
}
