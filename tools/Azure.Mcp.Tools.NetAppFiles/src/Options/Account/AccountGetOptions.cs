// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.NetAppFiles.Options.Account;

public class AccountGetOptions : ISubscriptionOption
{
    [Option(Description = "The name of the Azure NetApp Files account to retrieve. Must be 1-128 characters, start with an alphanumeric character, and contain only alphanumerics, underscores, and hyphens.")]
    public required string Account { get; set; }

    [Option(Description = OptionDescriptions.ResourceGroup)]
    public required string ResourceGroup { get; set; }

    [Option(Description = OptionDescriptions.Subscription)]
    public string? Subscription { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }
}