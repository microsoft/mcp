// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;

namespace Azure.Mcp.Tools.FileShares.Models;

/// <summary>
/// Represents Azure File Share data.
/// </summary>
public class FileShareData
{
    /// <summary>
    /// Gets or sets the resource identifier.
    /// </summary>
    public string? Id { get; set; }

    /// <summary>
    /// Gets or sets the resource name.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the resource type.
    /// </summary>
    public string? Type { get; set; }

    /// <summary>
    /// Gets or sets the resource location.
    /// </summary>
    public string? Location { get; set; }

    /// <summary>
    /// Gets or sets the resource tags.
    /// </summary>
    public Dictionary<string, string>? Tags { get; set; }

    /// <summary>
    /// Gets or sets the file share properties.
    /// </summary>
    public FileShareProperties? Properties { get; set; }
}

/// <summary>
/// Represents File Share properties.
/// </summary>
public class FileShareProperties
{
    /// <summary>
    /// Gets or sets the provisioning state.
    /// </summary>
    public string? ProvisioningState { get; set; }

    /// <summary>
    /// Gets or sets the access tier (Hot, Cool, or TransactionOptimized).
    /// </summary>
    public string? AccessTier { get; set; }

    /// <summary>
    /// Gets or sets the quota in GiB.
    /// </summary>
    public int? QuotaGiB { get; set; }

    /// <summary>
    /// Gets or sets whether the share is enabled for SMB multichannel.
    /// </summary>
    public bool? EnabledForSmb3MultichannelSupport { get; set; }

    /// <summary>
    /// Gets or sets the share creation time.
    /// </summary>
    public System.DateTime? CreatedTime { get; set; }

    /// <summary>
    /// Gets or sets the last modification time.
    /// </summary>
    public System.DateTime? LastModifiedTime { get; set; }
}
