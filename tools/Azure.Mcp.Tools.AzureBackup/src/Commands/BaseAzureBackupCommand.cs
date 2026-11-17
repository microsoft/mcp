// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.AzureBackup.Options;
using Microsoft.Mcp.Core.Commands;

namespace Azure.Mcp.Tools.AzureBackup.Commands;

public abstract class BaseAzureBackupCommand<[DynamicallyAccessedMembers(TrimAnnotations.CommandAnnotations)] TOptions, TResult>(ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<TOptions, TResult>(subscriptionResolver) where TOptions : BaseAzureBackupOptions
{
    public override void PostBindOptions(TOptions options)
    {
        options.Subscription = BackupVaultReferenceParser.ResolveSubscription(options.Subscription, options.Vault, options.ResourceGroup);
        base.PostBindOptions(options);
    }

    public override void ValidateOptions(TOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);

        var reference = BackupVaultReferenceParser.Normalize(
            options.Subscription, options.ResourceGroup, options.Vault, options.VaultType, rsvOnly: false, validationResult);
        options.ResourceGroup = reference.ResourceGroup ?? options.ResourceGroup;
        options.Vault = reference.Vault ?? options.Vault;
        options.VaultType = reference.VaultType;

        if (!string.IsNullOrEmpty(options.VaultType) &&
            !options.VaultType.Equals("rsv", StringComparison.OrdinalIgnoreCase) &&
            !options.VaultType.Equals("dpp", StringComparison.OrdinalIgnoreCase))
        {
            validationResult.Errors.Add("--vault-type must be 'rsv' (Recovery Services vault) or 'dpp' (Backup vault).");
        }
    }
}
