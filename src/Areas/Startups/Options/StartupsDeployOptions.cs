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
}