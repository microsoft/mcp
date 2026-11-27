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

public sealed class WorkspaceRoleAssignmentUpdateServiceTests()
{
    [Theory]
    [InlineData("D")]
    [InlineData("N")]
    [InlineData("B")]
    [InlineData("P")]
    [InlineData("X")]
    public static async Task Update_SendsExactlyOnePatchWithOnlyRoleAndIndependentAssignmentIdentity(string format)
    {
        var json = JsonNode.Parse(AssignmentJson)!;
        json["privateExtension"] = "private";
        json["principal"]!["privateExtension"] = "private";
        using var stream = new OperationTestStream(Encoding.UTF8.GetBytes(json.ToJsonString()));
        using var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StreamContent(stream) };
        response.Content.Headers.ContentType = new("application/json");
        response.Headers.Location = new Uri("https://example.invalid/private");
        response.Headers.TryAddWithoutValidation("x-ms-operation-id", AssignmentId);
        HttpContent? requestContent = null;
        using var handler = new FabricCoreHttpMessageHandler(async (request, token) =>
        {
            Assert.Equal(HttpMethod.Patch, request.Method);
            Assert.Equal(AssignmentUrl, request.RequestUri?.AbsoluteUri);
            Assert.Equal("Bearer", request.Headers.Authorization?.Scheme);
            Assert.Equal("role-list-test-token", request.Headers.Authorization?.Parameter);
            Assert.Equal("Fabric Core MCP", request.Headers.UserAgent.ToString());
            Assert.True(token.CanBeCanceled);
            requestContent = request.Content;
            Assert.Equal("application/json", requestContent!.Headers.ContentType?.MediaType);
            Assert.Equal("""{"role":"Viewer"}""", await requestContent.ReadAsStringAsync(token));
            return response;
        });
        using var client = new HttpClient(handler);
        var credential = Credential();

        var result = await new FabricCoreService(client, credential).UpdateWorkspaceRoleAssignmentAsync(
            Guid.Parse(WorkspaceId).ToString(format).ToUpperInvariant(),
            Guid.Parse(AssignmentId).ToString(format).ToUpperInvariant(), "Viewer", TestContext.Current.CancellationToken);

        Assert.Equal(Assignment, result.RoleAssignment);
        Assert.NotEqual(result.RoleAssignment.Id, result.RoleAssignment.Principal.Id);
        Assert.Equal("Updated", result.UpdateStatus);
        var output = JsonSerializer.Serialize(result, CoreJsonContext.Default.WorkspaceRoleAssignmentUpdateCommandResult);
        Assert.DoesNotContain("private", output);
        Assert.Equal(result, JsonSerializer.Deserialize(output, CoreJsonContext.Default.WorkspaceRoleAssignmentUpdateCommandResult));
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
    public static async Task Update_PreservesExplicitRolesAndAvailablePrincipalMetadataWithoutUsingPrincipalAsRequestIdentity(string type)
    {
        foreach (var role in new[] { "Admin", "Member", "Contributor", "Viewer", "FutureRole", " Viewer " })
        {
            var principal = new FabricPrincipal(Guid.Parse(PrincipalId), type, "",
                UserDetails: type == "User" ? new("") : null,
                GroupDetails: type == "Group" ? new("FutureGroup") : null,
                ServicePrincipalDetails: type == "ServicePrincipal" ? new(Guid.Parse(AppId)) : null,
                ServicePrincipalProfileDetails: type == "ServicePrincipalProfile"
                    ? new(new(Guid.Parse(AppId), "FutureParent")) : null);
            var assignment = Assignment with { Role = role, Principal = principal };
            using var handler = new FabricCoreHttpMessageHandler(async (request, token) =>
            {
                Assert.Equal(AssignmentUrl, request.RequestUri?.AbsoluteUri);
                using var body = JsonDocument.Parse(await request.Content!.ReadAsStringAsync(token));
                Assert.Equal("role", Assert.Single(body.RootElement.EnumerateObject()).Name);
                Assert.Equal(role, body.RootElement.GetProperty("role").GetString());
                return Response(JsonSerializer.Serialize(assignment, CoreJsonContext.Default.FabricWorkspaceRoleAssignment));
            });
            using var client = new HttpClient(handler);
            var result = await new FabricCoreService(client, Credential()).UpdateWorkspaceRoleAssignmentAsync(
                WorkspaceId, AssignmentId, role, TestContext.Current.CancellationToken);
            Assert.Equal(assignment, result.RoleAssignment);
            Assert.Equal(1, handler.CallCount);
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public static async Task Update_AllowsAbsentOrNullOptionalResponseFieldsButRequiresRequestRole(bool explicitNull)
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
        var result = await UpdateAsync(new FabricCoreService(client, Credential()));
        Assert.Equal(new FabricPrincipal(Guid.Parse(PrincipalId), "User"), result.RoleAssignment.Principal);
        Assert.DoesNotContain("displayName", JsonSerializer.Serialize(result, CoreJsonContext.Default.WorkspaceRoleAssignmentUpdateCommandResult));
    }

    [Theory]
    [InlineData("")]
    [InlineData("{private")]
    [InlineData("null")]
    [InlineData("[]")]
    [InlineData("{}")]
    [InlineData("""{"value":[]}""")]
    public static async Task Update_UnusableBodyStillReportsConfirmedUpdate(string json) => await AssertInvalidAsync(json);

    [Theory]
    [InlineData("id", "null")]
    [InlineData("id", "\"private\"")]
    [InlineData("id", "\"" + PrincipalId + "\"")]
    [InlineData("id", "\"00000000-0000-0000-0000-000000000000\"")]
    [InlineData("principal", "null")]
    [InlineData("principal", "{}")]
    [InlineData("role", "null")]
    [InlineData("role", "42")]
    [InlineData("role", "\" \"")]
    [InlineData("role", "\"Admin\"")]
    [InlineData("role", "\"viewer\"")]
    [InlineData("principal.id", "\"private\"")]
    [InlineData("principal.id", "\"00000000-0000-0000-0000-000000000000\"")]
    [InlineData("principal.type", "null")]
    [InlineData("principal.type", "42")]
    [InlineData("principal.type", "\" \"")]
    [InlineData("principal.userDetails", """{"userPrincipalName":false}""")]
    [InlineData("principal.groupDetails", """{"groupType":""}""")]
    [InlineData("principal.servicePrincipalDetails", """{"aadAppId":"00000000-0000-0000-0000-000000000000"}""")]
    [InlineData("principal.servicePrincipalProfileDetails", """{"parentPrincipal":{}}""")]
    public static async Task Update_RejectsMalformedMismatchedOrBroaderMetadataWithoutErasing200(string path, string value)
    {
        var json = JsonNode.Parse(AssignmentJson)!;
        var parts = path.Split('.');
        (parts.Length == 1 ? json : json["principal"]!)[parts[^1]] = JsonNode.Parse(value);
        await AssertInvalidAsync(json.ToJsonString());
    }

    [Theory]
    [InlineData("id")]
    [InlineData("principal")]
    [InlineData("role")]
    [InlineData("principal.id")]
    [InlineData("principal.type")]
    public static async Task Update_RejectsMissingRequiredMetadataWithoutErasing200(string path)
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
    public static async Task Update_RejectsDuplicateAndInvalidUnicodeEvenInOmittedFields(string extra) =>
        await AssertInvalidAsync(AssignmentJson[..^1] + "," + extra + "}");

    [Fact]
    public static async Task Update_RejectsInvalidUtf8WithoutLosingConfirmation()
    {
        var bytes = Encoding.UTF8.GetBytes(AssignmentJson[..^1] + ",\"extra\":\"\u00e9\"}");
        bytes[Array.IndexOf(bytes, (byte)0xc3)] = 0xff;
        using var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(bytes) };
        response.Content.Headers.ContentType = new("application/json");
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
        using var client = new HttpClient(handler);
        AssertConfirmed(await Assert.ThrowsAsync<WorkspaceRoleAssignmentUpdateRequestException>(() => UpdateAsync(new FabricCoreService(client, Credential()))));
    }

    [Theory]
    [InlineData(false, 0)]
    [InlineData(false, 1)]
    [InlineData(true, 0)]
    [InlineData(true, 1)]
    public static async Task Update_EnforcesExactResponseByteBudgetAndDisposesWithoutTruncation(bool knownLength, int excess)
    {
        const int limit = 1024 * 1024;
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
        if (excess == 0)
        {
            Assert.Equal(Assignment, (await UpdateAsync(new FabricCoreService(client, Credential()))).RoleAssignment);
        }
        else
        {
            var error = await Assert.ThrowsAsync<WorkspaceRoleAssignmentUpdateRequestException>(() => UpdateAsync(new FabricCoreService(client, Credential())));
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
    public static async Task Update_EnforcesExactInputDepthWithoutRetainingUnknownJson(int depth)
    {
        var json = AssignmentJson[..^1] + ",\"extra\":" + new string('[', depth - 1) + "0" + new string(']', depth - 1) + "}";
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(Response(json)));
        using var client = new HttpClient(handler);
        if (depth == 64)
        {
            Assert.Equal(Assignment, (await UpdateAsync(new FabricCoreService(client, Credential()))).RoleAssignment);
        }
        else
        {
            AssertConfirmed(await Assert.ThrowsAsync<WorkspaceRoleAssignmentUpdateRequestException>(() => UpdateAsync(new FabricCoreService(client, Credential()))));
        }
        Assert.Equal(1, handler.CallCount);
    }

    [Theory]
    [InlineData(false, 61)]
    [InlineData(true, 62)]
    public static async Task Update_RejectsUnserializableEnvelopeMetadataInsideConfirmedMutationBoundary(bool leafDetails, int outputDepth)
    {
        var json = NestedPrincipalAssignment(30, leafDetails);
        using var document = JsonDocument.Parse(json);
        Assert.Equal(outputDepth - 1, JsonDepth(document.RootElement));
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(Response(json, retryAfter: "45")));
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, Credential());
        if (outputDepth == 61)
        {
            var result = await service.UpdateWorkspaceRoleAssignmentAsync(WorkspaceId, AssignmentId, "Member", TestContext.Current.CancellationToken);
            Assert.Equal(61, JsonDepth(JsonSerializer.SerializeToElement(result, CoreJsonContext.Default.WorkspaceRoleAssignmentUpdateCommandResult)));
        }
        else
        {
            var error = await Assert.ThrowsAsync<WorkspaceRoleAssignmentUpdateRequestException>(() =>
                service.UpdateWorkspaceRoleAssignmentAsync(WorkspaceId, AssignmentId, "Member", TestContext.Current.CancellationToken));
            AssertConfirmed(error);
            Assert.Equal(TimeSpan.FromSeconds(45), error.RetryAfter?.Delta);
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
    [InlineData(HttpStatusCode.Conflict)]
    [InlineData(HttpStatusCode.RequestTimeout)]
    [InlineData(HttpStatusCode.TooManyRequests)]
    [InlineData(HttpStatusCode.ServiceUnavailable)]
    public static async Task Update_PreservesStatusAndSafeRetryGuidanceWithoutFallbackRetryOrPolling(HttpStatusCode status)
    {
        // cspell:disable-next-line
        using var response = Response("""{"errorCode":"UnknownError","message":"private","requestId":"private","isRetriable":true}""", status, "45");
        response.Headers.Location = new Uri("https://example.invalid/private");
        response.Headers.TryAddWithoutValidation("x-ms-operation-id", AssignmentId);
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
        using var client = new HttpClient(handler);
        var error = await Assert.ThrowsAsync<WorkspaceRoleAssignmentUpdateRequestException>(() => UpdateAsync(new FabricCoreService(client, Credential())));
        Assert.Equal(response.IsSuccessStatusCode ? HttpStatusCode.BadGateway : status, error.StatusCode);
        Assert.Equal(status, error.ResponseStatusCode);
        Assert.False(error.UpdateConfirmed);
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
    public static async Task Update_UnusableOptionalErrorMetadataPreservesStatusAndRetryAfter(string json)
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(Response(json, HttpStatusCode.Forbidden, "45")));
        using var client = new HttpClient(handler);
        var error = await Assert.ThrowsAsync<WorkspaceRoleAssignmentUpdateRequestException>(() => UpdateAsync(new FabricCoreService(client, Credential())));
        Assert.Equal(HttpStatusCode.Forbidden, error.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, error.ResponseStatusCode);
        Assert.True(error.ErrorMetadataUnavailable);
        Assert.Null(error.ErrorCode);
        Assert.Equal(TimeSpan.FromSeconds(45), error.RetryAfter?.Delta);
        Assert.Equal(1, handler.CallCount);
    }

    [Fact]
    public static async Task Update_BoundsOptionalErrorBodyWithoutLosingRejectionOrTiming()
    {
        using var stream = new PaddedJsonTestStream(Encoding.UTF8.GetBytes("""{"errorCode":"UnknownError"}"""), 1048577);
        using var response = new HttpResponseMessage(HttpStatusCode.TooManyRequests) { Content = new StreamContent(stream) };
        response.Content.Headers.ContentType = new("application/json");
        response.Headers.TryAddWithoutValidation("Retry-After", "45");
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
        using var client = new HttpClient(handler);
        var error = await Assert.ThrowsAsync<WorkspaceRoleAssignmentUpdateRequestException>(() => UpdateAsync(new FabricCoreService(client, Credential())));
        Assert.Equal(HttpStatusCode.TooManyRequests, error.StatusCode);
        Assert.True(error.ErrorMetadataUnavailable);
        Assert.Equal(TimeSpan.FromSeconds(45), error.RetryAfter?.Delta);
        Assert.Equal(1048577, stream.BytesRead);
        Assert.True(stream.Disposed);
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
    public static async Task Update_BodyFailuresPreserveConfirmedOrRejectedOutcomeAndRetryAfter(string failure)
    {
        foreach (var status in new[] { HttpStatusCode.OK, HttpStatusCode.TooManyRequests })
        {
            using var stream = new OperationTestStream(Encoding.UTF8.GetBytes(AssignmentJson), (_, _) => throw Failure(failure));
            using var response = new HttpResponseMessage(status) { Content = new StreamContent(stream) };
            response.Content.Headers.ContentType = new("application/json");
            response.Headers.TryAddWithoutValidation("Retry-After", "45");
            using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
            using var client = new HttpClient(handler);
            var error = await Assert.ThrowsAsync<WorkspaceRoleAssignmentUpdateRequestException>(() => UpdateAsync(new FabricCoreService(client, Credential())));
            Assert.Equal(status, error.ResponseStatusCode);
            Assert.Equal(status == HttpStatusCode.OK, error.UpdateConfirmed);
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
    public static async Task Update_UnsupportedSuccessMediaPreservesConfirmationWithoutReading(string? mediaType)
    {
        using var content = new UnreadableHttpContent();
        content.Headers.ContentType = mediaType is null ? null : new(mediaType);
        using var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = content };
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
        using var client = new HttpClient(handler);
        AssertConfirmed(await Assert.ThrowsAsync<WorkspaceRoleAssignmentUpdateRequestException>(() => UpdateAsync(new FabricCoreService(client, Credential()))));
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
    public static async Task Update_ReturnsOnlyValidatedRetryGuidance(string? header, long? expected)
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(Response(AssignmentJson, retryAfter: header)));
        using var client = new HttpClient(handler);
        var result = await UpdateAsync(new FabricCoreService(client, Credential()));
        Assert.Equal(expected, result.RetryAfterSeconds);
        Assert.Null(result.RetryAfterUtc);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public static async Task Update_PreservesHttpDatesButIgnoresAmbiguousRetryHeaders(bool multiple)
    {
        using var response = Response(AssignmentJson);
        response.Headers.TryAddWithoutValidation("Retry-After", multiple ? ["45", "60"] : ["Tue, 01 Jan 2030 00:00:00 GMT"]);
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
        using var client = new HttpClient(handler);
        var result = await UpdateAsync(new FabricCoreService(client, Credential()));
        Assert.Equal(multiple ? null : new DateTimeOffset(2030, 1, 1, 0, 0, 0, TimeSpan.Zero), result.RetryAfterUtc);
        Assert.Null(result.RetryAfterSeconds);
    }

    [Fact]
    public static async Task Update_ValidatesInputsAndAuthenticationBeforeHttp()
    {
        var credential = Credential();
        using var handler = new FabricCoreHttpMessageHandler((_, _) => throw new InvalidOperationException("No HTTP expected."));
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, credential);
        await Assert.ThrowsAsync<FabricItemInputException>(() => service.UpdateWorkspaceRoleAssignmentAsync("private", AssignmentId, "Viewer", TestContext.Current.CancellationToken));
        await Assert.ThrowsAsync<FabricItemInputException>(() => service.UpdateWorkspaceRoleAssignmentAsync(WorkspaceId, "private", "Viewer", TestContext.Current.CancellationToken));
        await Assert.ThrowsAsync<FabricItemInputException>(() => service.UpdateWorkspaceRoleAssignmentAsync(WorkspaceId, AssignmentId, "", TestContext.Current.CancellationToken));
        Assert.Empty(credential.ReceivedCalls());
        credential.GetTokenAsync(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>())
            .Returns(ValueTask.FromException<AccessToken>(new AuthenticationFailedException("private")));
        var error = await Assert.ThrowsAsync<WorkspaceRoleAssignmentUpdateRequestException>(() => UpdateAsync(service));
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
    public static async Task Update_CancellationDistinguishesUnsentUncertainAndConfirmedAndDisposes(string phase)
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
        using var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StreamContent(stream) };
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
            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => UpdateAsync(new FabricCoreService(client, credential), cancellation.Token));
            Assert.Empty(credential.ReceivedCalls());
        }
        else
        {
            var error = await Assert.ThrowsAsync<WorkspaceRoleAssignmentUpdateRequestException>(() => UpdateAsync(new FabricCoreService(client, credential), cancellation.Token));
            Assert.Equal(HttpStatusCode.RequestTimeout, error.StatusCode);
            Assert.Equal(phase == "authentication", error.RequestNotSent);
            Assert.Equal(phase is "headers" or "body" or "end-of-body", error.UpdateConfirmed);
            if (error.UpdateConfirmed)
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
    public static async Task Update_SendFailuresNeverImplyRejectionOrConfirmation(string failure)
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) => throw Failure(failure));
        using var client = new HttpClient(handler);
        var error = await Assert.ThrowsAsync<WorkspaceRoleAssignmentUpdateRequestException>(() => UpdateAsync(new FabricCoreService(client, Credential())));
        Assert.Null(error.ResponseStatusCode);
        Assert.False(error.UpdateConfirmed);
        Assert.False(error.RequestNotSent);
        Assert.Equal(1, handler.CallCount);
    }

    [Fact]
    public static async Task Update_IsolatesConcurrentCallerCredentialsWorkspaceAssignmentAndRole()
    {
        var caller = new AsyncLocal<string>();
        var requests = new ConcurrentDictionary<Guid, string>();
        var credential = Credential();
        credential.GetTokenAsync(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>())
            .Returns(_ => new AccessToken(caller.Value!, DateTimeOffset.MaxValue));
        using var handler = new FabricCoreHttpMessageHandler(async (request, token) =>
        {
            await Task.Yield();
            Assert.Equal(HttpMethod.Patch, request.Method);
            var id = Guid.Parse(request.RequestUri!.Segments[^1]);
            Assert.Equal(id.ToString(), request.Headers.Authorization?.Parameter);
            Assert.Equal($"/v1/workspaces/{id:D}/roleAssignments/{id:D}", request.RequestUri.AbsolutePath);
            using var body = JsonDocument.Parse(await request.Content!.ReadAsStringAsync(token));
            var role = body.RootElement.GetProperty("role").GetString()!;
            Assert.True(requests.TryAdd(id, role));
            return Response(JsonSerializer.Serialize(Assignment with { Id = id, Role = role }, CoreJsonContext.Default.FabricWorkspaceRoleAssignment));
        });
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, credential);
        await Task.WhenAll(Enumerable.Range(0, 12).Select(async index =>
        {
            var id = Guid.NewGuid();
            caller.Value = id.ToString();
            var role = index % 2 == 0 ? "Viewer" : "Member";
            var result = await service.UpdateWorkspaceRoleAssignmentAsync(id.ToString(), id.ToString(), role, TestContext.Current.CancellationToken);
            Assert.Equal(id, result.RoleAssignment.Id);
            Assert.Equal(Guid.Parse(PrincipalId), result.RoleAssignment.Principal.Id);
            Assert.Equal(role, result.RoleAssignment.Role);
        }));
        Assert.Equal(12, handler.CallCount);
        Assert.Equal(12, requests.Count);
    }

    private static Task<WorkspaceRoleAssignmentUpdateCommandResult> UpdateAsync(FabricCoreService service, CancellationToken? token = null) =>
        service.UpdateWorkspaceRoleAssignmentAsync(WorkspaceId, AssignmentId, "Viewer", token ?? TestContext.Current.CancellationToken);

    private static async Task AssertInvalidAsync(string json)
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(Response(json, retryAfter: "45")));
        using var client = new HttpClient(handler);
        var error = await Assert.ThrowsAsync<WorkspaceRoleAssignmentUpdateRequestException>(() => UpdateAsync(new FabricCoreService(client, Credential())));
        AssertConfirmed(error);
        Assert.Equal(TimeSpan.FromSeconds(45), error.RetryAfter?.Delta);
        Assert.Equal(1, handler.CallCount);
    }

    private static void AssertConfirmed(WorkspaceRoleAssignmentUpdateRequestException error)
    {
        Assert.True(error.UpdateConfirmed);
        Assert.Equal(HttpStatusCode.OK, error.ResponseStatusCode);
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
