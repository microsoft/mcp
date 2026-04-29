// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.FileShares.Commands.FileShare;
using Azure.Mcp.Tools.FileShares.Commands.Informational;
using Azure.Mcp.Tools.FileShares.Commands.PrivateEndpointConnection;
using Azure.Mcp.Tools.FileShares.Commands.Snapshot;
using Azure.Mcp.Tools.FileShares.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Areas;
using Microsoft.Mcp.Core.Commands;

namespace Azure.Mcp.Tools.FileShares;

public class FileSharesSetup : IAreaSetup
{
    public string Name => "fileshares";

    public string Title => "Azure File Shares";

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IFileSharesService, FileSharesService>();

        services.AddSingleton<FileShareGetCommand>();
        services.AddSingleton<FileShareCreateCommand>();
        services.AddSingleton<FileShareUpdateCommand>();
        services.AddSingleton<FileShareDeleteCommand>();
        services.AddSingleton<FileShareCheckNameAvailabilityCommand>();

        services.AddSingleton<SnapshotGetCommand>();
        services.AddSingleton<SnapshotCreateCommand>();
        services.AddSingleton<SnapshotUpdateCommand>();
        services.AddSingleton<SnapshotDeleteCommand>();

        services.AddSingleton<PrivateEndpointConnectionGetCommand>();
        services.AddSingleton<PrivateEndpointConnectionUpdateCommand>();

        services.AddSingleton<FileShareGetLimitsCommand>();
        services.AddSingleton<FileShareGetProvisioningRecommendationCommand>();
        services.AddSingleton<FileShareGetUsageDataCommand>();
    }

    public CommandGroup RegisterCommands(IServiceProvider serviceProvider)
    {
        var fileShares = new CommandGroup(Name, "File Shares operations - Commands for managing Azure File Shares.", Title);

        var fileShare = new CommandGroup("fileshare", "File share operations - Commands for managing file shares.");
        fileShares.AddSubGroup(fileShare);

        fileShare.AddCommand(serviceProvider.GetRequiredService<FileShareGetCommand>());
        fileShare.AddCommand(serviceProvider.GetRequiredService<FileShareCreateCommand>());
        fileShare.AddCommand(serviceProvider.GetRequiredService<FileShareUpdateCommand>());
        fileShare.AddCommand(serviceProvider.GetRequiredService<FileShareDeleteCommand>());
        fileShare.AddCommand(serviceProvider.GetRequiredService<FileShareCheckNameAvailabilityCommand>());

        var snapshot = new CommandGroup("snapshot", "File share snapshot operations - Commands for managing file share snapshots.");
        fileShare.AddSubGroup(snapshot);

        snapshot.AddCommand(serviceProvider.GetRequiredService<SnapshotGetCommand>());
        snapshot.AddCommand(serviceProvider.GetRequiredService<SnapshotCreateCommand>());
        snapshot.AddCommand(serviceProvider.GetRequiredService<SnapshotUpdateCommand>());
        snapshot.AddCommand(serviceProvider.GetRequiredService<SnapshotDeleteCommand>());

        var privateEndpoint = new CommandGroup("peconnection", "Private endpoint connection operations - Commands for managing private endpoint connections.");
        fileShare.AddSubGroup(privateEndpoint);

        privateEndpoint.AddCommand(serviceProvider.GetRequiredService<PrivateEndpointConnectionGetCommand>());
        privateEndpoint.AddCommand(serviceProvider.GetRequiredService<PrivateEndpointConnectionUpdateCommand>());

        // Register informational commands directly under fileshares
        fileShares.AddCommand(serviceProvider.GetRequiredService<FileShareGetLimitsCommand>());
        fileShares.AddCommand(serviceProvider.GetRequiredService<FileShareGetProvisioningRecommendationCommand>());
        fileShares.AddCommand(serviceProvider.GetRequiredService<FileShareGetUsageDataCommand>());

        return fileShares;
    }
}

