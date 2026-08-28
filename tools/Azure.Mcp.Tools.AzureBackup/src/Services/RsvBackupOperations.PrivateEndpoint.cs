// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Core;
using Azure.Mcp.Tools.AzureBackup.Models;
using Azure.ResourceManager;
using Azure.ResourceManager.Network;
using Azure.ResourceManager.Network.Models;
using Azure.ResourceManager.RecoveryServices;
using Azure.ResourceManager.RecoveryServices.Models;
using Azure.ResourceManager.RecoveryServicesBackup;
using Azure.ResourceManager.RecoveryServicesBackup.Models;
using Azure.ResourceManager.Resources;

namespace Azure.Mcp.Tools.AzureBackup.Services;

// PR 4: Private Endpoint operations on Recovery Services vaults (RSV).
// See azurebackup-rsv-mcp-improvements-plan.md §PR 4.
public sealed partial class RsvBackupOperations
{
    /// <summary>Sub-resource ("group") IDs supported by RSV. Primary region is <c>AzureBackup</c>;
    /// <c>AzureBackup_secondary</c> is used only for Cross-Region Restore.</summary>
    private static readonly string[] s_allowedGroupIds = ["AzureBackup", "AzureBackup_secondary"];

    public async Task<PrivateEndpointConnectionInfo> CreatePrivateEndpointAsync(
        string vaultName, string resourceGroup, string subscription,
        string privateEndpointName, string vnetSubnetId, string groupId,
        string? location, bool autoApprove,
        string? tenant,
        CancellationToken cancellationToken)
    {
        ValidateRequiredParameters(
            (nameof(vaultName), vaultName),
            (nameof(resourceGroup), resourceGroup),
            (nameof(subscription), subscription),
            (nameof(privateEndpointName), privateEndpointName),
            (nameof(vnetSubnetId), vnetSubnetId),
            (nameof(groupId), groupId));

        ValidateGroupId(groupId);
        var subnetResourceId = ParseSubnetId(vnetSubnetId);

        var armClient = await CreateArmClientAsync(tenant, cancellationToken: cancellationToken);
        var vaultId = RecoveryServicesVaultResource.CreateResourceIdentifier(subscription, resourceGroup, vaultName);
        var vaultResource = armClient.GetRecoveryServicesVaultResource(vaultId);
        var vault = await vaultResource.GetAsync(cancellationToken);
        var vaultLocation = vault.Value.Data.Location;

        // Server-side enforces the maximum PE count; we only pre-flight the "no protected items" rule
        // to give the caller a clearer error before creating the PE resource.
        await ValidatePrivateEndpointPreconditionsAsync(
            armClient, subscription, resourceGroup, vaultName, cancellationToken);

        var peLocation = string.IsNullOrWhiteSpace(location) ? vaultLocation.Name : location!;
        var rgId = ResourceGroupResource.CreateResourceIdentifier(subscription, resourceGroup);
        var rgResource = armClient.GetResourceGroupResource(rgId);
        var peCollection = rgResource.GetPrivateEndpoints();

        var connection = new NetworkPrivateLinkServiceConnection
        {
            Name = privateEndpointName,
            PrivateLinkServiceId = new ResourceIdentifier(vaultId.ToString()),
        };
        connection.GroupIds.Add(groupId);

        var peData = new PrivateEndpointData
        {
            Location = new AzureLocation(peLocation),
            Subnet = new SubnetData { Id = subnetResourceId },
            CustomNetworkInterfaceName = privateEndpointName + "-nic",
        };
        peData.PrivateLinkServiceConnections.Add(connection);

        var peOp = await peCollection.CreateOrUpdateAsync(WaitUntil.Started, privateEndpointName, peData, cancellationToken);
        await WaitForLroCompletionAsync(peOp, cancellationToken);

        // Refetch the vault to find the auto-created PEC that now points at our PE.
        vault = await vaultResource.GetAsync(cancellationToken);
        var expectedPeId = PrivateEndpointResource.CreateResourceIdentifier(subscription, resourceGroup, privateEndpointName);
        var pec = FindPrivateEndpointConnectionForPe(vault.Value, expectedPeId)
            ?? throw new InvalidOperationException(
                $"Private Endpoint '{privateEndpointName}' was created in resource group '{resourceGroup}', but no matching Private Endpoint Connection appeared on vault '{vaultName}'. This can happen if the ARM propagation is delayed; retry 'azurebackup vault privateendpoint get' shortly.");

        if (autoApprove && string.Equals(pec.Properties?.PrivateLinkServiceConnectionState?.Status?.ToString(), "Pending", StringComparison.OrdinalIgnoreCase))
        {
            return await SetPrivateEndpointConnectionStateAsync(
                vaultName, resourceGroup, subscription, ExtractPecName(pec.Id!),
                PrivateEndpointConnectionStatus.Approved,
                description: "Auto-approved by Azure MCP tool",
                tenant, cancellationToken);
        }

        return MapToPrivateEndpointConnectionInfo(pec);
    }

    public async Task<List<PrivateEndpointConnectionInfo>> ListPrivateEndpointsAsync(
        string vaultName, string resourceGroup, string subscription,
        string? tenant,
        CancellationToken cancellationToken)
    {
        ValidateRequiredParameters(
            (nameof(vaultName), vaultName),
            (nameof(resourceGroup), resourceGroup),
            (nameof(subscription), subscription));

        var armClient = await CreateArmClientAsync(tenant, cancellationToken: cancellationToken);
        var vaultId = RecoveryServicesVaultResource.CreateResourceIdentifier(subscription, resourceGroup, vaultName);
        var vaultResource = armClient.GetRecoveryServicesVaultResource(vaultId);
        var vault = await vaultResource.GetAsync(cancellationToken);

        var pecs = vault.Value.Data.Properties?.PrivateEndpointConnections;
        if (pecs is null || pecs.Count == 0)
        {
            return [];
        }

        return pecs.Select(MapToPrivateEndpointConnectionInfo).ToList();
    }

    public async Task<PrivateEndpointConnectionInfo> GetPrivateEndpointAsync(
        string vaultName, string resourceGroup, string subscription,
        string privateEndpointConnectionName,
        string? tenant,
        CancellationToken cancellationToken)
    {
        ValidateRequiredParameters(
            (nameof(vaultName), vaultName),
            (nameof(resourceGroup), resourceGroup),
            (nameof(subscription), subscription),
            (nameof(privateEndpointConnectionName), privateEndpointConnectionName));

        var armClient = await CreateArmClientAsync(tenant, cancellationToken: cancellationToken);
        var pecResource = GetBackupPrivateEndpointConnectionResource(
            armClient, subscription, resourceGroup, vaultName, privateEndpointConnectionName);

        var pec = await pecResource.GetAsync(cancellationToken);
        return MapToPrivateEndpointConnectionInfo(pec.Value.Data);
    }

    public async Task<OperationResult> DeletePrivateEndpointAsync(
        string vaultName, string resourceGroup, string subscription,
        string privateEndpointConnectionName,
        string? tenant,
        CancellationToken cancellationToken)
    {
        ValidateRequiredParameters(
            (nameof(vaultName), vaultName),
            (nameof(resourceGroup), resourceGroup),
            (nameof(subscription), subscription),
            (nameof(privateEndpointConnectionName), privateEndpointConnectionName));

        var armClient = await CreateArmClientAsync(tenant, cancellationToken: cancellationToken);
        var pecResource = GetBackupPrivateEndpointConnectionResource(
            armClient, subscription, resourceGroup, vaultName, privateEndpointConnectionName);

        var operation = await pecResource.DeleteAsync(WaitUntil.Started, cancellationToken);
        await WaitForLroCompletionAsync(operation, cancellationToken);

        return new OperationResult(
            "Succeeded",
            null,
            $"Private Endpoint Connection '{privateEndpointConnectionName}' deleted from vault '{vaultName}'. The underlying Private Endpoint (Microsoft.Network/privateEndpoints) must be deleted separately if it is no longer needed.");
    }

    public async Task<PrivateEndpointConnectionInfo> SetPrivateEndpointConnectionStateAsync(
        string vaultName, string resourceGroup, string subscription,
        string privateEndpointConnectionName, PrivateEndpointConnectionStatus targetStatus,
        string? description, string? tenant,
        CancellationToken cancellationToken)
    {
        ValidateRequiredParameters(
            (nameof(vaultName), vaultName),
            (nameof(resourceGroup), resourceGroup),
            (nameof(subscription), subscription),
            (nameof(privateEndpointConnectionName), privateEndpointConnectionName));

        var armClient = await CreateArmClientAsync(tenant, cancellationToken: cancellationToken);
        var pecResource = GetBackupPrivateEndpointConnectionResource(
            armClient, subscription, resourceGroup, vaultName, privateEndpointConnectionName);

        var current = await pecResource.GetAsync(cancellationToken);
        var currentStatus = current.Value.Data.Properties?.PrivateLinkServiceConnectionState?.Status;

        if (currentStatus == targetStatus)
        {
            return MapToPrivateEndpointConnectionInfo(current.Value.Data);
        }

        var props = current.Value.Data.Properties ?? new BackupPrivateEndpointConnectionProperties();
        props.PrivateLinkServiceConnectionState ??= new RecoveryServicesBackupPrivateLinkServiceConnectionState();
        props.PrivateLinkServiceConnectionState.Status = targetStatus;
        if (!string.IsNullOrWhiteSpace(description))
        {
            props.PrivateLinkServiceConnectionState.Description = description;
        }

        var updateData = new BackupPrivateEndpointConnectionData(current.Value.Data.Location)
        {
            ETag = current.Value.Data.ETag,
            Properties = props,
        };

        var operation = await pecResource.UpdateAsync(WaitUntil.Started, updateData, cancellationToken);
        await WaitForLroCompletionAsync(operation, cancellationToken);

        var refreshed = await pecResource.GetAsync(cancellationToken);
        return MapToPrivateEndpointConnectionInfo(refreshed.Value.Data);
    }

    private static BackupPrivateEndpointConnectionResource GetBackupPrivateEndpointConnectionResource(
        ArmClient armClient, string subscription, string resourceGroup, string vaultName, string privateEndpointConnectionName)
    {
        var id = BackupPrivateEndpointConnectionResource.CreateResourceIdentifier(
            subscription, resourceGroup, vaultName, privateEndpointConnectionName);
        return armClient.GetBackupPrivateEndpointConnectionResource(id);
    }

    private static void ValidateGroupId(string groupId)
    {
        if (!s_allowedGroupIds.Contains(groupId, StringComparer.OrdinalIgnoreCase))
        {
            throw new ArgumentException(
                $"Invalid --group-id '{groupId}'. Recovery Services vaults support only 'AzureBackup' (primary region) or 'AzureBackup_secondary' (paired region, Cross-Region Restore).");
        }
    }

    private static ResourceIdentifier ParseSubnetId(string subnetId)
    {
        ResourceIdentifier parsed;
        try
        {
            parsed = new ResourceIdentifier(subnetId);
        }
        catch (Exception ex) when (ex is FormatException or ArgumentException)
        {
            throw new ArgumentException(
                "Invalid --vnet-subnet-id. Expected an ARM resource ID of the form '/subscriptions/{sub}/resourceGroups/{rg}/providers/Microsoft.Network/virtualNetworks/{vnet}/subnets/{subnet}'.", ex);
        }

        if (parsed.ResourceType != "Microsoft.Network/virtualNetworks/subnets")
        {
            throw new ArgumentException(
                $"Invalid --vnet-subnet-id: resource type '{parsed.ResourceType}' is not 'Microsoft.Network/virtualNetworks/subnets'. Expected an ARM resource ID of the form '/subscriptions/{{sub}}/resourceGroups/{{rg}}/providers/Microsoft.Network/virtualNetworks/{{vnet}}/subnets/{{subnet}}'.");
        }

        return parsed;
    }

    private static async Task ValidatePrivateEndpointPreconditionsAsync(
        ArmClient armClient, string subscription, string resourceGroup, string vaultName,
        CancellationToken cancellationToken)
    {
        var rgId = ResourceGroupResource.CreateResourceIdentifier(subscription, resourceGroup);
        var rgResource = armClient.GetResourceGroupResource(rgId);

        await foreach (var item in rgResource.GetBackupProtectedItemsAsync(vaultName, cancellationToken: cancellationToken))
        {
            throw new InvalidOperationException(
                $"Vault '{vaultName}' already has protected items. A Private Endpoint can only be added to a vault with no protected items. Stop protection on existing items (retaining data if needed), then re-run 'azurebackup vault privateendpoint create'.");
        }
    }

    private static PrivateEndpointConnectionInfo MapToPrivateEndpointConnectionInfo(
        BackupPrivateEndpointConnectionData data)
    {
        var props = data.Properties;
        var state = props?.PrivateLinkServiceConnectionState;
        return new PrivateEndpointConnectionInfo(
            Id: data.Id?.ToString(),
            Name: data.Name ?? string.Empty,
            PrivateEndpointId: props?.PrivateEndpointId?.ToString(),
            GroupIds: props?.GroupIds?.Select(g => g.ToString()).ToList(),
            ProvisioningState: props?.ProvisioningState?.ToString(),
            ConnectionStatus: state?.Status?.ToString(),
            Description: state?.Description,
            ActionsRequired: state?.ActionsRequired);
    }

    private static PrivateEndpointConnectionInfo MapToPrivateEndpointConnectionInfo(
        RecoveryServicesPrivateEndpointConnectionVaultProperties pec)
    {
        var connection = pec.Properties;
        var state = connection?.PrivateLinkServiceConnectionState;
        return new PrivateEndpointConnectionInfo(
            Id: pec.Id?.ToString(),
            Name: pec.Name ?? string.Empty,
            PrivateEndpointId: connection?.PrivateEndpointId?.ToString(),
            GroupIds: connection?.GroupIds?.Select(g => g.ToString()).ToList(),
            ProvisioningState: connection?.ProvisioningState?.ToString(),
            ConnectionStatus: state?.Status?.ToString(),
            Description: state?.Description,
            ActionsRequired: state?.ActionsRequired);
    }

    private static RecoveryServicesPrivateEndpointConnectionVaultProperties? FindPrivateEndpointConnectionForPe(
        RecoveryServicesVaultResource vault, ResourceIdentifier expectedPrivateEndpointId)
    {
        var pecs = vault.Data.Properties?.PrivateEndpointConnections;
        if (pecs is null)
        {
            return null;
        }

        var expectedId = expectedPrivateEndpointId.ToString();
        foreach (var pec in pecs)
        {
            var peId = pec.Properties?.PrivateEndpointId?.ToString();
            if (!string.IsNullOrEmpty(peId) && StringComparer.OrdinalIgnoreCase.Equals(peId, expectedId))
            {
                return pec;
            }
        }

        return null;
    }

    private static string ExtractPecName(ResourceIdentifier pecId)
        => pecId.Name;
}
