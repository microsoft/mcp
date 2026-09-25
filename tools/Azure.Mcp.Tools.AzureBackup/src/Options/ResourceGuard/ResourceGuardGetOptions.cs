// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.AzureBackup.Options.ResourceGuard;

public sealed class ResourceGuardGetOptions : ISubscriptionOption
{
    [Option(Description = "Name of the Resource Guard to fetch. Omit to list Resource Guards (in --resource-group if provided, otherwise across the subscription).")]
    public string? ResourceGuard { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }

    [Option(Description = OptionDescriptions.Subscription)]
    public string? Subscription { get; set; }

    [Option(Description = OptionDescriptions.ResourceGroup)]
    public string? ResourceGroup { get; set; }
}
