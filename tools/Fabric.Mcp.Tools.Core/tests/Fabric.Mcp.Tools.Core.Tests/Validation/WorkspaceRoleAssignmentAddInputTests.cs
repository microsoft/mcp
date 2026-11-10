// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Fabric.Mcp.Tools.Core.Validation;
using Xunit;
using static Fabric.Mcp.Tools.Core.Tests.TestSupport.WorkspaceRoleAssignmentTestData;

namespace Fabric.Mcp.Tools.Core.Tests.Validation;

public sealed class WorkspaceRoleAssignmentAddInputTests()
{
    [Theory]
    [InlineData("User")]
    [InlineData("Group")]
    [InlineData("ServicePrincipal")]
    [InlineData("ServicePrincipalProfile")]
    [InlineData("EntireTenant")]
    [InlineData("FuturePrincipal")]
    public static void CreateRequest_PreservesExplicitPrincipalTypesAndRolesWithoutDefaultsOrEscalation(string type)
    {
        foreach (var role in new[] { "Admin", "Member", "Contributor", "Viewer", "FutureRole", " Viewer " })
        {
            var principal = new JsonObject { ["id"] = PrincipalId.ToUpperInvariant(), ["type"] = type };
            var request = WorkspaceRoleAssignmentAddInput.CreateRequest(
                Guid.Parse(WorkspaceId).ToString("B").ToUpperInvariant(), principal.ToJsonString(), role, TestContext.Current.CancellationToken);
            Assert.Equal(Guid.Parse(WorkspaceId), request.WorkspaceId);
            Assert.Equal(Guid.Parse(PrincipalId), request.PrincipalId);
            Assert.Equal(type, request.PrincipalType);
            using var body = JsonDocument.Parse(request.Body);
            Assert.Equal(["principal", "role"], body.RootElement.EnumerateObject().Select(property => property.Name).Order());
            Assert.Equal(role, body.RootElement.GetProperty("role").GetString());
            Assert.True(JsonNode.DeepEquals(principal, JsonNode.Parse(body.RootElement.GetProperty("principal").GetRawText())));
            Assert.Equal(2, body.RootElement.GetProperty("principal").EnumerateObject().Count());
        }
    }

    [Theory]
    [InlineData("""{"groupType":"SecurityGroup"}""")]
    [InlineData("""{"groupType":"DistributionList"}""")]
    [InlineData("""{"groupType":"Unknown"}""")]
    [InlineData("""{"groupType":"FutureGroup"}""")]
    [InlineData("""{"groupType":null}""")]
    [InlineData("{}")]
    [InlineData("null")]
    public static void CreateRequest_PreservesOptionalWritableGroupDetails(string details)
    {
        var principal = $$"""{"id":"{{PrincipalId}}","type":"Group","groupDetails":{{details}}}""";
        using var original = JsonDocument.Parse(principal);
        using var body = JsonDocument.Parse(WorkspaceRoleAssignmentAddInput.CreateRequest(WorkspaceId, principal, "Viewer", TestContext.Current.CancellationToken).Body);
        Assert.True(JsonElement.DeepEquals(original.RootElement, body.RootElement.GetProperty("principal")));
    }

    [Fact]
    public static void CreateRequest_PreservesExplicitProfileParentIncludingNestedWritableDetails()
    {
        var principal = $$"""
            {"id":"{{PrincipalId}}","type":"ServicePrincipalProfile",
             "servicePrincipalProfileDetails":{"parentPrincipal":
               {"id":"{{AppId}}","type":"ServicePrincipalProfile","servicePrincipalProfileDetails":
                 {"parentPrincipal":{"id":"{{AssignmentId}}","type":"Group","groupDetails":{"groupType":"SecurityGroup"} } } } } }
            """;
        using var original = JsonDocument.Parse(principal);
        using var body = JsonDocument.Parse(WorkspaceRoleAssignmentAddInput.CreateRequest(WorkspaceId, principal, "Member", TestContext.Current.CancellationToken).Body);
        Assert.True(JsonElement.DeepEquals(original.RootElement, body.RootElement.GetProperty("principal")));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("null")]
    [InlineData("[]")]
    [InlineData("{}")]
    [InlineData("\"private\"")]
    [InlineData("{private")]
    [InlineData("""{"type":"User"}""")]
    [InlineData("""{"id":"cccccccc-2222-3333-4444-dddddddddddd"}""")]
    [InlineData("""{"id":"00000000-0000-0000-0000-000000000000","type":"User"}""")]
    [InlineData("""{"id":"private","type":"User"}""")]
    [InlineData("""{"id":42,"type":"User"}""")]
    [InlineData("""{"id":"cccccccc-2222-3333-4444-dddddddddddd","type":null}""")]
    [InlineData("""{"id":"cccccccc-2222-3333-4444-dddddddddddd","type":42}""")]
    [InlineData("""{"id":"cccccccc-2222-3333-4444-dddddddddddd","type":" "}""")]
    [InlineData("""{"id":"cccccccc-2222-3333-4444-dddddddddddd","type":"User","type":"EntireTenant"}""")]
    [InlineData("""{"id":"cccccccc-2222-3333-4444-dddddddddddd","type":"User","Type":"EntireTenant"}""")]
    [InlineData("""{"id":"cccccccc-2222-3333-4444-dddddddddddd","type":"\uD800"}""")]
    [InlineData("""{"id":"cccccccc-2222-3333-4444-dddddddddddd","type":"User","private\uDC00":0}""")]
    public static void CreateRequest_RejectsMissingWrongTypedDuplicateAndInvalidUnicodePrincipals(string? principal)
    {
        var error = Assert.Throws<FabricItemInputException>(() =>
            WorkspaceRoleAssignmentAddInput.CreateRequest(WorkspaceId, principal!, "Viewer", TestContext.Current.CancellationToken));
        Assert.DoesNotContain("private", error.Message);
        Assert.DoesNotContain(PrincipalId, error.Message);
    }

    [Theory]
    [InlineData("displayName", "\"private\"")]
    [InlineData("userDetails", """{"userPrincipalName":"private"}""")]
    [InlineData("servicePrincipalDetails", """{"aadAppId":"dddddddd-3333-4444-5555-eeeeeeeeeeee"}""")]
    [InlineData("privateField", "null")]
    [InlineData("role", "\"Admin\"")]
    [InlineData("groupDetails", """{"groupType":"SecurityGroup"}""")]
    [InlineData("servicePrincipalProfileDetails", """{"parentPrincipal":{}}""")]
    public static void CreateRequest_RejectsReadOnlyUnknownAndInapplicableFieldsRatherThanDroppingThem(string name, string value)
    {
        var principal = JsonNode.Parse(PrincipalJson)!;
        principal[name] = JsonNode.Parse(value);
        Assert.Throws<FabricItemInputException>(() =>
            WorkspaceRoleAssignmentAddInput.CreateRequest(WorkspaceId, principal.ToJsonString(), "Viewer", TestContext.Current.CancellationToken));
    }

    [Theory]
    [InlineData("Group", "groupDetails", "[]")]
    [InlineData("Group", "groupDetails", """{"groupType":42}""")]
    [InlineData("Group", "groupDetails", """{"groupType":""}""")]
    [InlineData("Group", "groupDetails", """{"private":0}""")]
    [InlineData("ServicePrincipalProfile", "servicePrincipalProfileDetails", "42")]
    [InlineData("ServicePrincipalProfile", "servicePrincipalProfileDetails", """{"parentPrincipal":[]}""")]
    [InlineData("ServicePrincipalProfile", "servicePrincipalProfileDetails", """{"parentPrincipal":{}}""")]
    [InlineData("ServicePrincipalProfile", "servicePrincipalProfileDetails", """{"private":0}""")]
    [InlineData("ServicePrincipalProfile", "servicePrincipalProfileDetails", """{"parentPrincipal":{"id":"dddddddd-3333-4444-5555-eeeeeeeeeeee","type":"User","displayName":"private"}}""")]
    public static void CreateRequest_ValidatesNestedWritableShapes(string type, string name, string value)
    {
        var principal = JsonNode.Parse(PrincipalJson)!;
        principal["type"] = type;
        principal[name] = JsonNode.Parse(value);
        var error = Assert.Throws<FabricItemInputException>(() =>
            WorkspaceRoleAssignmentAddInput.CreateRequest(WorkspaceId, principal.ToJsonString(), "Viewer", TestContext.Current.CancellationToken));
        Assert.DoesNotContain("private", error.Message);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("private")]
    [InlineData("00000000-0000-0000-0000-000000000000")]
    public static void CreateRequest_RejectsInvalidWorkspaceIds(string? workspace) =>
        Assert.Throws<FabricItemInputException>(() =>
            WorkspaceRoleAssignmentAddInput.CreateRequest(workspace!, PrincipalJson, "Viewer", TestContext.Current.CancellationToken));

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public static void CreateRequest_RejectsMissingRoles(string? role) =>
        Assert.Throws<FabricItemInputException>(() =>
            WorkspaceRoleAssignmentAddInput.CreateRequest(WorkspaceId, PrincipalJson, role!, TestContext.Current.CancellationToken));

    [Theory]
    [InlineData(0xd800)]
    [InlineData(0xdc00)]
    public static void CreateRequest_RejectsInvalidUnicodeBeforeSerializationCanReplaceIt(int codeUnit)
    {
        var invalid = new string((char)codeUnit, 1);
        Assert.Throws<FabricItemInputException>(() =>
            WorkspaceRoleAssignmentAddInput.CreateRequest(WorkspaceId, PrincipalJson, invalid, TestContext.Current.CancellationToken));
        Assert.Throws<FabricItemInputException>(() =>
            WorkspaceRoleAssignmentAddInput.CreateRequest(WorkspaceId,
                PrincipalJson.Replace("User", invalid, StringComparison.Ordinal), "Viewer", TestContext.Current.CancellationToken));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    public static void CreateRequest_EnforcesExactPrincipalUtf8InputBudget(int excess)
    {
        const int limit = 1024 * 1024;
        var prefix = $$"""{"id":"{{PrincipalId}}","type":"Future\u00e9"}""".Replace("\\u00e9", "\u00e9", StringComparison.Ordinal);
        var principal = prefix + new string(' ', limit + excess - Encoding.UTF8.GetByteCount(prefix));
        Assert.Equal(limit + excess, Encoding.UTF8.GetByteCount(principal));
        if (excess == 0)
        {
            _ = WorkspaceRoleAssignmentAddInput.CreateRequest(WorkspaceId, principal, "Viewer", TestContext.Current.CancellationToken);
        }
        else
        {
            Assert.Throws<FabricItemInputException>(() => WorkspaceRoleAssignmentAddInput.CreateRequest(WorkspaceId, principal, "Viewer", TestContext.Current.CancellationToken));
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    public static void CreateRequest_EnforcesExactSerializedRequestBudgetIncludingEscaping(int excess)
    {
        const int limit = 1024 * 1024;
        var prefix = "\u00e9";
        var overhead = Encoding.UTF8.GetByteCount(WorkspaceRoleAssignmentAddInput.CreateRequest(WorkspaceId, PrincipalJson, prefix, TestContext.Current.CancellationToken).Body);
        var role = prefix + new string('x', limit + excess - overhead);
        Assert.True(Encoding.UTF8.GetByteCount(role) < limit);
        if (excess == 0)
        {
            Assert.Equal(limit, Encoding.UTF8.GetByteCount(
                WorkspaceRoleAssignmentAddInput.CreateRequest(WorkspaceId, PrincipalJson, role, TestContext.Current.CancellationToken).Body));
        }
        else
        {
            Assert.Throws<FabricItemInputException>(() => WorkspaceRoleAssignmentAddInput.CreateRequest(WorkspaceId, PrincipalJson, role, TestContext.Current.CancellationToken));
        }
    }

    [Theory]
    [InlineData(16, 32)]
    [InlineData(17, 33)]
    public static void CreateRequest_EnforcesDocumented32LevelPrincipalBudget(int principals, int depth)
    {
        var principal = new JsonObject
        {
            ["id"] = PrincipalId,
            ["type"] = "Group",
            ["groupDetails"] = new JsonObject { ["groupType"] = "SecurityGroup" }
        };
        if (depth == 33)
        {
            principal.Remove("groupDetails");
        }
        for (var index = 1; index < principals; index++)
        {
            principal = new JsonObject
            {
                ["id"] = PrincipalId,
                ["type"] = "ServicePrincipalProfile",
                ["servicePrincipalProfileDetails"] = new JsonObject { ["parentPrincipal"] = principal }
            };
        }
        using var json = JsonDocument.Parse(principal.ToJsonString());
        Assert.Equal(depth, JsonDepth(json.RootElement));
        if (depth == 32)
        {
            _ = WorkspaceRoleAssignmentAddInput.CreateRequest(WorkspaceId, principal.ToJsonString(), "Viewer", TestContext.Current.CancellationToken);
        }
        else
        {
            Assert.Throws<FabricItemInputException>(() =>
                WorkspaceRoleAssignmentAddInput.CreateRequest(WorkspaceId, principal.ToJsonString(), "Viewer", TestContext.Current.CancellationToken));
        }
    }
}
