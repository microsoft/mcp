// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Tools.AzureManagedLustre.Options;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.AzureManagedLustre.Commands;

public abstract class BaseAzureManagedLustreCommand<
    [DynamicallyAccessedMembers(TrimAnnotations.CommandAnnotations)] TOptions>(ILogger<BaseAzureManagedLustreCommand<TOptions>> logger)
    : SubscriptionCommand<TOptions> where TOptions : BaseAzureManagedLustreOptions, new()
{
    // Currently no additional options beyond subscription + resource group
    protected readonly ILogger<BaseAzureManagedLustreCommand<TOptions>> _logger = logger;

    public virtual ValidationResult ValidateRootSquashOptions(CommandResult commandResult, CommandResponse? commandResponse = null)
    {
        var rootSquashMode = commandResult.GetValueForOption(AzureManagedLustreOptionDefinitions.RootSquashModeOption);
        var noSquashNidLists = commandResult.GetValueForOption(AzureManagedLustreOptionDefinitions.NoSquashNidListsOption);
        var squashUid = commandResult.GetValueForOption(AzureManagedLustreOptionDefinitions.SquashUidOption);
        var squashGid = commandResult.GetValueForOption(AzureManagedLustreOptionDefinitions.SquashGidOption);


        // If root squash mode is provided and not 'none', require UID, GID and no squash NID list
        if (!string.IsNullOrWhiteSpace(rootSquashMode) && !rootSquashMode.Equals("None", StringComparison.OrdinalIgnoreCase))
        {
            if (!(squashUid.HasValue && squashGid.HasValue && !string.IsNullOrWhiteSpace(noSquashNidLists)))
            {
                if (commandResponse is not null)
                {
                    commandResponse.Status = 400;
                    commandResponse.Message = "When --root-squash-mode is not 'None', --squash-uid, --squash-gid and --no-squash-nid-list must be provided.";
                }
                return new ValidationResult { IsValid = false, ErrorMessage = commandResponse?.Message };
            }
        }

        return new ValidationResult { IsValid = true };

    }

    public virtual ValidationResult ValidateMaintenanceOptions(CommandResult commandResult, CommandResponse? commandResponse = null, bool update = false)
    {
        // Read values from the same option instances used during registration
        var maintenanceDay = update ? commandResult.GetValueForOption(AzureManagedLustreOptionDefinitions.OptionalMaintenanceDayOption) : commandResult.GetValueForOption(AzureManagedLustreOptionDefinitions.MaintenanceDayOption);
        var maintenanceTime = update ? commandResult.GetValueForOption(AzureManagedLustreOptionDefinitions.OptionalMaintenanceTimeOption) : commandResult.GetValueForOption(AzureManagedLustreOptionDefinitions.MaintenanceTimeOption);
        var updateWithoutMaintenance = string.IsNullOrWhiteSpace(maintenanceDay) && string.IsNullOrWhiteSpace(maintenanceTime) && update;

        if ((string.IsNullOrWhiteSpace(maintenanceDay) || string.IsNullOrWhiteSpace(maintenanceTime)) && !updateWithoutMaintenance)
        {
            if (commandResponse is not null)
            {
                commandResponse.Status = 400;
                commandResponse.Message = "When updating maintenance window, both --maintenance-day and --maintenance-time must be specified.";
            }
            return new ValidationResult { IsValid = false, ErrorMessage = commandResponse?.Message };
        }

        return new ValidationResult { IsValid = true };

    }

    public virtual ValidationResult ValidateHSMOptions(CommandResult commandResult, CommandResponse? commandResponse = null)
    {
        // Read values from the same option instances used during registration
        var hsmContainer = commandResult.GetValueForOption(AzureManagedLustreOptionDefinitions.HsmContainerOption);
        var hsmLogContainer = commandResult.GetValueForOption(AzureManagedLustreOptionDefinitions.HsmLogContainerOption);
        var hsmEnabled = !string.IsNullOrWhiteSpace(hsmContainer) || !string.IsNullOrWhiteSpace(hsmLogContainer);


        // Always require both values if one is specified.
        if (hsmEnabled && (string.IsNullOrWhiteSpace(hsmContainer) || string.IsNullOrWhiteSpace(hsmLogContainer)))
        {
            if (commandResponse is not null)
            {
                commandResponse.Status = 400;
                commandResponse.Message = "When enabling Azure Blob Integration both data container and log container must be specified.";
            }
            return new ValidationResult { IsValid = false, ErrorMessage = commandResponse?.Message };
        }

        return new ValidationResult { IsValid = true };

    }

    public virtual ValidationResult ValidateEncryptionOptions(CommandResult commandResult, CommandResponse? commandResponse = null)
    {
        // Read values from the same option instances used during registration
        var encryptionEnabled = commandResult.GetValueForOption(AzureManagedLustreOptionDefinitions.CustomEncryptionOption);
        var keyUrl = commandResult.GetValueForOption(AzureManagedLustreOptionDefinitions.KeyUrlOption);
        var sourceVault = commandResult.GetValueForOption(AzureManagedLustreOptionDefinitions.SourceVaultOption);
        var userAssignedIdentityId = commandResult.GetValueForOption(AzureManagedLustreOptionDefinitions.UserAssignedIdentityIdOption);

        if (encryptionEnabled == true)
        {
            if (string.IsNullOrWhiteSpace(keyUrl) || string.IsNullOrWhiteSpace(sourceVault) || string.IsNullOrWhiteSpace(userAssignedIdentityId))
            {

                if (commandResponse is not null)
                {
                    commandResponse.Status = 400;
                    commandResponse.Message = "Missing Required options: key-url, source-vault, user-assigned-identity when custom-encryption is set";
                }
                return new ValidationResult { IsValid = false, ErrorMessage = commandResponse?.Message };
            }
        }

        return new ValidationResult { IsValid = true };

    }
}
