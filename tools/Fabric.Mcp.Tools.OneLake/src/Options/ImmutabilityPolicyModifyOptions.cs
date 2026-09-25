// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Fabric.Mcp.Tools.OneLake.Options;

public sealed class ImmutabilityPolicyModifyOptions
{
    [Option(Description = OneLakeOptionDescriptions.WorkspaceId)]
    public string? WorkspaceId { get; set; }

    [Option(Description = OneLakeOptionDescriptions.Workspace)]
    public string? Workspace { get; set; }

    [Option(Description = "The scope of the immutability policy. Currently only 'DiagnosticLogs' is supported.")]
    public required string Scope { get; set; }

    [Option(Description = "Number of days to retain diagnostic logs (minimum 1). Cannot be reduced below the current value.")]
    public required int RetentionDays { get; set; }
}
