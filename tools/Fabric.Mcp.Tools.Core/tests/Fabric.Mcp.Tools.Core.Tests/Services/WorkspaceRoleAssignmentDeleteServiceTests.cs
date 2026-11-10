// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
// cspell:words Retriable

using System.Collections.Concurrent;
using System.Net;
using System.Text;
using System.Text.Json;
using Azure.Core;
using Azure.Identity;
using Fabric.Mcp.Tools.Core.Models;
using Fabric.Mcp.Tools.Core.Services;
using Fabric.Mcp.Tools.Core.Tests.TestSupport;
using NSubstitute;
using Xunit;
using static Fabric.Mcp.Tools.Core.Tests.TestSupport.WorkspaceRoleAssignmentTestData;

namespace Fabric.Mcp.Tools.Core.Tests.Services;

public sealed class WorkspaceRoleAssignmentDeleteServiceTests()
{
    [Fact]
    public static async Task Delete_SendsOneNormalizedAuthenticatedRequestWithoutBodyQueryOrSuccessRead()
    {
        using var content = new UnreadableHttpContent();
        using var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = content };
        response.Headers.TryAddWithoutValidation("Location", ["https://example.invalid/private", "/v1/operations/not-an-assignment"]);
        response.Headers.TryAddWithoutValidation("x-ms-operation-id", "private-ignored");
        response.Headers.TryAddWithoutValidation("Retry-After", ["private-invalid", "-1"]);
        using var handler = new FabricCoreHttpMessageHandler((request, token) =>
        {
            Assert.Equal(HttpMethod.Delete, request.Method);
            Assert.Equal(AssignmentUrl, request.RequestUri?.AbsoluteUri);
            Assert.DoesNotContain(PrincipalId, request.RequestUri!.AbsoluteUri);
            Assert.Empty(request.RequestUri.Query);
            Assert.Empty(request.RequestUri.Fragment);
            Assert.Null(request.Content);
            Assert.Equal("Bearer", request.Headers.Authorization?.Scheme);
            Assert.Equal("role-list-test-token", request.Headers.Authorization?.Parameter);
            Assert.Equal("Fabric Core MCP", request.Headers.UserAgent.ToString());
            Assert.True(token.CanBeCanceled);
            return Task.FromResult(response);
        });
        using var client = new HttpClient(handler);
        var credential = Credential();

        var result = await new FabricCoreService(client, credential).DeleteWorkspaceRoleAssignmentAsync(
            $"{{{WorkspaceId.ToUpperInvariant()}}}", Guid.Parse(AssignmentId).ToString("N").ToUpperInvariant(),
            TestContext.Current.CancellationToken);

        Assert.Equal(Guid.Parse(WorkspaceId), result.WorkspaceId);
        Assert.Equal(Guid.Parse(AssignmentId), result.WorkspaceRoleAssignmentId);
        Assert.Equal("Deleted", result.DeletionStatus);
        var output = JsonSerializer.SerializeToElement(result, CoreJsonContext.Default.WorkspaceRoleAssignmentDeleteCommandResult);
        Assert.Equal(["deletionStatus", "guidance", "workspaceId", "workspaceRoleAssignmentId"],
            output.EnumerateObject().Select(property => property.Name).Order());
        Assert.DoesNotContain(PrincipalId, output.GetRawText());
        Assert.DoesNotContain("private", output.GetRawText());
        Assert.Equal(1, handler.CallCount);
        Assert.True(content.IsDisposed);
        await credential.Received(1).GetTokenAsync(
            Arg.Is<TokenRequestContext>(context => context.Scopes.SequenceEqual(FabricEndpoints.FabricScopes)),
            TestContext.Current.CancellationToken);
    }

    [Fact]
    public static async Task Delete_Empty200IsSufficientAndAnExplicitRepeat404IsNotSuccess()
    {
        var count = 0;
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(
            Interlocked.Increment(ref count) == 1
                ? new HttpResponseMessage(HttpStatusCode.OK)
                : Response("""{"errorCode":"EntityNotFound","message":"private"}""", HttpStatusCode.NotFound)));
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, Credential());
        Assert.Equal("Deleted", (await DeleteAsync(service)).DeletionStatus);
        Assert.Equal(1, handler.CallCount);
        var error = await Assert.ThrowsAsync<WorkspaceRoleAssignmentDeleteRequestException>(() => DeleteAsync(service));
        Assert.Equal(HttpStatusCode.NotFound, error.StatusCode);
        Assert.Equal("EntityNotFound", error.ErrorCode);
        Assert.False(error.DeletionConfirmed);
        Assert.Equal(2, handler.CallCount);
    }

    [Theory]
    [InlineData(HttpStatusCode.Created, HttpStatusCode.BadGateway)]
    [InlineData(HttpStatusCode.Accepted, HttpStatusCode.BadGateway)]
    [InlineData(HttpStatusCode.NoContent, HttpStatusCode.BadGateway)]
    [InlineData(HttpStatusCode.PartialContent, HttpStatusCode.BadGateway)]
    [InlineData(HttpStatusCode.TemporaryRedirect, HttpStatusCode.TemporaryRedirect)]
    [InlineData(HttpStatusCode.BadRequest, HttpStatusCode.BadRequest)]
    [InlineData(HttpStatusCode.Unauthorized, HttpStatusCode.Unauthorized)]
    [InlineData(HttpStatusCode.Forbidden, HttpStatusCode.Forbidden)]
    [InlineData(HttpStatusCode.NotFound, HttpStatusCode.NotFound)]
    [InlineData(HttpStatusCode.RequestTimeout, HttpStatusCode.RequestTimeout)]
    [InlineData(HttpStatusCode.Conflict, HttpStatusCode.Conflict)]
    [InlineData(HttpStatusCode.TooManyRequests, HttpStatusCode.TooManyRequests)]
    [InlineData(HttpStatusCode.InternalServerError, HttpStatusCode.InternalServerError)]
    [InlineData(HttpStatusCode.ServiceUnavailable, HttpStatusCode.ServiceUnavailable)]
    public static async Task Delete_PreservesFailuresWithoutFallbackFollowingUrlsPollingOrRetrying(HttpStatusCode status, HttpStatusCode expected)
    {
        using var response = Response("""{"errorCode":"UnknownError","message":"private","isRetriable":true,"moreDetails":[{"message":"private"}]}""", status, "60");
        response.Headers.Location = new Uri("https://example.invalid/private");
        response.Headers.TryAddWithoutValidation("x-ms-operation-id", AssignmentId);
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
        using var client = new HttpClient(handler);
        var error = await Assert.ThrowsAsync<WorkspaceRoleAssignmentDeleteRequestException>(() => DeleteAsync(new FabricCoreService(client, Credential())));
        Assert.Equal(expected, error.StatusCode);
        Assert.Equal(status, error.ResponseStatusCode);
        Assert.Equal(TimeSpan.FromSeconds(60), error.RetryAfter?.Delta);
        Assert.Equal(response.IsSuccessStatusCode ? null : "UnknownError", error.ErrorCode);
        Assert.False(error.RequestNotSent);
        Assert.False(error.DeletionConfirmed);
        Assert.DoesNotContain("private", error.Message);
        Assert.Equal(1, handler.CallCount);
        Assert.Throws<ObjectDisposedException>(() => response.Content.ReadAsStream(TestContext.Current.CancellationToken));
    }

    [Theory]
    [InlineData("""{"errorCode":"\uD800"}""")]
    [InlineData("""{"errorCode":"\uDC00"}""")]
    [InlineData("""{"error\uD800Code":"UnknownError"}""")]
    [InlineData("""{"error\uDC00Code":"UnknownError"}""")]
    [InlineData("""{"errorCode":"UnknownError","errorCode":"Other"}""")]
    [InlineData("""{"errorCode":"private invalid"}""")]
    [InlineData("""{"errorCode":null}""")]
    [InlineData("""{"errorCode":12}""")]
    [InlineData("private-invalid-json")]
    [InlineData("[]")]
    [InlineData("")]
    public static async Task Delete_UnusableOptionalErrorMetadataKeepsKnownStatusAndRetryAfter(string json)
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(Response(json, HttpStatusCode.TooManyRequests, "60")));
        using var client = new HttpClient(handler);
        var error = await Assert.ThrowsAsync<WorkspaceRoleAssignmentDeleteRequestException>(() => DeleteAsync(new FabricCoreService(client, Credential())));
        Assert.Equal(HttpStatusCode.TooManyRequests, error.StatusCode);
        Assert.Equal(TimeSpan.FromSeconds(60), error.RetryAfter?.Delta);
        Assert.True(error.ErrorMetadataUnavailable);
        Assert.Null(error.ErrorCode);
        Assert.Equal(1, handler.CallCount);
    }

    [Theory]
    [InlineData(false, 0)]
    [InlineData(false, 1)]
    [InlineData(true, 0)]
    [InlineData(true, 1)]
    public static async Task Delete_BoundsOptionalErrorBytesExactlyAndDisposes(bool contentLength, int excess)
    {
        const int limit = FabricResponseBodyReader.DefaultMaxJsonBytes;
        const string empty = """{"errorCode":"UnknownError","padding":""}""";
        var json = empty.Replace("\"padding\":\"\"", $"\"padding\":\"{new string('x', limit - empty.Length + excess)}\"", StringComparison.Ordinal);
        using var stream = new OperationTestStream(Encoding.UTF8.GetBytes(json));
        using var response = new HttpResponseMessage(HttpStatusCode.Conflict) { Content = new StreamContent(stream) };
        response.Content.Headers.ContentType = new("application/json");
        response.Headers.TryAddWithoutValidation("Retry-After", "60");
        if (contentLength)
        {
            response.Content.Headers.ContentLength = limit + excess;
        }
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
        using var client = new HttpClient(handler);
        var error = await Assert.ThrowsAsync<WorkspaceRoleAssignmentDeleteRequestException>(() => DeleteAsync(new FabricCoreService(client, Credential())));
        Assert.Equal(HttpStatusCode.Conflict, error.StatusCode);
        Assert.Equal(TimeSpan.FromSeconds(60), error.RetryAfter?.Delta);
        Assert.Equal(excess == 0 ? "UnknownError" : null, error.ErrorCode);
        Assert.Equal(excess != 0, error.ErrorMetadataUnavailable);
        Assert.Equal(contentLength && excess > 0 ? 0 : limit + excess, stream.BytesRead);
        Assert.True(stream.Disposed);
        Assert.Equal(1, handler.CallCount);
    }

    [Theory]
    [InlineData(128, 63, false)]
    [InlineData(129, 63, true)]
    [InlineData(128, 64, true)]
    public static async Task Delete_BoundsOptionalErrorCodeAndJsonDepth(int codeLength, int nestedArrays, bool unavailable)
    {
        var code = new string('x', codeLength);
        var json = $"{{\"errorCode\":\"{code}\",\"extra\":" + new string('[', nestedArrays) + "0" + new string(']', nestedArrays) + "}";
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(Response(json, HttpStatusCode.Conflict, "60")));
        using var client = new HttpClient(handler);
        var error = await Assert.ThrowsAsync<WorkspaceRoleAssignmentDeleteRequestException>(() => DeleteAsync(new FabricCoreService(client, Credential())));
        Assert.Equal(HttpStatusCode.Conflict, error.StatusCode);
        Assert.Equal(unavailable, error.ErrorMetadataUnavailable);
        Assert.Equal(unavailable ? null : code, error.ErrorCode);
        Assert.Equal(TimeSpan.FromSeconds(60), error.RetryAfter?.Delta);
        Assert.Equal(1, handler.CallCount);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("private-invalid")]
    [InlineData("-1")]
    [InlineData("9223372036854775807")]
    [InlineData("0")]
    [InlineData("60")]
    [InlineData("Tue, 01 Jan 2030 00:00:00 GMT")]
    [InlineData("60, 120")]
    public static async Task Delete_PreservesOnlyValidRetryAfterWithoutRetrying(string? retryAfter)
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(Response("{}", HttpStatusCode.TooManyRequests, retryAfter)));
        using var client = new HttpClient(handler);
        var error = await Assert.ThrowsAsync<WorkspaceRoleAssignmentDeleteRequestException>(() => DeleteAsync(new FabricCoreService(client, Credential())));
        Assert.Equal(HttpStatusCode.TooManyRequests, error.StatusCode);
        if (retryAfter is "0" or "60")
        {
            Assert.Equal(TimeSpan.FromSeconds(retryAfter == "0" ? 0 : 60), error.RetryAfter?.Delta);
        }
        else if (retryAfter == "Tue, 01 Jan 2030 00:00:00 GMT")
        {
            Assert.Equal(new DateTimeOffset(2030, 1, 1, 0, 0, 0, TimeSpan.Zero), error.RetryAfter?.Date);
        }
        else
        {
            Assert.Null(error.RetryAfter);
        }
        Assert.Equal(1, handler.CallCount);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("00000000-0000-0000-0000-000000000000")]
    [InlineData("private/path")]
    [InlineData("https://example.invalid/private")]
    public static async Task Delete_ValidatesBothIdsBeforeAuthentication(string? value)
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) => throw new InvalidOperationException("No HTTP expected."));
        using var client = new HttpClient(handler);
        var credential = Credential();
        var service = new FabricCoreService(client, credential);
        await Assert.ThrowsAsync<ArgumentException>(() => service.DeleteWorkspaceRoleAssignmentAsync(value!, AssignmentId, TestContext.Current.CancellationToken));
        await Assert.ThrowsAsync<ArgumentException>(() => service.DeleteWorkspaceRoleAssignmentAsync(WorkspaceId, value!, TestContext.Current.CancellationToken));
        Assert.Equal(0, handler.CallCount);
        Assert.Empty(credential.ReceivedCalls());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public static async Task Delete_PreSendCancellationIsNotAnUncertainMutation(bool duringAuth)
    {
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        var credential = Credential();
        credential.GetTokenAsync(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>()).Returns(_ =>
        {
            cancellation.Cancel();
            return new AccessToken("role-list-test-token", DateTimeOffset.MaxValue);
        });
        if (!duringAuth)
        {
            cancellation.Cancel();
        }
        using var handler = new FabricCoreHttpMessageHandler((_, _) => throw new InvalidOperationException("No HTTP expected."));
        using var client = new HttpClient(handler);
        var error = await Assert.ThrowsAsync<WorkspaceRoleAssignmentDeleteRequestException>(() => DeleteAsync(new FabricCoreService(client, credential), cancellation.Token));
        Assert.Equal(HttpStatusCode.RequestTimeout, error.StatusCode);
        Assert.True(error.RequestNotSent);
        Assert.False(error.DeletionConfirmed);
        Assert.Null(error.ResponseStatusCode);
        Assert.Equal(0, handler.CallCount);
        await credential.Received(duringAuth ? 1 : 0).GetTokenAsync(Arg.Any<TokenRequestContext>(), cancellation.Token);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public static async Task Delete_AuthenticationAndTokenNetworkFailuresNeverSend(bool network)
    {
        var credential = Credential();
        credential.GetTokenAsync(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>())
            .Returns(ValueTask.FromException<AccessToken>(network ? new HttpRequestException("private") : new AuthenticationFailedException("private")));
        using var handler = new FabricCoreHttpMessageHandler((_, _) => throw new InvalidOperationException("No HTTP expected."));
        using var client = new HttpClient(handler);
        var error = await Assert.ThrowsAsync<WorkspaceRoleAssignmentDeleteRequestException>(() => DeleteAsync(new FabricCoreService(client, credential)));
        Assert.Equal(network ? HttpStatusCode.ServiceUnavailable : HttpStatusCode.Unauthorized, error.StatusCode);
        Assert.True(error.RequestNotSent);
        Assert.False(error.DeletionConfirmed);
        Assert.Equal(0, handler.CallCount);
    }

    [Theory]
    [InlineData("network", HttpStatusCode.ServiceUnavailable)]
    [InlineData("io", HttpStatusCode.ServiceUnavailable)]
    [InlineData("timeout", HttpStatusCode.GatewayTimeout)]
    [InlineData("cancellation", HttpStatusCode.RequestTimeout)]
    public static async Task Delete_FailedSendIsUncertainAndNeverRetried(string failure, HttpStatusCode status)
    {
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        using var handler = new FabricCoreHttpMessageHandler((_, token) =>
        {
            if (failure == "cancellation")
            {
                cancellation.Cancel();
                return Task.FromCanceled<HttpResponseMessage>(token);
            }
            throw failure switch
            {
                "network" => new HttpRequestException("private"),
                "io" => new IOException("private"),
                _ => new TimeoutException("private")
            };
        });
        using var client = new HttpClient(handler);
        var error = await Assert.ThrowsAsync<WorkspaceRoleAssignmentDeleteRequestException>(() => DeleteAsync(new FabricCoreService(client, Credential()), cancellation.Token));
        Assert.Equal(status, error.StatusCode);
        Assert.False(error.RequestNotSent);
        Assert.False(error.DeletionConfirmed);
        Assert.Null(error.ResponseStatusCode);
        Assert.DoesNotContain("private", error.Message);
        Assert.Equal(1, handler.CallCount);
    }

    [Fact]
    public static async Task Delete_LateCancellationAndUnreadableBodyDoNotEraseConfirmedDeletion()
    {
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        using var content = new UnreadableHttpContent();
        using var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = content };
        using var handler = new FabricCoreHttpMessageHandler((_, _) =>
        {
            cancellation.Cancel();
            return Task.FromResult(response);
        });
        using var client = new HttpClient(handler);
        var result = await DeleteAsync(new FabricCoreService(client, Credential()), cancellation.Token);
        Assert.Equal("Deleted", result.DeletionStatus);
        Assert.True(content.IsDisposed);
        Assert.Equal(1, handler.CallCount);
    }

    [Theory]
    [InlineData(HttpStatusCode.OK, "io")]
    [InlineData(HttpStatusCode.OK, "unsupported")]
    [InlineData(HttpStatusCode.OK, "invalid")]
    [InlineData(HttpStatusCode.TooManyRequests, "io")]
    [InlineData(HttpStatusCode.TooManyRequests, "unsupported")]
    [InlineData(HttpStatusCode.TooManyRequests, "invalid")]
    public static async Task Delete_LocalDisposalFailureKeepsKnownOutcomeAndRetryAfter(HttpStatusCode status, string failure)
    {
        var stream = Substitute.For<Stream>();
        stream.When(value => value.Close()).Do(_ => throw failure switch
        {
            "io" => new IOException("private"),
            "unsupported" => new NotSupportedException("private"),
            _ => new InvalidOperationException("private")
        });
        using var response = new HttpResponseMessage(status) { Content = new StreamContent(stream) };
        response.Headers.TryAddWithoutValidation("Retry-After", "60");
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
        using var client = new HttpClient(handler);
        var error = await Assert.ThrowsAsync<WorkspaceRoleAssignmentDeleteRequestException>(() => DeleteAsync(new FabricCoreService(client, Credential())));
        Assert.Equal(status == HttpStatusCode.OK, error.DeletionConfirmed);
        Assert.False(error.RequestNotSent);
        Assert.Equal(status == HttpStatusCode.OK ? HttpStatusCode.BadGateway : status, error.StatusCode);
        Assert.Equal(status, error.ResponseStatusCode);
        if (status != HttpStatusCode.OK)
        {
            Assert.Equal(TimeSpan.FromSeconds(60), error.RetryAfter?.Delta);
            Assert.True(error.ErrorMetadataUnavailable);
        }
        Assert.DoesNotContain("private", error.Message);
        Assert.Equal(1, handler.CallCount);
        stream.Received(1).Close();
    }

    [Fact]
    public static async Task Delete_NonJsonErrorBodyIsNotReadAndKeepsKnownFailure()
    {
        using var content = new UnreadableHttpContent();
        content.Headers.ContentType = new("text/plain");
        using var response = new HttpResponseMessage(HttpStatusCode.TooManyRequests) { Content = content };
        response.Headers.TryAddWithoutValidation("Retry-After", "60");
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
        using var client = new HttpClient(handler);
        var error = await Assert.ThrowsAsync<WorkspaceRoleAssignmentDeleteRequestException>(() => DeleteAsync(new FabricCoreService(client, Credential())));
        Assert.Equal(HttpStatusCode.TooManyRequests, error.StatusCode);
        Assert.Equal(TimeSpan.FromSeconds(60), error.RetryAfter?.Delta);
        Assert.True(error.ErrorMetadataUnavailable);
        Assert.True(content.IsDisposed);
        Assert.Equal(1, handler.CallCount);
    }

    [Theory]
    [InlineData("io")]
    [InlineData("network")]
    [InlineData("timeout")]
    [InlineData("cancellation")]
    [InlineData("unsupported")]
    [InlineData("invalid")]
    public static async Task Delete_FailedOptionalErrorReadsRetainStatusRetryAndDisposal(string failure)
    {
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        using var stream = new OperationTestStream(Encoding.UTF8.GetBytes("""{"errorCode":"UnknownError"}"""), (_, token) =>
        {
            if (failure == "cancellation")
            {
                cancellation.Cancel();
                return ValueTask.FromCanceled(token);
            }
            return ValueTask.FromException(failure switch
            {
                "io" => new IOException("private"),
                "network" => new HttpRequestException("private"),
                "unsupported" => new NotSupportedException("private"),
                "invalid" => new InvalidOperationException("private"),
                _ => new TimeoutException("private")
            });
        });
        using var response = new HttpResponseMessage(HttpStatusCode.TooManyRequests) { Content = new StreamContent(stream) };
        response.Content.Headers.ContentType = new("application/json");
        response.Headers.TryAddWithoutValidation("Retry-After", "60");
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
        using var client = new HttpClient(handler);
        var error = await Assert.ThrowsAsync<WorkspaceRoleAssignmentDeleteRequestException>(() => DeleteAsync(new FabricCoreService(client, Credential()), cancellation.Token));
        Assert.Equal(HttpStatusCode.TooManyRequests, error.StatusCode);
        Assert.Equal(TimeSpan.FromSeconds(60), error.RetryAfter?.Delta);
        Assert.True(error.ErrorMetadataUnavailable);
        Assert.True(stream.Disposed);
        Assert.Equal(1, handler.CallCount);
    }

    [Fact]
    public static async Task Delete_IsolatesConcurrentCallerCredentialsTargetsAndReceipts()
    {
        var identity = new AsyncLocal<string>();
        var credential = Credential();
        credential.GetTokenAsync(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>())
            .Returns(_ => new AccessToken(identity.Value!, DateTimeOffset.MaxValue));
        var observed = new ConcurrentBag<string>();
        using var handler = new FabricCoreHttpMessageHandler(async (request, _) =>
        {
            var current = request.Headers.Authorization!.Parameter!;
            observed.Add(current);
            await Task.Yield();
            Assert.Equal($"{FabricEndpoints.GetFabricApiBaseUrl()}/workspaces/{WorkspaceId}/roleAssignments/{current}", request.RequestUri?.AbsoluteUri);
            Assert.Equal(HttpMethod.Delete, request.Method);
            Assert.Null(request.Content);
            return new HttpResponseMessage(HttpStatusCode.OK);
        });
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, credential);
        await Task.WhenAll(Enumerable.Range(0, 8).Select(async _ =>
        {
            var id = Guid.NewGuid();
            identity.Value = id.ToString();
            var result = await service.DeleteWorkspaceRoleAssignmentAsync(WorkspaceId, id.ToString(), TestContext.Current.CancellationToken);
            Assert.Equal(Guid.Parse(WorkspaceId), result.WorkspaceId);
            Assert.Equal(id, result.WorkspaceRoleAssignmentId);
            Assert.Equal("Deleted", result.DeletionStatus);
        }));
        Assert.Equal(8, handler.CallCount);
        Assert.Equal(8, observed.Distinct().Count());
        await credential.Received(8).GetTokenAsync(Arg.Any<TokenRequestContext>(), TestContext.Current.CancellationToken);
    }

    [Fact]
    public static async Task Delete_UncertaintyRequiresASeparateExplicitInspection()
    {
        var methods = new List<HttpMethod>();
        using var handler = new FabricCoreHttpMessageHandler((request, _) =>
        {
            methods.Add(request.Method);
            Assert.Equal(AssignmentUrl, request.RequestUri?.AbsoluteUri);
            return request.Method == HttpMethod.Delete
                ? throw new HttpRequestException("private")
                : Task.FromResult(Response(AssignmentJson));
        });
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, Credential());
        var error = await Assert.ThrowsAsync<WorkspaceRoleAssignmentDeleteRequestException>(() => DeleteAsync(service));
        Assert.False(error.DeletionConfirmed);
        Assert.Equal([HttpMethod.Delete], methods);
        var inspected = await service.GetWorkspaceRoleAssignmentAsync(WorkspaceId, AssignmentId, TestContext.Current.CancellationToken);
        Assert.Equal(Assignment, inspected.RoleAssignment);
        Assert.Equal([HttpMethod.Delete, HttpMethod.Get], methods);
        Assert.Equal(2, handler.CallCount);
    }

    private static Task<WorkspaceRoleAssignmentDeleteCommandResult> DeleteAsync(FabricCoreService service, CancellationToken? cancellationToken = null) =>
        service.DeleteWorkspaceRoleAssignmentAsync(WorkspaceId, AssignmentId, cancellationToken ?? TestContext.Current.CancellationToken);
}
