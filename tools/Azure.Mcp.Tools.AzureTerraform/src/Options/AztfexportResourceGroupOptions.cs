// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.AzureTerraform.Options;

public sealed class AztfexportResourceGroupOptions
{
    [Option(Description = "The name of the Azure resource group to export.")]
    public required string ResourceGroup { get; set; }

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
