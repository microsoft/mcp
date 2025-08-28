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

    public ValidationResult ValidateRootSquashOptions(CommandResult commandResult, CommandResponse? commandResponse = null)
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
                if (commandResponse is not null)
                {
                    commandResponse.Status = HttpStatusCode.BadRequest;
                    commandResponse.Message = "When --root-squash-mode is not 'None', --squash-uid, --squash-gid and --no-squash-nid-list must be provided.";
                }
                return new ValidationResult { IsValid = false, ErrorMessage = commandResponse?.Message };
            }
        }

        return new ValidationResult { IsValid = true };

    }

    public ValidationResult ValidateMaintanenceOptionsCreate(CommandResult commandResult, CommandResponse? commandResponse)
    {
        return ValidateMaintenanceOptions(commandResult, commandResponse, false);
    }

    public ValidationResult ValidateMaintanenceOptionsUpdate(CommandResult commandResult, CommandResponse? commandResponse)
    {
        return ValidateMaintenanceOptions(commandResult, commandResponse, true);
    }
    public ValidationResult ValidateMaintenanceOptions(CommandResult commandResult, CommandResponse? commandResponse = null, bool update = false)
    {
        // Read values from the same option instances used during registration
        var maintenanceDay = update ? commandResult.GetValueOrDefault(AzureManagedLustreOptionDefinitions.OptionalMaintenanceDayOption) : commandResult.GetValueOrDefault(AzureManagedLustreOptionDefinitions.MaintenanceDayOption);
        var maintenanceTime = update ? commandResult.GetValueOrDefault(AzureManagedLustreOptionDefinitions.OptionalMaintenanceTimeOption) : commandResult.GetValueOrDefault(AzureManagedLustreOptionDefinitions.MaintenanceTimeOption);
        var updateWithoutMaintenance = string.IsNullOrWhiteSpace(maintenanceDay) && string.IsNullOrWhiteSpace(maintenanceTime) && update;

        if ((string.IsNullOrWhiteSpace(maintenanceDay) || string.IsNullOrWhiteSpace(maintenanceTime)) && !updateWithoutMaintenance)
        {
            if (commandResponse is not null)
            {
                commandResponse.Status = HttpStatusCode.BadRequest;
                commandResponse.Message = "When updating maintenance window, both --maintenance-day and --maintenance-time must be specified.";
            }
            return new ValidationResult { IsValid = false, ErrorMessage = commandResponse?.Message };
        }

        return new ValidationResult { IsValid = true };

    }



    public ValidationResult ValidateHSMOptions(CommandResult commandResult, CommandResponse? commandResponse = null)
    {
        // Read values from the same option instances used during registration
        var hsmContainer = commandResult.GetValueOrDefault(AzureManagedLustreOptionDefinitions.HsmContainerOption);
        var hsmLogContainer = commandResult.GetValueOrDefault(AzureManagedLustreOptionDefinitions.HsmLogContainerOption);
        var hsmEnabled = !string.IsNullOrWhiteSpace(hsmContainer) || !string.IsNullOrWhiteSpace(hsmLogContainer);


        // Always require both values if one is specified.
        if (hsmEnabled && (string.IsNullOrWhiteSpace(hsmContainer) || string.IsNullOrWhiteSpace(hsmLogContainer)))
        {
            if (commandResponse is not null)
            {
                commandResponse.Status = HttpStatusCode.BadRequest;
                commandResponse.Message = "When enabling Azure Blob Integration both data container and log container must be specified.";
            }
            return new ValidationResult { IsValid = false, ErrorMessage = commandResponse?.Message };
        }

        return new ValidationResult { IsValid = true };

    }

    public ValidationResult ValidateEncryptionOptions(CommandResult commandResult, CommandResponse? commandResponse = null)
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

                if (commandResponse is not null)
                {
                    commandResponse.Status = HttpStatusCode.BadRequest;
                    commandResponse.Message = "Missing Required options: key-url, source-vault, user-assigned-identity when custom-encryption is set";
                }
                return new ValidationResult { IsValid = false, ErrorMessage = commandResponse?.Message };
            }
        }

        return new ValidationResult { IsValid = true };

    }
}
