// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Concurrent;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Azure.Core;
using Azure.Identity;
using Fabric.Mcp.Tools.Core.Models;
using Fabric.Mcp.Tools.Core.Services;
using Fabric.Mcp.Tools.Core.Tests.TestSupport;
using NSubstitute;
using Xunit;
using static Fabric.Mcp.Tools.Core.Tests.TestSupport.WorkspaceRoleAssignmentTestData;

namespace Fabric.Mcp.Tools.Core.Tests.Services;

public sealed class WorkspaceRoleAssignmentListServiceTests()
{
    [Fact]
    public static async Task List_SendsOneAuthenticatedGetWithoutBodyLookupsOrEnrichment()
    {
        var page = JsonNode.Parse(Page())!;
        page["value"]![0]!["privateExtension"] = "private-content";
        page["value"]![0]!["principal"]!["privateExtension"] = "private-content";
        using var stream = new OperationTestStream(Encoding.UTF8.GetBytes(page.ToJsonString()));
        using var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StreamContent(stream) };
        response.Content.Headers.ContentType = new("application/json");
        response.Headers.Location = new Uri("https://example.invalid/never-follow");
        response.Headers.TryAddWithoutValidation("x-ms-operation-id", "not-an-operation");
        using var handler = new FabricCoreHttpMessageHandler((request, token) =>
        {
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal(CollectionUrl, request.RequestUri?.AbsoluteUri);
            Assert.Null(request.Content);
            Assert.Equal("Bearer", request.Headers.Authorization?.Scheme);
            Assert.Equal("role-list-test-token", request.Headers.Authorization?.Parameter);
            Assert.Equal("Fabric Core MCP", request.Headers.UserAgent.ToString());
            Assert.True(token.CanBeCanceled);
            return Task.FromResult(response);
        });
        using var client = new HttpClient(handler);
        var credential = Credential();

        var result = await new FabricCoreService(client, credential).ListWorkspaceRoleAssignmentsAsync(
            Guid.Parse(WorkspaceId).ToString("B").ToUpperInvariant(), cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(Assignment, Assert.Single(result.RoleAssignments));
        Assert.NotEqual(result.RoleAssignments[0].Id, result.RoleAssignments[0].Principal.Id);
        Assert.Null(result.ContinuationToken);
        var output = JsonSerializer.SerializeToElement(result, CoreJsonContext.Default.WorkspaceRoleAssignmentListCommandResult);
        Assert.Equal("roleAssignments", Assert.Single(output.EnumerateObject()).Name);
        Assert.Equal(["id", "principal", "role"], output.GetProperty("roleAssignments")[0].EnumerateObject().Select(property => property.Name).Order());
        Assert.DoesNotContain("private", output.GetRawText());
        Assert.Equal(1, handler.CallCount);
        Assert.True(stream.Disposed);
        Assert.Throws<ObjectDisposedException>(() => response.Content.ReadAsStream(TestContext.Current.CancellationToken));
        await credential.Received(1).GetTokenAsync(
            Arg.Is<TokenRequestContext>(context => context.Scopes.SequenceEqual(FabricEndpoints.FabricScopes)),
            TestContext.Current.CancellationToken);
    }

    [Theory]
    [InlineData("Admin")]
    [InlineData("Member")]
    [InlineData("Contributor")]
    [InlineData("Viewer")]
    [InlineData("FutureRole")]
    public static async Task List_PreservesAllDocumentedPrincipalDetailsAndFutureStrings(string role)
    {
        var user = Assignment.Principal;
        FabricPrincipal[] principals =
        [
            user,
            new(Guid.Parse(PrincipalId), "ServicePrincipal", ServicePrincipalDetails: new(Guid.Parse(AppId))),
            new(Guid.Parse(PrincipalId), "Group", GroupDetails: new("SecurityGroup")),
            new(Guid.Parse(PrincipalId), "Group", GroupDetails: new("DistributionList")),
            new(Guid.Parse(PrincipalId), "Group", GroupDetails: new("Unknown")),
            new(Guid.Parse(PrincipalId), "Group", GroupDetails: new("FutureGroup")),
            new(Guid.Parse(PrincipalId), "ServicePrincipalProfile",
                ServicePrincipalProfileDetails: new(new(Guid.Parse(AppId), "ServicePrincipal", ServicePrincipalDetails: new(Guid.Parse(AppId))))),
            new(Guid.Parse(PrincipalId), "EntireTenant"),
            new(Guid.Parse(PrincipalId), "FuturePrincipal")
        ];
        var assignments = principals.Select(principal => Assignment with { Role = role, Principal = principal }).ToArray();
        var page = new JsonObject
        {
            ["value"] = new JsonArray(assignments.Select(assignment =>
                JsonSerializer.SerializeToNode(assignment, CoreJsonContext.Default.FabricWorkspaceRoleAssignment)).ToArray())
        };
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(Response(page.ToJsonString())));
        using var client = new HttpClient(handler);
        var result = await ReadAsync(new FabricCoreService(client, Credential()));
        Assert.Equal(assignments, result.RoleAssignments);
        var serialized = JsonSerializer.Serialize(result, CoreJsonContext.Default.WorkspaceRoleAssignmentListCommandResult);
        Assert.Equal(assignments, JsonSerializer.Deserialize(serialized, CoreJsonContext.Default.WorkspaceRoleAssignmentListCommandResult)!.RoleAssignments);
        Assert.Equal(1, handler.CallCount);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public static async Task List_AcceptsAbsentOrNullOptionalPrincipalMetadata(bool explicitNull)
    {
        var page = JsonNode.Parse(Page())!;
        var principal = page["value"]![0]!["principal"]!.AsObject();
        foreach (var name in new[] { "displayName", "userDetails", "groupDetails", "servicePrincipalDetails", "servicePrincipalProfileDetails" })
        {
            principal.Remove(name);
            if (explicitNull)
            {
                principal[name] = null;
            }
        }
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(Response(page.ToJsonString())));
        using var client = new HttpClient(handler);
        var result = await ReadAsync(new FabricCoreService(client, Credential()));
        Assert.Equal(new FabricPrincipal(Guid.Parse(PrincipalId), "User"), Assert.Single(result.RoleAssignments).Principal);
        Assert.DoesNotContain("displayName", JsonSerializer.Serialize(result, CoreJsonContext.Default.WorkspaceRoleAssignmentListCommandResult));
    }

    [Theory]
    [InlineData("{}")]
    [InlineData("""{"userPrincipalName":null}""")]
    [InlineData("""{"userPrincipalName":""}""")]
    [InlineData("""{"userPrincipalName":" \u00e9\ud83d\ude80 "}""")]
    public static async Task List_PreservesEmptyNamesAndOptionalDetailObjects(string details)
    {
        var page = JsonNode.Parse(Page())!;
        var principal = page["value"]![0]!["principal"]!;
        principal["displayName"] = "";
        principal["userDetails"] = JsonNode.Parse(details);
        principal["groupDetails"] = new JsonObject { ["groupType"] = null };
        principal["servicePrincipalDetails"] = new JsonObject { ["aadAppId"] = null };
        principal["servicePrincipalProfileDetails"] = new JsonObject { ["parentPrincipal"] = null };
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(Response(page.ToJsonString())));
        using var client = new HttpClient(handler);
        var result = await ReadAsync(new FabricCoreService(client, Credential()));
        var actual = Assert.Single(result.RoleAssignments).Principal;
        Assert.Equal("", actual.DisplayName);
        Assert.Equal(JsonNode.Parse(details)!["userPrincipalName"]?.GetValue<string>(), actual.UserDetails?.UserPrincipalName);
        Assert.Equal(new FabricGroupDetails(), actual.GroupDetails);
        Assert.Equal(new FabricServicePrincipalDetails(), actual.ServicePrincipalDetails);
        Assert.Equal(new FabricServicePrincipalProfileDetails(), actual.ServicePrincipalProfileDetails);
    }

    [Theory]
    [InlineData(false, false, false)]
    [InlineData(false, false, true)]
    [InlineData(false, true, false)]
    [InlineData(false, true, true)]
    [InlineData(true, false, false)]
    [InlineData(true, false, true)]
    [InlineData(true, true, false)]
    [InlineData(true, true, true)]
    public static async Task List_AcceptsAbsentOrNullContinuationOnEmptyAndNonemptyFinalPages(bool nullToken, bool nullUri, bool empty)
    {
        var page = JsonNode.Parse(Page(empty: empty))!.AsObject();
        if (nullToken)
        {
            page["continuationToken"] = null;
        }
        if (nullUri)
        {
            page["continuationUri"] = null;
        }
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(Response(page.ToJsonString())));
        using var client = new HttpClient(handler);
        var result = await ReadAsync(new FabricCoreService(client, Credential()));
        Assert.Equal(empty ? [] : new[] { Assignment }, result.RoleAssignments);
        Assert.Null(result.ContinuationToken);
        Assert.DoesNotContain("continuation", JsonSerializer.Serialize(result, CoreJsonContext.Default.WorkspaceRoleAssignmentListCommandResult));
        Assert.Equal(1, handler.CallCount);
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public static async Task List_ContinuesTokenOnlyPagesOnlyOnExplicitCallForSameWorkspace(bool nullUri, bool empty)
    {
        var page = JsonNode.Parse(Page(DocumentedToken, empty: empty))!;
        if (nullUri)
        {
            page["continuationUri"] = null;
        }
        var requests = new List<string>();
        using var handler = new FabricCoreHttpMessageHandler((request, _) =>
        {
            requests.Add(request.RequestUri!.AbsoluteUri);
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Null(request.Content);
            return Task.FromResult(Response(requests.Count == 1 ? page.ToJsonString() : """{"value":[]}"""));
        });
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, Credential());
        var first = await ReadAsync(service);
        Assert.Equal(empty ? [] : new[] { Assignment }, first.RoleAssignments);
        Assert.Equal(DocumentedToken, first.ContinuationToken);
        Assert.Equal([CollectionUrl], requests);
        var second = await ReadAsync(service, first.ContinuationToken);
        Assert.Empty(second.RoleAssignments);
        Assert.Null(second.ContinuationToken);
        Assert.Equal([CollectionUrl, $"{CollectionUrl}?continuationToken={DocumentedToken}"], requests);
        Assert.Equal(2, handler.CallCount);
    }

    [Theory]
    [InlineData(DocumentedToken, DocumentedToken)]
    [InlineData("raw+/= &?#", "raw%2B%2F%3D%20%26%3F%23")]
    [InlineData(" raw ", "%20raw%20")]
    [InlineData("%3D%2f%26%3F%23%2B%25", "%3D%2f%26%3F%23%2B%25")]
    [InlineData("tail%2", "tail%252")]
    [InlineData("%G1%", "%25G1%25")]
    [InlineData("%2541%2526", "%2541%2526")]
    [InlineData("%41%7e%2D%5f%2E", "A~-_.")]
    [InlineData("\u00e9\U0001F680 /", "%C3%A9%F0%9F%9A%80%20%2F")]
    [InlineData("%0D%0A", "%0D%0A")]
    public static async Task List_PreservesOpaqueTokensAndEncodesOnlyRawSpans(string token, string encoded)
    {
        var requests = new List<string>();
        using var handler = new FabricCoreHttpMessageHandler((request, _) =>
        {
            requests.Add(request.RequestUri!.AbsoluteUri);
            Assert.Empty(request.RequestUri.Fragment);
            return Task.FromResult(Response(requests.Count == 1
                ? Page(token, $"{CollectionUrl}?continuationToken={FabricCoreHttpHelpers.EncodeContinuationToken(token)}")
                : """{"value":[]}"""));
        });
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, Credential());
        var first = await ReadAsync(service);
        Assert.Equal(token, first.ContinuationToken);
        Assert.Equal([CollectionUrl], requests);
        var second = await ReadAsync(service, first.ContinuationToken);
        Assert.Empty(second.RoleAssignments);
        Assert.Equal([CollectionUrl, $"{CollectionUrl}?continuationToken={encoded}"], requests);
    }

    [Theory]
    [InlineData("opaque/../cursor", "opaque/../cursor")]
    [InlineData("opaque/./cursor", "opaque/./cursor")]
    [InlineData("opaque/../cursor", "opaque/%2E%2E/cursor")]
    public static async Task List_DoesNotTreatOpaqueQueryDotSegmentsAsPathTraversal(string token, string queryValue)
    {
        using var handler = new FabricCoreHttpMessageHandler((request, _) =>
        {
            Assert.Equal(new Uri(CollectionUrl).AbsolutePath, request.RequestUri!.AbsolutePath);
            return Task.FromResult(Response(Page(token, $"{CollectionUrl}?continuationToken={queryValue}")));
        });
        using var client = new HttpClient(handler);
        Assert.Equal(token, (await ReadAsync(new FabricCoreService(client, Credential()), token)).ContinuationToken);
        Assert.Equal(1, handler.CallCount);
    }

    public static TheoryData<string> UntrustedUris => new()
    {
        "",
        "https://example.invalid/private?continuationToken=next",
        CollectionUrl.Replace("https:", "http:", StringComparison.Ordinal) + "?continuationToken=next",
        CollectionUrl.Replace("api.fabric.microsoft.com", "api.fabric.microsoft.com:444", StringComparison.Ordinal) + "?continuationToken=next",
        CollectionUrl.Replace("api.fabric.microsoft.com", "user@api.fabric.microsoft.com", StringComparison.Ordinal) + "?continuationToken=next",
        CollectionUrl.Replace(WorkspaceId, AssignmentId, StringComparison.Ordinal) + "?continuationToken=next",
        CollectionUrl.Replace("/roleAssignments", "/./roleAssignments", StringComparison.Ordinal) + "?continuationToken=next",
        CollectionUrl.Replace("/roleAssignments", "/private/../roleAssignments", StringComparison.Ordinal) + "?continuationToken=next",
        CollectionUrl.Replace("/roleAssignments", "/%72oleAssignments", StringComparison.Ordinal) + "?continuationToken=next",
        CollectionUrl.Replace("/roleAssignments", "\\roleAssignments", StringComparison.Ordinal) + "?continuationToken=next",
        "/v1/workspaces/" + WorkspaceId + "/roleAssignments?continuationToken=next",
        CollectionUrl + "/" + AssignmentId + "?continuationToken=next",
        CollectionUrl,
        CollectionUrl + "?continuationToken=wrong",
        CollectionUrl + "?continuationToken=next#private",
        CollectionUrl + "?continuationToken=next&extra=private",
        CollectionUrl + "?continuationToken=next&continuationToken=next",
        CollectionUrl + "?ContinuationToken=next",
        CollectionUrl + "?continuationToken=next&",
        CollectionUrl + "?role=Admin"
    };

    [Theory]
    [MemberData(nameof(UntrustedUris))]
    public static async Task List_RejectsUntrustedOrContradictoryContinuationUris(string uri) =>
        await AssertInvalidPageAsync(Page("next", uri));

    [Theory]
    [InlineData("""{"continuationToken":""}""")]
    [InlineData("""{"continuationToken":" "}""")]
    [InlineData("""{"continuationToken":false}""")]
    [InlineData("""{"continuationToken":0}""")]
    [InlineData("""{"continuationToken":[]}""")]
    [InlineData("""{"continuationToken":{}}""")]
    [InlineData("""{"continuationToken":"private\r\n"}""")]
    [InlineData("""{"continuationToken":"\uD800"}""")]
    [InlineData("""{"continuationUri":""}""")]
    [InlineData("""{"continuationUri":false}""")]
    [InlineData("""{"continuationUri":0}""")]
    [InlineData("""{"continuationUri":[]}""")]
    [InlineData("""{"continuationUri":{}}""")]
    [InlineData("""{"continuationToken":null,"continuationUri":"https://example.invalid/private"}""")]
    [InlineData("""{"continuationToken":false,"continuationUri":null}""")]
    [InlineData("""{"continuationToken":"next","continuationUri":""}""")]
    [InlineData("""{"continuationToken":"next","continuationUri":42}""")]
    [InlineData("""{"continuationToken":"next","ContinuationToken":"another"}""")]
    public static async Task List_RejectsInvalidContinuationRatherThanImplyingCompletion(string metadata) =>
        await AssertInvalidPageAsync($"{{\"value\":[],{metadata[1..]}");

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public static async Task List_RejectsEvenTrustedUriOnlyPages(bool nullToken)
    {
        var page = JsonNode.Parse(Page(uri: $"{CollectionUrl}?continuationToken=next"))!;
        if (nullToken)
        {
            page["continuationToken"] = null;
        }
        await AssertInvalidPageAsync(page.ToJsonString());
    }

    [Fact]
    public static async Task List_PreservesOrderRepeatedEntriesAndCompletePagesWithoutACountLimit()
    {
        var assignments = Enumerable.Range(1, 1001).Select(index => Assignment with
        {
            Id = Guid.Parse($"00000000-0000-4000-8000-{index:000000000000}")
        }).Reverse().ToList();
        assignments.Add(assignments[0]);
        var page = new JsonObject
        {
            ["value"] = new JsonArray(assignments.Select(assignment =>
                JsonSerializer.SerializeToNode(assignment, CoreJsonContext.Default.FabricWorkspaceRoleAssignment)).ToArray())
        };
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(Response(page.ToJsonString())));
        using var client = new HttpClient(handler);
        var result = await ReadAsync(new FabricCoreService(client, Credential()));
        Assert.Equal(assignments, result.RoleAssignments);
        Assert.Equal(1002, result.RoleAssignments.Count);
        Assert.Equal(1, handler.CallCount);
    }

    [Theory]
    [InlineData("")]
    [InlineData("{private")]
    [InlineData("null")]
    [InlineData("[]")]
    [InlineData("{}")]
    [InlineData("""{"value":null}""")]
    [InlineData("""{"value":{}}""")]
    [InlineData("""{"value":[null]}""")]
    [InlineData("""{"value":[{}]}""")]
    [InlineData("""{"value":[1]}""")]
    [InlineData("""{"value":[],"value":[]}""")]
    [InlineData("""{"value":[],"Value":[{}]}""")]
    // cspell:disable-next-line
    [InlineData("""{"value":[],"\u0076alue":[]}""")]
    [InlineData("""{"value":[],"extra":{"same":1,"same":2}}""")]
    [InlineData("""{"value":[],"extra":{"text":"\uD800"}}""")]
    [InlineData("""{"value":[],"extra":{"name\uDC00":"private"}}""")]
    public static async Task List_RejectsMalformedMissingDuplicateAndInvalidUnicodeMetadata(string json) =>
        await AssertInvalidPageAsync(json);

    [Theory]
    [InlineData("id", "\"00000000-0000-0000-0000-000000000000\"")]
    [InlineData("id", "\"private\"")]
    [InlineData("id", "null")]
    [InlineData("principal", "null")]
    [InlineData("principal", "[]")]
    [InlineData("principal", "42")]
    [InlineData("principal", "{}")]
    [InlineData("role", "null")]
    [InlineData("role", "\" \"")]
    [InlineData("role", "42")]
    [InlineData("role", "\"\\uD800\"")]
    public static async Task List_RejectsInvalidLaterRowsWithoutPartialResults(string name, string value)
    {
        var valid = JsonSerializer.Serialize(Assignment, CoreJsonContext.Default.FabricWorkspaceRoleAssignment);
        var invalid = JsonNode.Parse(valid)!.AsObject();
        invalid.Remove(name);
        var invalidJson = invalid.ToJsonString();
        await AssertInvalidPageAsync($"{{\"value\":[{valid},{invalidJson[..^1]},\"{name}\":{value}}}]}}");
    }

    [Theory]
    [InlineData("id")]
    [InlineData("principal")]
    [InlineData("role")]
    [InlineData("principal.id")]
    [InlineData("principal.type")]
    public static async Task List_RejectsMissingRequiredFields(string path)
    {
        var page = JsonNode.Parse(Page())!;
        var parts = path.Split('.');
        var parent = parts.Length == 1 ? page["value"]![0]! : page["value"]![0]!["principal"]!;
        parent.AsObject().Remove(parts[^1]);
        await AssertInvalidPageAsync(page.ToJsonString());
    }

    [Theory]
    [InlineData("id", "null")]
    [InlineData("id", "\"private\"")]
    [InlineData("id", "\"00000000-0000-0000-0000-000000000000\"")]
    [InlineData("type", "null")]
    [InlineData("type", "\" \"")]
    [InlineData("type", "42")]
    [InlineData("displayName", "42")]
    [InlineData("displayName", "\"\\uD800\"")]
    [InlineData("userDetails", "42")]
    [InlineData("userDetails", """{"userPrincipalName":false}""")]
    [InlineData("groupDetails", "[]")]
    [InlineData("groupDetails", """{"groupType":0}""")]
    [InlineData("groupDetails", """{"groupType":" "}""")]
    [InlineData("servicePrincipalDetails", """{"aadAppId":"private"}""")]
    [InlineData("servicePrincipalDetails", """{"aadAppId":"00000000-0000-0000-0000-000000000000"}""")]
    [InlineData("servicePrincipalProfileDetails", "false")]
    [InlineData("servicePrincipalProfileDetails", """{"parentPrincipal":{}}""")]
    [InlineData("servicePrincipalProfileDetails", """{"parentPrincipal":42}""")]
    public static async Task List_RejectsMalformedKnownPrincipalFields(string name, string value)
    {
        var page = JsonNode.Parse(Page())!;
        var principal = page["value"]![0]!["principal"]!.AsObject();
        principal.Remove(name);
        var json = principal.ToJsonString();
        var malformed = $"{json[..^1]},\"{name}\":{value}}}";
        await AssertInvalidPageAsync($"{{\"value\":[{{\"id\":\"{AssignmentId}\",\"role\":\"Admin\",\"principal\":{malformed}}}]}}");
    }

    [Fact]
    public static async Task List_RejectsDuplicatePrincipalFieldsAndMalformedUtf8EvenInOmittedFields()
    {
        var page = Page();
        await AssertInvalidPageAsync(page.Replace("\"type\":\"User\"", "\"type\":\"User\",\"Type\":\"Group\"", StringComparison.Ordinal));
        var body = Encoding.UTF8.GetBytes("{\"value\":[],\"extra\":\"\u00e9\"}");
        body[Array.IndexOf(body, (byte)0xc3)] = 0xff;
        using var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(body) };
        response.Content.Headers.ContentType = new("application/json");
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
        using var client = new HttpClient(handler);
        await Assert.ThrowsAsync<JsonException>(() => ReadAsync(new FabricCoreService(client, Credential())));
        Assert.Throws<ObjectDisposedException>(() => response.Content.ReadAsStream(TestContext.Current.CancellationToken));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("00000000-0000-0000-0000-000000000000")]
    [InlineData("../roleAssignments")]
    [InlineData("https://example.invalid")]
    [InlineData(WorkspaceId + "?query=private")]
    public static async Task List_ValidatesWorkspaceBeforeAuthentication(string? value)
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) => throw new InvalidOperationException("No HTTP expected."));
        using var client = new HttpClient(handler);
        var credential = Credential();
        var error = await Assert.ThrowsAsync<ArgumentException>(() =>
            new FabricCoreService(client, credential).ListWorkspaceRoleAssignmentsAsync(value!, cancellationToken: TestContext.Current.CancellationToken));
        Assert.Equal("workspaceId", error.ParamName);
        Assert.Empty(credential.ReceivedCalls());
        Assert.Equal(0, handler.CallCount);
    }

    [Fact]
    public static async Task List_RejectsInvalidTokensBeforeAuthentication()
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) => throw new InvalidOperationException("No HTTP expected."));
        using var client = new HttpClient(handler);
        var credential = Credential();
        foreach (var token in new[] { "", " \u00a0", "private\0", "private\r\n", "private\t", "\uD800", "\uDC00" })
        {
            var error = await Assert.ThrowsAsync<ArgumentException>(() => ReadAsync(new FabricCoreService(client, credential), token));
            Assert.Equal("continuationToken", error.ParamName);
            Assert.DoesNotContain("private", error.Message);
        }
        Assert.Empty(credential.ReceivedCalls());
        Assert.Equal(0, handler.CallCount);
    }

    [Theory]
    [InlineData(false, 0)]
    [InlineData(false, 1)]
    [InlineData(true, 0)]
    [InlineData(true, 1)]
    public static async Task List_EnforcesExactUtf8TokenBudgetOnInputAndOutput(bool multibyte, int excess)
    {
        var token = new string(multibyte ? '\u00e9' : 'a', multibyte ? 4096 : 8192) + new string('x', excess);
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(Response(Page(token))));
        using var client = new HttpClient(handler);
        var credential = Credential();
        var service = new FabricCoreService(client, credential);
        if (excess == 0)
        {
            Assert.Equal(token, (await ReadAsync(service, token)).ContinuationToken);
        }
        else
        {
            await Assert.ThrowsAsync<ArgumentException>(() => ReadAsync(service, token));
            Assert.Empty(credential.ReceivedCalls());
            Assert.Equal(0, handler.CallCount);
            await Assert.ThrowsAsync<JsonException>(() => ReadAsync(service));
        }
        Assert.Equal(1, handler.CallCount);
    }

    [Theory]
    [InlineData(false, 0)]
    [InlineData(false, 1)]
    [InlineData(true, 0)]
    [InlineData(true, 1)]
    public static async Task List_EnforcesExactResponseByteBudgetWithoutTruncation(bool knownLength, int excess)
    {
        const int limit = 1024 * 1024;
        Assert.Equal(limit, FabricResponseBodyReader.DefaultMaxJsonBytes);
        using var stream = new PaddedJsonTestStream(Encoding.UTF8.GetBytes(Page()), limit + excess);
        using var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StreamContent(stream) };
        response.Content.Headers.ContentType = new("application/json");
        if (knownLength)
        {
            response.Content.Headers.ContentLength = limit + excess;
        }
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, Credential());
        if (excess == 0)
        {
            Assert.Equal(Assignment, Assert.Single((await ReadAsync(service)).RoleAssignments));
        }
        else
        {
            Assert.Contains("1 MiB", (await Assert.ThrowsAsync<JsonException>(() => ReadAsync(service))).Message);
        }
        Assert.Equal(knownLength && excess > 0 ? 0 : limit + excess, stream.BytesRead);
        Assert.True(stream.Disposed);
        Assert.Equal(1, handler.CallCount);
    }

    [Theory]
    [InlineData(64)]
    [InlineData(65)]
    public static async Task List_Enforces64InputLevelsWithoutRetainingUnknownJson(int depth)
    {
        var json = "{\"value\":[],\"extra\":" + new string('[', depth - 1) + "0" + new string(']', depth - 1) + "}";
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(Response(json)));
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, Credential());
        if (depth == 64)
        {
            Assert.Equal("{\"roleAssignments\":[]}", JsonSerializer.Serialize(
                await ReadAsync(service), CoreJsonContext.Default.WorkspaceRoleAssignmentListCommandResult));
        }
        else
        {
            await Assert.ThrowsAsync<JsonException>(() => ReadAsync(service));
        }
    }

    [Theory]
    [InlineData(29)]
    [InlineData(30)]
    public static async Task List_ReservesEnvelopeDepthWithoutTruncatingRecursivePrincipalMetadata(int count)
    {
        var json = NestedPrincipalPage(count);
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(Response(json)));
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, Credential());
        if (count == 29)
        {
            var result = await ReadAsync(service);
            var principal = Assert.Single(result.RoleAssignments).Principal;
            for (var index = 1; index < count; index++)
            {
                principal = Assert.IsType<FabricPrincipal>(principal.ServicePrincipalProfileDetails?.ParentPrincipal);
            }
            Assert.Equal("", principal.UserDetails?.UserPrincipalName);
            using var writer = new Utf8JsonWriter(Stream.Null, new JsonWriterOptions { MaxDepth = 61 });
            JsonSerializer.Serialize(writer, result, CoreJsonContext.Default.WorkspaceRoleAssignmentListCommandResult);
        }
        else
        {
            await Assert.ThrowsAsync<JsonException>(() => ReadAsync(service));
        }
        Assert.Equal(1, handler.CallCount);
    }

    [Theory]
    [InlineData(HttpStatusCode.Created)]
    [InlineData(HttpStatusCode.Accepted)]
    [InlineData(HttpStatusCode.NoContent)]
    [InlineData(HttpStatusCode.PartialContent)]
    [InlineData(HttpStatusCode.Found)]
    [InlineData(HttpStatusCode.TemporaryRedirect)]
    [InlineData(HttpStatusCode.BadRequest)]
    [InlineData(HttpStatusCode.Unauthorized)]
    [InlineData(HttpStatusCode.Forbidden)]
    [InlineData(HttpStatusCode.NotFound)]
    [InlineData(HttpStatusCode.TooManyRequests)]
    [InlineData(HttpStatusCode.ServiceUnavailable)]
    public static async Task List_PreservesHttpStatusAndRetryGuidanceWithoutReadingErrorsOrFollowingUrls(HttpStatusCode status)
    {
        using var content = new UnreadableHttpContent();
        using var response = new HttpResponseMessage(status) { Content = content };
        response.Headers.Location = new Uri("https://example.invalid/private");
        response.Headers.TryAddWithoutValidation("Retry-After", "45");
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
        using var client = new HttpClient(handler);
        var error = await Assert.ThrowsAsync<WorkspaceRoleAssignmentListRequestException>(() => ReadAsync(new FabricCoreService(client, Credential())));
        Assert.Equal(response.IsSuccessStatusCode ? HttpStatusCode.BadGateway : status, error.StatusCode);
        Assert.Equal(TimeSpan.FromSeconds(45), error.RetryAfter?.Delta);
        Assert.True(content.IsDisposed);
        Assert.Equal(1, handler.CallCount);
    }

    [Theory]
    [InlineData("0", 0L)]
    [InlineData("45", 45L)]
    [InlineData("2147483647", 2147483647L)]
    [InlineData(null, null)]
    [InlineData("", null)]
    [InlineData("-1", null)]
    [InlineData("1.5", null)]
    [InlineData("2147483648", null)]
    [InlineData("45, 60", null)]
    [InlineData("private", null)]
    public static async Task List_ReturnsOnlyValidatedRetrySeconds(string? header, long? expected)
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(Response(Page(), retryAfter: header)));
        using var client = new HttpClient(handler);
        var result = await ReadAsync(new FabricCoreService(client, Credential()));
        Assert.Equal(expected, result.RetryAfterSeconds);
        Assert.Null(result.RetryAfterUtc);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public static async Task List_PreservesRetryDateButRejectsMultipleRetryHeaders(bool multiple)
    {
        using var response = Response(Page());
        response.Headers.TryAddWithoutValidation("Retry-After", multiple ? ["45", "60"] : ["Tue, 01 Jan 2030 00:00:00 GMT"]);
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
        using var client = new HttpClient(handler);
        var result = await ReadAsync(new FabricCoreService(client, Credential()));
        Assert.Equal(multiple ? null : new DateTimeOffset(2030, 1, 1, 0, 0, 0, TimeSpan.Zero), result.RetryAfterUtc);
        Assert.Null(result.RetryAfterSeconds);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("text/html")]
    [InlineData("application/octet-stream")]
    public static async Task List_RejectsUnsupportedMediaBeforeReading(string? mediaType)
    {
        using var content = new UnreadableHttpContent();
        content.Headers.ContentType = mediaType is null ? null : new(mediaType);
        using var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = content };
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
        using var client = new HttpClient(handler);
        await Assert.ThrowsAsync<JsonException>(() => ReadAsync(new FabricCoreService(client, Credential())));
        Assert.True(content.IsDisposed);
    }

    [Fact]
    public static async Task List_DoesNotMisclassifyUnrelatedReaderFailuresAsMalformedJson()
    {
        using var stream = new OperationTestStream(Encoding.UTF8.GetBytes(Page()),
            (_, _) => throw new InvalidOperationException("private-reader-failure"));
        using var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StreamContent(stream) };
        response.Content.Headers.ContentType = new("application/json");
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
        using var client = new HttpClient(handler);
        await Assert.ThrowsAsync<InvalidOperationException>(() => ReadAsync(new FabricCoreService(client, Credential())));
        Assert.True(stream.Disposed);
    }

    [Fact]
    public static async Task List_AuthenticationFailureSendsNoHttp()
    {
        var credential = Credential();
        credential.GetTokenAsync(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>())
            .Returns(ValueTask.FromException<AccessToken>(new AuthenticationFailedException("private")));
        using var handler = new FabricCoreHttpMessageHandler((_, _) => throw new InvalidOperationException("No HTTP expected."));
        using var client = new HttpClient(handler);
        await Assert.ThrowsAsync<AuthenticationFailedException>(() => ReadAsync(new FabricCoreService(client, credential)));
        Assert.Equal(0, handler.CallCount);
    }

    [Theory]
    [InlineData("before")]
    [InlineData("authentication")]
    [InlineData("http")]
    [InlineData("headers")]
    [InlineData("body")]
    [InlineData("end-of-body")]
    public static async Task List_PropagatesCancellationAndDisposesAvailableResponses(string phase)
    {
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        var credential = Credential();
        credential.GetTokenAsync(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>()).Returns(_ =>
        {
            if (phase == "authentication")
            {
                cancellation.Cancel();
            }
            return new AccessToken("role-list-test-token", DateTimeOffset.MaxValue);
        });
        using var stream = new OperationTestStream(Encoding.UTF8.GetBytes(Page()), (count, token) =>
        {
            if (phase == "body" || (phase == "end-of-body" && count == 0))
            {
                cancellation.Cancel();
                if (phase == "body")
                {
                    token.ThrowIfCancellationRequested();
                }
            }
            return ValueTask.CompletedTask;
        });
        using var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StreamContent(stream) };
        response.Content.Headers.ContentType = new("application/json");
        using var handler = new FabricCoreHttpMessageHandler((_, token) =>
        {
            if (phase is "http" or "headers")
            {
                cancellation.Cancel();
            }
            return phase == "http" ? Task.FromCanceled<HttpResponseMessage>(token) : Task.FromResult(response);
        });
        using var client = new HttpClient(handler);
        if (phase == "before")
        {
            await cancellation.CancelAsync();
        }
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            ReadAsync(new FabricCoreService(client, credential), cancellationToken: cancellation.Token));
        Assert.Equal(phase is "before" or "authentication" ? 0 : 1, handler.CallCount);
        if (phase is "headers" or "body" or "end-of-body")
        {
            Assert.True(stream.Disposed);
        }
        await credential.Received(phase == "before" ? 0 : 1).GetTokenAsync(Arg.Any<TokenRequestContext>(), cancellation.Token);
    }

    [Fact]
    public static async Task List_IsolatesConcurrentIdentitiesWorkspacesAndPages()
    {
        var identity = new AsyncLocal<string>();
        var credential = Credential();
        credential.GetTokenAsync(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>())
            .Returns(_ => new AccessToken(identity.Value!, DateTimeOffset.MaxValue));
        var requests = new ConcurrentBag<string>();
        using var handler = new FabricCoreHttpMessageHandler(async (request, _) =>
        {
            var workspace = request.RequestUri!.Segments[3].TrimEnd('/');
            Assert.Equal(workspace, request.Headers.Authorization?.Parameter);
            Assert.Equal($"?continuationToken={workspace}", request.RequestUri.Query);
            requests.Add(workspace);
            await Task.Yield();
            var assignment = Assignment with { Principal = Assignment.Principal with { Id = Guid.Parse(workspace) } };
            var json = JsonSerializer.Serialize(assignment, CoreJsonContext.Default.FabricWorkspaceRoleAssignment);
            return Response($"{{\"value\":[{json}],\"continuationToken\":\"{workspace}\"}}");
        });
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, credential);
        await Task.WhenAll(Enumerable.Range(0, 8).Select(async _ =>
        {
            var workspace = Guid.NewGuid().ToString();
            identity.Value = workspace;
            var result = await service.ListWorkspaceRoleAssignmentsAsync(workspace, workspace, TestContext.Current.CancellationToken);
            Assert.Equal(Guid.Parse(workspace), Assert.Single(result.RoleAssignments).Principal.Id);
            Assert.Equal(workspace, result.ContinuationToken);
        }));
        Assert.Equal(8, handler.CallCount);
        Assert.Equal(8, requests.Distinct().Count());
        Assert.Null(client.DefaultRequestHeaders.Authorization);
    }

    private static async Task AssertInvalidPageAsync(string json)
    {
        using var response = Response(json);
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
        using var client = new HttpClient(handler);
        var error = await Assert.ThrowsAsync<JsonException>(() => ReadAsync(new FabricCoreService(client, Credential())));
        Assert.Contains("No partial page", error.Message);
        Assert.DoesNotContain("private", error.Message);
        Assert.Equal(1, handler.CallCount);
        Assert.Throws<ObjectDisposedException>(() => response.Content.ReadAsStream(TestContext.Current.CancellationToken));
    }

    private static Task<WorkspaceRoleAssignmentListCommandResult> ReadAsync(
        FabricCoreService service, string? token = null, CancellationToken? cancellationToken = null) =>
        service.ListWorkspaceRoleAssignmentsAsync(WorkspaceId, token, cancellationToken ?? TestContext.Current.CancellationToken);
}
