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

internal static class WorkspaceRoleAssignmentTestData
{
    internal const string WorkspaceId = "aaaaaaaa-0000-1111-2222-bbbbbbbbbbbb";
    internal const string AssignmentId = "bbbbbbbb-1111-2222-3333-cccccccccccc";
    internal const string PrincipalId = "cccccccc-2222-3333-4444-dddddddddddd";
    internal const string AppId = "dddddddd-3333-4444-5555-eeeeeeeeeeee";
    // cspell:disable-next-line
    internal const string DocumentedToken = "LDEsMTAwMDAwLDA%3D";
    internal static string CollectionUrl => $"{FabricEndpoints.GetFabricApiBaseUrl()}/workspaces/{WorkspaceId}/roleAssignments";
    internal static string AssignmentUrl => $"{CollectionUrl}/{AssignmentId}";
    internal static string AssignmentJson => JsonSerializer.Serialize(Assignment, CoreJsonContext.Default.FabricWorkspaceRoleAssignment);
    internal static string PrincipalJson => $$"""{"id":"{{PrincipalId}}","type":"User"}""";
    internal static FabricWorkspaceRoleAssignment Assignment => new(
        Guid.Parse(AssignmentId),
        new(Guid.Parse(PrincipalId), "User", "Analyst", new("analyst@example.invalid")),
        "Viewer");

    internal static string Page(string? token = null, string? uri = null, bool empty = false)
    {
        var page = new JsonObject
        {
            ["value"] = empty ? new JsonArray() : new JsonArray(
                JsonSerializer.SerializeToNode(Assignment, CoreJsonContext.Default.FabricWorkspaceRoleAssignment))
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

    internal static string NestedPrincipalPage(int count) =>
        new JsonObject { ["value"] = new JsonArray(JsonNode.Parse(NestedPrincipalAssignment(count))) }.ToJsonString();

    internal static string NestedPrincipalAssignment(int count, bool leafDetails = true)
    {
        var principal = new JsonObject
        {
            ["id"] = PrincipalId,
            ["type"] = "User"
        };
        if (leafDetails)
        {
            principal["userDetails"] = new JsonObject { ["userPrincipalName"] = "" };
        }
        for (var index = 1; index < count; index++)
        {
            principal = new JsonObject
            {
                ["id"] = PrincipalId,
                ["type"] = "ServicePrincipalProfile",
                ["servicePrincipalProfileDetails"] = new JsonObject { ["parentPrincipal"] = principal }
            };
        }
        return new JsonObject
        {
            ["id"] = AssignmentId,
            ["role"] = "Member",
            ["principal"] = principal
        }.ToJsonString();
    }

    internal static TokenCredential Credential()
    {
        var credential = Substitute.For<TokenCredential>();
        credential.GetTokenAsync(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>())
            .Returns(new AccessToken("role-list-test-token", DateTimeOffset.MaxValue));
        return credential;
    }

    internal static int JsonDepth(JsonElement value) => value.ValueKind switch
    {
        JsonValueKind.Object => 1 + value.EnumerateObject().Select(property => JsonDepth(property.Value)).DefaultIfEmpty().Max(),
        JsonValueKind.Array => 1 + value.EnumerateArray().Select(JsonDepth).DefaultIfEmpty().Max(),
        _ => 0
    };

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
