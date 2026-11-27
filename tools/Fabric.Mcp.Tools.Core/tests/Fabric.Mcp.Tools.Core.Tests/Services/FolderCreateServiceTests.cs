// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
// cspell:words Retriable parentfolderid

using System.Collections.Concurrent;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Azure.Core;
using Fabric.Mcp.Tools.Core.Models;
using Fabric.Mcp.Tools.Core.Services;
using Fabric.Mcp.Tools.Core.Tests.TestSupport;
using Fabric.Mcp.Tools.Core.Validation;
using NSubstitute;
using Xunit;
using static Fabric.Mcp.Tools.Core.Tests.TestSupport.FolderCreateTestData;
using static Fabric.Mcp.Tools.Core.Tests.TestSupport.FolderListTestData;

namespace Fabric.Mcp.Tools.Core.Tests.Services;

public sealed class FolderCreateServiceTests()
{
    [Theory]
    [InlineData(null)]
    [InlineData(ParentFolderId)]
    public static async Task Create_SendsExactlyOnePostWithOnlyExplicitFieldsAndDisposes(string? parent)
    {
        const string name = "Sales \u00e9\U0001F680";
        using var stream = new OperationTestStream(Encoding.UTF8.GetBytes(CreatedFolder(parent, name)[..^1] + ",\"extra\":\"private\"}"));
        using var response = new HttpResponseMessage(HttpStatusCode.Created) { Content = new StreamContent(stream) };
        response.Content.Headers.ContentType = new("application/json");
        response.Headers.TryAddWithoutValidation("Location", ["https://example.invalid/never-follow", "/v1/operations/unused"]);
        response.Headers.TryAddWithoutValidation("x-ms-operation-id", "private");
        response.Headers.TryAddWithoutValidation("Retry-After", "0");
        using var handler = new FabricCoreHttpMessageHandler(async (request, token) =>
        {
            Assert.Equal(HttpMethod.Post, request.Method);
            Assert.Equal(CollectionUrl, request.RequestUri?.AbsoluteUri);
            Assert.Equal("application/json", request.Content?.Headers.ContentType?.MediaType);
            Assert.Equal("Bearer", request.Headers.Authorization?.Scheme);
            Assert.Equal("folder-test-token", request.Headers.Authorization?.Parameter);
            Assert.Equal("Fabric Core MCP", request.Headers.UserAgent.ToString());
            using var body = JsonDocument.Parse(await request.Content!.ReadAsStringAsync(token));
            Assert.Equal(parent is null ? ["displayName"] : new[] { "displayName", "parentFolderId" },
                body.RootElement.EnumerateObject().Select(property => property.Name).Order());
            Assert.Equal(name, body.RootElement.GetProperty("displayName").GetString());
            if (parent is not null)
            {
                Assert.Equal(parent, body.RootElement.GetProperty("parentFolderId").GetString());
            }
            return response;
        });
        using var client = new HttpClient(handler);
        var credential = Credential();
        var result = await new FabricCoreService(client, credential).CreateFolderAsync(
            $"{{{WorkspaceId.ToUpperInvariant()}}}", name, parent is null ? null : Guid.Parse(parent).ToString("N").ToUpperInvariant(),
            TestContext.Current.CancellationToken);

        Assert.Equal(Guid.Parse(WorkspaceId), result.Folder.WorkspaceId);
        Assert.Equal(Guid.Parse(FolderId), result.Folder.Id);
        Assert.Equal(parent is null ? null : Guid.Parse(parent), result.Folder.ParentFolderId);
        Assert.Equal(name, result.Folder.DisplayName);
        Assert.Equal("Created", result.CreationStatus);
        Assert.Equal(0, result.RetryAfterSeconds);
        var output = JsonSerializer.SerializeToElement(result, CoreJsonContext.Default.FolderCreateCommandResult);
        Assert.DoesNotContain("private", output.GetRawText());
        Assert.Equal(parent is not null, output.GetProperty("folder").TryGetProperty("parentFolderId", out _));
        Assert.Equal(1, handler.CallCount);
        Assert.True(stream.Disposed);
        Assert.Throws<ObjectDisposedException>(() => response.Content.ReadAsStream(TestContext.Current.CancellationToken));
        await credential.Received(1).GetTokenAsync(
            Arg.Is<TokenRequestContext>(context => context.Scopes.SequenceEqual(FabricEndpoints.FabricScopes)), TestContext.Current.CancellationToken);
    }

    [Theory]
    [InlineData(null, false)]
    [InlineData(null, true)]
    [InlineData(ParentFolderId, false)]
    [InlineData(ParentFolderId, true)]
    public static async Task Create_DoesNotInventAbsentOrNullOptionalParentMetadata(string? requestedParent, bool explicitNull)
    {
        var json = CreatedFolder()[..^1] + (explicitNull ? ",\"parentFolderId\":null}" : "}");
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(Response(json, HttpStatusCode.Created)));
        using var client = new HttpClient(handler);
        var result = await CreateAsync(new FabricCoreService(client, Credential()), requestedParent);
        Assert.Null(result.Folder.ParentFolderId);
        Assert.DoesNotContain("parentFolderId", JsonSerializer.Serialize(result, CoreJsonContext.Default.FolderCreateCommandResult));
        Assert.Equal(1, handler.CallCount);
    }

    [Fact]
    public static async Task Create_RejectsInvalidInputsBeforeAuthentication()
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) => throw new InvalidOperationException("No HTTP expected."));
        using var client = new HttpClient(handler);
        var credential = Credential();
        var service = new FabricCoreService(client, credential);
        foreach (var name in InvalidNames)
        {
            await Assert.ThrowsAsync<FabricItemInputException>(() => service.CreateFolderAsync(
                WorkspaceId, name, cancellationToken: TestContext.Current.CancellationToken));
        }
        foreach (var id in new[] { "", "private-invalid", Guid.Empty.ToString(), "../private" })
        {
            await Assert.ThrowsAsync<FabricItemInputException>(() => service.CreateFolderAsync(id, "Sales", cancellationToken: TestContext.Current.CancellationToken));
            await Assert.ThrowsAsync<FabricItemInputException>(() => CreateAsync(service, id));
        }
        Assert.Empty(credential.ReceivedCalls());
        Assert.Equal(0, handler.CallCount);
    }

    [Theory]
    [InlineData("credentials", HttpStatusCode.Unauthorized)]
    [InlineData("authentication", HttpStatusCode.Unauthorized)]
    [InlineData("operation-canceled", HttpStatusCode.RequestTimeout)]
    [InlineData("timeout", HttpStatusCode.GatewayTimeout)]
    public static async Task Create_AuthenticationFailuresAreKnownNotSent(string failure, HttpStatusCode expected)
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) => throw new InvalidOperationException("No HTTP expected."));
        using var client = new HttpClient(handler);
        var credential = Credential();
        credential.GetTokenAsync(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>())
            .Returns<ValueTask<AccessToken>>(_ => throw FabricCoreErrorTestData.CreateException(failure));
        var error = await Assert.ThrowsAsync<FolderCreateRequestException>(() => CreateAsync(new FabricCoreService(client, credential)));
        Assert.Equal(expected, error.StatusCode);
        Assert.True(error.RequestNotSent);
        Assert.False(error.CreationConfirmed);
        Assert.Equal(0, handler.CallCount);
        Assert.DoesNotContain("private", error.Message);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public static async Task Create_CancellationBeforeSendNeverClaimsPossibleCreation(bool duringAuthentication)
    {
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        var credential = Credential();
        credential.GetTokenAsync(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>()).Returns(_ =>
        {
            cancellation.Cancel();
            return new AccessToken("unused", DateTimeOffset.MaxValue);
        });
        if (!duringAuthentication)
        {
            cancellation.Cancel();
        }
        using var handler = new FabricCoreHttpMessageHandler((_, _) => throw new InvalidOperationException("No HTTP expected."));
        using var client = new HttpClient(handler);
        var error = await Assert.ThrowsAsync<FolderCreateRequestException>(() =>
            new FabricCoreService(client, credential).CreateFolderAsync(WorkspaceId, "Sales", cancellationToken: cancellation.Token));
        Assert.True(error.RequestNotSent);
        Assert.False(error.CreationConfirmed);
        Assert.Equal(HttpStatusCode.RequestTimeout, error.StatusCode);
        Assert.Equal(duringAuthentication ? 1 : 0, credential.ReceivedCalls().Count());
        Assert.Equal(0, handler.CallCount);
    }

    [Theory]
    [InlineData("network", HttpStatusCode.ServiceUnavailable)]
    [InlineData("io", HttpStatusCode.ServiceUnavailable)]
    [InlineData("operation-canceled", HttpStatusCode.RequestTimeout)]
    [InlineData("timeout", HttpStatusCode.GatewayTimeout)]
    public static async Task Create_SendFailuresRemainUncertainWithoutRetry(string failure, HttpStatusCode expected)
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) => throw failure switch
        {
            "network" => new HttpRequestException("private"),
            "io" => new IOException("private"),
            _ => FabricCoreErrorTestData.CreateException(failure)
        });
        using var client = new HttpClient(handler);
        var error = await Assert.ThrowsAsync<FolderCreateRequestException>(() => CreateAsync(new FabricCoreService(client, Credential())));
        Assert.Equal(expected, error.StatusCode);
        Assert.False(error.RequestNotSent);
        Assert.False(error.CreationConfirmed);
        Assert.Equal(1, handler.CallCount);
    }

    [Theory]
    [InlineData(HttpStatusCode.OK, HttpStatusCode.BadGateway)]
    [InlineData(HttpStatusCode.Accepted, HttpStatusCode.BadGateway)]
    [InlineData(HttpStatusCode.NoContent, HttpStatusCode.BadGateway)]
    [InlineData(HttpStatusCode.TemporaryRedirect, HttpStatusCode.TemporaryRedirect)]
    [InlineData(HttpStatusCode.BadRequest, HttpStatusCode.BadRequest)]
    [InlineData(HttpStatusCode.Unauthorized, HttpStatusCode.Unauthorized)]
    [InlineData(HttpStatusCode.Forbidden, HttpStatusCode.Forbidden)]
    [InlineData(HttpStatusCode.NotFound, HttpStatusCode.NotFound)]
    [InlineData(HttpStatusCode.Conflict, HttpStatusCode.Conflict)]
    [InlineData(HttpStatusCode.TooManyRequests, HttpStatusCode.TooManyRequests)]
    [InlineData(HttpStatusCode.ServiceUnavailable, HttpStatusCode.ServiceUnavailable)]
    public static async Task Create_Requires201AndNeverFollowsUrlsOrRetries(HttpStatusCode status, HttpStatusCode expected)
    {
        using var response = Response("""{"errorCode":"FolderDisplayNameAlreadyInUse","message":"private","isRetriable":true}""", status, "60");
        response.Headers.Location = new Uri(CollectionUrl + "/" + FolderId);
        response.Headers.TryAddWithoutValidation("x-ms-operation-id", FolderId);
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
        using var client = new HttpClient(handler);
        var error = await Assert.ThrowsAsync<FolderCreateRequestException>(() => CreateAsync(new FabricCoreService(client, Credential())));
        Assert.Equal(expected, error.StatusCode);
        Assert.Equal(response.IsSuccessStatusCode ? null : "FolderDisplayNameAlreadyInUse", error.ErrorCode);
        Assert.Equal(TimeSpan.FromSeconds(60), error.RetryAfter?.Delta);
        Assert.False(error.CreationConfirmed);
        Assert.False(error.RequestNotSent);
        Assert.Equal(1, handler.CallCount);
        Assert.Throws<ObjectDisposedException>(() => response.Content.ReadAsStream(TestContext.Current.CancellationToken));
    }

    [Fact]
    public static async Task Create_RejectsMalformedRequiredOrContradictoryMetadataButPreservesConfirmedCreation()
    {
        var valid = CreatedFolder(ParentFolderId);
        var invalid = new List<string>
        {
            "", "null", "[]", "{}", "private", valid.Replace(FolderId, Guid.Empty.ToString(), StringComparison.Ordinal),
            valid.Replace(WorkspaceId, FolderId, StringComparison.Ordinal),
            valid.Replace(ParentFolderId, FolderId, StringComparison.Ordinal),
            valid.Replace(ParentFolderId, Guid.Empty.ToString(), StringComparison.Ordinal),
            valid.Replace("\"Sales\"", "null", StringComparison.Ordinal),
            valid.Replace("\"Sales\"", "\"\\uD800\"", StringComparison.Ordinal),
            valid.Replace("\"Sales\"", "\"\\uDC00\"", StringComparison.Ordinal),
            valid.Replace("\"Sales\"", "45", StringComparison.Ordinal),
            valid[..^1] + ",\"id\":\"" + FolderId + "\"}",
            valid[..^1] + ",\"ID\":\"" + FolderId + "\"}",
            valid[..^1] + ",\"unknown\":\"\\uD800\"}",
            valid[..^1] + ",\"unknown\\uD800\":\"private\"}",
            valid[..^1] + ",\"parentfolderid\":\"" + FolderId + "\"}"
        };
        foreach (var field in new[] { "id", "displayName", "workspaceId" })
        {
            var node = JsonNode.Parse(valid)!.AsObject();
            node.Remove(field);
            invalid.Add(node.ToJsonString());
        }
        foreach (var json in invalid)
        {
            using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(Response(json, HttpStatusCode.Created, "60")));
            using var client = new HttpClient(handler);
            var error = await Assert.ThrowsAsync<FolderCreateRequestException>(() => CreateAsync(new FabricCoreService(client, Credential()), ParentFolderId));
            Assert.True(error.CreationConfirmed);
            Assert.False(error.RequestNotSent);
            Assert.Equal(HttpStatusCode.BadGateway, error.StatusCode);
            Assert.Equal(TimeSpan.FromSeconds(60), error.RetryAfter?.Delta);
            Assert.Equal(1, handler.CallCount);
        }
        using var rootHandler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(Response(valid, HttpStatusCode.Created)));
        using var rootClient = new HttpClient(rootHandler);
        Assert.True((await Assert.ThrowsAsync<FolderCreateRequestException>(() =>
            CreateAsync(new FabricCoreService(rootClient, Credential())))).CreationConfirmed);
    }

    [Theory]
    [InlineData(HttpStatusCode.Created)]
    [InlineData(HttpStatusCode.TooManyRequests)]
    public static async Task Create_UnavailableBodiesPreserveCreationOrOriginalFailure(HttpStatusCode status)
    {
        foreach (var failure in new[] { "type", "io", "network", "cancellation", "timeout" })
        {
            using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
            using var stream = new OperationTestStream(Encoding.UTF8.GetBytes("private"), (_, token) =>
            {
                if (failure == "cancellation")
                {
                    cancellation.Cancel();
                    token.ThrowIfCancellationRequested();
                }
                return failure switch
                {
                    "io" => throw new IOException("private"),
                    "network" => throw new HttpRequestException("private"),
                    "timeout" => throw new TimeoutException("private"),
                    _ => ValueTask.CompletedTask
                };
            });
            using var response = new HttpResponseMessage(status)
            {
                Content = new StreamContent(stream)
            };
            response.Content.Headers.ContentType = new(failure == "type" ? "text/html" : "application/json");
            response.Headers.TryAddWithoutValidation("Retry-After", "60");
            using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
            using var client = new HttpClient(handler);
            var error = await Assert.ThrowsAsync<FolderCreateRequestException>(() => new FabricCoreService(client, Credential()).CreateFolderAsync(
                WorkspaceId, "Sales", cancellationToken: cancellation.Token));
            Assert.Equal(status == HttpStatusCode.Created, error.CreationConfirmed);
            Assert.Equal(status != HttpStatusCode.Created, error.ErrorMetadataUnavailable);
            Assert.Equal(TimeSpan.FromSeconds(60), error.RetryAfter?.Delta);
            if (status != HttpStatusCode.Created)
            {
                Assert.Equal(status, error.StatusCode);
            }
            Assert.Equal(1, handler.CallCount);
            Assert.Throws<ObjectDisposedException>(() => response.Content.ReadAsStream(TestContext.Current.CancellationToken));
        }
    }

    [Theory]
    [InlineData("""{"errorCode":"\uD800"}""")]
    [InlineData("""{"error\uD800Code":"FolderNotFound"}""")]
    [InlineData("""{"errorCode":"FolderNotFound","errorCode":"Other"}""")]
    [InlineData("""{"errorCode":"private invalid"}""")]
    [InlineData("""{"errorCode":null}""")]
    [InlineData("private")]
    [InlineData("[]")]
    public static async Task Create_UnusableErrorJsonRetainsStatusAndRetryAfter(string json)
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(Response(json, HttpStatusCode.TooManyRequests, "60")));
        using var client = new HttpClient(handler);
        var error = await Assert.ThrowsAsync<FolderCreateRequestException>(() => CreateAsync(new FabricCoreService(client, Credential())));
        Assert.Equal(HttpStatusCode.TooManyRequests, error.StatusCode);
        Assert.Equal(TimeSpan.FromSeconds(60), error.RetryAfter?.Delta);
        Assert.True(error.ErrorMetadataUnavailable);
        Assert.Null(error.ErrorCode);
        Assert.False(error.CreationConfirmed);
    }

    [Theory]
    [InlineData(1048576, false)]
    [InlineData(1048577, false)]
    [InlineData(1048576, true)]
    [InlineData(1048577, true)]
    public static async Task Create_EnforcesExactResponseByteBudget(long length, bool declaredLength)
    {
        using var stream = new PaddedJsonTestStream(Encoding.UTF8.GetBytes(CreatedFolder()), length);
        using var response = new HttpResponseMessage(HttpStatusCode.Created) { Content = new StreamContent(stream) };
        response.Content.Headers.ContentType = new("application/json");
        if (declaredLength)
        {
            response.Content.Headers.ContentLength = length;
        }
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, Credential());
        if (length == 1048576)
        {
            Assert.Equal("Created", (await CreateAsync(service)).CreationStatus);
        }
        else
        {
            Assert.True((await Assert.ThrowsAsync<FolderCreateRequestException>(() => CreateAsync(service))).CreationConfirmed);
            Assert.Equal(declaredLength ? 0 : 1048577, stream.BytesRead);
        }
        Assert.True(stream.Disposed);
        Assert.Equal(1, handler.CallCount);
    }

    [Theory]
    [InlineData(1048576, false)]
    [InlineData(1048577, false)]
    [InlineData(1048576, true)]
    [InlineData(1048577, true)]
    public static async Task Create_ErrorBodyBudgetPreservesKnownStatusAndRetryAfter(long length, bool declaredLength)
    {
        using var stream = new PaddedJsonTestStream(Encoding.UTF8.GetBytes("""{"errorCode":"TooManyFolders"}"""), length);
        using var response = new HttpResponseMessage(HttpStatusCode.TooManyRequests) { Content = new StreamContent(stream) };
        response.Content.Headers.ContentType = new("application/json");
        response.Headers.TryAddWithoutValidation("Retry-After", "0");
        if (declaredLength)
        {
            response.Content.Headers.ContentLength = length;
        }
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
        using var client = new HttpClient(handler);
        var error = await Assert.ThrowsAsync<FolderCreateRequestException>(() => CreateAsync(new FabricCoreService(client, Credential())));
        Assert.Equal(HttpStatusCode.TooManyRequests, error.StatusCode);
        Assert.Equal(TimeSpan.Zero, error.RetryAfter?.Delta);
        Assert.Equal(length > 1048576, error.ErrorMetadataUnavailable);
        Assert.Equal(length > 1048576 ? null : "TooManyFolders", error.ErrorCode);
        Assert.False(error.CreationConfirmed);
        Assert.True(stream.Disposed);
        Assert.Equal(1, handler.CallCount);
    }

    [Fact]
    public static async Task Create_InvalidUtf8PreservesConfirmedCreation()
    {
        var bytes = Encoding.UTF8.GetBytes(CreatedFolder());
        bytes[Array.IndexOf(bytes, (byte)'S')] = 0xff;
        using var response = new HttpResponseMessage(HttpStatusCode.Created) { Content = new ByteArrayContent(bytes) };
        response.Content.Headers.ContentType = new("application/json");
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
        using var client = new HttpClient(handler);
        var error = await Assert.ThrowsAsync<FolderCreateRequestException>(() => CreateAsync(new FabricCoreService(client, Credential())));
        Assert.True(error.CreationConfirmed);
        Assert.Equal(HttpStatusCode.BadGateway, error.StatusCode);
        Assert.Equal(1, handler.CallCount);
    }

    [Theory]
    [InlineData(64)]
    [InlineData(65)]
    public static async Task Create_BoundsUnknownJsonDepthBeforeProjectingSafeMetadata(int depth)
    {
        var json = CreatedFolder()[..^1] + ",\"extra\":" + new string('[', depth - 1) + "0" + new string(']', depth - 1) + "}";
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(Response(json, HttpStatusCode.Created)));
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, Credential());
        if (depth == 64)
        {
            Assert.Equal("Created", (await CreateAsync(service)).CreationStatus);
        }
        else
        {
            Assert.True((await Assert.ThrowsAsync<FolderCreateRequestException>(() => CreateAsync(service))).CreationConfirmed);
        }
        Assert.Equal(1, handler.CallCount);
    }

    [Theory]
    [InlineData("0")]
    [InlineData("120")]
    [InlineData("Tue, 01 Jan 2030 00:00:00 GMT")]
    [InlineData("-1")]
    [InlineData("private")]
    [InlineData("1, 2")]
    public static async Task Create_ReturnsOnlyValidatedRetryHints(string retryAfter)
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(Response(CreatedFolder(), HttpStatusCode.Created, retryAfter)));
        using var client = new HttpClient(handler);
        var result = await CreateAsync(new FabricCoreService(client, Credential()));
        Assert.Equal(retryAfter is "0" or "120" ? long.Parse(retryAfter) : null, result.RetryAfterSeconds);
        Assert.Equal(retryAfter.StartsWith("Tue", StringComparison.Ordinal), result.RetryAfterUtc.HasValue);
        Assert.Equal(1, handler.CallCount);
    }

    [Fact]
    public static async Task Create_IsolatesConcurrentCallerCredentialsAndInputs()
    {
        var identity = new AsyncLocal<string?>();
        var credential = Credential();
        credential.GetTokenAsync(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>())
            .Returns(_ => new AccessToken(identity.Value!, DateTimeOffset.MaxValue));
        var contexts = Enumerable.Range(0, 12).ToDictionary(index => $"caller-{index}",
            index => (Workspace: Guid.NewGuid(), Parent: index % 2 == 0 ? Guid.NewGuid() : (Guid?)null));
        var observed = new ConcurrentBag<string>();
        using var handler = new FabricCoreHttpMessageHandler(async (request, token) =>
        {
            await Task.Yield();
            using var body = JsonDocument.Parse(await request.Content!.ReadAsStringAsync(token));
            var name = body.RootElement.GetProperty("displayName").GetString()!;
            Assert.Equal(name, request.Headers.Authorization?.Parameter);
            var context = contexts[name];
            Assert.Equal($"{FabricEndpoints.GetFabricApiBaseUrl()}/workspaces/{context.Workspace:D}/folders", request.RequestUri?.AbsoluteUri);
            Assert.Equal(context.Parent.HasValue, body.RootElement.TryGetProperty("parentFolderId", out var parent));
            if (context.Parent.HasValue)
            {
                Assert.Equal(context.Parent, parent.GetGuid());
            }
            observed.Add(name);
            return Response(JsonSerializer.Serialize(new FabricFolder(Guid.NewGuid(), name, context.Workspace, context.Parent),
                CoreJsonContext.Default.FabricFolder), HttpStatusCode.Created);
        });
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, credential);
        var results = await Task.WhenAll(Enumerable.Range(0, 12).Select(async index =>
        {
            identity.Value = $"caller-{index}";
            var context = contexts[identity.Value];
            var result = await service.CreateFolderAsync(context.Workspace.ToString(), identity.Value, context.Parent?.ToString(),
                TestContext.Current.CancellationToken);
            Assert.Equal(context.Workspace, result.Folder.WorkspaceId);
            Assert.Equal(context.Parent, result.Folder.ParentFolderId);
            return result;
        }));
        Assert.Equal(12, handler.CallCount);
        Assert.Equal(observed.Order(), results.Select(result => result.Folder.DisplayName).Order());
        Assert.Equal(12, observed.Distinct().Count());
    }

    private static Task<FolderCreateCommandResult> CreateAsync(FabricCoreService service, string? parent = null) =>
        service.CreateFolderAsync(WorkspaceId, "Sales", parent, TestContext.Current.CancellationToken);
}
