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

        services.AddSingleton<FileShareGetLimitsCommand>();
        services.AddSingleton<FileShareGetProvisioningRecommendationCommand>();
        services.AddSingleton<FileShareGetUsageDataCommand>();

        services.AddSingleton<PrivateEndpointConnectionGetCommand>();
        services.AddSingleton<PrivateEndpointConnectionUpdateCommand>();
        services.AddSingleton<PrivateEndpointConnectionDeleteCommand>();
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

        // Register private endpoint connection commands
        var pecGroup = new CommandGroup("privateendpointconnection", "Private endpoint connection operations - Commands for managing private endpoint connections.");
        fileShares.AddSubGroup(pecGroup);

        var pecGet = serviceProvider.GetRequiredService<PrivateEndpointConnectionGetCommand>();
        pecGroup.AddCommand(pecGet.Name, pecGet);

        var pecUpdate = serviceProvider.GetRequiredService<PrivateEndpointConnectionUpdateCommand>();
        pecGroup.AddCommand(pecUpdate.Name, pecUpdate);

        var pecDelete = serviceProvider.GetRequiredService<PrivateEndpointConnectionDeleteCommand>();
        pecGroup.AddCommand(pecDelete.Name, pecDelete);

        // Register informational commands in a subgroup
        var infoGroup = new CommandGroup("info", "Informational operations - Commands for retrieving file share limits, usage data, and provisioning recommendations.");
        fileShare.AddSubGroup(infoGroup);

        var limits = serviceProvider.GetRequiredService<FileShareGetLimitsCommand>();
        infoGroup.AddCommand(limits.Name, limits);

        var recommendation = serviceProvider.GetRequiredService<FileShareGetProvisioningRecommendationCommand>();
        infoGroup.AddCommand(recommendation.Name, recommendation);

        var usage = serviceProvider.GetRequiredService<FileShareGetUsageDataCommand>();
        infoGroup.AddCommand(usage.Name, usage);

        return fileShares;
    }
}

