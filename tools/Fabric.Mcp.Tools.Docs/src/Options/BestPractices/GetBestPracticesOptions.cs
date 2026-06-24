// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Fabric.Mcp.Tools.Docs.Options.BestPractices;

public class GetBestPracticesOptions
{
    [Option(Description = "The best practice topic to retrieve documentation for.")]
    public required string Topic { get; set; }
}
