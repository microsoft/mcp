// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.AzureBackup.Models;

namespace Azure.Mcp.Tools.AzureBackup.Services;

/// <summary>
/// One page of an RSV backupProtectedItems listing.
/// </summary>
/// <param name="Items">The protected items read from the page.</param>
/// <param name="NextLink">The absolute URL of the next page, or null when this is the last page.</param>
internal sealed record RsvProtectedItemPage(List<ProtectedItemInfo> Items, string? NextLink);
