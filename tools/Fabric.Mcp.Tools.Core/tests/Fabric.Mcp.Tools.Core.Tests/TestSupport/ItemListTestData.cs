// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Core;
using NSubstitute;

namespace Fabric.Mcp.Tools.Core.Tests.TestSupport;

internal static class ItemListTestData
{
    public const string WorkspaceId = "aaaaaaaa-0000-1111-2222-bbbbbbbbbbbb";
    public const string FolderId = "bbbbbbbb-1111-2222-3333-cccccccccccc";
    public const string ItemId = "cccccccc-2222-3333-4444-dddddddddddd";
    public const string Token = "ABCsMTAwMDAwLDA%3D";
    public const string ContinuationUri = "https://api.fabric.microsoft.com/v1/workspaces/xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx/items?continuationToken=ABCsMTAwMDAwLDA%3D";
    public const string ItemsUrl = $"https://api.fabric.microsoft.com/v1/workspaces/{WorkspaceId}/items";
    public const string PageJson = """
        {
          "value": [
            {
              "id": "cccccccc-2222-3333-4444-dddddddddddd",
              "displayName": "Sales lakehouse",
              "type": "FutureItemType",
              "workspaceId": "aaaaaaaa-0000-1111-2222-bbbbbbbbbbbb",
              "description": "Inventory metadata",
              "folderId": "bbbbbbbb-1111-2222-3333-cccccccccccc",
              "logicalId": "dddddddd-3333-4444-5555-eeeeeeeeeeee",
              "tags": [{ "id": "eeeeeeee-4444-5555-6666-ffffffffffff", "displayName": "Sales" }],
              "sensitivityLabel": { "id": "ffffffff-5555-6666-7777-aaaaaaaaaaaa" },
              "definition": { "payload": "excluded-definition" },
              "defaultIdentity": { "userDetails": { "userPrincipalName": "excluded-identity" } },
              "data": "excluded-data"
            }
          ],
          "continuationToken": "ABCsMTAwMDAwLDA%3D",
          "continuationUri": "https://api.fabric.microsoft.com/v1/workspaces/xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx/items?continuationToken=ABCsMTAwMDAwLDA%3D"
        }
        """;

    public static TokenCredential CreateCredential()
    {
        var credential = Substitute.For<TokenCredential>();
        credential.GetTokenAsync(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>())
            .Returns(new ValueTask<AccessToken>(new AccessToken("test-token", DateTimeOffset.MaxValue)));
        return credential;
    }

    public static HttpResponseMessage Response(string json = """{"value":[]}""", HttpStatusCode status = HttpStatusCode.OK) =>
        new(status) { Content = new StringContent(json) };
}
