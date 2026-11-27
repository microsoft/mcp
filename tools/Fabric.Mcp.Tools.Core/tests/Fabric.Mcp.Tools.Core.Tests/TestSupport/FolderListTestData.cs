// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Azure.Core;
using Fabric.Mcp.Tools.Core.Models;
using NSubstitute;

namespace Fabric.Mcp.Tools.Core.Tests.TestSupport;

internal static class FolderListTestData
{
    internal const string WorkspaceId = "aaaaaaaa-0000-1111-2222-bbbbbbbbbbbb";
    internal const string FolderId = "bbbbbbbb-1111-2222-3333-cccccccccccc";
    internal const string ParentFolderId = "aaaaaaaa-6666-7777-8888-bbbbbbbbbbbb";
    // cspell:disable-next-line
    internal const string DocumentedToken = "MAEsMTbwMDAwLDA%5D";
    internal static string CollectionUrl => $"{FabricEndpoints.GetFabricApiBaseUrl()}/workspaces/{WorkspaceId}/folders";
    internal static FabricFolder Folder => new(Guid.Parse(FolderId), "Sales", Guid.Parse(WorkspaceId), Guid.Parse(ParentFolderId));

    internal static string Page(string? token = null, string? uri = null, bool empty = false)
    {
        var page = new JsonObject
        {
            ["value"] = empty ? new JsonArray() : new JsonArray(JsonSerializer.SerializeToNode(Folder, CoreJsonContext.Default.FabricFolder))
        };
        if (token is not null)
        {
            page["continuationToken"] = token;
        }
        if (uri is not null)
        {
            page["continuationUri"] = uri;
        }
        return page.ToJsonString();
    }

    internal static TokenCredential Credential()
    {
        var credential = Substitute.For<TokenCredential>();
        credential.GetTokenAsync(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>())
            .Returns(new AccessToken("folder-test-token", DateTimeOffset.MaxValue));
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
