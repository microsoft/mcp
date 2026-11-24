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
using Fabric.Mcp.Tools.Core.Validation;
using NSubstitute;
using Xunit;
using static Fabric.Mcp.Tools.Core.Tests.TestSupport.WorkspaceRoleAssignmentTestData;

namespace Fabric.Mcp.Tools.Core.Tests.Services;

public sealed class WorkspaceRoleAssignmentAddServiceTests()
{
    [Theory]
    [InlineData("D")]
    [InlineData("N")]
    [InlineData("B")]
    [InlineData("P")]
    [InlineData("X")]
    public static async Task Add_SendsExactlyOnePostWithOnlyExplicitPrincipalAndRoleAndIndependentAssignmentId(string format)
    {
        var json = JsonNode.Parse(AssignmentJson)!;
        json["privateExtension"] = "private";
        json["principal"]!["privateExtension"] = "private";
        using var stream = new OperationTestStream(Encoding.UTF8.GetBytes(json.ToJsonString()));
        using var response = new HttpResponseMessage(HttpStatusCode.Created) { Content = new StreamContent(stream) };
        response.Content.Headers.ContentType = new("application/json");
        response.Headers.Location = new Uri("https://example.invalid/private");
        response.Headers.TryAddWithoutValidation("x-ms-operation-id", AssignmentId);
        HttpContent? requestContent = null;
        using var handler = new FabricCoreHttpMessageHandler(async (request, token) =>
        {
            Assert.Equal(HttpMethod.Post, request.Method);
            Assert.Equal(CollectionUrl, request.RequestUri?.AbsoluteUri);
            Assert.Equal("Bearer", request.Headers.Authorization?.Scheme);
            Assert.Equal("role-list-test-token", request.Headers.Authorization?.Parameter);
            Assert.Equal("Fabric Core MCP", request.Headers.UserAgent.ToString());
            Assert.True(token.CanBeCanceled);
            requestContent = request.Content;
            Assert.Equal("application/json", requestContent!.Headers.ContentType?.MediaType);
            using var body = JsonDocument.Parse(await requestContent.ReadAsStringAsync(token));
            Assert.Equal(["principal", "role"], body.RootElement.EnumerateObject().Select(property => property.Name).Order());
            Assert.Equal("Viewer", body.RootElement.GetProperty("role").GetString());
            var principal = body.RootElement.GetProperty("principal");
            Assert.Equal(["id", "type"], principal.EnumerateObject().Select(property => property.Name).Order());
            Assert.Equal(PrincipalId, principal.GetProperty("id").GetString());
            Assert.Equal("User", principal.GetProperty("type").GetString());
            return response;
        });
        using var client = new HttpClient(handler);
        var credential = Credential();

        var result = await new FabricCoreService(client, credential).AddWorkspaceRoleAssignmentAsync(
            Guid.Parse(WorkspaceId).ToString(format).ToUpperInvariant(), PrincipalJson, "Viewer", TestContext.Current.CancellationToken);

        Assert.Equal(Assignment, result.RoleAssignment);
        Assert.NotEqual(result.RoleAssignment.Id, result.RoleAssignment.Principal.Id);
        Assert.Equal("Created", result.CreationStatus);
        var output = JsonSerializer.Serialize(result, CoreJsonContext.Default.WorkspaceRoleAssignmentAddCommandResult);
        Assert.DoesNotContain("private", output);
        Assert.Equal(result, JsonSerializer.Deserialize(output, CoreJsonContext.Default.WorkspaceRoleAssignmentAddCommandResult));
        Assert.Equal(1, handler.CallCount);
        Assert.True(stream.Disposed);
        Assert.NotNull(requestContent);
        Assert.Throws<ObjectDisposedException>(() => requestContent.ReadAsStream(TestContext.Current.CancellationToken));
        Assert.Throws<ObjectDisposedException>(() => response.Content.ReadAsStream(TestContext.Current.CancellationToken));
        await credential.Received(1).GetTokenAsync(
            Arg.Is<TokenRequestContext>(context => context.Scopes.SequenceEqual(FabricEndpoints.FabricScopes)),
            TestContext.Current.CancellationToken);
    }

    [Theory]
    [InlineData("User")]
    [InlineData("Group")]
    [InlineData("ServicePrincipal")]
    [InlineData("ServicePrincipalProfile")]
    [InlineData("EntireTenant")]
    [InlineData("FuturePrincipal")]
    public static async Task Add_PreservesDocumentedAndExtensibleIntentAndAvailablePrincipalMetadata(string type)
    {
        foreach (var role in new[] { "Admin", "Member", "Contributor", "Viewer", "FutureRole" })
        {
            var principal = new FabricPrincipal(Guid.Parse(PrincipalId), type, "",
                GroupDetails: type == "Group" ? new("FutureGroup") : null,
                ServicePrincipalDetails: type == "ServicePrincipal" ? new(Guid.Parse(AppId)) : null,
                ServicePrincipalProfileDetails: type == "ServicePrincipalProfile"
                    ? new(new(Guid.Parse(AppId), "FutureParent")) : null);
            var assignment = Assignment with { Role = role, Principal = principal };
            var input = new JsonObject { ["id"] = PrincipalId, ["type"] = type };
            if (type == "Group")
            {
                input["groupDetails"] = new JsonObject { ["groupType"] = "FutureGroup" };
            }
            if (type == "ServicePrincipalProfile")
            {
                input["servicePrincipalProfileDetails"] = new JsonObject
                {
                    ["parentPrincipal"] = new JsonObject { ["id"] = AppId, ["type"] = "FutureParent" }
                };
            }
            using var handler = new FabricCoreHttpMessageHandler(async (request, token) =>
            {
                var body = JsonNode.Parse(await request.Content!.ReadAsStringAsync(token))!;
                Assert.True(JsonNode.DeepEquals(input, body["principal"]));
                Assert.Equal(role, body["role"]!.GetValue<string>());
                return Response(JsonSerializer.Serialize(assignment, CoreJsonContext.Default.FabricWorkspaceRoleAssignment), HttpStatusCode.Created);
            });
            using var client = new HttpClient(handler);
            var result = await new FabricCoreService(client, Credential()).AddWorkspaceRoleAssignmentAsync(
                WorkspaceId, input.ToJsonString(), role, TestContext.Current.CancellationToken);
            Assert.Equal(assignment, result.RoleAssignment);
            Assert.Equal(1, handler.CallCount);
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public static async Task Add_AllowsAbsentOrNullOptionalResponseMetadataWithoutInventingDetails(bool explicitNull)
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
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(Response(json.ToJsonString(), HttpStatusCode.Created)));
        using var client = new HttpClient(handler);
        var result = await AddAsync(new FabricCoreService(client, Credential()));
        Assert.Equal(new FabricPrincipal(Guid.Parse(PrincipalId), "User"), result.RoleAssignment.Principal);
        Assert.DoesNotContain("displayName", JsonSerializer.Serialize(result, CoreJsonContext.Default.WorkspaceRoleAssignmentAddCommandResult));
    }

    [Theory]
    [InlineData("")]
    [InlineData("{private")]
    [InlineData("null")]
    [InlineData("[]")]
    [InlineData("{}")]
    [InlineData("""{"value":[]}""")]
    public static async Task Add_ReportsConfirmedCreationForUnusableResponseBodies(string json) =>
        await AssertInvalidAsync(json);

    [Theory]
    [InlineData("id", "null")]
    [InlineData("id", "\"private\"")]
    [InlineData("id", "\"00000000-0000-0000-0000-000000000000\"")]
    [InlineData("principal", "null")]
    [InlineData("principal", "{}")]
    [InlineData("role", "null")]
    [InlineData("role", "42")]
    [InlineData("role", "\" \"")]
    [InlineData("role", "\"Admin\"")]
    [InlineData("role", "\"viewer\"")]
    [InlineData("principal.id", "\"" + AppId + "\"")]
    [InlineData("principal.id", "\"private\"")]
    [InlineData("principal.type", "\"EntireTenant\"")]
    [InlineData("principal.type", "null")]
    [InlineData("principal.type", "42")]
    [InlineData("principal.userDetails", """{"userPrincipalName":false}""")]
    [InlineData("principal.servicePrincipalDetails", """{"aadAppId":"00000000-0000-0000-0000-000000000000"}""")]
    [InlineData("principal.servicePrincipalProfileDetails", """{"parentPrincipal":{}}""")]
    public static async Task Add_RejectsInvalidMetadataOrAChangedPrincipalOrAccessLevelWithoutErasing201(string path, string value)
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
    public static async Task Add_RejectsMissingRequiredMetadataWithoutErasing201(string path)
    {
        var json = JsonNode.Parse(AssignmentJson)!;
        var parts = path.Split('.');
        (parts.Length == 1 ? json : json["principal"]!).AsObject().Remove(parts[^1]);
        await AssertInvalidAsync(json.ToJsonString());
    }

    [Theory]
    [InlineData("\"id\":\"" + AssignmentId + "\"")]
    [InlineData("\"Id\":\"" + AssignmentId + "\"")]
    [InlineData("\"\\u0069d\":\"" + AssignmentId + "\"")]
    [InlineData(""" "private":{"same":1,"Same":2} """)]
    [InlineData(""" "private":{"text":"\uD800"} """)]
    [InlineData(""" "private":{"name\uDC00":"private"} """)]
    public static async Task Add_RejectsDuplicateAndInvalidUnicodeEvenInOmittedFields(string extra) =>
        await AssertInvalidAsync(AssignmentJson[..^1] + "," + extra + "}");

    [Fact]
    public static async Task Add_RejectsInvalidUtf8WithoutLosingConfirmedCreation()
    {
        var bytes = Encoding.UTF8.GetBytes(AssignmentJson[..^1] + ",\"extra\":\"\u00e9\"}");
        bytes[Array.IndexOf(bytes, (byte)0xc3)] = 0xff;
        using var response = new HttpResponseMessage(HttpStatusCode.Created) { Content = new ByteArrayContent(bytes) };
        response.Content.Headers.ContentType = new("application/json");
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
        using var client = new HttpClient(handler);
        AssertConfirmed(await Assert.ThrowsAsync<WorkspaceRoleAssignmentAddRequestException>(() => AddAsync(new FabricCoreService(client, Credential()))));
    }

    [Theory]
    [InlineData(false, 0)]
    [InlineData(false, 1)]
    [InlineData(true, 0)]
    [InlineData(true, 1)]
    public static async Task Add_EnforcesExactResponseByteBudgetAndDisposesWithoutTruncation(bool knownLength, int excess)
    {
        const int limit = 1024 * 1024;
        using var stream = new PaddedJsonTestStream(Encoding.UTF8.GetBytes(AssignmentJson), limit + excess);
        using var response = new HttpResponseMessage(HttpStatusCode.Created) { Content = new StreamContent(stream) };
        response.Content.Headers.ContentType = new("application/json");
        response.Headers.TryAddWithoutValidation("Retry-After", "45");
        if (knownLength)
        {
            response.Content.Headers.ContentLength = limit + excess;
        }
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
        using var client = new HttpClient(handler);
        if (excess == 0)
        {
            Assert.Equal(Assignment, (await AddAsync(new FabricCoreService(client, Credential()))).RoleAssignment);
        }
        else
        {
            var error = await Assert.ThrowsAsync<WorkspaceRoleAssignmentAddRequestException>(() => AddAsync(new FabricCoreService(client, Credential())));
            AssertConfirmed(error);
            Assert.Equal(TimeSpan.FromSeconds(45), error.RetryAfter?.Delta);
        }
        Assert.Equal(knownLength && excess > 0 ? 0 : limit + excess, stream.BytesRead);
        Assert.True(stream.Disposed);
        Assert.Equal(1, handler.CallCount);
    }

    [Theory]
    [InlineData(64)]
    [InlineData(65)]
    public static async Task Add_EnforcesExactInputDepthWithoutRetainingUnknownJson(int depth)
    {
        var json = AssignmentJson[..^1] + ",\"extra\":" + new string('[', depth - 1) + "0" + new string(']', depth - 1) + "}";
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(Response(json, HttpStatusCode.Created)));
        using var client = new HttpClient(handler);
        if (depth == 64)
        {
            Assert.Equal(Assignment, (await AddAsync(new FabricCoreService(client, Credential()))).RoleAssignment);
        }
        else
        {
            AssertConfirmed(await Assert.ThrowsAsync<WorkspaceRoleAssignmentAddRequestException>(() => AddAsync(new FabricCoreService(client, Credential()))));
        }
        Assert.Equal(1, handler.CallCount);
    }

    [Theory]
    [InlineData(false, 61)]
    [InlineData(true, 62)]
    public static async Task Add_RejectsUnserializableEnvelopeMetadataInsideConfirmedMutationBoundary(bool leafDetails, int outputDepth)
    {
        var json = NestedPrincipalAssignment(30, leafDetails);
        using var document = JsonDocument.Parse(json);
        Assert.Equal(outputDepth - 1, JsonDepth(document.RootElement));
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(Response(json, HttpStatusCode.Created, "45")));
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, Credential());
        var input = PrincipalJson.Replace("User", "ServicePrincipalProfile", StringComparison.Ordinal);
        if (outputDepth == 61)
        {
            var result = await service.AddWorkspaceRoleAssignmentAsync(WorkspaceId, input, "Member", TestContext.Current.CancellationToken);
            Assert.Equal(61, JsonDepth(JsonSerializer.SerializeToElement(result, CoreJsonContext.Default.WorkspaceRoleAssignmentAddCommandResult)));
        }
        else
        {
            var error = await Assert.ThrowsAsync<WorkspaceRoleAssignmentAddRequestException>(() =>
                service.AddWorkspaceRoleAssignmentAsync(WorkspaceId, input, "Member", TestContext.Current.CancellationToken));
            AssertConfirmed(error);
            Assert.Equal(TimeSpan.FromSeconds(45), error.RetryAfter?.Delta);
        }
        Assert.Equal(1, handler.CallCount);
    }

    [Theory]
    [InlineData(HttpStatusCode.OK)]
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
    [InlineData(HttpStatusCode.Conflict)]
    [InlineData(HttpStatusCode.RequestTimeout)]
    [InlineData(HttpStatusCode.TooManyRequests)]
    [InlineData(HttpStatusCode.ServiceUnavailable)]
    public static async Task Add_PreservesStatusAndSafeRetryGuidanceWithoutRetryPollingOrConflictUpdate(HttpStatusCode status)
    {
        // cspell:disable-next-line
        using var response = Response("""{"errorCode":"UnknownError","message":"private","requestId":"private","isRetriable":true}""", status, "45");
        response.Headers.Location = new Uri("https://example.invalid/private");
        response.Headers.TryAddWithoutValidation("x-ms-operation-id", AssignmentId);
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
        using var client = new HttpClient(handler);
        var error = await Assert.ThrowsAsync<WorkspaceRoleAssignmentAddRequestException>(() => AddAsync(new FabricCoreService(client, Credential())));
        Assert.Equal(response.IsSuccessStatusCode ? HttpStatusCode.BadGateway : status, error.StatusCode);
        Assert.Equal(status, error.ResponseStatusCode);
        Assert.False(error.CreationConfirmed);
        Assert.False(error.RequestNotSent);
        Assert.Equal(response.IsSuccessStatusCode ? null : "UnknownError", error.ErrorCode);
        Assert.Equal(TimeSpan.FromSeconds(45), error.RetryAfter?.Delta);
        Assert.DoesNotContain("private", error.Message);
        Assert.Equal(1, handler.CallCount);
        Assert.Throws<ObjectDisposedException>(() => response.Content.ReadAsStream(TestContext.Current.CancellationToken));
    }

    [Theory]
    [InlineData("{private")]
    [InlineData("""{"errorCode":"private\nunsafe"}""")]
    [InlineData("""{"errorCode":"\uD800"}""")]
    [InlineData("""{"errorCode":"UnknownError","errorCode":"Other"}""")]
    [InlineData("""{"errorCode":42,"message":"private"}""")]
    [InlineData("{}")]
    public static async Task Add_UnusableOptionalErrorMetadataDoesNotReplaceHttpStatusOrRetryAfter(string json)
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(Response(json, HttpStatusCode.Forbidden, "45")));
        using var client = new HttpClient(handler);
        var error = await Assert.ThrowsAsync<WorkspaceRoleAssignmentAddRequestException>(() => AddAsync(new FabricCoreService(client, Credential())));
        Assert.Equal(HttpStatusCode.Forbidden, error.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, error.ResponseStatusCode);
        Assert.True(error.ErrorMetadataUnavailable);
        Assert.Null(error.ErrorCode);
        Assert.Equal(TimeSpan.FromSeconds(45), error.RetryAfter?.Delta);
        Assert.Equal(1, handler.CallCount);
    }

    [Theory]
    [InlineData("json")]
    [InlineData("io")]
    [InlineData("http")]
    [InlineData("canceled")]
    [InlineData("timeout")]
    [InlineData("reader")]
    [InlineData("unsupported")]
    public static async Task Add_BodyFailuresPreserveConfirmedOrRejectedOutcomeAndRetryAfter(string failure)
    {
        foreach (var status in new[] { HttpStatusCode.Created, HttpStatusCode.TooManyRequests })
        {
            using var stream = new OperationTestStream(Encoding.UTF8.GetBytes(AssignmentJson), (_, _) => throw Failure(failure));
            using var response = new HttpResponseMessage(status) { Content = new StreamContent(stream) };
            response.Content.Headers.ContentType = new("application/json");
            response.Headers.TryAddWithoutValidation("Retry-After", "45");
            using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
            using var client = new HttpClient(handler);
            var error = await Assert.ThrowsAsync<WorkspaceRoleAssignmentAddRequestException>(() => AddAsync(new FabricCoreService(client, Credential())));
            Assert.Equal(status, error.ResponseStatusCode);
            Assert.Equal(status == HttpStatusCode.Created, error.CreationConfirmed);
            Assert.False(error.RequestNotSent);
            Assert.Equal(status == HttpStatusCode.TooManyRequests ? status : failure switch
            {
                "canceled" => HttpStatusCode.RequestTimeout,
                "timeout" => HttpStatusCode.GatewayTimeout,
                _ => HttpStatusCode.BadGateway
            }, error.StatusCode);
            Assert.Equal(TimeSpan.FromSeconds(45), error.RetryAfter?.Delta);
            Assert.True(stream.Disposed);
            Assert.Equal(1, handler.CallCount);
        }
    }

    [Theory]
    [InlineData(null)]
    [InlineData("text/html")]
    [InlineData("application/octet-stream")]
    public static async Task Add_UnsupportedMediaStillPreservesConfirmedCreationWithoutReading(string? mediaType)
    {
        using var content = new UnreadableHttpContent();
        content.Headers.ContentType = mediaType is null ? null : new(mediaType);
        using var response = new HttpResponseMessage(HttpStatusCode.Created) { Content = content };
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
        using var client = new HttpClient(handler);
        AssertConfirmed(await Assert.ThrowsAsync<WorkspaceRoleAssignmentAddRequestException>(() => AddAsync(new FabricCoreService(client, Credential()))));
        Assert.True(content.IsDisposed);
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
    public static async Task Add_ReturnsOnlyValidatedRetryGuidance(string? header, long? expected)
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(Response(AssignmentJson, HttpStatusCode.Created, header)));
        using var client = new HttpClient(handler);
        var result = await AddAsync(new FabricCoreService(client, Credential()));
        Assert.Equal(expected, result.RetryAfterSeconds);
        Assert.Null(result.RetryAfterUtc);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public static async Task Add_PreservesHttpDatesButIgnoresAmbiguousRetryHeaders(bool multiple)
    {
        using var response = Response(AssignmentJson, HttpStatusCode.Created);
        response.Headers.TryAddWithoutValidation("Retry-After", multiple ? ["45", "60"] : ["Tue, 01 Jan 2030 00:00:00 GMT"]);
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
        using var client = new HttpClient(handler);
        var result = await AddAsync(new FabricCoreService(client, Credential()));
        Assert.Equal(multiple ? null : new DateTimeOffset(2030, 1, 1, 0, 0, 0, TimeSpan.Zero), result.RetryAfterUtc);
        Assert.Null(result.RetryAfterSeconds);
    }

    [Fact]
    public static async Task Add_ValidatesInputsAndAuthenticationBeforeHttp()
    {
        var credential = Credential();
        using var handler = new FabricCoreHttpMessageHandler((_, _) => throw new InvalidOperationException("No HTTP expected."));
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, credential);
        await Assert.ThrowsAsync<FabricItemInputException>(() => service.AddWorkspaceRoleAssignmentAsync("private", PrincipalJson, "Viewer", TestContext.Current.CancellationToken));
        await Assert.ThrowsAsync<FabricItemInputException>(() => service.AddWorkspaceRoleAssignmentAsync(WorkspaceId, "private", "Viewer", TestContext.Current.CancellationToken));
        await Assert.ThrowsAsync<FabricItemInputException>(() => service.AddWorkspaceRoleAssignmentAsync(WorkspaceId, PrincipalJson, "", TestContext.Current.CancellationToken));
        Assert.Empty(credential.ReceivedCalls());
        credential.GetTokenAsync(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>())
            .Returns(ValueTask.FromException<AccessToken>(new AuthenticationFailedException("private")));
        var error = await Assert.ThrowsAsync<WorkspaceRoleAssignmentAddRequestException>(() => AddAsync(service));
        Assert.Equal(HttpStatusCode.Unauthorized, error.StatusCode);
        Assert.True(error.RequestNotSent);
        Assert.Null(error.ResponseStatusCode);
        Assert.Equal(0, handler.CallCount);
    }

    [Theory]
    [InlineData("before")]
    [InlineData("authentication")]
    [InlineData("http")]
    [InlineData("headers")]
    [InlineData("body")]
    [InlineData("end-of-body")]
    public static async Task Add_CancellationDistinguishesUnsentUncertainAndConfirmedAndDisposes(string phase)
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
        using var stream = new OperationTestStream(Encoding.UTF8.GetBytes(AssignmentJson), (count, _) =>
        {
            if (phase == "body" || (phase == "end-of-body" && count == 0))
            {
                cancellation.Cancel();
            }
            return ValueTask.CompletedTask;
        });
        using var response = new HttpResponseMessage(HttpStatusCode.Created) { Content = new StreamContent(stream) };
        response.Content.Headers.ContentType = new("application/json");
        response.Headers.TryAddWithoutValidation("Retry-After", "45");
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
            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => AddAsync(new FabricCoreService(client, credential), cancellation.Token));
            Assert.Empty(credential.ReceivedCalls());
        }
        else
        {
            var error = await Assert.ThrowsAsync<WorkspaceRoleAssignmentAddRequestException>(() => AddAsync(new FabricCoreService(client, credential), cancellation.Token));
            Assert.Equal(HttpStatusCode.RequestTimeout, error.StatusCode);
            Assert.Equal(phase == "authentication", error.RequestNotSent);
            Assert.Equal(phase is "headers" or "body" or "end-of-body", error.CreationConfirmed);
            if (error.CreationConfirmed)
            {
                Assert.True(stream.Disposed);
                Assert.Equal(TimeSpan.FromSeconds(45), error.RetryAfter?.Delta);
            }
        }
        Assert.Equal(phase is "before" or "authentication" ? 0 : 1, handler.CallCount);
    }

    [Theory]
    [InlineData("io")]
    [InlineData("http")]
    [InlineData("timeout")]
    public static async Task Add_SendFailuresNeverImplyRejectionOrConfirmedGrant(string failure)
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) => throw Failure(failure));
        using var client = new HttpClient(handler);
        var error = await Assert.ThrowsAsync<WorkspaceRoleAssignmentAddRequestException>(() => AddAsync(new FabricCoreService(client, Credential())));
        Assert.Null(error.ResponseStatusCode);
        Assert.False(error.CreationConfirmed);
        Assert.False(error.RequestNotSent);
        Assert.Equal(1, handler.CallCount);
    }

    [Fact]
    public static async Task Add_IsolatesConcurrentCallerCredentialsPrincipalAndRole()
    {
        var caller = new AsyncLocal<string>();
        var requests = new ConcurrentDictionary<Guid, string>();
        var credential = Credential();
        credential.GetTokenAsync(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>())
            .Returns(_ => new AccessToken(caller.Value!, DateTimeOffset.MaxValue));
        using var handler = new FabricCoreHttpMessageHandler(async (request, token) =>
        {
            await Task.Yield();
            using var body = JsonDocument.Parse(await request.Content!.ReadAsStringAsync(token));
            var id = body.RootElement.GetProperty("principal").GetProperty("id").GetGuid();
            var role = body.RootElement.GetProperty("role").GetString()!;
            Assert.Equal(id.ToString(), request.Headers.Authorization?.Parameter);
            Assert.True(requests.TryAdd(id, role));
            var assignment = new FabricWorkspaceRoleAssignment(Guid.NewGuid(), new(id, "User"), role);
            return Response(JsonSerializer.Serialize(assignment, CoreJsonContext.Default.FabricWorkspaceRoleAssignment), HttpStatusCode.Created);
        });
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, credential);
        await Task.WhenAll(Enumerable.Range(0, 12).Select(async index =>
        {
            var id = Guid.NewGuid();
            caller.Value = id.ToString();
            var role = index % 2 == 0 ? "Viewer" : "Member";
            var result = await service.AddWorkspaceRoleAssignmentAsync(WorkspaceId,
                $$"""{"id":"{{id}}","type":"User"}""", role, TestContext.Current.CancellationToken);
            Assert.Equal(id, result.RoleAssignment.Principal.Id);
            Assert.Equal(role, result.RoleAssignment.Role);
        }));
        Assert.Equal(12, handler.CallCount);
        Assert.Equal(12, requests.Count);
    }

    private static Task<WorkspaceRoleAssignmentAddCommandResult> AddAsync(FabricCoreService service, CancellationToken? token = null) =>
        service.AddWorkspaceRoleAssignmentAsync(WorkspaceId, PrincipalJson, "Viewer", token ?? TestContext.Current.CancellationToken);

    private static async Task AssertInvalidAsync(string json)
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(Response(json, HttpStatusCode.Created, "45")));
        using var client = new HttpClient(handler);
        var error = await Assert.ThrowsAsync<WorkspaceRoleAssignmentAddRequestException>(() => AddAsync(new FabricCoreService(client, Credential())));
        AssertConfirmed(error);
        Assert.Equal(TimeSpan.FromSeconds(45), error.RetryAfter?.Delta);
        Assert.Equal(1, handler.CallCount);
    }

    private static void AssertConfirmed(WorkspaceRoleAssignmentAddRequestException error)
    {
        Assert.True(error.CreationConfirmed);
        Assert.Equal(HttpStatusCode.Created, error.ResponseStatusCode);
        Assert.Equal(HttpStatusCode.BadGateway, error.StatusCode);
        Assert.False(error.RequestNotSent);
        Assert.DoesNotContain("private", error.Message);
    }

    private static Exception Failure(string kind) => kind switch
    {
        "json" => new JsonException("private"),
        "io" => new IOException("private"),
        "http" => new HttpRequestException("private", null, HttpStatusCode.BadGateway),
        "canceled" => new OperationCanceledException("private"),
        "timeout" => new TimeoutException("private"),
        "reader" => new InvalidOperationException("private-reader"),
        "unsupported" => new NotSupportedException("private-reader"),
        _ => throw new ArgumentOutOfRangeException(nameof(kind))
    };
}
