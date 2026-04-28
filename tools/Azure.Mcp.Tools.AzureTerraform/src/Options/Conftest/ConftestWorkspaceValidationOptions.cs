// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.AzureTerraform.Options;

public sealed class ConftestWorkspaceValidationOptions
{
    public string? WorkspaceFolder { get; set; }
    public string? PolicySet { get; set; }
    public string? SeverityFilter { get; set; }
    public string? CustomPolicies { get; set; }
}
