// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace AzureMcp.Areas.Startups.Options;

public sealed class StartupsDeployOptions : AzureMcp.Options.GlobalOptions
{
    // Match base class but provide default value
    public new string? ResourceGroup { get; set; }
    
    // Required input parameters without null! 
    public string StorageAccount { get; set; } = string.Empty;
    public string SourcePath { get; set; } = string.Empty;
    public string Container { get; set; } = "$web";
    public string Subscription { get; set; } = string.Empty;

    // Ensure we're using the base class Tenant property
    public new string? Tenant 
    { 
        get => base.Tenant;
        set => base.Tenant = value;
    }
}