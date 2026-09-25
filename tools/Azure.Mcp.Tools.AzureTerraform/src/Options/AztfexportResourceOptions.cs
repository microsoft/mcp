// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.AzureTerraform.Options;

public sealed class AztfexportResourceOptions
{
    [Option(Description = "The full Azure resource ID to export (e.g., /subscriptions/.../resourceGroups/.../providers/Microsoft.Storage/storageAccounts/mystorageaccount).")]
    public required string ResourceId { get; set; }

    [Option(Description = AzureTerraformOptionDescriptions.OutputFolder)]
    public string? OutputFolder { get; set; }

    [Option(Description = AzureTerraformOptionDescriptions.Provider)]
    public string? Provider { get; set; }

    [Option(Description = "Custom resource name to use in the generated Terraform configuration.")]
    public string? TerraformResourceName { get; set; }

    [Option(Description = AzureTerraformOptionDescriptions.IncludeRoleAssignment)]
    public bool IncludeRoleAssignment { get; set; }

    [Option(Description = AzureTerraformOptionDescriptions.Parallelism, DefaultValue = 10)]
    public int Parallelism { get; set; }

    [Option(Description = AzureTerraformOptionDescriptions.ContinueOnError, DefaultValue = true)]
    public bool ContinueOnError { get; set; }
}
