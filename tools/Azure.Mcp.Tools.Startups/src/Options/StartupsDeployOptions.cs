// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Startups.Options;

public class StartupsDeployOptions : SubscriptionOptions
{
    public string? StorageAccount { get; set; }
    public string? SourcePath { get; set; }
    public string Container { get; set; } = "$web";
    public bool Overwrite { get; set; } = false;
}
