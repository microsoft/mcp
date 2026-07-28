// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Fabric.Mcp.Tools.OneLake.Options;

public sealed class WorkspaceListOptions
{
    [Option(Description = OneLakeOptionDescriptions.ContinuationToken)]
    public string? ContinuationToken { get; set; }

    [Option(Description = OneLakeOptionDescriptions.Format)]
    public string? Format { get; set; }
}
