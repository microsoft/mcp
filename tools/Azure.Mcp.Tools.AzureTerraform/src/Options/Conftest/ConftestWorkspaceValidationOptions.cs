// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.AzureTerraform.Options.Conftest;

public sealed class ConftestWorkspaceValidationOptions
{
    [Option(Description = "Path to the Terraform workspace folder containing .tf files to validate.")]
    public required string WorkspaceFolder { get; set; }

    [Option(Description = AzureTerraformOptionDescriptions.PolicySet)]
    public string? PolicySet { get; set; }

    [Option(Description = AzureTerraformOptionDescriptions.SeverityFilter)]
    public string? SeverityFilter { get; set; }

    [Option(Description = AzureTerraformOptionDescriptions.CustomPolicies)]
    public string? CustomPolicies { get; set; }
}
