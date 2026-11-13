// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

// cspell:ignore LDEs

using System.Net;
using Azure.Core;
using NSubstitute;

namespace Fabric.Mcp.Tools.Core.Tests.TestSupport;

internal static class WorkspaceListTestData
{
    internal const string WorkspaceId = "aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa";
    internal const string ContinuationToken = "LDEsMTAwMDAwLDA%3D";
    internal const string EmptyPage = """{"value":[]}""";
    internal const string MinimalPage = """
        {"value":[{"id":"aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa","displayName":"My workspace","type":"Personal"}]}
        """;
    internal const string FullPage = """
        {
          "value": [{
            "id": "aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa",
            "displayName": "Finance",
            "type": "FutureWorkspaceType",
            "description": "",
            "capacityId": "bbbbbbbb-bbbb-4bbb-8bbb-bbbbbbbbbbbb",
            "capacityRegion": "Future Region",
            "domainId": "cccccccc-cccc-4ccc-8ccc-cccccccccccc",
            "tags": [{"id":"dddddddd-dddd-4ddd-8ddd-dddddddddddd","displayName":"Finance"}],
            "apiEndpoint": "https://workspace.example.test",
            "definition": {"payload":"excluded-definition"},
            "members": [{"secret":"excluded-members"}]
          }],
          "continuationToken": "LDEsMTAwMDAwLDA%3D",
          "continuationUri": "https://continuation.example.test/workspaces?continuationToken=LDEsMTAwMDAwLDA%3D"
        }
        """;

    internal static TokenCredential CreateCredential()
    {
        var credential = Substitute.For<TokenCredential>();
        credential.GetTokenAsync(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>())
            .Returns(new AccessToken("test-token", DateTimeOffset.MaxValue));
        return credential;
    }

    internal static HttpResponseMessage CreateResponse(string body = EmptyPage, HttpStatusCode status = HttpStatusCode.OK) =>
        new(status) { Content = new StringContent(body) };
}
