// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.AzureTerraform.Models;

namespace Azure.Mcp.Tools.AzureTerraform.Services;

public interface IAztfexportService
{
    Task<bool> IsAztfexportAvailableAsync(CancellationToken cancellationToken = default);

    AztfexportCommandResult GenerateResourceCommand(
        string resourceId,
        string? outputFolderName = null,
        string provider = "azurerm",
        string? resourceName = null,
        bool includeRoleAssignment = false,
        int parallelism = 10,
        bool continueOnError = true);

    AztfexportCommandResult GenerateResourceGroupCommand(
        string resourceGroupName,
        string? outputFolderName = null,
        string provider = "azurerm",
        string? namePattern = null,
        bool includeRoleAssignment = false,
        int parallelism = 10,
        bool continueOnError = true);

    AztfexportCommandResult GenerateQueryCommand(
        string query,
        string? outputFolderName = null,
        string provider = "azurerm",
        string? namePattern = null,
        bool includeRoleAssignment = false,
        int parallelism = 10,
        bool continueOnError = true);

    static InstallationHelp GetInstallationHelp() => AztfexportService.GetAztfexportInstallationHelp();
}
