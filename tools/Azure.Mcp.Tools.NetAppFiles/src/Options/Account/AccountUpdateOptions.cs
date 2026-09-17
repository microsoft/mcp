// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.NetAppFiles.Options.Account;

public sealed class AccountUpdateOptions : ISubscriptionOption
{
    [Option(Description = "The name of the Azure NetApp Files account to update. Must be 1-128 characters, start with an alphanumeric character, and contain only alphanumerics, underscores, and hyphens.")]
    public required string Account { get; set; }

    [Option(Description = "Resource tags as a JSON key-value object. Providing an empty object removes all tags.")]
    public string? Tags { get; set; }

    [Option(Description = "The domain for NFSv4 user ID mapping. This setting applies to all Azure NetApp Files accounts in the subscription and region and affects non-LDAP NFSv4 volumes.")]
    public string? NfsV4IdDomain { get; set; }

    [Option(Description = OptionDescriptions.ResourceGroup)]
    public required string ResourceGroup { get; set; }

    [Option(Description = OptionDescriptions.Subscription)]
    public string? Subscription { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }
}
