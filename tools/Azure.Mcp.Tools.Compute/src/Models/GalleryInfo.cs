// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Compute.Models;

/// <summary>
/// Represents an Azure Compute Gallery.
/// </summary>
public class GalleryInfo
{
    /// <summary>
    /// Gets or sets the name of the gallery.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the resource ID of the gallery.
    /// </summary>
    public string? Id { get; set; }

    /// <summary>
    /// Gets or sets the resource group containing the gallery.
    /// </summary>
    public string? ResourceGroup { get; set; }

    /// <summary>
    /// Gets or sets the location of the gallery.
    /// </summary>
    public string? Location { get; set; }

    /// <summary>
    /// Gets or sets the description of the gallery.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the globally unique name assigned to the gallery by Azure.
    /// </summary>
    public string? UniqueName { get; set; }

    /// <summary>
    /// Gets or sets the provisioning state of the gallery.
    /// </summary>
    public string? ProvisioningState { get; set; }

    /// <summary>
    /// Gets or sets the sharing permissions of the gallery (e.g., Private, Groups, Community).
    /// </summary>
    public string? SharingPermissions { get; set; }

    /// <summary>
    /// Gets or sets the tags applied to the gallery.
    /// </summary>
    public Dictionary<string, string>? Tags { get; set; }
}
