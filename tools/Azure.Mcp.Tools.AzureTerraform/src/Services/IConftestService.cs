// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.AzureTerraform.Models;

namespace Azure.Mcp.Tools.AzureTerraform.Services;

public interface IConftestService
{
    Task<bool> IsConftestAvailableAsync(CancellationToken cancellationToken = default);

    ConftestCommandResult GenerateWorkspaceValidationCommand(
        string workspaceFolder,
        string policySet = "all",
        string? severityFilter = null,
        string? customPolicies = null);

    ConftestCommandResult GeneratePlanValidationCommand(
        string planFolder,
        string policySet = "all",
        string? severityFilter = null,
        string? customPolicies = null);

    static InstallationHelp GetInstallationHelp() => ConftestService.GetConftestInstallationHelp();
}
