// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using System.Text.Json.Serialization;
using Azure.Mcp.Tools.Storage.Commands;

namespace Azure.Mcp.Tools.Storage.Services.Models;

internal sealed class StorageAccountCreateOrUpdateContent
{
    /// <summary> The location of the resource. </summary>
    public string? Location { get; set; }
    /// <summary> The storage account SKU. </summary>
    public StorageAccountSku? Sku { get; set; }
    /// <summary> Properties of the storage account. </summary>
    public StorageAccountProperties? Properties { get; set; }
    /// <summary> The kind of storage account. </summary>
    public string? Kind { get; set; }
}
