// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Fabric.Mcp.Tools.OneLake.Options;

public sealed class WorkspaceListOptions
{
    [Option(Description = "Token for retrieving the next page of results.")]
    public string? ContinuationToken { get; set; }

    [Option(Description = "Output format for OneLake API responses. Use 'json' for parsed objects, 'xml' for raw XML API response, or 'raw' for unprocessed API response. Supported values: 'json' (default), 'xml', 'raw'.")]
    public string? Format { get; set; }
}
