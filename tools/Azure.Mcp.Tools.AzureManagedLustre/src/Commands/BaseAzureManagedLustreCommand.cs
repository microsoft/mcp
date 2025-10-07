// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine.Parsing;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Tools.AzureManagedLustre.Options;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.AzureManagedLustre.Commands;

public abstract class BaseAzureManagedLustreCommand<
    [DynamicallyAccessedMembers(TrimAnnotations.CommandAnnotations)] TOptions>(ILogger<BaseAzureManagedLustreCommand<TOptions>> logger)
    : SubscriptionCommand<TOptions> where TOptions : BaseAzureManagedLustreOptions, new()
{
    // Currently no additional options beyond subscription + resource group
    protected readonly ILogger<BaseAzureManagedLustreCommand<TOptions>> _logger = logger;

    public void ValidateRootSquashOptions(CommandResult commandResult)
    {
        var rootSquashMode = commandResult.GetValueOrDefault(AzureManagedLustreOptionDefinitions.RootSquashModeOption);
        var noSquashNidLists = commandResult.GetValueOrDefault(AzureManagedLustreOptionDefinitions.NoSquashNidListsOption);
        var squashUid = commandResult.GetValueOrDefault(AzureManagedLustreOptionDefinitions.SquashUidOption);
        var squashGid = commandResult.GetValueOrDefault(AzureManagedLustreOptionDefinitions.SquashGidOption);


        // If root squash mode is provided and not 'none', require UID, GID and no squash NID list
        if (!string.IsNullOrWhiteSpace(rootSquashMode) && !rootSquashMode.Equals("None", StringComparison.OrdinalIgnoreCase))
        {
            if (!(squashUid.HasValue && squashGid.HasValue && !string.IsNullOrWhiteSpace(noSquashNidLists)))
            {
                commandResult.AddError("When --root-squash-mode is not 'None', --squash-uid, --squash-gid and --no-squash-nid-list must be provided.");
            }
        }
    }

    public void ValidateMaintanenceOptionsCreate(CommandResult commandResult)
    {
        ValidateMaintenanceOptions(commandResult, false);
    }

    public void ValidateMaintanenceOptionsUpdate(CommandResult commandResult)
    {
        ValidateMaintenanceOptions(commandResult, true);
    }

    public void ValidateMaintenanceOptions(CommandResult commandResult, bool update = false)
    {
        // Read values from the same option instances used during registration
        var maintenanceDay = update ? commandResult.GetValueOrDefault(AzureManagedLustreOptionDefinitions.OptionalMaintenanceDayOption) : commandResult.GetValueOrDefault(AzureManagedLustreOptionDefinitions.MaintenanceDayOption);
        var maintenanceTime = update ? commandResult.GetValueOrDefault(AzureManagedLustreOptionDefinitions.OptionalMaintenanceTimeOption) : commandResult.GetValueOrDefault(AzureManagedLustreOptionDefinitions.MaintenanceTimeOption);
        var updateWithoutMaintenance = string.IsNullOrWhiteSpace(maintenanceDay) && string.IsNullOrWhiteSpace(maintenanceTime) && update;

        if ((string.IsNullOrWhiteSpace(maintenanceDay) || string.IsNullOrWhiteSpace(maintenanceTime)) && !updateWithoutMaintenance)
        {
            commandResult.AddError("When updating maintenance window, both --maintenance-day and --maintenance-time must be specified.");
        }
    }

    public void ValidateHSMOptions(CommandResult commandResult)
    {
        // Read values from the same option instances used during registration
        var hsmContainer = commandResult.GetValueOrDefault(AzureManagedLustreOptionDefinitions.HsmContainerOption);
        var hsmLogContainer = commandResult.GetValueOrDefault(AzureManagedLustreOptionDefinitions.HsmLogContainerOption);
        var hsmEnabled = !string.IsNullOrWhiteSpace(hsmContainer) || !string.IsNullOrWhiteSpace(hsmLogContainer);


        // Always require both values if one is specified.
        if (hsmEnabled && (string.IsNullOrWhiteSpace(hsmContainer) || string.IsNullOrWhiteSpace(hsmLogContainer)))
        {
            commandResult.AddError("When enabling Azure Blob Integration both data container and log container must be specified.");
        }
    }

    public void ValidateEncryptionOptions(CommandResult commandResult)
    {
        // Read values from the same option instances used during registration
        var encryptionEnabled = commandResult.GetValueOrDefault(AzureManagedLustreOptionDefinitions.CustomEncryptionOption);
        var keyUrl = commandResult.GetValueOrDefault(AzureManagedLustreOptionDefinitions.KeyUrlOption);
        var sourceVault = commandResult.GetValueOrDefault(AzureManagedLustreOptionDefinitions.SourceVaultOption);
        var userAssignedIdentityId = commandResult.GetValueOrDefault(AzureManagedLustreOptionDefinitions.UserAssignedIdentityIdOption);

        if (encryptionEnabled == true)
        {
            if (string.IsNullOrWhiteSpace(keyUrl) || string.IsNullOrWhiteSpace(sourceVault) || string.IsNullOrWhiteSpace(userAssignedIdentityId))
            {
                commandResult.AddError("Missing Required options: key-url, source-vault, user-assigned-identity when custom-encryption is set");
                ;
            }
        }
    }

    public void ValidateHasUpdateOptions(CommandResult commandResult)
    {
        var maintenanceDay = commandResult.GetValueOrDefault(AzureManagedLustreOptionDefinitions.OptionalMaintenanceDayOption);
        var maintenanceTime = commandResult.GetValueOrDefault(AzureManagedLustreOptionDefinitions.OptionalMaintenanceTimeOption);
        var rootSquashMode = commandResult.GetValueOrDefault(AzureManagedLustreOptionDefinitions.RootSquashModeOption);

        if (string.IsNullOrWhiteSpace(maintenanceDay) &&
            string.IsNullOrWhiteSpace(maintenanceTime) &&
            string.IsNullOrWhiteSpace(rootSquashMode)
            )
        {
            commandResult.AddError("At least one of maintenance-day/time or root-squash fields must be provided.");
        }
    }
}
