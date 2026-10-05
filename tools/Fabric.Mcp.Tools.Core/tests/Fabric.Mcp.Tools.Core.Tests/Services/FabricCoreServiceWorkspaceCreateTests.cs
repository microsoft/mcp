// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;
using Azure.Core;
using Azure.Identity;
using Fabric.Mcp.Tools.Core.Models;
using Fabric.Mcp.Tools.Core.Services;
using Fabric.Mcp.Tools.Core.Tests.TestSupport;
using NSubstitute;
using Xunit;

namespace Fabric.Mcp.Tools.Core.Tests.Services;

public sealed class FabricCoreServiceWorkspaceCreateTests()
{
    [Fact]
    public async Task CreateWorkspaceAsync_SendsOneAuthenticatedPostWithCanonicalAssignments()
    {
        var credential = WorkspaceCreateTestData.CreateCredential();
        HttpRequestMessage? sentRequest = null;
        using var handler = new FabricCoreHttpMessageHandler(async (request, cancellationToken) =>
        {
            sentRequest = request;
            Assert.Equal(HttpMethod.Post, request.Method);
            Assert.Equal("https://api.fabric.microsoft.com/v1/workspaces", request.RequestUri?.AbsoluteUri);
            Assert.Equal("Bearer", request.Headers.Authorization?.Scheme);
            Assert.Equal("offline-token", request.Headers.Authorization?.Parameter);
            Assert.Equal("Fabric Core MCP", request.Headers.UserAgent.ToString());
            Assert.NotNull(request.Content);
            Assert.Equal("application/json", request.Content.Headers.ContentType?.MediaType);
            Assert.Equal("utf-8", request.Content.Headers.ContentType?.CharSet);
            using var body = JsonDocument.Parse(await request.Content.ReadAsStringAsync(cancellationToken));
            Assert.Equal(" Sales \u65b0 \"workspace\" ", body.RootElement.GetProperty("displayName").GetString());
            Assert.Equal("Description\n<&>", body.RootElement.GetProperty("description").GetString());
            Assert.Equal(WorkspaceCreateTestData.CapacityId, body.RootElement.GetProperty("capacityId").GetString());
            Assert.Equal(WorkspaceCreateTestData.DomainId, body.RootElement.GetProperty("domainId").GetString());
            Assert.Equal(4, body.RootElement.EnumerateObject().Count());
            return WorkspaceCreateTestData.CreateResponse(WorkspaceCreateTestData.FullMetadata);
        });
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, credential);
        var request = new CreateWorkspaceRequest
        {
            DisplayName = " Sales \u65b0 \"workspace\" ",
            Description = "Description\n<&>",
            CapacityId = Guid.Parse(WorkspaceCreateTestData.CapacityId).ToString("B").ToUpperInvariant(),
            DomainId = Guid.Parse(WorkspaceCreateTestData.DomainId).ToString("N").ToUpperInvariant()
        };

        var result = await service.CreateWorkspaceAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(Guid.Parse(WorkspaceCreateTestData.WorkspaceId), result.Workspace.Id);
        Assert.Equal(Guid.Parse(WorkspaceCreateTestData.CapacityId), result.Workspace.CapacityId);
        Assert.Equal(Guid.Parse(WorkspaceCreateTestData.DomainId), result.Workspace.DomainId);
        Assert.Equal("FutureWorkspaceType", result.Workspace.Type);
        Assert.Equal("Future Region", result.Workspace.CapacityRegion);
        Assert.Equal("Planning", Assert.Single(result.Workspace.Tags!).DisplayName);
        Assert.Equal(WorkspaceCreateTestData.Location, result.Location);
        Assert.StartsWith("{", request.CapacityId);
        Assert.Equal(1, handler.CallCount);
        await credential.Received(1).GetTokenAsync(
            Arg.Is<TokenRequestContext>(context =>
                context.Scopes.Length == 1 && context.Scopes[0] == "https://api.fabric.microsoft.com/.default"),
            TestContext.Current.CancellationToken);
        Assert.NotNull(sentRequest?.Content);
        await Assert.ThrowsAsync<ObjectDisposedException>(() => sentRequest.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    [InlineData(6)]
    [InlineData(7)]
    public async Task CreateWorkspaceAsync_OmitsOnlyUnspecifiedOptionalFields(int fields)
    {
        var request = new CreateWorkspaceRequest
        {
            DisplayName = "New workspace",
            Description = (fields & 1) == 0 ? null : "",
            CapacityId = (fields & 2) == 0 ? null : WorkspaceCreateTestData.CapacityId,
            DomainId = (fields & 4) == 0 ? null : WorkspaceCreateTestData.DomainId
        };
        using var handler = new FabricCoreHttpMessageHandler(async (message, cancellationToken) =>
        {
            using var body = JsonDocument.Parse(await message.Content!.ReadAsStringAsync(cancellationToken));
            Assert.Equal("New workspace", body.RootElement.GetProperty("displayName").GetString());
            Assert.Equal(request.Description is not null, body.RootElement.TryGetProperty("description", out var description));
            Assert.Equal(request.CapacityId is not null, body.RootElement.TryGetProperty("capacityId", out _));
            Assert.Equal(request.DomainId is not null, body.RootElement.TryGetProperty("domainId", out _));
            if (request.Description is not null)
            {
                Assert.Equal("", description.GetString());
            }

            return WorkspaceCreateTestData.CreateResponse();
        });
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, WorkspaceCreateTestData.CreateCredential());

        await service.CreateWorkspaceAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(1, handler.CallCount);
    }

    public static TheoryData<string?, string?, string?, string?> InvalidRequests
    {
        get
        {
            var data = new TheoryData<string?, string?, string?, string?>
            {
                { null, null, null, null },
                { "", null, null, null },
                { " \t ", null, null, null },
                { new string('n', 257), null, null, null },
                { "Admin monitoring", null, null, null },
                { " admin MONITORING ", null, null, null },
                { "New workspace", new string('d', 4001), null, null }
            };
            foreach (var value in new[] { "", " ", "not-a-uuid", "00000000-0000-0000-0000-000000000000" })
            {
                data.Add("New workspace", null, value, null);
                data.Add("New workspace", null, null, value);
            }

            return data;
        }
    }

    [Theory]
    [MemberData(nameof(InvalidRequests))]
    public async Task CreateWorkspaceAsync_ValidatesBeforeAcquiringCredentials(string? displayName, string? description, string? capacityId, string? domainId)
    {
        var credential = WorkspaceCreateTestData.CreateCredential();
        using var handler = new FabricCoreHttpMessageHandler((_, _) => throw new InvalidOperationException("HTTP must not be called."));
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, credential);
        var request = new CreateWorkspaceRequest
        {
            DisplayName = displayName!,
            Description = description,
            CapacityId = capacityId,
            DomainId = domainId
        };

        await Assert.ThrowsAsync<ArgumentException>(() => service.CreateWorkspaceAsync(request, TestContext.Current.CancellationToken));

        Assert.Equal(0, handler.CallCount);
        await credential.DidNotReceive().GetTokenAsync(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateWorkspaceAsync_RejectsNullRequestBeforeAuthentication()
    {
        var credential = WorkspaceCreateTestData.CreateCredential();
        using var handler = new FabricCoreHttpMessageHandler((_, _) => throw new InvalidOperationException("HTTP must not be called."));
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, credential);

        await Assert.ThrowsAsync<ArgumentNullException>(() => service.CreateWorkspaceAsync(null!, TestContext.Current.CancellationToken));

        await credential.DidNotReceive().GetTokenAsync(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>());
        Assert.Equal(0, handler.CallCount);
    }

    [Theory]
    [InlineData(1, 0)]
    [InlineData(256, 4000)]
    public async Task CreateWorkspaceAsync_AcceptsExactLengthBoundaries(int nameLength, int descriptionLength)
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(WorkspaceCreateTestData.CreateResponse()));
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, WorkspaceCreateTestData.CreateCredential());

        await service.CreateWorkspaceAsync(new()
        {
            DisplayName = new string('n', nameLength),
            Description = new string('d', descriptionLength)
        }, TestContext.Current.CancellationToken);

        Assert.Equal(1, handler.CallCount);
    }

    [Theory]
    [InlineData(null)]
    [InlineData(WorkspaceCreateTestData.Location)]
    [InlineData("/v1/workspaces/cfafbeb1-8037-4d0c-896e-a46fb22287ff")]
    [InlineData("https://metadata-only.example/not-followed")]
    public async Task CreateWorkspaceAsync_ReturnsLocationOnlyAsMetadata(string? location)
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) =>
            Task.FromResult(WorkspaceCreateTestData.CreateResponse(location: location)));
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, WorkspaceCreateTestData.CreateCredential());

        var result = await service.CreateWorkspaceAsync(new() { DisplayName = "New workspace" }, TestContext.Current.CancellationToken);

        Assert.Equal(location, result.Location);
        Assert.Null(result.Workspace.CapacityId);
        Assert.Null(result.Workspace.DomainId);
        Assert.Null(result.Workspace.ApiEndpoint);
        Assert.Equal(1, handler.CallCount);
    }

    [Theory]
    [InlineData("")]
    [InlineData("http://[")]
    public async Task CreateWorkspaceAsync_RejectsMalformedLocationWithoutRepeatingCreation(string location)
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) =>
            Task.FromResult(WorkspaceCreateTestData.CreateResponse(location: location)));
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, WorkspaceCreateTestData.CreateCredential());

        await Assert.ThrowsAsync<JsonException>(() =>
            service.CreateWorkspaceAsync(new() { DisplayName = "New workspace" }, TestContext.Current.CancellationToken));

        Assert.Equal(1, handler.CallCount);
    }

    [Fact]
    public async Task CreateWorkspaceAsync_RejectsMultipleLocationsWithoutRepeatingCreation()
    {
        using var response = WorkspaceCreateTestData.CreateResponse(location: null);
        response.Headers.TryAddWithoutValidation("Location",
            [WorkspaceCreateTestData.Location, "https://metadata-only.example/not-followed"]);
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, WorkspaceCreateTestData.CreateCredential());

        await Assert.ThrowsAsync<JsonException>(() =>
            service.CreateWorkspaceAsync(new() { DisplayName = "New workspace" }, TestContext.Current.CancellationToken));

        Assert.Equal(1, handler.CallCount);
    }

    public static TheoryData<string> InvalidMetadata
    {
        get
        {
            var data = new TheoryData<string> { "", "null", "[]", "{}", "not-json" };
            foreach (var name in new[] { "id", "displayName", "type" })
            {
                data.Add(WorkspaceCreateTestData.WithoutMetadataProperty(name));
                data.Add(WorkspaceCreateTestData.WithMetadataProperty(name, null));
                data.Add(WorkspaceCreateTestData.WithMetadataProperty(name, ""));
            }

            foreach (var name in new[] { "id", "capacityId", "domainId" })
            {
                data.Add(WorkspaceCreateTestData.WithMetadataProperty(name, "not-a-uuid"));
                data.Add(WorkspaceCreateTestData.WithMetadataProperty(name, Guid.Empty.ToString("D")));
            }

            data.Add(WorkspaceCreateTestData.WithMetadataProperty("displayName", " "));
            data.Add(WorkspaceCreateTestData.WithMetadataProperty("type", " "));
            data.Add(WorkspaceCreateTestData.WithMetadataProperty("tags", JsonNode.Parse("[null]")));
            data.Add(WorkspaceCreateTestData.WithMetadataProperty("tags", JsonNode.Parse("[{}]")));
            data.Add(WorkspaceCreateTestData.WithMetadataProperty("tags",
                JsonNode.Parse("""[{"id":"00000000-0000-0000-0000-000000000000","displayName":"Tag"}]""")));
            data.Add(WorkspaceCreateTestData.WithMetadataProperty("tags",
                JsonNode.Parse("""[{"id":"de627df0-2328-4c05-98f7-b03457a2daef","displayName":" "}]""")));
            return data;
        }
    }

    [Theory]
    [MemberData(nameof(InvalidMetadata))]
    public async Task CreateWorkspaceAsync_RejectsMalformedSuccessInsteadOfInventingAResult(string metadata)
    {
        using var response = WorkspaceCreateTestData.CreateResponse(metadata);
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, WorkspaceCreateTestData.CreateCredential());

        await Assert.ThrowsAsync<JsonException>(() =>
            service.CreateWorkspaceAsync(new() { DisplayName = "New workspace" }, TestContext.Current.CancellationToken));

        Assert.Equal(1, handler.CallCount);
        await Assert.ThrowsAsync<ObjectDisposedException>(() => response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
    }

    [Theory]
    [InlineData(HttpStatusCode.OK)]
    [InlineData(HttpStatusCode.Accepted)]
    [InlineData(HttpStatusCode.NoContent)]
    public async Task CreateWorkspaceAsync_RejectsUnexpectedSuccessWithoutPolling(HttpStatusCode status)
    {
        using var response = new HttpResponseMessage(status) { Content = new StringContent(WorkspaceCreateTestData.MinimalMetadata) };
        response.Headers.TryAddWithoutValidation("Location", "https://metadata-only.example/operation");
        response.Headers.TryAddWithoutValidation("Retry-After", "0");
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, WorkspaceCreateTestData.CreateCredential());

        var exception = await Assert.ThrowsAnyAsync<HttpRequestException>(() =>
            service.CreateWorkspaceAsync(new() { DisplayName = "New workspace" }, TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.BadGateway, exception.StatusCode);
        Assert.Equal(1, handler.CallCount);
        await Assert.ThrowsAsync<ObjectDisposedException>(() => response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
    }

    [Theory]
    [InlineData(HttpStatusCode.BadRequest)]
    [InlineData(HttpStatusCode.Unauthorized)]
    [InlineData(HttpStatusCode.Forbidden)]
    [InlineData(HttpStatusCode.NotFound)]
    [InlineData(HttpStatusCode.Conflict)]
    [InlineData(HttpStatusCode.TooManyRequests)]
    [InlineData(HttpStatusCode.InternalServerError)]
    [InlineData(HttpStatusCode.ServiceUnavailable)]
    public async Task CreateWorkspaceAsync_PreservesFailuresWithoutRetryOrBackendDetails(HttpStatusCode status)
    {
        using var response = new HttpResponseMessage(status) { Content = new StringContent(WorkspaceCreateTestData.SecretMarker) };
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
        using var client = new HttpClient(handler);
        var credential = WorkspaceCreateTestData.CreateCredential();
        var service = new FabricCoreService(client, credential);

        var exception = await Assert.ThrowsAnyAsync<HttpRequestException>(() =>
            service.CreateWorkspaceAsync(new() { DisplayName = "New workspace" }, TestContext.Current.CancellationToken));

        Assert.Equal(status, exception.StatusCode);
        Assert.DoesNotContain(WorkspaceCreateTestData.SecretMarker, exception.Message);
        Assert.Equal(1, handler.CallCount);
        await credential.Received(1).GetTokenAsync(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>());
        await Assert.ThrowsAsync<ObjectDisposedException>(() => response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task CreateWorkspaceAsync_DisposesSuccessfulResponseButNotInjectedClient()
    {
        using var firstResponse = WorkspaceCreateTestData.CreateResponse();
        using var secondResponse = WorkspaceCreateTestData.CreateResponse();
        var responses = new Queue<HttpResponseMessage>([firstResponse, secondResponse]);
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(responses.Dequeue()));
        using var client = new HttpClient(handler);
        var credential = WorkspaceCreateTestData.CreateCredential();
        var service = new FabricCoreService(client, credential);

        await service.CreateWorkspaceAsync(new() { DisplayName = "First workspace" }, TestContext.Current.CancellationToken);
        await service.CreateWorkspaceAsync(new() { DisplayName = "Second workspace" }, TestContext.Current.CancellationToken);

        Assert.Equal(2, handler.CallCount);
        await credential.Received(2).GetTokenAsync(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>());
        await Assert.ThrowsAsync<ObjectDisposedException>(() => firstResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        await Assert.ThrowsAsync<ObjectDisposedException>(() => secondResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task CreateWorkspaceAsync_PropagatesCancellationBeforeAuthentication()
    {
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        cancellation.Cancel();
        using var handler = new FabricCoreHttpMessageHandler((_, _) => throw new InvalidOperationException("HTTP must not be called."));
        using var client = new HttpClient(handler);
        var credential = WorkspaceCreateTestData.CreateCredential();
        var service = new FabricCoreService(client, credential);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            service.CreateWorkspaceAsync(new() { DisplayName = "New workspace" }, cancellation.Token));

        Assert.Equal(0, handler.CallCount);
        await credential.DidNotReceive().GetTokenAsync(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateWorkspaceAsync_PropagatesCancellationDuringAuthentication()
    {
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        var credential = Substitute.For<TokenCredential>();
        credential.GetTokenAsync(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>()).Returns(call =>
        {
            var token = call.ArgAt<CancellationToken>(1);
            Assert.Equal(cancellation.Token, token);
            cancellation.Cancel();
            return ValueTask.FromCanceled<AccessToken>(token);
        });
        using var handler = new FabricCoreHttpMessageHandler((_, _) => throw new InvalidOperationException("HTTP must not be called."));
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, credential);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            service.CreateWorkspaceAsync(new() { DisplayName = "New workspace" }, cancellation.Token));

        Assert.Equal(0, handler.CallCount);
    }

    [Fact]
    public async Task CreateWorkspaceAsync_DoesNotSendAfterAuthenticationFailure()
    {
        var credential = Substitute.For<TokenCredential>();
        credential.GetTokenAsync(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>())
            .Returns(ValueTask.FromException<AccessToken>(new AuthenticationFailedException(WorkspaceCreateTestData.SecretMarker)));
        using var handler = new FabricCoreHttpMessageHandler((_, _) => throw new InvalidOperationException("HTTP must not be called."));
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, credential);

        await Assert.ThrowsAsync<AuthenticationFailedException>(() =>
            service.CreateWorkspaceAsync(new() { DisplayName = "New workspace" }, TestContext.Current.CancellationToken));

        Assert.Equal(0, handler.CallCount);
    }

    [Fact]
    public async Task CreateWorkspaceAsync_PropagatesHttpCancellationWithoutRetry()
    {
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        using var handler = new FabricCoreHttpMessageHandler((_, token) =>
        {
            cancellation.Cancel();
            Assert.True(token.IsCancellationRequested);
            return Task.FromCanceled<HttpResponseMessage>(token);
        });
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, WorkspaceCreateTestData.CreateCredential());

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            service.CreateWorkspaceAsync(new() { DisplayName = "New workspace" }, cancellation.Token));

        Assert.Equal(1, handler.CallCount);
    }

    [Fact]
    public async Task CreateWorkspaceAsync_CancelsDeserializationAndDisposesResponseStream()
    {
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        using var stream = new WorkspaceCreateCancellationStream(cancellation);
        using var response = new HttpResponseMessage(HttpStatusCode.Created) { Content = new StreamContent(stream) };
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, WorkspaceCreateTestData.CreateCredential());

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            service.CreateWorkspaceAsync(new() { DisplayName = "New workspace" }, cancellation.Token));

        Assert.Equal(cancellation.Token, stream.ReadCancellationToken);
        Assert.True(stream.IsDisposed);
        Assert.Equal(1, handler.CallCount);
    }

    [Fact]
    public async Task CreateWorkspaceAsync_KeepsConcurrentRequestBodiesAndCredentialsIsolated()
    {
        var identity = new AsyncLocal<string?>();
        var requestsReady = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var tokenRequestCount = 0;
        var credential = Substitute.For<TokenCredential>();
        credential.GetTokenAsync(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>())
            .Returns(call => new ValueTask<AccessToken>(GetTokenAsync(call.ArgAt<CancellationToken>(1))));
        using var handler = new FabricCoreHttpMessageHandler(async (request, cancellationToken) =>
        {
            using var body = JsonDocument.Parse(await request.Content!.ReadAsStringAsync(cancellationToken));
            var name = body.RootElement.GetProperty("displayName").GetString();
            Assert.Equal(name, request.Headers.Authorization?.Parameter);
            return WorkspaceCreateTestData.CreateResponse(WorkspaceCreateTestData.WithMetadataProperty("displayName", name));
        });
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, credential);

        var results = await Task.WhenAll(CreateAsync("First identity"), CreateAsync("Second identity"));

        Assert.Equal(["First identity", "Second identity"], results.Select(static result => result.Workspace.DisplayName));
        Assert.Equal(2, handler.CallCount);
        Assert.Null(client.DefaultRequestHeaders.Authorization);

        async Task<AccessToken> GetTokenAsync(CancellationToken cancellationToken)
        {
            var name = identity.Value;
            Assert.NotNull(name);
            if (Interlocked.Increment(ref tokenRequestCount) == 2)
            {
                requestsReady.SetResult();
            }

            await requestsReady.Task.WaitAsync(cancellationToken);
            return new(name, DateTimeOffset.UtcNow.AddHours(1));
        }

        async Task<WorkspaceCreateResult> CreateAsync(string name)
        {
            identity.Value = name;
            return await service.CreateWorkspaceAsync(new() { DisplayName = name }, TestContext.Current.CancellationToken);
        }
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task ExistingPostOperations_PreserveRequestAndResponseBehavior(bool createItem)
    {
        using var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(createItem
                ? """{"id":"item-id","displayName":"Existing item","type":"Lakehouse","workspaceId":"workspace-id"}"""
                : """{"value":[{"id":"item-id","displayName":"Existing item","type":"Lakehouse"}],"continuationToken":"cursor"}""")
        };
        using var handler = new FabricCoreHttpMessageHandler(async (request, cancellationToken) =>
        {
            Assert.Equal(HttpMethod.Post, request.Method);
            Assert.Equal(createItem
                ? "https://api.fabric.microsoft.com/v1/workspaces/workspace-id/items"
                : "https://api.fabric.microsoft.com/v1/catalog/search", request.RequestUri?.AbsoluteUri);
            Assert.Equal("offline-token", request.Headers.Authorization?.Parameter);
            using var body = JsonDocument.Parse(await request.Content!.ReadAsStringAsync(cancellationToken));
            if (createItem)
            {
                Assert.Equal("Existing item", body.RootElement.GetProperty("displayName").GetString());
                Assert.Equal("Lakehouse", body.RootElement.GetProperty("type").GetString());
            }
            else
            {
                Assert.Equal("Existing", body.RootElement.GetProperty("search").GetString());
                Assert.Equal(10, body.RootElement.GetProperty("pageSize").GetInt32());
            }

            return response;
        });
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, WorkspaceCreateTestData.CreateCredential());

        if (createItem)
        {
            var result = await service.CreateItemAsync("workspace-id",
                new() { DisplayName = "Existing item", Type = "Lakehouse" }, TestContext.Current.CancellationToken);
            Assert.Equal("item-id", result.Id);
            Assert.Equal("workspace-id", result.WorkspaceId);
        }
        else
        {
            var result = await service.SearchCatalogAsync(new() { Search = "Existing", PageSize = 10 }, TestContext.Current.CancellationToken);
            Assert.Equal("item-id", Assert.Single(result.Value).Id);
            Assert.Equal("cursor", result.ContinuationToken);
        }

        Assert.Equal(1, handler.CallCount);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task ExistingPostOperations_PreserveFailureBehavior(bool createItem)
    {
        using var response = new HttpResponseMessage(HttpStatusCode.BadRequest) { Content = new StringContent("existing failure") };
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, WorkspaceCreateTestData.CreateCredential());

        var exception = await Assert.ThrowsAsync<HttpRequestException>(async () =>
        {
            if (createItem)
            {
                await service.CreateItemAsync("workspace-id", new() { DisplayName = "Item", Type = "Lakehouse" }, TestContext.Current.CancellationToken);
            }
            else
            {
                await service.SearchCatalogAsync(new() { Search = "Item" }, TestContext.Current.CancellationToken);
            }
        });

        Assert.Contains("existing failure", exception.Message);
        Assert.Null(exception.StatusCode);
        Assert.Equal(1, handler.CallCount);
    }
}
