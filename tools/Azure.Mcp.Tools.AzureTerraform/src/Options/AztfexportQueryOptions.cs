// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.AzureTerraform.Options;

public sealed class AztfexportQueryOptions
{
    [Option(Description = "Azure Resource Graph KQL WHERE clause to select resources for export (e.g., \"type =~ 'Microsoft.Storage/storageAccounts'\").")]
    public required string Query { get; set; }

    [Option(Description = AzureTerraformOptionDescriptions.OutputFolder)]
    public string? OutputFolder { get; set; }

    [Option(Description = AzureTerraformOptionDescriptions.Provider)]
    public string? Provider { get; set; }

    [Option(Description = AzureTerraformOptionDescriptions.NamePattern)]
    public string? NamePattern { get; set; }

    [Option(Description = AzureTerraformOptionDescriptions.IncludeRoleAssignment)]
    public bool IncludeRoleAssignment { get; set; }

    [Option(Description = AzureTerraformOptionDescriptions.Parallelism, DefaultValue = 10)]
    public int Parallelism { get; set; }

    [Option(Description = AzureTerraformOptionDescriptions.ContinueOnError, DefaultValue = true)]
    public bool ContinueOnError { get; set; }
}
