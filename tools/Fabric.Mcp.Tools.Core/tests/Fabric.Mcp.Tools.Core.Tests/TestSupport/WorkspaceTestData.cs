// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Core;
using Fabric.Mcp.Tools.Core.Models;
using NSubstitute;

namespace Fabric.Mcp.Tools.Core.Tests.TestSupport;

internal static class WorkspaceTestData
{
    public const string WorkspaceId = "aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa";
    public const string RelatedId = "bbbbbbbb-bbbb-4bbb-8bbb-bbbbbbbbbbbb";
    public const string MinimalJson = """
        {"id":"aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa","displayName":"Finance","type":"Workspace"}
        """;
    public const string FullJson = """
        {
          "id": "aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa",
          "displayName": "Finance",
          "type": "Workspace",
          "description": "Workspace metadata",
          "capacityId": "bbbbbbbb-bbbb-4bbb-8bbb-bbbbbbbbbbbb",
          "capacityAssignmentProgress": "InProgress",
          "capacityRegion": "East US",
          "domainId": "bbbbbbbb-bbbb-4bbb-8bbb-bbbbbbbbbbbb",
          "workspaceIdentity": {
            "applicationId": "bbbbbbbb-bbbb-4bbb-8bbb-bbbbbbbbbbbb",
            "servicePrincipalId": "cccccccc-cccc-4ccc-8ccc-cccccccccccc"
          },
          "oneLakeEndpoints": {
            "blobEndpoint": "https://aaaaaaaaaaaa4aaa8aaaaaaaaaaaaaaa.zcf.blob.fabric.microsoft.com",
            "dfsEndpoint": "https://aaaaaaaaaaaa4aaa8aaaaaaaaaaaaaaa.zcf.dfs.fabric.microsoft.com"
          },
          "apiEndpoint": "https://aaaaaaaaaaaa4aaa8aaaaaaaaaaaaaaa.zcf.w.api.fabric.microsoft.com",
          "tags": [{"id":"bbbbbbbb-bbbb-4bbb-8bbb-bbbbbbbbbbbb","displayName":"Finance"}],
          "unexpectedProperty": {"secret":"excluded-backend-property"}
        }
        """;

    public static FabricWorkspaceMetadata CreateWorkspace() => new()
    {
        Id = Guid.Parse(WorkspaceId),
        DisplayName = "Finance",
        Type = "Workspace"
    };

    public static TokenCredential CreateCredential()
    {
        var credential = Substitute.For<TokenCredential>();
        credential.GetTokenAsync(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>())
            .Returns(new ValueTask<AccessToken>(new AccessToken("test-token", DateTimeOffset.MaxValue)));
        return credential;
    }
}
