// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.AzureBackup.Commands.Dr;
using Azure.Mcp.Tools.AzureBackup.Commands.Governance;
using Azure.Mcp.Tools.AzureBackup.Commands.Job;
using Azure.Mcp.Tools.AzureBackup.Commands.Policy;
using Azure.Mcp.Tools.AzureBackup.Commands.ProtectableItem;
using Azure.Mcp.Tools.AzureBackup.Commands.ProtectedItem;
using Azure.Mcp.Tools.AzureBackup.Commands.RecoveryPoint;
using Azure.Mcp.Tools.AzureBackup.Commands.Vault;
using Azure.Mcp.Tools.AzureBackup.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Areas;
using Microsoft.Mcp.Core.Commands;

namespace Azure.Mcp.Tools.AzureBackup;

public class AzureBackupSetup : IAreaSetup
{
    public string Name => "azurebackup";

    public string Title => "Manage Azure Backup";

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IRsvBackupOperations, RsvBackupOperations>();
        services.AddSingleton<IDppBackupOperations, DppBackupOperations>();
        services.AddSingleton<IAzureBackupService, AzureBackupService>();

        services.AddSingleton<VaultGetCommand>();
        services.AddSingleton<VaultCreateCommand>();
        services.AddSingleton<VaultUpdateCommand>();

        services.AddSingleton<PolicyGetCommand>();
        services.AddSingleton<PolicyCreateCommand>();

        services.AddSingleton<ProtectedItemGetCommand>();
        services.AddSingleton<ProtectedItemProtectCommand>();

        services.AddSingleton<ProtectableItemListCommand>();

        services.AddSingleton<JobGetCommand>();

        services.AddSingleton<RecoveryPointGetCommand>();

        services.AddSingleton<GovernanceFindUnprotectedCommand>();
        services.AddSingleton<GovernanceImmutabilityCommand>();
        services.AddSingleton<GovernanceSoftDeleteCommand>();

        services.AddSingleton<DrEnableCrrCommand>();
    }

    public CommandGroup RegisterCommands(IServiceProvider serviceProvider)
    {
        var azureBackup = new CommandGroup(Name,
            """
            Azure Backup operations – Unified commands to manage backup across Recovery Services vaults (RSV)
            and Backup vaults (DPP/Data Protection). Supports vault management, protected item operations,
            policy management, job monitoring, recovery point browsing, governance, and disaster recovery.
            Use --vault-type to specify vault type or let the system auto-detect.
            """,
            Title);

        var vault = new CommandGroup("vault", "Backup vault operations – Get vault details or list all vaults, create, and update vaults.");
        azureBackup.AddSubGroup(vault);
        RegisterCommand<VaultGetCommand>(serviceProvider, vault);
        RegisterCommand<VaultCreateCommand>(serviceProvider, vault);
        RegisterCommand<VaultUpdateCommand>(serviceProvider, vault);

        var policy = new CommandGroup("policy", "Backup policy operations – Get policy details or list all policies, and create policies.");
        azureBackup.AddSubGroup(policy);
        RegisterCommand<PolicyGetCommand>(serviceProvider, policy);
        RegisterCommand<PolicyCreateCommand>(serviceProvider, policy);

        var protectedItem = new CommandGroup("protecteditem", "Protected item operations – Get protected item details or list all, and enable backup protection.");
        azureBackup.AddSubGroup(protectedItem);
        RegisterCommand<ProtectedItemGetCommand>(serviceProvider, protectedItem);
        RegisterCommand<ProtectedItemProtectCommand>(serviceProvider, protectedItem);

        var protectableItem = new CommandGroup("protectableitem", "Protectable item operations – List discovered databases available for protection.");
        azureBackup.AddSubGroup(protectableItem);
        RegisterCommand<ProtectableItemListCommand>(serviceProvider, protectableItem);

        var job = new CommandGroup("job", "Backup job operations – Get job details or list all jobs in a vault.");
        azureBackup.AddSubGroup(job);
        RegisterCommand<JobGetCommand>(serviceProvider, job);

        var recoveryPoint = new CommandGroup("recoverypoint", "Recovery point operations – Get recovery point details or list all for a protected item.");
        azureBackup.AddSubGroup(recoveryPoint);
        RegisterCommand<RecoveryPointGetCommand>(serviceProvider, recoveryPoint);

        var governance = new CommandGroup("governance", "Governance operations – Find unprotected resources, configure immutability and soft delete.");
        azureBackup.AddSubGroup(governance);
        RegisterCommand<GovernanceFindUnprotectedCommand>(serviceProvider, governance);
        RegisterCommand<GovernanceImmutabilityCommand>(serviceProvider, governance);
        RegisterCommand<GovernanceSoftDeleteCommand>(serviceProvider, governance);

        var dr = new CommandGroup("dr", "Disaster recovery operations – Enable Cross-Region Restore on a GRS vault.");
        azureBackup.AddSubGroup(dr);
        RegisterCommand<DrEnableCrrCommand>(serviceProvider, dr);

        return azureBackup;
    }

    private static void RegisterCommand<T>(IServiceProvider serviceProvider, CommandGroup group) where T : IBaseCommand
    {
        var cmd = serviceProvider.GetRequiredService<T>();
        group.AddCommand(cmd.Name, cmd);
    }
}
