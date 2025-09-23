// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Storage.Services.Models;

/// <summary>
/// A class representing the storage account SKU.
/// </summary>
internal sealed class StorageAccountSku
{
    /// <summary> The SKU name. </summary>
    public string? Name { get; set; }
    /// <summary> The SKU tier. </summary>
    public string? Tier { get; set; }
}
