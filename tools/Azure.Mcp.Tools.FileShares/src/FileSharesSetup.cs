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

        var fileShareGet = serviceProvider.GetRequiredService<FileShareGetCommand>();
        fileShare.AddCommand(fileShareGet.Name, fileShareGet);

        var fileShareCreate = serviceProvider.GetRequiredService<FileShareCreateCommand>();
        fileShare.AddCommand(fileShareCreate.Name, fileShareCreate);

        var fileShareUpdate = serviceProvider.GetRequiredService<FileShareUpdateCommand>();
        fileShare.AddCommand(fileShareUpdate.Name, fileShareUpdate);

        var fileShareDelete = serviceProvider.GetRequiredService<FileShareDeleteCommand>();
        fileShare.AddCommand(fileShareDelete.Name, fileShareDelete);

        var checkName = serviceProvider.GetRequiredService<FileShareCheckNameAvailabilityCommand>();
        fileShare.AddCommand(checkName.Name, checkName);

        var snapshot = new CommandGroup("snapshot", "File share snapshot operations - Commands for managing file share snapshots.");
        fileShare.AddSubGroup(snapshot);

        var snapshotGet = serviceProvider.GetRequiredService<SnapshotGetCommand>();
        snapshot.AddCommand(snapshotGet.Name, snapshotGet);

        var snapshotCreate = serviceProvider.GetRequiredService<SnapshotCreateCommand>();
        snapshot.AddCommand(snapshotCreate.Name, snapshotCreate);

        var snapshotUpdate = serviceProvider.GetRequiredService<SnapshotUpdateCommand>();
        snapshot.AddCommand(snapshotUpdate.Name, snapshotUpdate);

        var snapshotDelete = serviceProvider.GetRequiredService<SnapshotDeleteCommand>();
        snapshot.AddCommand(snapshotDelete.Name, snapshotDelete);

        var privateEndpoint = new CommandGroup("privateendpointconnection", "Private endpoint connection operations - Commands for managing private endpoint connections.");
        fileShare.AddSubGroup(privateEndpoint);

        var privateEndpointGet = serviceProvider.GetRequiredService<PrivateEndpointConnectionGetCommand>();
        privateEndpoint.AddCommand(privateEndpointGet.Name, privateEndpointGet);

        var privateEndpointUpdate = serviceProvider.GetRequiredService<PrivateEndpointConnectionUpdateCommand>();
        privateEndpoint.AddCommand(privateEndpointUpdate.Name, privateEndpointUpdate);

        // Register informational commands directly under fileshares
        var limits = serviceProvider.GetRequiredService<FileShareGetLimitsCommand>();
        fileShares.AddCommand(limits.Name, limits);

        var recommendation = serviceProvider.GetRequiredService<FileShareGetProvisioningRecommendationCommand>();
        fileShares.AddCommand(recommendation.Name, recommendation);

        var usage = serviceProvider.GetRequiredService<FileShareGetUsageDataCommand>();
        fileShares.AddCommand(usage.Name, usage);

        return fileShares;
    }
}

