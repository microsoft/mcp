// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
namespace AzureMcp.Areas.Startups.Options;

public sealed class StartupsDeployOptions : AzureMcp.Options.SubscriptionOptions
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