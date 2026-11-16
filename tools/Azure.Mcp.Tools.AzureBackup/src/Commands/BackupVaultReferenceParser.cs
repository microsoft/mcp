// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.AzureBackup.Services;
using Microsoft.Mcp.Core.Commands;

namespace Azure.Mcp.Tools.AzureBackup.Commands;

/// <summary>
/// Accepts a vault name or a full Recovery Services/Backup vault resource ID for --vault, and a resource group
/// name or resource group resource ID for --resource-group. Resource IDs are reduced to names so services never
/// build ARM identifiers from path-shaped input.
/// </summary>
internal static class BackupVaultReferenceParser
{
    private const string RsvVaultType = "rsv";
    private const string DppVaultType = "dpp";

    // Every message is static and contains no caller-supplied values, so it is also telemetry safe.
    internal const string EmptyVaultMessage = "--vault cannot be empty.";
    internal const string InvalidVaultMessage = "--vault must be a vault name or a full vault resource ID ('/subscriptions/{subscription}/resourceGroups/{resourceGroup}/providers/Microsoft.RecoveryServices/vaults/{vault}' or '/subscriptions/{subscription}/resourceGroups/{resourceGroup}/providers/Microsoft.DataProtection/backupVaults/{vault}').";
    internal const string RsvOnlyVaultMessage = "This command supports only Recovery Services vaults; --vault cannot be a Backup vault (Microsoft.DataProtection/backupVaults) resource ID.";
    internal const string EmptyResourceGroupMessage = "--resource-group cannot be empty.";
    internal const string InvalidResourceGroupMessage = "--resource-group must be a resource group name or a resource group resource ID ('/subscriptions/{subscription}/resourceGroups/{resourceGroup}').";
    internal const string ResourceGroupMismatchMessage = "The resource group in the --vault resource ID does not match --resource-group.";
    internal const string SubscriptionMismatchMessage = "The subscription in the --vault or --resource-group resource ID does not match --subscription.";
    internal const string ResourceIdSubscriptionMismatchMessage = "The --vault and --resource-group resource IDs refer to different subscriptions.";
    internal const string SubscriptionNameWithResourceIdMessage = "When --vault or --resource-group is a resource ID, --subscription must be omitted or be the subscription ID (GUID), not a subscription name.";
    internal const string VaultTypeMismatchMessage = "--vault-type does not match the --vault resource ID (Microsoft.RecoveryServices/vaults is 'rsv'; Microsoft.DataProtection/backupVaults is 'dpp').";

    /// <summary>
    /// Returns the subscription embedded in the --vault or --resource-group resource ID when --subscription was
    /// omitted, so the default subscription is not used for a fully qualified vault.
    /// </summary>
    internal static string? ResolveSubscription(string? subscription, string? vault, string? resourceGroup)
    {
        if (!string.IsNullOrWhiteSpace(subscription))
        {
            return subscription;
        }

        if (vault is not null && TryParseVaultId(vault, out var vaultSubscription, out _, out _, out _))
        {
            return vaultSubscription;
        }

        if (resourceGroup is not null && TryParseResourceGroupId(resourceGroup, out var resourceGroupSubscription, out _))
        {
            return resourceGroupSubscription;
        }

        return subscription;
    }

    /// <summary>
    /// Normalizes --vault and --resource-group to names and infers --vault-type from a vault resource ID.
    /// A <see langword="null"/> vault or resource group is treated as omitted and returned unchanged.
    /// </summary>
    internal static BackupVaultReference Normalize(
        string? subscription,
        string? resourceGroup,
        string? vault,
        string? vaultType,
        bool rsvOnly,
        ValidationResult validationResult)
    {
        string? resourceGroupSubscription = null;
        var resourceGroupValid = false;
        if (resourceGroup is not null)
        {
            if (string.IsNullOrWhiteSpace(resourceGroup))
            {
                AddError(validationResult, EmptyResourceGroupMessage);
            }
            else if (!IsResourceId(resourceGroup))
            {
                resourceGroupValid = true;
            }
            else if (TryParseResourceGroupId(resourceGroup, out var parsedSubscription, out var resourceGroupName))
            {
                resourceGroupSubscription = parsedSubscription;
                resourceGroup = resourceGroupName;
                resourceGroupValid = true;
            }
            else
            {
                AddError(validationResult, InvalidResourceGroupMessage);
            }
        }

        string? vaultSubscription = null;
        if (vault is not null)
        {
            if (string.IsNullOrWhiteSpace(vault))
            {
                AddError(validationResult, EmptyVaultMessage);
            }
            else if (IsResourceId(vault))
            {
                if (TryParseVaultId(vault, out var parsedSubscription, out var vaultResourceGroup, out var vaultName, out var idVaultType))
                {
                    vaultSubscription = parsedSubscription;
                    vault = vaultName;
                    vaultType = ReconcileVaultType(vaultType, idVaultType, rsvOnly, validationResult);

                    if (resourceGroup is null)
                    {
                        resourceGroup = vaultResourceGroup;
                    }
                    else if (resourceGroupValid && !resourceGroup.Equals(vaultResourceGroup, StringComparison.OrdinalIgnoreCase))
                    {
                        AddError(validationResult, ResourceGroupMismatchMessage);
                    }
                }
                else
                {
                    AddError(validationResult, InvalidVaultMessage);
                }
            }
        }

        if (vaultSubscription is not null && resourceGroupSubscription is not null &&
            !SameSubscription(vaultSubscription, resourceGroupSubscription))
        {
            AddError(validationResult, ResourceIdSubscriptionMismatchMessage);
        }
        else if ((vaultSubscription ?? resourceGroupSubscription) is { } idSubscription &&
            !string.IsNullOrWhiteSpace(subscription))
        {
            // A subscription display name is resolved only after validation, so it cannot be compared with the
            // subscription in the resource ID; require the ID form to avoid acting on a same-named vault elsewhere.
            if (!Guid.TryParse(subscription, out _))
            {
                AddError(validationResult, SubscriptionNameWithResourceIdMessage);
            }
            else if (!SameSubscription(subscription, idSubscription))
            {
                AddError(validationResult, SubscriptionMismatchMessage);
            }
        }

        return new BackupVaultReference(resourceGroup, vault, vaultType);
    }

    private static string? ReconcileVaultType(string? vaultType, string idVaultType, bool rsvOnly, ValidationResult validationResult)
    {
        if (rsvOnly)
        {
            if (idVaultType != RsvVaultType)
            {
                AddError(validationResult, RsvOnlyVaultMessage);
            }

            return vaultType;
        }

        if (string.IsNullOrWhiteSpace(vaultType))
        {
            return idVaultType;
        }

        // An unrecognized --vault-type is reported by the command's own validation.
        if ((vaultType.Equals(RsvVaultType, StringComparison.OrdinalIgnoreCase) ||
             vaultType.Equals(DppVaultType, StringComparison.OrdinalIgnoreCase)) &&
            !vaultType.Equals(idVaultType, StringComparison.OrdinalIgnoreCase))
        {
            AddError(validationResult, VaultTypeMismatchMessage);
        }

        return vaultType;
    }

    // Vault and resource group names cannot contain '/', so any slash means the caller supplied a resource ID.
    private static bool IsResourceId(string value) => value.Contains('/');

    private static bool TryParseVaultId(
        string value,
        out string subscription,
        out string resourceGroup,
        out string vault,
        out string vaultType)
    {
        subscription = resourceGroup = vault = vaultType = string.Empty;
        var segments = SplitResourceId(value);
        if (segments is not { Length: 8 } ||
            !IsSubscriptionAndResourceGroupPrefix(segments) ||
            !segments[4].Equals("providers", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (segments[5].Equals("Microsoft.RecoveryServices", StringComparison.OrdinalIgnoreCase) &&
            segments[6].Equals("vaults", StringComparison.OrdinalIgnoreCase))
        {
            vaultType = RsvVaultType;
        }
        else if (segments[5].Equals("Microsoft.DataProtection", StringComparison.OrdinalIgnoreCase) &&
            segments[6].Equals("backupVaults", StringComparison.OrdinalIgnoreCase))
        {
            vaultType = DppVaultType;
        }
        else
        {
            return false;
        }

        subscription = segments[1];
        resourceGroup = segments[3];
        vault = segments[7];
        return true;
    }

    private static bool TryParseResourceGroupId(string value, out string subscription, out string resourceGroup)
    {
        subscription = resourceGroup = string.Empty;
        var segments = SplitResourceId(value);
        if (segments is not { Length: 4 } || !IsSubscriptionAndResourceGroupPrefix(segments))
        {
            return false;
        }

        subscription = segments[1];
        resourceGroup = segments[3];
        return true;
    }

    private static bool IsSubscriptionAndResourceGroupPrefix(string[] segments) =>
        segments[0].Equals("subscriptions", StringComparison.OrdinalIgnoreCase) &&
        Guid.TryParse(segments[1], out _) &&
        segments[2].Equals("resourceGroups", StringComparison.OrdinalIgnoreCase);

    private static string[]? SplitResourceId(string value)
    {
        var trimmed = value.Trim();
        if (trimmed.StartsWith('/'))
        {
            trimmed = trimmed[1..];
        }

        if (trimmed.EndsWith('/'))
        {
            trimmed = trimmed[..^1];
        }

        var segments = trimmed.Split('/');
        return segments.All(RsvOperationInputValidator.IsSafeSegment) ? segments : null;
    }

    private static bool SameSubscription(string left, string right) =>
        Guid.TryParse(left, out var leftId) && Guid.TryParse(right, out var rightId) && leftId == rightId;

    private static void AddError(ValidationResult validationResult, string message) =>
        validationResult.AddError(message, message);
}
