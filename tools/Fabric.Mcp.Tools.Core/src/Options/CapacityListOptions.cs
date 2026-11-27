// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Fabric.Mcp.Tools.Core.Options;

public sealed class CapacityListOptions()
{
    [Option(Description = "The continuation token returned by a previous capacity listing. Pass it unchanged to retrieve the next page, or omit it for the first page.")]
    public string? ContinuationToken { get; set; }
}
