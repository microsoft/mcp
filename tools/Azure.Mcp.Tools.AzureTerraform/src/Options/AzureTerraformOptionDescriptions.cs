// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.AzureTerraform.Options;

internal static class AzureTerraformOptionDescriptions
{
    internal const string ModuleName = "The name of the Azure Verified Module (e.g., avm-res-storage-storageaccount).";
    internal const string OutputFolder = "Output folder name for the generated Terraform files.";
    internal const string Provider = "Terraform provider to use: 'azurerm' (default) or 'azapi'.";
    internal const string NamePattern = "Pattern for naming resources in the generated Terraform configuration.";
    internal const string IncludeRoleAssignment = "Include role assignments in the export.";
    internal const string Parallelism = "Number of parallel operations (default: 10, max: 50).";
    internal const string ContinueOnError = "Continue export even if some resources fail (default: true).";
    internal const string PolicySet = "Policy set to use for validation: 'all' (default), 'Azure-Proactive-Resiliency-Library-v2', or 'avmsec'.";
    internal const string SeverityFilter = "Severity filter for avmsec policies: 'high', 'medium', 'low', or 'info'. Only applicable when policy-set is 'avmsec'.";
    internal const string CustomPolicies = "Comma-separated list of custom policy paths to include in validation.";
}
