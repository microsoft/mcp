// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text;
using Azure.Core;
using Fabric.Mcp.Tools.Core.Models;
using NSubstitute;

namespace Fabric.Mcp.Tools.Core.Tests.TestSupport;

internal static class WorkspaceUpdateTestData
{
    internal const string WorkspaceId = "33bae707-5fe7-4352-89bd-061a1318b60a";
    internal const string WorkspaceJson = """
        {
          "id": "33bae707-5fe7-4352-89bd-061a1318b60a",
          "displayName": "Finance",
          "type": "FutureWorkspaceType",
          "description": "",
          "capacityId": "excluded-capacity",
          "domainId": "excluded-domain",
          "workspaceIdentity": { "servicePrincipalId": "excluded-identity" },
          "tags": [{ "displayName": "excluded-tag" }],
          "apiEndpoint": "https://excluded-endpoint.invalid",
          "items": ["excluded-item-data"],
          "futureProperty": "excluded-future-property"
        }
        """;

    internal static WorkspaceUpdateResponse CreateWorkspace() => new()
    {
        Id = Guid.Parse(WorkspaceId),
        DisplayName = "Finance",
        Type = "Workspace",
        Description = ""
    };

    internal static HttpResponseMessage CreateResponse(string json = WorkspaceJson, HttpStatusCode status = HttpStatusCode.OK) => new(status)
    {
        Content = new StringContent(json, Encoding.UTF8, "application/json")
    };

    internal static TokenCredential CreateCredential()
    {
        var credential = Substitute.For<TokenCredential>();
        credential.GetTokenAsync(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>())
            .Returns(new AccessToken("test-token", DateTimeOffset.MaxValue));
        return credential;
    }
}
