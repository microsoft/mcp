// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Core.Services.Azure;
using Microsoft.Mcp.Core.Models.Paging;

namespace Azure.Mcp.Core.Tests.Services.Azure;

internal sealed class ResourceGraphPagingTestService(IAzureService azureService) : BaseAzureResourceService(azureService)
{
    public Task<ResourceQueryResults<string>> QueryAsync(
        PageRequest page,
        string resourceType = "Microsoft.Storage/storageAccounts",
        string? resourceGroup = null,
        CancellationToken cancellationToken = default) =>
        ExecuteResourceQueryPageAsync(
            resourceType, resourceGroup, ResourceGraphPagingTestFactory.Subscription,
            ReadId, page, cancellationToken: cancellationToken);

    public Task<ResourceQueryResults<string>> QueryLegacyAsync(CancellationToken cancellationToken) =>
        ExecuteResourceQueryAsync(
            "Microsoft.Storage/storageAccounts", null, ResourceGraphPagingTestFactory.Subscription,
            ReadId, cancellationToken: cancellationToken);

    private static string ReadId(JsonElement element) =>
        element.GetProperty("id").GetString() ?? throw new JsonException("A resource ID is required.");
}
