// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using AzureMcp.Options;

namespace AzureMcp.Areas.Startups.Options;

public class StartupsDeployOptions : SubscriptionOptions
{
    public new string? ResourceGroup { get; set; } = string.Empty;
    public string StorageAccount { get; set; } = string.Empty;
    public string SourcePath { get; set; } = string.Empty;
    public string Container { get; set; } = "$web";
    public string SubscriptionId { get; set; } = string.Empty;
    public new string? Tenant 
    { 
        get => base.Tenant;
        set => base.Tenant = value;
    }
}