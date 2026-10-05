// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text;
using System.Text.Json;
using Azure.Core;
using Fabric.Mcp.Tools.Core.Models;
using Fabric.Mcp.Tools.Core.Services;
using Fabric.Mcp.Tools.Core.Tests.TestSupport;
using NSubstitute;
using Xunit;

namespace Fabric.Mcp.Tools.Core.Tests.Services;

public class FabricCoreServiceWorkspaceUpdateTests()
{
    [Theory]
    [InlineData("Finance", null)]
    [InlineData(null, "")]
    [InlineData(null, "New description")]
    [InlineData("Finance", "")]
    [InlineData(" \u8ca1\u52d9 ", "\u03b1\n\"quoted\" \\ text")]
    public async Task UpdateWorkspaceAsync_SendsOnePatchWithOnlySuppliedFields(string? displayName, string? description)
    {
        HttpRequestMessage? captured = null;
        using var handler = new FabricCoreHttpMessageHandler(async (request, cancellationToken) =>
        {
            captured = request;
            Assert.Equal(HttpMethod.Patch, request.Method);
            Assert.Equal($"https://api.fabric.microsoft.com/v1/workspaces/{WorkspaceUpdateTestData.WorkspaceId}", request.RequestUri?.AbsoluteUri);
            Assert.Equal("Bearer", request.Headers.Authorization?.Scheme);
            Assert.Equal("test-token", request.Headers.Authorization?.Parameter);
            Assert.Equal("Fabric Core MCP", request.Headers.UserAgent.ToString());
            Assert.NotNull(request.Content);
            Assert.Equal("application/json", request.Content.Headers.ContentType?.MediaType);
            using var body = JsonDocument.Parse(await request.Content.ReadAsStringAsync(cancellationToken));
            Assert.Equal((displayName is null ? 0 : 1) + (description is null ? 0 : 1), body.RootElement.EnumerateObject().Count());
            Assert.Equal(displayName is not null, body.RootElement.TryGetProperty("displayName", out var name));
            Assert.Equal(description is not null, body.RootElement.TryGetProperty("description", out var details));
            if (displayName is not null)
            {
                Assert.Equal(displayName, name.GetString());
            }
            if (description is not null)
            {
                Assert.Equal(description, details.GetString());
            }
            return WorkspaceUpdateTestData.CreateResponse();
        });
        using var client = new HttpClient(handler);
        var credential = WorkspaceUpdateTestData.CreateCredential();
        var service = new FabricCoreService(client, credential);

        var workspace = await service.UpdateWorkspaceAsync(
            $"{{{WorkspaceUpdateTestData.WorkspaceId.ToUpperInvariant()}}}",
            new() { DisplayName = displayName, Description = description },
            TestContext.Current.CancellationToken);

        Assert.Equal(Guid.Parse(WorkspaceUpdateTestData.WorkspaceId), workspace.Id);
        Assert.Equal("FutureWorkspaceType", workspace.Type);
        Assert.Equal("", workspace.Description);
        Assert.Equal(1, handler.CallCount);
        Assert.NotNull(captured?.Content);
        await Assert.ThrowsAsync<ObjectDisposedException>(() => captured.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        await credential.Received(1).GetTokenAsync(
            Arg.Is<TokenRequestContext>(context => context.Scopes.SequenceEqual(FabricEndpoints.FabricScopes)),
            TestContext.Current.CancellationToken);
    }

    [Theory]
    [InlineData(null, "Finance", null)]
    [InlineData("", "Finance", null)]
    [InlineData("not-a-uuid", null, "")]
    [InlineData("00000000-0000-0000-0000-000000000000", null, "")]
    [InlineData("../other?private=value", null, "")]
    [InlineData(WorkspaceUpdateTestData.WorkspaceId, null, null)]
    [InlineData(WorkspaceUpdateTestData.WorkspaceId, "", "valid")]
    [InlineData(WorkspaceUpdateTestData.WorkspaceId, " ", null)]
    [InlineData(WorkspaceUpdateTestData.WorkspaceId, "Admin monitoring", null)]
    [InlineData(WorkspaceUpdateTestData.WorkspaceId, "aDmIn MoNiToRiNg", "")]
    public async Task UpdateWorkspaceAsync_RejectsInvalidInputBeforeCredentials(string? workspaceId, string? displayName, string? description)
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) => throw new InvalidOperationException("Unexpected HTTP call."));
        using var client = new HttpClient(handler);
        var credential = WorkspaceUpdateTestData.CreateCredential();
        var service = new FabricCoreService(client, credential);

        await Assert.ThrowsAsync<ArgumentException>(() => service.UpdateWorkspaceAsync(
            workspaceId!, new() { DisplayName = displayName, Description = description }, TestContext.Current.CancellationToken));

        Assert.Empty(credential.ReceivedCalls());
        Assert.Equal(0, handler.CallCount);
    }

    [Theory]
    [InlineData(256, 4000, true)]
    [InlineData(257, 1, false)]
    [InlineData(1, 4001, false)]
    public async Task UpdateWorkspaceAsync_ValidatesDirectServiceLengthBoundaries(int nameLength, int descriptionLength, bool valid)
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(WorkspaceUpdateTestData.CreateResponse()));
        using var client = new HttpClient(handler);
        var credential = WorkspaceUpdateTestData.CreateCredential();
        var service = new FabricCoreService(client, credential);
        var request = new UpdateWorkspaceRequest { DisplayName = new('n', nameLength), Description = new('d', descriptionLength) };

        if (valid)
        {
            await service.UpdateWorkspaceAsync(WorkspaceUpdateTestData.WorkspaceId, request, TestContext.Current.CancellationToken);
        }
        else
        {
            await Assert.ThrowsAsync<ArgumentException>(() => service.UpdateWorkspaceAsync(
                WorkspaceUpdateTestData.WorkspaceId, request, TestContext.Current.CancellationToken));
            Assert.Empty(credential.ReceivedCalls());
        }
        Assert.Equal(valid ? 1 : 0, handler.CallCount);
    }

    [Fact]
    public async Task UpdateWorkspaceAsync_RejectsNullRequestBeforeCredentials()
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) => throw new InvalidOperationException("Unexpected HTTP call."));
        using var client = new HttpClient(handler);
        var credential = WorkspaceUpdateTestData.CreateCredential();

        await Assert.ThrowsAsync<ArgumentNullException>(() => new FabricCoreService(client, credential).UpdateWorkspaceAsync(
            WorkspaceUpdateTestData.WorkspaceId, null!, TestContext.Current.CancellationToken));

        Assert.Empty(credential.ReceivedCalls());
        Assert.Equal(0, handler.CallCount);
    }

    [Theory]
    [InlineData(400, 400)]
    [InlineData(401, 401)]
    [InlineData(403, 403)]
    [InlineData(404, 404)]
    [InlineData(409, 409)]
    [InlineData(429, 429)]
    [InlineData(500, 500)]
    [InlineData(503, 503)]
    [InlineData(307, 307)]
    [InlineData(201, 502)]
    [InlineData(202, 502)]
    [InlineData(204, 502)]
    public async Task UpdateWorkspaceAsync_PreservesFailuresWithoutApplicationRetriesOrUrlFollowing(int upstreamStatus, int expectedStatus)
    {
        using var body = new MemoryStream(Encoding.UTF8.GetBytes("private-backend-payload"));
        using var response = new HttpResponseMessage((HttpStatusCode)upstreamStatus) { Content = new StreamContent(body) };
        response.Headers.Location = new Uri("https://response-location.invalid/never-follow");
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, WorkspaceUpdateTestData.CreateCredential());

        var exception = await Assert.ThrowsAsync<HttpRequestException>(() => service.UpdateWorkspaceAsync(
            WorkspaceUpdateTestData.WorkspaceId, new() { Description = "" }, TestContext.Current.CancellationToken));

        Assert.Equal((HttpStatusCode)expectedStatus, exception.StatusCode);
        Assert.DoesNotContain("private-", exception.Message);
        Assert.Null(exception.InnerException);
        Assert.Equal(1, handler.CallCount);
        Assert.False(body.CanRead);
    }

    [Theory]
    [InlineData("0", "Wait at least 0 seconds")]
    [InlineData(" 000120 ", "Wait at least 120 seconds")]
    [InlineData("120", "Wait at least 120 seconds")]
    [InlineData("2147483647", "Wait at least 2147483647 seconds")]
    [InlineData("Wed, 21 Oct 2015 07:28:00 GMT", "Retry after 2015-10-21 07:28:00 UTC")]
    [InlineData("Tue, 01 Jan 2030 00:00:00 GMT", "Retry after 2030-01-01 00:00:00 UTC")]
    [InlineData(null, "Unable to update")]
    [InlineData("", "Unable to update")]
    [InlineData(" ", "Unable to update")]
    [InlineData("-1", "Unable to update")]
    [InlineData("+1", "Unable to update")]
    [InlineData("1.5", "Unable to update")]
    [InlineData("2147483648", "Unable to update")]
    [InlineData("private-header-value", "Unable to update")]
    [InlineData("999999999999999999999999999", "Unable to update")]
    [InlineData("10, 20", "Unable to update")]
    public async Task UpdateWorkspaceAsync_UsesOnlyValidatedRetryAfter(string? retryAfter, string expected)
    {
        using var response = WorkspaceUpdateTestData.CreateResponse("private-body", HttpStatusCode.TooManyRequests);
        if (retryAfter is not null)
        {
            Assert.True(response.Headers.TryAddWithoutValidation("Retry-After", retryAfter));
        }
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, WorkspaceUpdateTestData.CreateCredential());

        var exception = await Assert.ThrowsAnyAsync<HttpRequestException>(() => service.UpdateWorkspaceAsync(
            WorkspaceUpdateTestData.WorkspaceId, new() { Description = "" }, TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.TooManyRequests, exception.StatusCode);
        Assert.Contains(expected, exception.Message);
        Assert.DoesNotContain("private-", exception.Message);
        Assert.Equal(1, handler.CallCount);
    }

    [Fact]
    public async Task UpdateWorkspaceAsync_DoesNotExposeAmbiguousRetryAfterValues()
    {
        using var response = WorkspaceUpdateTestData.CreateResponse("private-body", HttpStatusCode.TooManyRequests);
        Assert.True(response.Headers.TryAddWithoutValidation("Retry-After", ["30", "60"]));
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
        using var client = new HttpClient(handler);

        var exception = await Assert.ThrowsAsync<HttpRequestException>(() =>
            new FabricCoreService(client, WorkspaceUpdateTestData.CreateCredential()).UpdateWorkspaceAsync(
                WorkspaceUpdateTestData.WorkspaceId, new() { Description = "" }, TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.TooManyRequests, exception.StatusCode);
        Assert.DoesNotContain("30", exception.Message);
        Assert.DoesNotContain("60", exception.Message);
        Assert.Equal(1, handler.CallCount);
    }

    [Theory]
    [InlineData("")]
    [InlineData("null")]
    [InlineData("{}")]
    [InlineData("[]")]
    [InlineData("private-invalid-json")]
    [InlineData("""{"id":"not-a-uuid","displayName":"Finance","type":"Workspace"}""")]
    [InlineData("""{"id":"00000000-0000-0000-0000-000000000000","displayName":"Finance","type":"Workspace"}""")]
    [InlineData("""{"id":"8ca5dd7f-41db-482a-85d9-21086b17799b","displayName":"Finance","type":"Workspace"}""")]
    [InlineData("""{"id":"33bae707-5fe7-4352-89bd-061a1318b60a","displayName":"","type":"Workspace"}""")]
    [InlineData("""{"id":"33bae707-5fe7-4352-89bd-061a1318b60a","displayName":"Finance","type":null}""")]
    [InlineData("""{"id":"33bae707-5fe7-4352-89bd-061a1318b60a","displayName":"Finance"}""")]
    [InlineData("""{"id":"33bae707-5fe7-4352-89bd-061a1318b60a","displayName":"Finance","type":"Workspace","description":{}}""")]
    public async Task UpdateWorkspaceAsync_RejectsInvalidSuccessPayloadsAndDisposesResponse(string json)
    {
        using var body = new MemoryStream(Encoding.UTF8.GetBytes(json));
        using var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StreamContent(body) };
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
        using var client = new HttpClient(handler);

        await Assert.ThrowsAsync<JsonException>(() => new FabricCoreService(client, WorkspaceUpdateTestData.CreateCredential()).UpdateWorkspaceAsync(
            WorkspaceUpdateTestData.WorkspaceId, new() { Description = "" }, TestContext.Current.CancellationToken));

        Assert.False(body.CanRead);
        Assert.Equal(1, handler.CallCount);
    }

    [Fact]
    public async Task UpdateWorkspaceAsync_AcceptsMissingDescriptionAndDisposesSuccessfulResponse()
    {
        using var body = new MemoryStream(Encoding.UTF8.GetBytes(
            """{"id":"33bae707-5fe7-4352-89bd-061a1318b60a","displayName":"Finance","type":"Workspace"}"""));
        using var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StreamContent(body) };
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
        using var client = new HttpClient(handler);

        var workspace = await new FabricCoreService(client, WorkspaceUpdateTestData.CreateCredential()).UpdateWorkspaceAsync(
            WorkspaceUpdateTestData.WorkspaceId, new() { DisplayName = "Finance" }, TestContext.Current.CancellationToken);

        Assert.Null(workspace.Description);
        Assert.False(body.CanRead);
        Assert.Equal(1, handler.CallCount);
    }

    [Fact]
    public async Task UpdateWorkspaceAsync_PropagatesCredentialCancellationBeforeSend()
    {
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        var credential = WorkspaceUpdateTestData.CreateCredential();
        credential.GetTokenAsync(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>()).Returns(call =>
        {
            Assert.Equal(cancellation.Token, call.Arg<CancellationToken>());
            cancellation.Cancel();
            return ValueTask.FromCanceled<AccessToken>(cancellation.Token);
        });
        using var handler = new FabricCoreHttpMessageHandler((_, _) => throw new InvalidOperationException("Unexpected HTTP call."));
        using var client = new HttpClient(handler);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => new FabricCoreService(client, credential).UpdateWorkspaceAsync(
            WorkspaceUpdateTestData.WorkspaceId, new() { Description = "" }, cancellation.Token));

        Assert.Equal(0, handler.CallCount);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task UpdateWorkspaceAsync_PropagatesCancellationDuringSendOrBodyRead(bool duringBodyRead)
    {
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        using var content = new WorkspaceUpdateCancellationContent(cancellation);
        using var handler = new FabricCoreHttpMessageHandler((_, token) =>
        {
            Assert.True(token.CanBeCanceled);
            if (duringBodyRead)
            {
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = content });
            }
            cancellation.Cancel();
            return Task.FromCanceled<HttpResponseMessage>(token);
        });
        using var client = new HttpClient(handler);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            new FabricCoreService(client, WorkspaceUpdateTestData.CreateCredential()).UpdateWorkspaceAsync(
                WorkspaceUpdateTestData.WorkspaceId, new() { Description = "" }, cancellation.Token));

        Assert.True(cancellation.IsCancellationRequested);
        Assert.Equal(1, handler.CallCount);
        if (duringBodyRead)
        {
            Assert.True(content.IsDisposed);
        }
    }

    [Fact]
    public async Task UpdateWorkspaceAsync_KeepsConcurrentCredentialsAndBodiesIsolated()
    {
        var identity = new AsyncLocal<string>();
        var credential = WorkspaceUpdateTestData.CreateCredential();
        credential.GetTokenAsync(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>())
            .Returns(_ => new AccessToken(identity.Value!, DateTimeOffset.MaxValue));
        var bothStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var started = 0;
        using var handler = new FabricCoreHttpMessageHandler(async (request, token) =>
        {
            if (Interlocked.Increment(ref started) == 2)
            {
                bothStarted.SetResult();
            }
            await bothStarted.Task.WaitAsync(TimeSpan.FromSeconds(10), token);
            Assert.NotNull(request.Content);
            using var body = JsonDocument.Parse(await request.Content.ReadAsStringAsync(token));
            var description = body.RootElement.GetProperty("description").GetString();
            Assert.Equal(description, request.Headers.Authorization?.Parameter);
            var workspace = WorkspaceUpdateTestData.CreateWorkspace();
            workspace.Description = description;
            return WorkspaceUpdateTestData.CreateResponse(JsonSerializer.Serialize(workspace, CoreJsonContext.Default.WorkspaceUpdateResponse));
        });
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, credential);

        async Task<WorkspaceUpdateResponse> UpdateAsAsync(string user)
        {
            identity.Value = user;
            return await service.UpdateWorkspaceAsync(WorkspaceUpdateTestData.WorkspaceId, new() { Description = user }, TestContext.Current.CancellationToken);
        }

        var results = await Task.WhenAll(UpdateAsAsync("user-a"), UpdateAsAsync("user-b"));

        Assert.Equal(["user-a", "user-b"], results.Select(static workspace => workspace.Description));
        Assert.Equal(2, handler.CallCount);
        await credential.Received(2).GetTokenAsync(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(true, false)]
    [InlineData(false, false)]
    [InlineData(true, true)]
    [InlineData(false, true)]
    public async Task ExistingPostOperations_PreserveRequestAndResponseBehavior(bool create, bool fail)
    {
        using var handler = new FabricCoreHttpMessageHandler(async (request, token) =>
        {
            Assert.Equal(HttpMethod.Post, request.Method);
            Assert.Equal(
                create ? $"https://api.fabric.microsoft.com/v1/workspaces/{WorkspaceUpdateTestData.WorkspaceId}/items" : "https://api.fabric.microsoft.com/v1/catalog/search",
                request.RequestUri?.AbsoluteUri);
            Assert.Equal("test-token", request.Headers.Authorization?.Parameter);
            Assert.NotNull(request.Content);
            using var body = JsonDocument.Parse(await request.Content.ReadAsStringAsync(token));
            Assert.Equal("Finance", body.RootElement.GetProperty(create ? "displayName" : "search").GetString());
            return WorkspaceUpdateTestData.CreateResponse(
                fail ? "existing-error" : create ? """{"id":"item-id","displayName":"Finance","type":"Lakehouse"}""" : """{"value":[],"continuationToken":"next"}""",
                fail ? HttpStatusCode.BadRequest : HttpStatusCode.OK);
        });
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, WorkspaceUpdateTestData.CreateCredential());

        async Task ExecuteAsync()
        {
            if (create)
            {
                var item = await service.CreateItemAsync(
                    WorkspaceUpdateTestData.WorkspaceId, new() { DisplayName = "Finance", Type = "Lakehouse" }, TestContext.Current.CancellationToken);
                Assert.Equal("item-id", item.Id);
            }
            else
            {
                var result = await service.SearchCatalogAsync(new() { Search = "Finance" }, TestContext.Current.CancellationToken);
                Assert.Empty(result.Value);
                Assert.Equal("next", result.ContinuationToken);
            }
        }

        if (fail)
        {
            var exception = await Assert.ThrowsAsync<HttpRequestException>(ExecuteAsync);
            Assert.Contains("existing-error", exception.Message);
            Assert.Null(exception.StatusCode);
        }
        else
        {
            await ExecuteAsync();
        }
        Assert.Equal(1, handler.CallCount);
    }
}
