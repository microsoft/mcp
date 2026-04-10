// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.AzureTerraform.Options;

public static class AzureTerraformOptionDefinitions
{
    public const string ResourceTypeName = "resource-type";
    public const string DocTypeName = "doc-type";
    public const string ArgumentNameOption = "argument";
    public const string AttributeNameOption = "attribute";

    public const string AzApiResourceTypeName = "resource-type";
    public const string ApiVersionName = "api-version";

    public const string AvmModuleNameOption = "module-name";
    public const string AvmModuleVersionOption = "module-version";

    public const string ResourceIdOption = "resource-id";
    public const string ResourceGroupNameOption = "resource-group";
    public const string AzureResourceGraphQueryOption = "query";
    public const string OutputFolderNameOption = "output-folder";
    public const string TerraformProviderOption = "provider";
    public const string TerraformResourceNameOption = "terraform-resource-name";
    public const string NamePatternOption = "name-pattern";
    public const string IncludeRoleAssignmentOption = "include-role-assignment";
    public const string ParallelismOption = "parallelism";
    public const string ContinueOnErrorOption = "continue-on-error";

    public static readonly Option<string> ResourceType = new(
        $"--{ResourceTypeName}"
    )
    {
        Description = "The AzureRM Terraform resource type name (e.g., azurerm_resource_group, azurerm_storage_account). The 'azurerm_' prefix is optional.",
        Required = true
    };

    public static readonly Option<string> DocType = new(
        $"--{DocTypeName}"
    )
    {
        Description = "The documentation type to retrieve. Options: 'resource' (default), 'data-source'.",
        Required = false
    };

    public static readonly Option<string> ArgumentName = new(
        $"--{ArgumentNameOption}"
    )
    {
        Description = "Filter results to a specific argument name.",
        Required = false
    };

    public static readonly Option<string> AttributeName = new(
        $"--{AttributeNameOption}"
    )
    {
        Description = "Filter results to a specific attribute name.",
        Required = false
    };

    public static readonly Option<string> AzApiResourceType = new(
        $"--{AzApiResourceTypeName}"
    )
    {
        Description = "The Azure resource type in ARM format (e.g., Microsoft.Compute/virtualMachines, Microsoft.Storage/storageAccounts).",
        Required = true
    };

    public static readonly Option<string> ApiVersion = new(
        $"--{ApiVersionName}"
    )
    {
        Description = "The API version to use for the resource schema. If omitted, the latest stable version is used.",
        Required = false
    };

    public static readonly Option<string> AvmModuleName = new(
        $"--{AvmModuleNameOption}"
    )
    {
        Description = "The name of the Azure Verified Module (e.g., avm-res-storage-storageaccount).",
        Required = true
    };

    public static readonly Option<string> AvmModuleVersion = new(
        $"--{AvmModuleVersionOption}"
    )
    {
        Description = "The version of the Azure Verified Module (e.g., 0.4.0).",
        Required = true
    };

    public static readonly Option<string> ResourceId = new(
        $"--{ResourceIdOption}"
    )
    {
        Description = "The full Azure resource ID to export (e.g., /subscriptions/.../resourceGroups/.../providers/Microsoft.Storage/storageAccounts/mystorageaccount).",
        Required = true
    };

    public static readonly Option<string> ResourceGroupName = new(
        $"--{ResourceGroupNameOption}"
    )
    {
        Description = "The name of the Azure resource group to export.",
        Required = true
    };

    public static readonly Option<string> AzureResourceGraphQuery = new(
        $"--{AzureResourceGraphQueryOption}"
    )
    {
        Description = "Azure Resource Graph KQL WHERE clause to select resources for export (e.g., \"type =~ 'Microsoft.Storage/storageAccounts'\").",
        Required = true
    };

    public static readonly Option<string> OutputFolderName = new(
        $"--{OutputFolderNameOption}"
    )
    {
        Description = "Output folder name for the generated Terraform files.",
        Required = false
    };

    public static readonly Option<string> TerraformProvider = new(
        $"--{TerraformProviderOption}"
    )
    {
        Description = "Terraform provider to use: 'azurerm' (default) or 'azapi'.",
        Required = false
    };

    public static readonly Option<string> TerraformResourceName = new(
        $"--{TerraformResourceNameOption}"
    )
    {
        Description = "Custom resource name to use in the generated Terraform configuration.",
        Required = false
    };

    public static readonly Option<string> NamePattern = new(
        $"--{NamePatternOption}"
    )
    {
        Description = "Pattern for naming resources in the generated Terraform configuration.",
        Required = false
    };

    public static readonly Option<bool> IncludeRoleAssignment = new(
        $"--{IncludeRoleAssignmentOption}"
    )
    {
        Description = "Include role assignments in the export.",
        Required = false
    };

    public static readonly Option<int> Parallelism = new(
        $"--{ParallelismOption}"
    )
    {
        Description = "Number of parallel operations (default: 10, max: 50).",
        Required = false
    };

    public static readonly Option<bool> ContinueOnError = new(
        $"--{ContinueOnErrorOption}"
    )
    {
        Description = "Continue export even if some resources fail (default: true).",
        Required = false
    };

    public const string WorkspaceFolderOption = "workspace-folder";
    public const string PlanFolderOption = "plan-folder";
    public const string PolicySetOption = "policy-set";
    public const string SeverityFilterOption = "severity-filter";
    public const string CustomPoliciesOption = "custom-policies";

    public static readonly Option<string> WorkspaceFolder = new(
        $"--{WorkspaceFolderOption}"
    )
    {
        Description = "Path to the Terraform workspace folder containing .tf files to validate.",
        Required = true
    };

    public static readonly Option<string> PlanFolder = new(
        $"--{PlanFolderOption}"
    )
    {
        Description = "Path to the folder containing the Terraform plan JSON file (tfplan.json) to validate.",
        Required = true
    };

    public static readonly Option<string> PolicySet = new(
        $"--{PolicySetOption}"
    )
    {
        Description = "Policy set to use for validation: 'all' (default), 'Azure-Proactive-Resiliency-Library-v2', or 'avmsec'.",
        Required = false
    };

    public static readonly Option<string> SeverityFilter = new(
        $"--{SeverityFilterOption}"
    )
    {
        Description = "Severity filter for avmsec policies: 'high', 'medium', 'low', or 'info'. Only applicable when policy-set is 'avmsec'.",
        Required = false
    };

    public static readonly Option<string> CustomPolicies = new(
        $"--{CustomPoliciesOption}"
    )
    {
        Description = "Comma-separated list of custom policy paths to include in validation.",
        Required = false
    };
}
