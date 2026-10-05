// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Azure.Core;
using Fabric.Mcp.Tools.Core.Models;
using Fabric.Mcp.Tools.Core.Services;
using Fabric.Mcp.Tools.Core.Tests.TestSupport;
using NSubstitute;
using Xunit;

namespace Fabric.Mcp.Tools.Core.Tests.Services;

public class WorkspaceGetServiceTests()
{
    [Theory]
    [InlineData(null, "")]
    [InlineData(true, "?preferWorkspaceSpecificEndpoints=true")]
    [InlineData(false, "?preferWorkspaceSpecificEndpoints=false")]
    public async Task GetWorkspaceAsync_SendsNormalizedGetAndReturnsTypedMetadata(bool? preference, string query)
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(WorkspaceTestData.FullJson));
        using var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StreamContent(stream) };
        using var handler = new FabricCoreHttpMessageHandler((request, token) =>
        {
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal($"{FabricEndpoints.FabricApiBaseUrl}/workspaces/{WorkspaceTestData.WorkspaceId}{query}", request.RequestUri?.AbsoluteUri);
            Assert.Equal("Bearer", request.Headers.Authorization?.Scheme);
            Assert.Equal("test-token", request.Headers.Authorization?.Parameter);
            Assert.Equal("Fabric Core MCP", request.Headers.UserAgent.ToString());
            Assert.Null(request.Content);
            Assert.True(token.CanBeCanceled);
            return Task.FromResult(response);
        });
        using var client = new HttpClient(handler);
        var credential = WorkspaceTestData.CreateCredential();
        var service = new FabricCoreService(client, credential);

        var workspace = await service.GetWorkspaceAsync(
            $"{{{WorkspaceTestData.WorkspaceId.ToUpperInvariant()}}}", preference, TestContext.Current.CancellationToken);

        Assert.Equal(Guid.Parse(WorkspaceTestData.WorkspaceId), workspace.Id);
        Assert.Equal("Finance", workspace.DisplayName);
        Assert.Equal("Workspace", workspace.Type);
        Assert.Equal("Workspace metadata", workspace.Description);
        Assert.Equal(Guid.Parse(WorkspaceTestData.RelatedId), workspace.CapacityId);
        Assert.Equal(Guid.Parse(WorkspaceTestData.RelatedId), workspace.DomainId);
        Assert.Equal("InProgress", workspace.CapacityAssignmentProgress);
        Assert.Equal("East US", workspace.CapacityRegion);
        Assert.Equal(Guid.Parse(WorkspaceTestData.RelatedId), workspace.WorkspaceIdentity?.ApplicationId);
        Assert.Equal(Guid.Parse("cccccccc-cccc-4ccc-8ccc-cccccccccccc"), workspace.WorkspaceIdentity?.ServicePrincipalId);
        Assert.Equal("https://aaaaaaaaaaaa4aaa8aaaaaaaaaaaaaaa.zcf.blob.fabric.microsoft.com", workspace.OneLakeEndpoints?.BlobEndpoint);
        Assert.Equal("https://aaaaaaaaaaaa4aaa8aaaaaaaaaaaaaaa.zcf.dfs.fabric.microsoft.com", workspace.OneLakeEndpoints?.DfsEndpoint);
        Assert.Equal("https://aaaaaaaaaaaa4aaa8aaaaaaaaaaaaaaa.zcf.w.api.fabric.microsoft.com", workspace.ApiEndpoint);
        var tag = Assert.Single(Assert.IsType<List<FabricWorkspaceAppliedTag>>(workspace.Tags));
        Assert.Equal(Guid.Parse(WorkspaceTestData.RelatedId), tag.Id);
        Assert.Equal("Finance", tag.DisplayName);
        Assert.DoesNotContain("excluded-backend-property", JsonSerializer.Serialize(workspace, CoreJsonContext.Default.FabricWorkspaceMetadata));
        Assert.Equal(1, handler.CallCount);
        Assert.False(stream.CanRead);
        await credential.Received(1).GetTokenAsync(
            Arg.Is<TokenRequestContext>(context => context.Scopes.SequenceEqual(FabricEndpoints.FabricScopes)),
            TestContext.Current.CancellationToken);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("Finance")]
    [InlineData("00000000-0000-0000-0000-000000000000")]
    [InlineData(WorkspaceTestData.WorkspaceId + "/items")]
    [InlineData(WorkspaceTestData.WorkspaceId + "?query=true")]
    [InlineData("https://example.com")]
    public async Task GetWorkspaceAsync_RejectsInvalidIdBeforeAuthentication(string? workspaceId)
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) => throw new InvalidOperationException("HTTP must not be called"));
        using var client = new HttpClient(handler);
        var credential = WorkspaceTestData.CreateCredential();
        var service = new FabricCoreService(client, credential);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.GetWorkspaceAsync(workspaceId!, cancellationToken: TestContext.Current.CancellationToken));

        Assert.Empty(credential.ReceivedCalls());
        Assert.Equal(0, handler.CallCount);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("null")]
    public async Task GetWorkspaceAsync_OmitsAbsentOrNullOptionalFields(string? optionalValue)
    {
        var payload = JsonNode.Parse(WorkspaceTestData.MinimalJson)!.AsObject();
        if (optionalValue is not null)
        {
            foreach (var name in new[] { "description", "capacityId", "capacityAssignmentProgress", "capacityRegion", "domainId", "workspaceIdentity", "oneLakeEndpoints", "apiEndpoint", "tags" })
            {
                payload[name] = null;
            }
        }

        var workspace = await GetWorkspaceFromJsonAsync(payload.ToJsonString());

        using var result = JsonDocument.Parse(JsonSerializer.Serialize(workspace, CoreJsonContext.Default.FabricWorkspaceMetadata));
        Assert.Equal(["displayName", "id", "type"], result.RootElement.EnumerateObject().Select(property => property.Name).OrderBy(name => name));
    }

    [Fact]
    public async Task GetWorkspaceAsync_PreservesEmptyAndFutureMetadata()
    {
        var payload = JsonNode.Parse(WorkspaceTestData.MinimalJson)!.AsObject();
        payload["description"] = "";
        payload["type"] = "FutureWorkspaceType";
        payload["capacityAssignmentProgress"] = "FutureProgress";
        payload["capacityRegion"] = "Future Region";
        payload["workspaceIdentity"] = new JsonObject { ["applicationId"] = WorkspaceTestData.RelatedId };
        payload["oneLakeEndpoints"] = new JsonObject { ["dfsEndpoint"] = "https://example.com/metadata-only" };
        payload["tags"] = new JsonArray();

        var workspace = await GetWorkspaceFromJsonAsync(payload.ToJsonString());

        Assert.Equal("", workspace.Description);
        Assert.Equal("FutureWorkspaceType", workspace.Type);
        Assert.Equal("FutureProgress", workspace.CapacityAssignmentProgress);
        Assert.Equal("Future Region", workspace.CapacityRegion);
        Assert.Equal(Guid.Parse(WorkspaceTestData.RelatedId), workspace.WorkspaceIdentity?.ApplicationId);
        Assert.Null(workspace.WorkspaceIdentity?.ServicePrincipalId);
        Assert.Equal("https://example.com/metadata-only", workspace.OneLakeEndpoints?.DfsEndpoint);
        Assert.Null(workspace.OneLakeEndpoints?.BlobEndpoint);
        Assert.Null(workspace.ApiEndpoint);
        Assert.Empty(Assert.IsType<List<FabricWorkspaceAppliedTag>>(workspace.Tags));
    }

    [Theory]
    [InlineData("")]
    [InlineData("null")]
    [InlineData("{}")]
    [InlineData("[]")]
    [InlineData("{invalid-json")]
    public async Task GetWorkspaceAsync_RejectsInvalidResponseBodies(string json)
    {
        await Assert.ThrowsAsync<JsonException>(() => GetWorkspaceFromJsonAsync(json));
    }

    [Theory]
    [InlineData("id", null)]
    [InlineData("id", "null")]
    [InlineData("id", "\"not-a-uuid\"")]
    [InlineData("id", "\"00000000-0000-0000-0000-000000000000\"")]
    [InlineData("id", "\"bbbbbbbb-bbbb-4bbb-8bbb-bbbbbbbbbbbb\"")]
    [InlineData("displayName", null)]
    [InlineData("displayName", "null")]
    [InlineData("displayName", "\" \"")]
    [InlineData("displayName", "123")]
    [InlineData("type", null)]
    [InlineData("type", "null")]
    [InlineData("type", "\"\"")]
    [InlineData("capacityId", "\"not-a-uuid\"")]
    [InlineData("domainId", "42")]
    [InlineData("workspaceIdentity", "{\"applicationId\":\"invalid\"}")]
    [InlineData("oneLakeEndpoints", "{\"blobEndpoint\":false}")]
    [InlineData("apiEndpoint", "42")]
    [InlineData("tags", "{}")]
    [InlineData("tags", "[null]")]
    [InlineData("tags", "[{}]")]
    [InlineData("tags", "[{\"id\":\"00000000-0000-0000-0000-000000000000\",\"displayName\":\"Finance\"}]")]
    [InlineData("tags", "[{\"id\":\"bbbbbbbb-bbbb-4bbb-8bbb-bbbbbbbbbbbb\",\"displayName\":null}]")]
    public async Task GetWorkspaceAsync_RejectsInvalidMetadata(string property, string? value)
    {
        var payload = JsonNode.Parse(WorkspaceTestData.MinimalJson)!.AsObject();
        if (value is null)
        {
            payload.Remove(property);
        }
        else
        {
            payload[property] = JsonNode.Parse(value);
        }

        await Assert.ThrowsAsync<JsonException>(() => GetWorkspaceFromJsonAsync(payload.ToJsonString()));
    }

    [Theory]
    [InlineData(HttpStatusCode.BadRequest, HttpStatusCode.BadRequest)]
    [InlineData(HttpStatusCode.Unauthorized, HttpStatusCode.Unauthorized)]
    [InlineData(HttpStatusCode.Forbidden, HttpStatusCode.Forbidden)]
    [InlineData(HttpStatusCode.NotFound, HttpStatusCode.NotFound)]
    [InlineData(HttpStatusCode.TooManyRequests, HttpStatusCode.TooManyRequests)]
    [InlineData(HttpStatusCode.InternalServerError, HttpStatusCode.InternalServerError)]
    [InlineData(HttpStatusCode.ServiceUnavailable, HttpStatusCode.ServiceUnavailable)]
    [InlineData(HttpStatusCode.Accepted, HttpStatusCode.BadGateway)]
    [InlineData(HttpStatusCode.NoContent, HttpStatusCode.BadGateway)]
    public async Task GetWorkspaceAsync_PreservesFailuresAndRejectsUnexpectedSuccess(HttpStatusCode status, HttpStatusCode expected)
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("private-backend-detail"));
        using var response = new HttpResponseMessage(status) { Content = new StreamContent(stream) };
        response.Headers.Location = new Uri("https://example.com/must-not-poll");
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, WorkspaceTestData.CreateCredential());

        var exception = await Assert.ThrowsAsync<HttpRequestException>(() =>
            service.GetWorkspaceAsync(WorkspaceTestData.WorkspaceId, cancellationToken: TestContext.Current.CancellationToken));

        Assert.Equal(expected, exception.StatusCode);
        Assert.DoesNotContain("private-backend-detail", exception.Message);
        Assert.Equal(1, handler.CallCount);
        Assert.False(stream.CanRead);
    }

    [Theory]
    [InlineData("0", "Wait at least 0 seconds")]
    [InlineData("120", "Wait at least 120 seconds")]
    [InlineData("Tue, 01 Jan 2030 00:00:00 GMT", "Retry after 2030-01-01 00:00:00 UTC")]
    [InlineData("-1", "Unable to retrieve")]
    [InlineData("1.5", "Unable to retrieve")]
    [InlineData("999999999999999999999999999999999999", "Unable to retrieve")]
    [InlineData("private-header-detail", "Unable to retrieve")]
    [InlineData("60, 120", "Unable to retrieve")]
    public async Task GetWorkspaceAsync_OnlyUsesValidatedRetryAfter(string retryAfter, string expected)
    {
        using var response = new HttpResponseMessage(HttpStatusCode.TooManyRequests) { Content = new StringContent("private-backend-detail") };
        Assert.True(response.Headers.TryAddWithoutValidation("Retry-After", retryAfter));
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, WorkspaceTestData.CreateCredential());

        var exception = await Assert.ThrowsAnyAsync<HttpRequestException>(() =>
            service.GetWorkspaceAsync(WorkspaceTestData.WorkspaceId, cancellationToken: TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.TooManyRequests, exception.StatusCode);
        Assert.Contains(expected, exception.Message);
        Assert.DoesNotContain("private-header-detail", exception.Message);
        Assert.DoesNotContain("private-backend-detail", exception.Message);
        Assert.Equal(1, handler.CallCount);
    }

    [Fact]
    public async Task GetWorkspaceAsync_RejectsMultipleRetryAfterHeaders()
    {
        using var response = new HttpResponseMessage(HttpStatusCode.TooManyRequests);
        Assert.True(response.Headers.TryAddWithoutValidation("Retry-After", ["60", "120"]));
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, WorkspaceTestData.CreateCredential());

        var exception = await Assert.ThrowsAsync<HttpRequestException>(() =>
            service.GetWorkspaceAsync(WorkspaceTestData.WorkspaceId, cancellationToken: TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.TooManyRequests, exception.StatusCode);
        Assert.Equal("Unable to retrieve Fabric workspace metadata.", exception.Message);
    }

    [Fact]
    public async Task GetWorkspaceAsync_AcquiresTokenForEachRequest()
    {
        var credential = WorkspaceTestData.CreateCredential();
        credential.GetTokenAsync(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>())
            .Returns(new AccessToken("first-token", DateTimeOffset.MaxValue), new AccessToken("second-token", DateTimeOffset.MaxValue));
        var tokens = new List<string?>();
        using var handler = new FabricCoreHttpMessageHandler((request, _) =>
        {
            tokens.Add(request.Headers.Authorization?.Parameter);
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(WorkspaceTestData.MinimalJson) });
        });
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, credential);

        await service.GetWorkspaceAsync(WorkspaceTestData.WorkspaceId, cancellationToken: TestContext.Current.CancellationToken);
        await service.GetWorkspaceAsync(WorkspaceTestData.WorkspaceId, cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(["first-token", "second-token"], tokens);
        Assert.Null(client.DefaultRequestHeaders.Authorization);
    }

    [Fact]
    public async Task GetWorkspaceAsync_PropagatesAuthenticationCancellation()
    {
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        await cancellation.CancelAsync();
        var credential = WorkspaceTestData.CreateCredential();
        credential.GetTokenAsync(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>())
            .Returns(_ => ValueTask.FromCanceled<AccessToken>(cancellation.Token));
        using var handler = new FabricCoreHttpMessageHandler((_, _) => throw new InvalidOperationException("HTTP must not be called"));
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, credential);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            service.GetWorkspaceAsync(WorkspaceTestData.WorkspaceId, cancellationToken: cancellation.Token));

        Assert.Equal(0, handler.CallCount);
        await credential.Received(1).GetTokenAsync(Arg.Any<TokenRequestContext>(), cancellation.Token);
    }

    [Fact]
    public async Task GetWorkspaceAsync_PropagatesHttpCancellation()
    {
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        var started = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        using var handler = new FabricCoreHttpMessageHandler(async (_, token) =>
        {
            started.SetResult();
            await Task.Delay(Timeout.InfiniteTimeSpan, token);
            throw new InvalidOperationException("Cancellation must interrupt the send");
        });
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, WorkspaceTestData.CreateCredential());

        var request = service.GetWorkspaceAsync(WorkspaceTestData.WorkspaceId, cancellationToken: cancellation.Token);
        await started.Task.WaitAsync(TestContext.Current.CancellationToken);
        await cancellation.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => request);
        Assert.Equal(1, handler.CallCount);
    }

    [Theory]
    [InlineData(true, false)]
    [InlineData(false, false)]
    [InlineData(true, true)]
    [InlineData(false, true)]
    public async Task ExistingOperations_PreserveRequestsAndResponseHandling(bool createItem, bool failure)
    {
        var credential = WorkspaceTestData.CreateCredential();
        using var response = new HttpResponseMessage(failure ? HttpStatusCode.BadRequest : HttpStatusCode.OK)
        {
            Content = new StringContent(failure ? "legacy-service-error" :
                createItem ? """{"id":"item-id","displayName":"Lakehouse","type":"Lakehouse"}""" : """{"value":[],"continuationToken":"next-page"}""")
        };
        using var handler = new FabricCoreHttpMessageHandler(async (request, token) =>
        {
            Assert.Equal(HttpMethod.Post, request.Method);
            Assert.Equal($"{FabricEndpoints.FabricApiBaseUrl}/{(createItem ? $"workspaces/{WorkspaceTestData.WorkspaceId}/items" : "catalog/search")}", request.RequestUri?.AbsoluteUri);
            Assert.Equal("test-token", request.Headers.Authorization?.Parameter);
            Assert.NotNull(request.Content);
            Assert.Equal("application/json", request.Content.Headers.ContentType?.MediaType);
            using var document = JsonDocument.Parse(await request.Content.ReadAsStringAsync(token));
            Assert.Equal(createItem ? "Lakehouse" : "Finance",
                document.RootElement.GetProperty(createItem ? "displayName" : "search").GetString());
            return response;
        });
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, credential);

        if (failure)
        {
            var exception = await Assert.ThrowsAsync<HttpRequestException>(async () =>
            {
                if (createItem)
                {
                    await service.CreateItemAsync(WorkspaceTestData.WorkspaceId, new() { DisplayName = "Lakehouse", Type = "Lakehouse" }, TestContext.Current.CancellationToken);
                }
                else
                {
                    await service.SearchCatalogAsync(new() { Search = "Finance" }, TestContext.Current.CancellationToken);
                }
            });
            Assert.Contains("legacy-service-error", exception.Message);
            Assert.Null(exception.StatusCode);
        }
        else if (createItem)
        {
            var item = await service.CreateItemAsync(WorkspaceTestData.WorkspaceId, new() { DisplayName = "Lakehouse", Type = "Lakehouse" }, TestContext.Current.CancellationToken);
            Assert.Equal("item-id", item.Id);
        }
        else
        {
            var catalog = await service.SearchCatalogAsync(new() { Search = "Finance" }, TestContext.Current.CancellationToken);
            Assert.Empty(catalog.Value);
            Assert.Equal("next-page", catalog.ContinuationToken);
        }

        Assert.Equal(1, handler.CallCount);
        await credential.Received(1).GetTokenAsync(
            Arg.Is<TokenRequestContext>(context => context.Scopes.SequenceEqual(FabricEndpoints.FabricScopes)),
            TestContext.Current.CancellationToken);
    }

    private static async Task<FabricWorkspaceMetadata> GetWorkspaceFromJsonAsync(string json)
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
        using var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StreamContent(stream) };
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, WorkspaceTestData.CreateCredential());

        try
        {
            return await service.GetWorkspaceAsync(
                WorkspaceTestData.WorkspaceId.Replace("-", "", StringComparison.Ordinal), cancellationToken: TestContext.Current.CancellationToken);
        }
        finally
        {
            Assert.False(stream.CanRead);
            Assert.Equal(1, handler.CallCount);
        }
    }
}
