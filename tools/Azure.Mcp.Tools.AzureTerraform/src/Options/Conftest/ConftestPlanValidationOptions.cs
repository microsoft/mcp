// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.AzureTerraform.Options;

public sealed class ConftestPlanValidationOptions
{
    public string? PlanFolder { get; set; }
    public string? PolicySet { get; set; }
    public string? SeverityFilter { get; set; }
    public string? CustomPolicies { get; set; }
}
