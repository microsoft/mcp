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

public sealed class WorkspaceRoleAssignmentGetServiceTests()
{
    [Theory]
    [InlineData("D")]
    [InlineData("N")]
    [InlineData("B")]
    [InlineData("P")]
    [InlineData("X")]
    public static async Task Get_SendsOneAuthenticatedGetWithoutBodyQueryLookupsOrEnrichment(string format)
    {
        var json = JsonNode.Parse(AssignmentJson)!;
        json["privateExtension"] = "private-content";
        json["principal"]!["privateExtension"] = "private-content";
        using var stream = new OperationTestStream(Encoding.UTF8.GetBytes(json.ToJsonString()));
        using var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StreamContent(stream) };
        response.Content.Headers.ContentType = new("application/json");
        response.Headers.Location = new Uri("https://example.invalid/never-follow");
        response.Headers.TryAddWithoutValidation("x-ms-operation-id", "not-an-operation");
        using var handler = new FabricCoreHttpMessageHandler((request, token) =>
        {
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal(AssignmentUrl, request.RequestUri?.AbsoluteUri);
            Assert.Null(request.Content);
            Assert.Equal("Bearer", request.Headers.Authorization?.Scheme);
            Assert.Equal("role-list-test-token", request.Headers.Authorization?.Parameter);
            Assert.Equal("Fabric Core MCP", request.Headers.UserAgent.ToString());
            Assert.True(token.CanBeCanceled);
            return Task.FromResult(response);
        });
        using var client = new HttpClient(handler);
        var credential = Credential();

        var result = await new FabricCoreService(client, credential).GetWorkspaceRoleAssignmentAsync(
            Guid.Parse(WorkspaceId).ToString(format).ToUpperInvariant(),
            Guid.Parse(AssignmentId).ToString(format).ToUpperInvariant(), TestContext.Current.CancellationToken);

        Assert.Equal(Assignment, result.RoleAssignment);
        Assert.NotEqual(result.RoleAssignment.Id, result.RoleAssignment.Principal.Id);
        var output = JsonSerializer.SerializeToElement(result, CoreJsonContext.Default.WorkspaceRoleAssignmentGetCommandResult);
        Assert.Equal("roleAssignment", Assert.Single(output.EnumerateObject()).Name);
        Assert.Equal(["id", "principal", "role"], output.GetProperty("roleAssignment").EnumerateObject().Select(property => property.Name).Order());
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
    public static async Task Get_PreservesDocumentedPrincipalDetailsAndFutureStrings(string role)
    {
        FabricPrincipal[] principals =
        [
            Assignment.Principal,
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
        foreach (var principal in principals)
        {
            var assignment = Assignment with { Role = role, Principal = principal };
            using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(Response(
                JsonSerializer.Serialize(assignment, CoreJsonContext.Default.FabricWorkspaceRoleAssignment))));
            using var client = new HttpClient(handler);
            var result = await ReadAsync(new FabricCoreService(client, Credential()));
            Assert.Equal(assignment, result.RoleAssignment);
            var output = JsonSerializer.Serialize(result, CoreJsonContext.Default.WorkspaceRoleAssignmentGetCommandResult);
            Assert.Equal(result, JsonSerializer.Deserialize(output, CoreJsonContext.Default.WorkspaceRoleAssignmentGetCommandResult));
            Assert.Equal(1, handler.CallCount);
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public static async Task Get_AcceptsAbsentOrNullOptionalPrincipalMetadata(bool explicitNull)
    {
        var json = JsonNode.Parse(AssignmentJson)!;
        var principal = json["principal"]!.AsObject();
        foreach (var name in new[] { "displayName", "userDetails", "groupDetails", "servicePrincipalDetails", "servicePrincipalProfileDetails" })
        {
            principal.Remove(name);
            if (explicitNull)
            {
                principal[name] = null;
            }
        }
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(Response(json.ToJsonString())));
        using var client = new HttpClient(handler);
        var result = await ReadAsync(new FabricCoreService(client, Credential()));
        Assert.Equal(new FabricPrincipal(Guid.Parse(PrincipalId), "User"), result.RoleAssignment.Principal);
        Assert.DoesNotContain("displayName", JsonSerializer.Serialize(result, CoreJsonContext.Default.WorkspaceRoleAssignmentGetCommandResult));
    }

    [Theory]
    [InlineData("{}")]
    [InlineData("""{"userPrincipalName":null}""")]
    [InlineData("""{"userPrincipalName":""}""")]
    [InlineData("""{"userPrincipalName":" \u00e9\ud83d\ude80 "}""")]
    public static async Task Get_PreservesEmptyNamesAndOptionalDetailObjects(string details)
    {
        var json = JsonNode.Parse(AssignmentJson)!;
        var principal = json["principal"]!;
        principal["displayName"] = "";
        principal["userDetails"] = JsonNode.Parse(details);
        principal["groupDetails"] = new JsonObject { ["groupType"] = null };
        principal["servicePrincipalDetails"] = new JsonObject { ["aadAppId"] = null };
        principal["servicePrincipalProfileDetails"] = new JsonObject { ["parentPrincipal"] = null };
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(Response(json.ToJsonString())));
        using var client = new HttpClient(handler);
        var actual = (await ReadAsync(new FabricCoreService(client, Credential()))).RoleAssignment.Principal;
        Assert.Equal("", actual.DisplayName);
        Assert.Equal(JsonNode.Parse(details)!["userPrincipalName"]?.GetValue<string>(), actual.UserDetails?.UserPrincipalName);
        Assert.Equal(new FabricGroupDetails(), actual.GroupDetails);
        Assert.Equal(new FabricServicePrincipalDetails(), actual.ServicePrincipalDetails);
        Assert.Equal(new FabricServicePrincipalProfileDetails(), actual.ServicePrincipalProfileDetails);
    }

    [Theory]
    [InlineData("")]
    [InlineData("{private")]
    [InlineData("null")]
    [InlineData("[]")]
    [InlineData("{}")]
    [InlineData("""{"value":[]}""")]
    [InlineData("""{"id":"private"}""")]
    public static async Task Get_RejectsMalformedMissingAndCollectionResponses(string json) =>
        await AssertInvalidAsync(json);

    [Theory]
    [InlineData("id", "null")]
    [InlineData("id", "\"private\"")]
    [InlineData("id", "\"00000000-0000-0000-0000-000000000000\"")]
    [InlineData("id", "\"" + PrincipalId + "\"")]
    [InlineData("role", "null")]
    [InlineData("role", "\" \"")]
    [InlineData("role", "42")]
    [InlineData("principal", "null")]
    [InlineData("principal", "[]")]
    [InlineData("principal", "{}")]
    [InlineData("principal.id", "null")]
    [InlineData("principal.id", "\"private\"")]
    [InlineData("principal.id", "\"00000000-0000-0000-0000-000000000000\"")]
    [InlineData("principal.type", "null")]
    [InlineData("principal.type", "\" \"")]
    [InlineData("principal.type", "42")]
    [InlineData("principal.displayName", "42")]
    [InlineData("principal.userDetails", "42")]
    [InlineData("principal.userDetails", """{"userPrincipalName":false}""")]
    [InlineData("principal.groupDetails", "[]")]
    [InlineData("principal.groupDetails", """{"groupType":0}""")]
    [InlineData("principal.groupDetails", """{"groupType":" "}""")]
    [InlineData("principal.servicePrincipalDetails", """{"aadAppId":"private"}""")]
    [InlineData("principal.servicePrincipalDetails", """{"aadAppId":"00000000-0000-0000-0000-000000000000"}""")]
    [InlineData("principal.servicePrincipalProfileDetails", "false")]
    [InlineData("principal.servicePrincipalProfileDetails", """{"parentPrincipal":{}}""")]
    [InlineData("principal.servicePrincipalProfileDetails", """{"parentPrincipal":42}""")]
    public static async Task Get_RejectsMalformedKnownFieldsAndWrongAssignmentIdentity(string path, string value)
    {
        var json = JsonNode.Parse(AssignmentJson)!;
        var parts = path.Split('.');
        var parent = parts.Length == 1 ? json : json["principal"]!;
        parent[parts[^1]] = JsonNode.Parse(value);
        await AssertInvalidAsync(json.ToJsonString());
    }

    [Theory]
    [InlineData("id")]
    [InlineData("principal")]
    [InlineData("role")]
    [InlineData("principal.id")]
    [InlineData("principal.type")]
    public static async Task Get_RejectsMissingRequiredFields(string path)
    {
        var json = JsonNode.Parse(AssignmentJson)!;
        var parts = path.Split('.');
        var parent = parts.Length == 1 ? json : json["principal"]!;
        parent.AsObject().Remove(parts[^1]);
        await AssertInvalidAsync(json.ToJsonString());
    }

    [Theory]
    [InlineData("\"id\":\"" + AssignmentId + "\"")]
    [InlineData("\"Id\":\"" + AssignmentId + "\"")]
    [InlineData("\"\\u0069d\":\"" + AssignmentId + "\"")]
    [InlineData(""" "extra":{"same":1,"same":2} """)]
    [InlineData(""" "extra":{"same":1,"Same":2} """)]
    [InlineData(""" "extra":{"text":"\uD800"} """)]
    [InlineData(""" "extra":{"name\uDC00":"private"} """)]
    public static async Task Get_RejectsDuplicateAndInvalidUnicodeEvenInOmittedFields(string extra) =>
        await AssertInvalidAsync(AssignmentJson[..^1] + "," + extra + "}");

    [Fact]
    public static async Task Get_RejectsMalformedPrincipalUnicodeDuplicateFieldsAndUtf8()
    {
        await AssertInvalidAsync(AssignmentJson.Replace("\"type\":\"User\"", "\"type\":\"User\",\"Type\":\"Group\"", StringComparison.Ordinal));
        await AssertInvalidAsync(AssignmentJson.Replace("\"type\":\"User\"", "\"type\":\"\\uD800\"", StringComparison.Ordinal));
        var body = Encoding.UTF8.GetBytes(AssignmentJson[..^1] + ",\"extra\":\"\u00e9\"}");
        body[Array.IndexOf(body, (byte)0xc3)] = 0xff;
        using var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(body) };
        response.Content.Headers.ContentType = new("application/json");
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
        using var client = new HttpClient(handler);
        var error = await Assert.ThrowsAsync<WorkspaceRoleAssignmentGetRequestException>(() => ReadAsync(new FabricCoreService(client, Credential())));
        Assert.Equal(HttpStatusCode.BadGateway, error.StatusCode);
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
    public static async Task Get_ValidatesBothIdsBeforeAuthentication(string? value)
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) => throw new InvalidOperationException("No HTTP expected."));
        using var client = new HttpClient(handler);
        var credential = Credential();
        var service = new FabricCoreService(client, credential);
        var workspace = await Assert.ThrowsAsync<ArgumentException>(() =>
            service.GetWorkspaceRoleAssignmentAsync(value!, AssignmentId, TestContext.Current.CancellationToken));
        var assignment = await Assert.ThrowsAsync<ArgumentException>(() =>
            service.GetWorkspaceRoleAssignmentAsync(WorkspaceId, value!, TestContext.Current.CancellationToken));
        Assert.Equal("workspaceId", workspace.ParamName);
        Assert.Equal("workspaceRoleAssignmentId", assignment.ParamName);
        Assert.Empty(credential.ReceivedCalls());
        Assert.Equal(0, handler.CallCount);
    }

    [Theory]
    [InlineData(false, 0)]
    [InlineData(false, 1)]
    [InlineData(true, 0)]
    [InlineData(true, 1)]
    public static async Task Get_EnforcesExactResponseByteBudgetWithoutTruncation(bool knownLength, int excess)
    {
        const int limit = 1024 * 1024;
        Assert.Equal(limit, FabricResponseBodyReader.DefaultMaxJsonBytes);
        using var stream = new PaddedJsonTestStream(Encoding.UTF8.GetBytes(AssignmentJson), limit + excess);
        using var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StreamContent(stream) };
        response.Content.Headers.ContentType = new("application/json");
        response.Headers.TryAddWithoutValidation("Retry-After", "45");
        if (knownLength)
        {
            response.Content.Headers.ContentLength = limit + excess;
        }
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, Credential());
        if (excess == 0)
        {
            Assert.Equal(Assignment, (await ReadAsync(service)).RoleAssignment);
        }
        else
        {
            var error = await Assert.ThrowsAsync<WorkspaceRoleAssignmentGetRequestException>(() => ReadAsync(service));
            Assert.Equal(HttpStatusCode.BadGateway, error.StatusCode);
            Assert.Equal(TimeSpan.FromSeconds(45), error.RetryAfter?.Delta);
        }
        Assert.Equal(knownLength && excess > 0 ? 0 : limit + excess, stream.BytesRead);
        Assert.True(stream.Disposed);
        Assert.Equal(1, handler.CallCount);
    }

    [Theory]
    [InlineData(64)]
    [InlineData(65)]
    public static async Task Get_Enforces64InputLevelsWithoutRetainingUnknownJson(int depth)
    {
        var json = AssignmentJson[..^1] + ",\"extra\":" + new string('[', depth - 1) + "0" + new string(']', depth - 1) + "}";
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(Response(json)));
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, Credential());
        if (depth == 64)
        {
            Assert.Equal(Assignment, (await ReadAsync(service)).RoleAssignment);
        }
        else
        {
            var error = await Assert.ThrowsAsync<WorkspaceRoleAssignmentGetRequestException>(() => ReadAsync(service));
            Assert.Equal(HttpStatusCode.BadGateway, error.StatusCode);
        }
    }

    [Theory]
    [InlineData(false, 61)]
    [InlineData(true, 62)]
    public static async Task Get_ReservesExactSingletonEnvelopeDepthWithoutTruncatingParents(bool leafDetails, int outputDepth)
    {
        var json = NestedPrincipalAssignment(30, leafDetails);
        using var input = JsonDocument.Parse(json);
        Assert.Equal(outputDepth - 1, JsonDepth(input.RootElement));
        var assignment = input.RootElement.Deserialize(CoreJsonContext.Default.FabricWorkspaceRoleAssignment)!;
        var output = JsonSerializer.SerializeToElement(new WorkspaceRoleAssignmentGetCommandResult(assignment),
            CoreJsonContext.Default.WorkspaceRoleAssignmentGetCommandResult);
        Assert.Equal(outputDepth, JsonDepth(output));
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(Response(json)));
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, Credential());
        if (outputDepth == 61)
        {
            var result = await ReadAsync(service);
            Assert.Equal(assignment, result.RoleAssignment);
            var principal = result.RoleAssignment.Principal;
            for (var index = 1; index < 30; index++)
            {
                principal = Assert.IsType<FabricPrincipal>(principal.ServicePrincipalProfileDetails?.ParentPrincipal);
            }
            Assert.Equal("User", principal.Type);
            Assert.Null(principal.UserDetails);
        }
        else
        {
            var error = await Assert.ThrowsAsync<WorkspaceRoleAssignmentGetRequestException>(() => ReadAsync(service));
            Assert.Equal(HttpStatusCode.BadGateway, error.StatusCode);
        }
        Assert.Equal(1, handler.CallCount);
    }

    [Theory]
    [InlineData(HttpStatusCode.Created)]
    [InlineData(HttpStatusCode.Accepted)]
    [InlineData(HttpStatusCode.NoContent)]
    [InlineData(HttpStatusCode.PartialContent)]
    [InlineData(HttpStatusCode.MovedPermanently)]
    [InlineData(HttpStatusCode.Found)]
    [InlineData(HttpStatusCode.TemporaryRedirect)]
    [InlineData(HttpStatusCode.BadRequest)]
    [InlineData(HttpStatusCode.Unauthorized)]
    [InlineData(HttpStatusCode.Forbidden)]
    [InlineData(HttpStatusCode.NotFound)]
    [InlineData(HttpStatusCode.TooManyRequests)]
    [InlineData(HttpStatusCode.ServiceUnavailable)]
    public static async Task Get_PreservesHttpStatusAndRetryGuidanceWithoutReadingErrorsOrFollowingUrls(HttpStatusCode status)
    {
        using var content = new UnreadableHttpContent();
        using var response = new HttpResponseMessage(status) { Content = content };
        response.Headers.Location = new Uri("https://example.invalid/private");
        response.Headers.TryAddWithoutValidation("x-ms-operation-id", AssignmentId);
        response.Headers.TryAddWithoutValidation("Retry-After", "45");
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
        using var client = new HttpClient(handler);
        var error = await Assert.ThrowsAsync<WorkspaceRoleAssignmentGetRequestException>(() => ReadAsync(new FabricCoreService(client, Credential())));
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
    public static async Task Get_ReturnsOnlyValidatedRetrySeconds(string? header, long? expected)
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(Response(AssignmentJson, retryAfter: header)));
        using var client = new HttpClient(handler);
        var result = await ReadAsync(new FabricCoreService(client, Credential()));
        Assert.Equal(expected, result.RetryAfterSeconds);
        Assert.Null(result.RetryAfterUtc);
        Assert.Equal(1, handler.CallCount);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public static async Task Get_PreservesRetryDateButRejectsMultipleRetryHeaders(bool multiple)
    {
        using var response = Response(AssignmentJson);
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
    public static async Task Get_RejectsUnsupportedMediaBeforeReading(string? mediaType)
    {
        using var content = new UnreadableHttpContent();
        content.Headers.ContentType = mediaType is null ? null : new(mediaType);
        using var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = content };
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
        using var client = new HttpClient(handler);
        var error = await Assert.ThrowsAsync<WorkspaceRoleAssignmentGetRequestException>(() => ReadAsync(new FabricCoreService(client, Credential())));
        Assert.Equal(HttpStatusCode.BadGateway, error.StatusCode);
        Assert.True(content.IsDisposed);
    }

    [Theory]
    [InlineData("json", HttpStatusCode.BadGateway)]
    [InlineData("io", HttpStatusCode.ServiceUnavailable)]
    [InlineData("http", HttpStatusCode.BadGateway)]
    [InlineData("canceled", HttpStatusCode.RequestTimeout)]
    [InlineData("timeout", HttpStatusCode.GatewayTimeout)]
    public static async Task Get_PreservesRetryGuidanceOnBodyFailures(string failure, HttpStatusCode status)
    {
        using var stream = new OperationTestStream(Encoding.UTF8.GetBytes(AssignmentJson), (_, _) => throw (failure switch
        {
            "json" => new JsonException("private"),
            "io" => new IOException("private"),
            "http" => new HttpRequestException("private", null, HttpStatusCode.BadGateway),
            "canceled" => new OperationCanceledException("private"),
            "timeout" => new TimeoutException("private"),
            _ => new InvalidOperationException()
        }));
        using var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StreamContent(stream) };
        response.Content.Headers.ContentType = new("application/json");
        response.Headers.TryAddWithoutValidation("Retry-After", "45");
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
        using var client = new HttpClient(handler);
        var error = await Assert.ThrowsAsync<WorkspaceRoleAssignmentGetRequestException>(() => ReadAsync(new FabricCoreService(client, Credential())));
        Assert.Equal(status, error.StatusCode);
        Assert.Equal(TimeSpan.FromSeconds(45), error.RetryAfter?.Delta);
        Assert.DoesNotContain("private", error.Message);
        Assert.True(stream.Disposed);
    }

    [Fact]
    public static async Task Get_DoesNotMisclassifyUnrelatedReaderFailuresAsMalformedJson()
    {
        using var stream = new OperationTestStream(Encoding.UTF8.GetBytes(AssignmentJson),
            (_, _) => throw new InvalidOperationException("private-reader-failure"));
        using var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StreamContent(stream) };
        response.Content.Headers.ContentType = new("application/json");
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
        using var client = new HttpClient(handler);
        await Assert.ThrowsAsync<InvalidOperationException>(() => ReadAsync(new FabricCoreService(client, Credential())));
        Assert.True(stream.Disposed);
    }

    [Fact]
    public static async Task Get_AuthenticationFailureSendsNoHttp()
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
    public static async Task Get_PropagatesCancellationAndDisposesAvailableResponses(string phase)
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
        using var stream = new OperationTestStream(Encoding.UTF8.GetBytes(AssignmentJson), (count, token) =>
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
        var service = new FabricCoreService(client, credential);
        if (phase is "before" or "authentication" or "http")
        {
            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => ReadAsync(service, cancellation.Token));
        }
        else
        {
            var error = await Assert.ThrowsAsync<WorkspaceRoleAssignmentGetRequestException>(() => ReadAsync(service, cancellation.Token));
            Assert.Equal(HttpStatusCode.RequestTimeout, error.StatusCode);
            Assert.True(stream.Disposed);
        }
        Assert.Equal(phase is "before" or "authentication" ? 0 : 1, handler.CallCount);
        await credential.Received(phase == "before" ? 0 : 1).GetTokenAsync(Arg.Any<TokenRequestContext>(), cancellation.Token);
    }

    [Fact]
    public static async Task Get_IsolatesConcurrentIdentitiesWorkspacesAndAssignments()
    {
        var identity = new AsyncLocal<string>();
        var credential = Credential();
        credential.GetTokenAsync(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>())
            .Returns(_ => new AccessToken(identity.Value!, DateTimeOffset.MaxValue));
        var requests = new ConcurrentBag<string>();
        using var handler = new FabricCoreHttpMessageHandler(async (request, _) =>
        {
            var workspace = request.RequestUri!.Segments[3].TrimEnd('/');
            var assignmentId = Guid.Parse(request.RequestUri.Segments[^1]);
            Assert.Equal(workspace, request.Headers.Authorization?.Parameter);
            Assert.Empty(request.RequestUri.Query);
            requests.Add(workspace);
            await Task.Yield();
            var assignment = Assignment with { Id = assignmentId, Principal = Assignment.Principal with { Id = Guid.Parse(workspace) } };
            return Response(JsonSerializer.Serialize(assignment, CoreJsonContext.Default.FabricWorkspaceRoleAssignment));
        });
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, credential);
        await Task.WhenAll(Enumerable.Range(0, 8).Select(async _ =>
        {
            var workspace = Guid.NewGuid().ToString();
            var assignment = Guid.NewGuid();
            identity.Value = workspace;
            var result = await service.GetWorkspaceRoleAssignmentAsync(workspace, assignment.ToString(), TestContext.Current.CancellationToken);
            Assert.Equal(assignment, result.RoleAssignment.Id);
            Assert.Equal(Guid.Parse(workspace), result.RoleAssignment.Principal.Id);
        }));
        Assert.Equal(8, handler.CallCount);
        Assert.Equal(8, requests.Distinct().Count());
        Assert.Null(client.DefaultRequestHeaders.Authorization);
    }

    private static async Task AssertInvalidAsync(string json)
    {
        using var response = Response(json, retryAfter: "45");
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
        using var client = new HttpClient(handler);
        var error = await Assert.ThrowsAsync<WorkspaceRoleAssignmentGetRequestException>(() => ReadAsync(new FabricCoreService(client, Credential())));
        Assert.Equal(HttpStatusCode.BadGateway, error.StatusCode);
        Assert.Equal(TimeSpan.FromSeconds(45), error.RetryAfter?.Delta);
        Assert.DoesNotContain("private", error.Message);
        Assert.Equal(1, handler.CallCount);
        Assert.Throws<ObjectDisposedException>(() => response.Content.ReadAsStream(TestContext.Current.CancellationToken));
    }

    private static Task<WorkspaceRoleAssignmentGetCommandResult> ReadAsync(
        FabricCoreService service, CancellationToken? cancellationToken = null) =>
        service.GetWorkspaceRoleAssignmentAsync(WorkspaceId, AssignmentId, cancellationToken ?? TestContext.Current.CancellationToken);
}
