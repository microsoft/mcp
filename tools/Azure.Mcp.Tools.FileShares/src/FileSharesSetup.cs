// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.FileShares.Commands.FileShare;
using Azure.Mcp.Tools.FileShares.Commands.Informational;
using Azure.Mcp.Tools.FileShares.Commands.PrivateEndpointConnection;
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

        services.AddSingleton<FileShareListCommand>();
        services.AddSingleton<FileShareGetCommand>();
        services.AddSingleton<FileShareCreateOrUpdateCommand>();
        services.AddSingleton<FileShareDeleteCommand>();
        services.AddSingleton<FileShareCheckNameAvailabilityCommand>();

        services.AddSingleton<FileShareSnapshotListCommand>();
        services.AddSingleton<FileShareSnapshotGetCommand>();
        services.AddSingleton<FileShareSnapshotCreateCommand>();

        services.AddSingleton<FileShareGetLimitsCommand>();
        services.AddSingleton<FileShareGetProvisioningRecommendationCommand>();
        services.AddSingleton<FileShareGetUsageDataCommand>();

        services.AddSingleton<PrivateEndpointConnectionListCommand>();
        services.AddSingleton<PrivateEndpointConnectionGetCommand>();
        services.AddSingleton<PrivateEndpointConnectionUpdateCommand>();
        services.AddSingleton<PrivateEndpointConnectionDeleteCommand>();
    }

    public CommandGroup RegisterCommands(IServiceProvider serviceProvider)
    {
        var fileShares = new CommandGroup(Name, "File Shares operations - Commands for managing Azure File Shares.", Title);

        var fileShare = new CommandGroup("fileshare", "File share operations - Commands for managing file shares.");
        fileShares.AddSubGroup(fileShare);

        var fileShareList = serviceProvider.GetRequiredService<FileShareListCommand>();
        fileShare.AddCommand(fileShareList.Name, fileShareList);

        var fileShareGet = serviceProvider.GetRequiredService<FileShareGetCommand>();
        fileShare.AddCommand(fileShareGet.Name, fileShareGet);

        var fileShareCreate = serviceProvider.GetRequiredService<FileShareCreateOrUpdateCommand>();
        fileShare.AddCommand(fileShareCreate.Name, fileShareCreate);

        var fileShareDelete = serviceProvider.GetRequiredService<FileShareDeleteCommand>();
        fileShare.AddCommand(fileShareDelete.Name, fileShareDelete);

        var checkName = serviceProvider.GetRequiredService<FileShareCheckNameAvailabilityCommand>();
        fileShare.AddCommand(checkName.Name, checkName);

        var snapshot = new CommandGroup("snapshot", "File share snapshot operations - Commands for managing file share snapshots.");
        fileShare.AddSubGroup(snapshot);

        var snapshotList = serviceProvider.GetRequiredService<FileShareSnapshotListCommand>();
        snapshot.AddCommand(snapshotList.Name, snapshotList);

        var snapshotGet = serviceProvider.GetRequiredService<FileShareSnapshotGetCommand>();
        snapshot.AddCommand(snapshotGet.Name, snapshotGet);

        var snapshotCreate = serviceProvider.GetRequiredService<FileShareSnapshotCreateCommand>();
        snapshot.AddCommand(snapshotCreate.Name, snapshotCreate);

        // Register private endpoint connection commands
        var pecGroup = new CommandGroup("privateendpointconnection", "Private endpoint connection operations - Commands for managing private endpoint connections.");
        fileShares.AddSubGroup(pecGroup);

        var pecList = serviceProvider.GetRequiredService<PrivateEndpointConnectionListCommand>();
        pecGroup.AddCommand(pecList.Name, pecList);

        var pecGet = serviceProvider.GetRequiredService<PrivateEndpointConnectionGetCommand>();
        pecGroup.AddCommand(pecGet.Name, pecGet);

        var pecUpdate = serviceProvider.GetRequiredService<PrivateEndpointConnectionUpdateCommand>();
        pecGroup.AddCommand(pecUpdate.Name, pecUpdate);

        var pecDelete = serviceProvider.GetRequiredService<PrivateEndpointConnectionDeleteCommand>();
        pecGroup.AddCommand(pecDelete.Name, pecDelete);

        // Register informational commands
        var limits = serviceProvider.GetRequiredService<FileShareGetLimitsCommand>();
        fileShares.AddCommand(limits.Name, limits);

        var recommendation = serviceProvider.GetRequiredService<FileShareGetProvisioningRecommendationCommand>();
        fileShares.AddCommand(recommendation.Name, recommendation);

        var usage = serviceProvider.GetRequiredService<FileShareGetUsageDataCommand>();
        fileShares.AddCommand(usage.Name, usage);

        return fileShares;
    }
}

