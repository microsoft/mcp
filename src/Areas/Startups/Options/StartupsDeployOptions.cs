// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using AzureMcp.Options;

namespace AzureMcp.Areas.Startups.Options;

public class StartupsDeployOptions : SubscriptionOptions
{
    public string? StorageAccount { get; set; }
    public string? SourcePath { get; set; }
    public string Container { get; set; } = "$web";
    public bool Overwrite { get; set; }

    public string? DeployType { get; set; }
    public string? ReactProject { get; set; }
    public bool Build { get; set; } = true;
    public string? BuildPath { get; set; }
}