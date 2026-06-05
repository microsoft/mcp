// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Fabric.Mcp.Tools.OneLake.Options;

public sealed class ImmutabilityPolicyModifyOptions : GlobalOptions
{
    public string? WorkspaceId { get; set; }
    public string? Scope { get; set; }
    public int? RetentionDays { get; set; }
}

