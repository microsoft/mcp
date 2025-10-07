// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Services.Azure.Models;

namespace Azure.Mcp.Tools.Storage.Services.Models;

internal sealed class StorageAccountCreateOrUpdateContent
{
    /// <summary> The location of the resource. </summary>
    public string? Location { get; set; }
    /// <summary> The storage account SKU. </summary>
    public ResourceSku? Sku { get; set; }
    /// <summary> Properties of the storage account. </summary>
    public StorageAccountProperties? Properties { get; set; }
    /// <summary> The kind of storage account. </summary>
    public string? Kind { get; set; }
}
