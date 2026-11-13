// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text;
using System.Text.Json;
using Azure.Core;
using Fabric.Mcp.Tools.Core.Models;
using NSubstitute;

namespace Fabric.Mcp.Tools.Core.Tests.TestSupport;

internal static class ItemMoveTestData
{
    internal const string WorkspaceId = "aaaaaaaa-0000-1111-2222-bbbbbbbbbbbb";
    internal const string ItemId = "cccccccc-2222-3333-4444-dddddddddddd";
    internal const string ChildId = "dddddddd-3333-4444-5555-eeeeeeeeeeee";
    internal const string FolderId = "bbbbbbbb-1111-2222-3333-cccccccccccc";
    internal static string ItemUrl => $"{FabricEndpoints.GetFabricApiBaseUrl()}/workspaces/{WorkspaceId}/items/{ItemId}";

    internal static FabricMovedItem Item(string? folder = null) =>
        new("Lakehouse", Guid.Parse(ItemId), "MyLakehouse", Guid.Parse(WorkspaceId),
            "A lakehouse used by the sales team.", folder is null ? null : Guid.Parse(folder));

    internal static string Collection(string? folder = null)
    {
        var item = JsonSerializer.Serialize(Item(folder), CoreJsonContext.Default.FabricMovedItem);
        var child = JsonSerializer.Serialize(Item(folder) with { Id = Guid.Parse(ChildId), Type = "SQLEndpoint", Description = null },
            CoreJsonContext.Default.FabricMovedItem);
        return $"{{\"value\":[{item},{child}]}}";
    }

    internal static TokenCredential Credential()
    {
        var credential = Substitute.For<TokenCredential>();
        credential.GetTokenAsync(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>())
            .Returns(new AccessToken("item-move-test-token", DateTimeOffset.MaxValue));
        return credential;
    }

    internal static HttpResponseMessage Response(string json, HttpStatusCode status = HttpStatusCode.OK, string? retryAfter = null)
    {
        var response = new HttpResponseMessage(status) { Content = new StringContent(json, Encoding.UTF8, "application/json") };
        if (retryAfter is not null)
        {
            response.Headers.TryAddWithoutValidation("Retry-After", retryAfter);
        }
        return response;
    }
}
