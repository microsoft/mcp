// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.AzureBackup.Options.Operation;

public sealed class OperationGetOptions : ISubscriptionOption
{
    [Option(Description = "The opaque RSV operation ID, not a job ID, resource ID, or URL. Supply the decoded ID from the operation-status response header.")]
    public required string Operation { get; set; }

    [Option(Description = "The Recovery Services vault name. Backup vaults (DPP) are not supported.")]
    public required string Vault { get; set; }

    [Option(Description = "The RSV protection container name. Supply together with --protected-item for item-scoped operation status.")]
    public string? Container { get; set; }

    [Option(Description = "The RSV protected item name. Supply together with --container; otherwise status is queried at vault scope.")]
    public string? ProtectedItem { get; set; }

    [Option(Description = "The backup fabric name for item scope. Defaults to Azure; only applicable with --container and --protected-item.")]
    public string? Fabric { get; set; }

    [Option(Description = "The resource group containing the Recovery Services vault.")]
    public required string ResourceGroup { get; set; }

    [Option(Description = "The Azure subscription ID or display name. Required.")]
    public required string? Subscription { get; set; }

    [Option(Description = "The Microsoft Entra tenant ID or display name.")]
    public string? Tenant { get; set; }
}
