// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Nodes;
using Microsoft.Mcp.Core.Models.Paging;

namespace Azure.Mcp.Core.Services.Azure;

/// <summary>
/// Carries Resource Graph paging state and resolved scope for constructing a portable continuation.
/// </summary>
/// <param name="PageSize">The effective maximum number of resources requested for the page.</param>
/// <param name="ContinuationToken">
/// The encoded next-page token, or <see langword="null"/> when no further page is available.
/// </param>
/// <param name="Subscription">The resolved subscription ID, independent of machine-local defaults.</param>
/// <param name="Tenant">The resolved tenant ID to use for the next page.</param>
/// <param name="ResourceGroup">The resource group filter, or <see langword="null"/> for the subscription scope.</param>
public sealed record ResourceQueryPage(
    int PageSize,
    string? ContinuationToken,
    string Subscription,
    string Tenant,
    string? ResourceGroup)
{
    /// <summary>
    /// Builds command-facing metadata with the arguments needed to continue on a compatible server instance.
    /// </summary>
    /// <param name="commandId">The stable identifier of the logical command returning the page.</param>
    /// <returns>
    /// Metadata containing the page size and, when continuation is available, the command identifier,
    /// next-page token, and complete continuation arguments. A terminal page contains only the page size.
    /// </returns>
    public PagingMetadata ToPagingMetadata(string commandId)
    {
        if (ContinuationToken is null)
        {
            return new(PageSize);
        }

        var arguments = new JsonObject
        {
            ["subscription"] = Subscription,
            ["tenant"] = Tenant,
            ["page-size"] = PageSize,
            ["continuation-token"] = ContinuationToken
        };
        if (ResourceGroup is not null)
        {
            arguments["resource-group"] = ResourceGroup;
        }

        return new(PageSize, ContinuationToken, commandId, arguments);
    }
}
