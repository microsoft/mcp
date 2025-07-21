// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using AzureMcp.Models.Option;

namespace AzureMcp.Areas.Startups.Options;

public sealed class StartupsDeployOptions : AzureMcp.Options.GlobalOptions
{
    public new string ResourceGroup { get; set; } = string.Empty;
    public string StorageAccount { get; set; } = string.Empty;
    public string SourcePath { get; set; } = string.Empty;
    public string Container { get; set; } = "$web";
    public string Subscription { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}