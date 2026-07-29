// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.AzureTerraform.Options.Conftest;

public sealed class ConftestPlanValidationOptions
{
    [Option(Description = "Path to the folder containing the Terraform plan JSON file (tfplan.json) to validate.")]
    public required string PlanFolder { get; set; }

    [Option(Description = AzureTerraformOptionDescriptions.PolicySet)]
    public string? PolicySet { get; set; }

    [Option(Description = AzureTerraformOptionDescriptions.SeverityFilter)]
    public string? SeverityFilter { get; set; }

    [Option(Description = AzureTerraformOptionDescriptions.CustomPolicies)]
    public string? CustomPolicies { get; set; }
}
