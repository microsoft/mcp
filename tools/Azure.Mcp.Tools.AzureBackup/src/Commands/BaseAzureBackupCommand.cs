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
    public override void ValidateOptions(TOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);

        if (!string.IsNullOrEmpty(options.VaultType) &&
            !options.VaultType.Equals("rsv", StringComparison.OrdinalIgnoreCase) &&
            !options.VaultType.Equals("dpp", StringComparison.OrdinalIgnoreCase))
        {
            validationResult.Errors.Add("--vault-type must be 'rsv' (Recovery Services vault) or 'dpp' (Backup vault).");
        }
    }
}
