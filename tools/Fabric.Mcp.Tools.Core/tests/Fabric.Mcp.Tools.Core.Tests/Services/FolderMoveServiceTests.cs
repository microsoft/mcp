// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
// cspell:words Retriable

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
using static Fabric.Mcp.Tools.Core.Tests.TestSupport.FolderListTestData;

namespace Fabric.Mcp.Tools.Core.Tests.Services;

public sealed class FolderMoveServiceTests()
{
    [Theory]
    [InlineData(null, false)]
    [InlineData(null, true)]
    [InlineData(ParentFolderId, false)]
    public static async Task Move_SendsExactlyOnePostWithOnlyTheOptionalTargetAndReturnsObservedMetadata(string? target, bool explicitNull)
    {
        const string name = "Legacy / \u00e9\U0001F680";
        var json = Snapshot(target, name)[..^1] + ",\"extra\":\"private\"" + (explicitNull ? ",\"parentFolderId\":null}" : "}");
        using var stream = new OperationTestStream(Encoding.UTF8.GetBytes(json));
        using var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StreamContent(stream) };
        response.Content.Headers.ContentType = new("application/json");
        response.Headers.TryAddWithoutValidation("Location", ["https://example.invalid/never-follow", "/v1/operations/unused"]);
        response.Headers.TryAddWithoutValidation("x-ms-operation-id", "private");
        response.Headers.TryAddWithoutValidation("Retry-After", "0");
        HttpContent? requestContent = null;
        using var handler = new FabricCoreHttpMessageHandler(async (request, token) =>
        {
            Assert.Equal(HttpMethod.Post, request.Method);
            Assert.Equal($"{CollectionUrl}/{FolderId}/move", request.RequestUri?.AbsoluteUri);
            Assert.Equal("application/json", request.Content?.Headers.ContentType?.MediaType);
            Assert.Equal("Bearer", request.Headers.Authorization?.Scheme);
            Assert.Equal("folder-test-token", request.Headers.Authorization?.Parameter);
            Assert.Equal("Fabric Core MCP", request.Headers.UserAgent.ToString());
            Assert.True(token.CanBeCanceled);
            requestContent = request.Content;
            var body = await request.Content!.ReadAsStringAsync(token);
            Assert.Equal(target is null ? "{}" : $"{{\"targetFolderId\":\"{target}\"}}", body);
            return response;
        });
        using var client = new HttpClient(handler);
        var credential = Credential();

        var result = await new FabricCoreService(client, credential).MoveFolderAsync(
            $"{{{WorkspaceId.ToUpperInvariant()}}}", Guid.Parse(FolderId).ToString("N").ToUpperInvariant(),
            target is null ? null : Guid.Parse(target).ToString("B").ToUpperInvariant(), TestContext.Current.CancellationToken);

        Assert.Equal(Guid.Parse(WorkspaceId), result.Folder.WorkspaceId);
        Assert.Equal(Guid.Parse(FolderId), result.Folder.Id);
        Assert.Equal(target is null ? null : Guid.Parse(target), result.Folder.ParentFolderId);
        Assert.Equal(name, result.Folder.DisplayName);
        Assert.Equal("Moved", result.MoveStatus);
        Assert.Equal(0, result.RetryAfterSeconds);
        var output = JsonSerializer.SerializeToElement(result, CoreJsonContext.Default.FolderMoveCommandResult);
        Assert.Equal(["folder", "guidance", "moveStatus", "retryAfterSeconds"],
            output.EnumerateObject().Select(property => property.Name).Order());
        Assert.DoesNotContain("private", output.GetRawText());
        Assert.Equal(target is not null, output.GetProperty("folder").TryGetProperty("parentFolderId", out _));
        Assert.Equal(1, handler.CallCount);
        Assert.True(stream.Disposed);
        Assert.Throws<ObjectDisposedException>(() => response.Content.ReadAsStream(TestContext.Current.CancellationToken));
        Assert.Throws<ObjectDisposedException>(() => requestContent!.ReadAsStream(TestContext.Current.CancellationToken));
        await credential.Received(1).GetTokenAsync(
            Arg.Is<TokenRequestContext>(context => context.Scopes.SequenceEqual(FabricEndpoints.FabricScopes)), TestContext.Current.CancellationToken);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public static async Task Move_DoesNotInventMissingOptionalParentMetadata(bool explicitNull)
    {
        var json = Snapshot();
        if (explicitNull)
        {
            json = json[..^1] + ",\"parentFolderId\":null}";
        }
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(Response(json)));
        using var client = new HttpClient(handler);
        var result = await MoveAsync(new FabricCoreService(client, Credential()), ParentFolderId);
        Assert.Null(result.Folder.ParentFolderId);
        Assert.False(JsonSerializer.SerializeToElement(result, CoreJsonContext.Default.FolderMoveCommandResult)
            .GetProperty("folder").TryGetProperty("parentFolderId", out _));
        Assert.Equal(1, handler.CallCount);
    }

    [Fact]
    public static async Task Move_PreservesLegacyReturnedNamesWithoutCreationValidation()
    {
        foreach (var name in new[] { "", " ", " legacy/name ", "$recycle.bin", "CON", new string('a', 256), "\u0001\u0080", "\u00e9\U0001F680" })
        {
            using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(Response(Snapshot(name: name))));
            using var client = new HttpClient(handler);
            Assert.Equal(name, (await MoveAsync(new FabricCoreService(client, Credential()))).Folder.DisplayName);
            Assert.Equal(1, handler.CallCount);
        }
    }

    [Fact]
    public static async Task Move_RejectsInvalidIdsAndSelfTargetsBeforeAuthentication()
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) => throw new InvalidOperationException("No HTTP expected."));
        using var client = new HttpClient(handler);
        var credential = Credential();
        var service = new FabricCoreService(client, credential);
        foreach (var id in new[] { null!, "", " ", "private-invalid", Guid.Empty.ToString(), "../private" })
        {
            await Assert.ThrowsAsync<FabricItemInputException>(() => service.MoveFolderAsync(id, FolderId, cancellationToken: TestContext.Current.CancellationToken));
            await Assert.ThrowsAsync<FabricItemInputException>(() => service.MoveFolderAsync(WorkspaceId, id, cancellationToken: TestContext.Current.CancellationToken));
            if (id is not null)
            {
                await Assert.ThrowsAsync<FabricItemInputException>(() => MoveAsync(service, id));
            }
        }
        await Assert.ThrowsAsync<FabricItemInputException>(() => MoveAsync(service, Guid.Parse(FolderId).ToString("B").ToUpperInvariant()));
        Assert.Empty(credential.ReceivedCalls());
        Assert.Equal(0, handler.CallCount);
    }

    [Theory]
    [InlineData("credentials", HttpStatusCode.Unauthorized)]
    [InlineData("authentication", HttpStatusCode.Unauthorized)]
    [InlineData("operation-canceled", HttpStatusCode.RequestTimeout)]
    [InlineData("timeout", HttpStatusCode.GatewayTimeout)]
    public static async Task Move_AuthenticationFailuresAreKnownNotSent(string failure, HttpStatusCode expected)
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) => throw new InvalidOperationException("No HTTP expected."));
        using var client = new HttpClient(handler);
        var credential = Credential();
        credential.GetTokenAsync(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>())
            .Returns<ValueTask<AccessToken>>(_ => throw FabricCoreErrorTestData.CreateException(failure));
        var error = await Assert.ThrowsAsync<FolderMoveRequestException>(() => MoveAsync(new FabricCoreService(client, credential)));
        Assert.Equal(expected, error.StatusCode);
        Assert.True(error.RequestNotSent);
        Assert.False(error.MoveConfirmed);
        Assert.Null(error.ResponseStatusCode);
        Assert.Equal(0, handler.CallCount);
        Assert.DoesNotContain("private", error.Message);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public static async Task Move_CancellationBeforeSendNeverClaimsPossibleMovement(bool duringAuthentication)
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
        var error = await Assert.ThrowsAsync<FolderMoveRequestException>(() =>
            new FabricCoreService(client, credential).MoveFolderAsync(WorkspaceId, FolderId, cancellationToken: cancellation.Token));
        Assert.True(error.RequestNotSent);
        Assert.False(error.MoveConfirmed);
        Assert.Equal(HttpStatusCode.RequestTimeout, error.StatusCode);
        Assert.Equal(duringAuthentication ? 1 : 0, credential.ReceivedCalls().Count());
        Assert.Equal(0, handler.CallCount);
    }

    [Theory]
    [InlineData("network", HttpStatusCode.ServiceUnavailable)]
    [InlineData("io", HttpStatusCode.ServiceUnavailable)]
    [InlineData("operation-canceled", HttpStatusCode.RequestTimeout)]
    [InlineData("timeout", HttpStatusCode.GatewayTimeout)]
    public static async Task Move_SendFailuresRemainUncertainWithoutRetry(string failure, HttpStatusCode expected)
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) => throw failure switch
        {
            "network" => new HttpRequestException("private"),
            "io" => new IOException("private"),
            _ => FabricCoreErrorTestData.CreateException(failure)
        });
        using var client = new HttpClient(handler);
        var error = await Assert.ThrowsAsync<FolderMoveRequestException>(() => MoveAsync(new FabricCoreService(client, Credential())));
        Assert.Equal(expected, error.StatusCode);
        Assert.False(error.RequestNotSent);
        Assert.False(error.MoveConfirmed);
        Assert.Null(error.ResponseStatusCode);
        Assert.Equal(1, handler.CallCount);
    }

    [Theory]
    [InlineData(HttpStatusCode.Created, HttpStatusCode.BadGateway)]
    [InlineData(HttpStatusCode.Accepted, HttpStatusCode.BadGateway)]
    [InlineData(HttpStatusCode.NoContent, HttpStatusCode.BadGateway)]
    [InlineData(HttpStatusCode.TemporaryRedirect, HttpStatusCode.TemporaryRedirect)]
    [InlineData(HttpStatusCode.BadRequest, HttpStatusCode.BadRequest)]
    [InlineData(HttpStatusCode.Unauthorized, HttpStatusCode.Unauthorized)]
    [InlineData(HttpStatusCode.Forbidden, HttpStatusCode.Forbidden)]
    [InlineData(HttpStatusCode.NotFound, HttpStatusCode.NotFound)]
    [InlineData(HttpStatusCode.RequestTimeout, HttpStatusCode.RequestTimeout)]
    [InlineData(HttpStatusCode.Conflict, HttpStatusCode.Conflict)]
    [InlineData(HttpStatusCode.TooManyRequests, HttpStatusCode.TooManyRequests)]
    [InlineData(HttpStatusCode.InternalServerError, HttpStatusCode.InternalServerError)]
    public static async Task Move_Requires200AndNeverFollowsUrlsRetriesOrChoosesAnotherDestination(HttpStatusCode status, HttpStatusCode expected)
    {
        using var response = Response("""{"errorCode":"FolderDepthOutOfRange","message":"private","isRetriable":true}""", status, "60");
        response.Headers.Location = new Uri(CollectionUrl + "/" + FolderId);
        response.Headers.TryAddWithoutValidation("x-ms-operation-id", FolderId);
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
        using var client = new HttpClient(handler);
        var error = await Assert.ThrowsAsync<FolderMoveRequestException>(() => MoveAsync(new FabricCoreService(client, Credential())));
        Assert.Equal(expected, error.StatusCode);
        Assert.Equal(status, error.ResponseStatusCode);
        Assert.Equal(response.IsSuccessStatusCode ? null : "FolderDepthOutOfRange", error.ErrorCode);
        Assert.Equal(TimeSpan.FromSeconds(60), error.RetryAfter?.Delta);
        Assert.False(error.MoveConfirmed);
        Assert.False(error.RequestNotSent);
        Assert.Equal(1, handler.CallCount);
        Assert.Throws<ObjectDisposedException>(() => response.Content.ReadAsStream(TestContext.Current.CancellationToken));
    }

    [Fact]
    public static async Task Move_RejectsMalformedMismatchedOrContradictoryMetadataButPreservesConfirmedMovement()
    {
        var valid = Snapshot(ParentFolderId);
        var invalid = new List<string>
        {
            "", "null", "[]", "{}", "private", valid[..^1],
            valid.Replace(FolderId, ParentFolderId, StringComparison.Ordinal),
            valid.Replace(WorkspaceId, FolderId, StringComparison.Ordinal),
            valid.Replace(ParentFolderId, FolderId, StringComparison.Ordinal),
            valid.Replace(ParentFolderId, WorkspaceId, StringComparison.Ordinal),
            valid.Replace("\"Sales\"", "null", StringComparison.Ordinal),
            valid.Replace("\"Sales\"", "\"\\uD800\"", StringComparison.Ordinal),
            valid.Replace("\"Sales\"", "\"\\uDC00\"", StringComparison.Ordinal),
            valid.Replace("\"Sales\"", "45", StringComparison.Ordinal),
            valid[..^1] + ",\"id\":\"" + FolderId + "\"}",
            valid[..^1] + ",\"ID\":\"" + FolderId + "\"}",
            valid[..^1] + ",\"\\u0069d\":\"" + FolderId + "\"}",
            valid[..^1] + ",\"unknown\":\"\\uD800\"}",
            valid[..^1] + ",\"unknown\\uD800\":\"private\"}",
            valid[..^1] + ",\"unknown\":{\"name\":1,\"Name\":2}}",
            valid[..^1] + ",\"unknown\":[{\"name\":\"\\uDC00\"}]}"
        };
        foreach (var field in new[] { "id", "displayName", "workspaceId" })
        {
            var node = JsonNode.Parse(valid)!.AsObject();
            node.Remove(field);
            invalid.Add(node.ToJsonString());
        }
        foreach (var field in new[] { "id", "workspaceId", "parentFolderId" })
        {
            foreach (var value in new JsonNode?[] { null, JsonValue.Create(""), JsonValue.Create("private"),
                JsonValue.Create(Guid.Empty), JsonValue.Create(3), new JsonObject() })
            {
                if (field == "parentFolderId" && value is null)
                {
                    continue;
                }
                var node = JsonNode.Parse(valid)!;
                node[field] = value;
                invalid.Add(node.ToJsonString());
            }
        }
        foreach (var json in invalid)
        {
            using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(Response(json, retryAfter: "60")));
            using var client = new HttpClient(handler);
            var error = await Assert.ThrowsAsync<FolderMoveRequestException>(() => MoveAsync(new FabricCoreService(client, Credential()), ParentFolderId));
            Assert.True(error.MoveConfirmed);
            Assert.False(error.RequestNotSent);
            Assert.Equal(HttpStatusCode.BadGateway, error.StatusCode);
            Assert.Equal(HttpStatusCode.OK, error.ResponseStatusCode);
            Assert.Equal(TimeSpan.FromSeconds(60), error.RetryAfter?.Delta);
            Assert.Equal(1, handler.CallCount);
        }
    }

    [Fact]
    public static async Task Move_ToRootRejectsANonNullReturnedParentWithoutFallingBack()
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(Response(Snapshot(ParentFolderId))));
        using var client = new HttpClient(handler);
        var error = await Assert.ThrowsAsync<FolderMoveRequestException>(() => MoveAsync(new FabricCoreService(client, Credential())));
        Assert.True(error.MoveConfirmed);
        Assert.Equal(HttpStatusCode.BadGateway, error.StatusCode);
        Assert.Equal(1, handler.CallCount);
    }

    [Theory]
    [InlineData(HttpStatusCode.OK)]
    [InlineData(HttpStatusCode.TooManyRequests)]
    [InlineData(HttpStatusCode.Conflict)]
    public static async Task Move_UnavailableBodiesPreserveConfirmationOrOriginalFailure(HttpStatusCode status)
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
                    "network" => throw new HttpRequestException("private", null, HttpStatusCode.BadGateway),
                    "timeout" => throw new TimeoutException("private"),
                    _ => ValueTask.CompletedTask
                };
            });
            using var response = new HttpResponseMessage(status) { Content = new StreamContent(stream) };
            response.Content.Headers.ContentType = new(failure == "type" ? "text/html" : "application/json");
            response.Headers.TryAddWithoutValidation("Retry-After", "60");
            using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
            using var client = new HttpClient(handler);
            var error = await Assert.ThrowsAsync<FolderMoveRequestException>(() => new FabricCoreService(client, Credential())
                .MoveFolderAsync(WorkspaceId, FolderId, cancellationToken: cancellation.Token));
            Assert.Equal(status == HttpStatusCode.OK, error.MoveConfirmed);
            Assert.Equal(status != HttpStatusCode.OK, error.ErrorMetadataUnavailable);
            Assert.Equal(status, error.ResponseStatusCode);
            Assert.Equal(TimeSpan.FromSeconds(60), error.RetryAfter?.Delta);
            if (status != HttpStatusCode.OK)
            {
                Assert.Equal(status, error.StatusCode);
            }
            Assert.Equal(1, handler.CallCount);
            Assert.True(stream.Disposed);
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
    public static async Task Move_UnusableErrorJsonRetainsStatusAndRetryAfter(string json)
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(Response(json, HttpStatusCode.TooManyRequests, "60")));
        using var client = new HttpClient(handler);
        var error = await Assert.ThrowsAsync<FolderMoveRequestException>(() => MoveAsync(new FabricCoreService(client, Credential())));
        Assert.Equal(HttpStatusCode.TooManyRequests, error.StatusCode);
        Assert.Equal(TimeSpan.FromSeconds(60), error.RetryAfter?.Delta);
        Assert.True(error.ErrorMetadataUnavailable);
        Assert.Null(error.ErrorCode);
        Assert.False(error.MoveConfirmed);
        Assert.Equal(1, handler.CallCount);
    }

    [Theory]
    [InlineData(1048576, false, HttpStatusCode.OK)]
    [InlineData(1048577, false, HttpStatusCode.OK)]
    [InlineData(1048576, true, HttpStatusCode.OK)]
    [InlineData(1048577, true, HttpStatusCode.OK)]
    [InlineData(1048576, false, HttpStatusCode.TooManyRequests)]
    [InlineData(1048577, false, HttpStatusCode.TooManyRequests)]
    [InlineData(1048576, true, HttpStatusCode.TooManyRequests)]
    [InlineData(1048577, true, HttpStatusCode.TooManyRequests)]
    public static async Task Move_BoundsSuccessAndErrorBodiesExactly(long length, bool declaredLength, HttpStatusCode status)
    {
        var json = status == HttpStatusCode.OK
            ? Snapshot().Replace("Sales", "\u00e9\U0001F680", StringComparison.Ordinal)
            : """{"errorCode":"RequestBlocked"}""";
        if (status == HttpStatusCode.OK)
        {
            Assert.True(Encoding.UTF8.GetByteCount(json) > json.Length);
        }
        using var stream = new PaddedJsonTestStream(Encoding.UTF8.GetBytes(json), length);
        using var response = new HttpResponseMessage(status) { Content = new StreamContent(stream) };
        response.Content.Headers.ContentType = new("application/json");
        response.Headers.TryAddWithoutValidation("Retry-After", "0");
        if (declaredLength)
        {
            response.Content.Headers.ContentLength = length;
        }
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, Credential());
        if (length == 1048576 && status == HttpStatusCode.OK)
        {
            Assert.Equal("Moved", (await MoveAsync(service)).MoveStatus);
        }
        else
        {
            var error = await Assert.ThrowsAsync<FolderMoveRequestException>(() => MoveAsync(service));
            Assert.Equal(status == HttpStatusCode.OK ? HttpStatusCode.BadGateway : status, error.StatusCode);
            Assert.Equal(status == HttpStatusCode.OK, error.MoveConfirmed);
            Assert.Equal(TimeSpan.Zero, error.RetryAfter?.Delta);
            Assert.Equal(status != HttpStatusCode.OK && length > 1048576, error.ErrorMetadataUnavailable);
            Assert.Equal(status != HttpStatusCode.OK && length == 1048576 ? "RequestBlocked" : null, error.ErrorCode);
        }
        Assert.Equal(length > 1048576 && declaredLength ? 0 : length, stream.BytesRead);
        Assert.True(stream.Disposed);
        Assert.Equal(1, handler.CallCount);
    }

    [Fact]
    public static async Task Move_InvalidUtf8PreservesConfirmedMovement()
    {
        var bytes = Encoding.UTF8.GetBytes(Snapshot());
        bytes[Array.IndexOf(bytes, (byte)'S')] = 0xff;
        using var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(bytes) };
        response.Content.Headers.ContentType = new("application/json");
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
        using var client = new HttpClient(handler);
        var error = await Assert.ThrowsAsync<FolderMoveRequestException>(() => MoveAsync(new FabricCoreService(client, Credential())));
        Assert.True(error.MoveConfirmed);
        Assert.Equal(HttpStatusCode.BadGateway, error.StatusCode);
        Assert.Equal(1, handler.CallCount);
    }

    [Theory]
    [InlineData(64)]
    [InlineData(65)]
    public static async Task Move_BoundsUnknownJsonDepthBeforeProjectingSafeMetadata(int depth)
    {
        var json = Snapshot()[..^1] + ",\"extra\":" + new string('[', depth - 1) + "0" + new string(']', depth - 1) + "}";
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(Response(json)));
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, Credential());
        if (depth == 64)
        {
            Assert.Equal("Moved", (await MoveAsync(service)).MoveStatus);
        }
        else
        {
            Assert.True((await Assert.ThrowsAsync<FolderMoveRequestException>(() => MoveAsync(service))).MoveConfirmed);
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
    public static async Task Move_ReturnsOnlyValidatedRetryHints(string retryAfter)
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(Response(Snapshot(), retryAfter: retryAfter)));
        using var client = new HttpClient(handler);
        var result = await MoveAsync(new FabricCoreService(client, Credential()));
        Assert.Equal(retryAfter is "0" or "120" ? long.Parse(retryAfter) : null, result.RetryAfterSeconds);
        Assert.Equal(retryAfter.StartsWith("Tue", StringComparison.Ordinal), result.RetryAfterUtc.HasValue);
        Assert.Equal(1, handler.CallCount);
    }

    [Theory]
    [InlineData(HttpStatusCode.OK)]
    [InlineData(HttpStatusCode.TooManyRequests)]
    public static async Task Move_RejectsAmbiguousRetryHeadersOnSuccessAndFailure(HttpStatusCode status)
    {
        using var response = Response(status == HttpStatusCode.OK ? Snapshot() : """{"errorCode":"RequestBlocked"}""", status);
        response.Headers.TryAddWithoutValidation("Retry-After", ["1", "2"]);
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, Credential());
        if (status == HttpStatusCode.OK)
        {
            var result = await MoveAsync(service);
            Assert.Null(result.RetryAfterSeconds);
            Assert.Null(result.RetryAfterUtc);
        }
        else
        {
            var error = await Assert.ThrowsAsync<FolderMoveRequestException>(() => MoveAsync(service));
            Assert.Equal(status, error.StatusCode);
            Assert.Null(error.RetryAfter);
            Assert.Equal("RequestBlocked", error.ErrorCode);
        }
        Assert.Equal(1, handler.CallCount);
    }

    [Fact]
    public static async Task Move_IsolatesConcurrentCallerCredentialsAndDestinations()
    {
        var identity = new AsyncLocal<string?>();
        var credential = Credential();
        credential.GetTokenAsync(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>())
            .Returns(_ => new AccessToken(identity.Value!, DateTimeOffset.MaxValue));
        var contexts = Enumerable.Range(0, 12).ToDictionary(index => $"caller-{index}",
            index => (Workspace: Guid.NewGuid(), Folder: Guid.NewGuid(), Target: index % 2 == 0 ? Guid.NewGuid() : (Guid?)null));
        var observed = new ConcurrentBag<string>();
        using var handler = new FabricCoreHttpMessageHandler(async (request, token) =>
        {
            await Task.Yield();
            var caller = request.Headers.Authorization!.Parameter!;
            var context = contexts[caller];
            Assert.Equal(HttpMethod.Post, request.Method);
            Assert.Equal($"{FabricEndpoints.GetFabricApiBaseUrl()}/workspaces/{context.Workspace:D}/folders/{context.Folder:D}/move", request.RequestUri?.AbsoluteUri);
            var body = await request.Content!.ReadAsStringAsync(token);
            Assert.Equal(context.Target is { } target ? $"{{\"targetFolderId\":\"{target:D}\"}}" : "{}", body);
            observed.Add(caller);
            return Response(JsonSerializer.Serialize(new FabricFolder(context.Folder, caller, context.Workspace, context.Target),
                CoreJsonContext.Default.FabricFolder));
        });
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, credential);
        var results = await Task.WhenAll(Enumerable.Range(0, 12).Select(async index =>
        {
            identity.Value = $"caller-{index}";
            var context = contexts[identity.Value];
            var result = await service.MoveFolderAsync(context.Workspace.ToString(), context.Folder.ToString(), context.Target?.ToString(),
                TestContext.Current.CancellationToken);
            Assert.Equal(context.Workspace, result.Folder.WorkspaceId);
            Assert.Equal(context.Folder, result.Folder.Id);
            Assert.Equal(context.Target, result.Folder.ParentFolderId);
            return result;
        }));
        Assert.Equal(12, handler.CallCount);
        Assert.Equal(observed.Order(), results.Select(result => result.Folder.DisplayName).Order());
        Assert.Equal(12, observed.Distinct().Count());
    }

    [Fact]
    public static async Task Move_UncertainOutcomeDoesNotReadBackUntilAnExplicitGet()
    {
        using var handler = new FabricCoreHttpMessageHandler((request, _) =>
        {
            if (request.Method == HttpMethod.Post)
            {
                throw new HttpRequestException("private");
            }
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal($"{CollectionUrl}/{FolderId}", request.RequestUri?.AbsoluteUri);
            return Task.FromResult(Response(Snapshot(ParentFolderId)));
        });
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, Credential());
        var error = await Assert.ThrowsAsync<FolderMoveRequestException>(() => MoveAsync(service, ParentFolderId));
        Assert.Null(error.ResponseStatusCode);
        Assert.Equal(1, handler.CallCount);

        var snapshot = await service.GetFolderAsync(WorkspaceId, FolderId, TestContext.Current.CancellationToken);
        Assert.Equal(Guid.Parse(ParentFolderId), snapshot.Folder.ParentFolderId);
        Assert.Equal(2, handler.CallCount);
    }

    private static string Snapshot(string? parent = null, string name = "Sales") =>
        JsonSerializer.Serialize(Folder with { DisplayName = name, ParentFolderId = parent is null ? null : Guid.Parse(parent) },
            CoreJsonContext.Default.FabricFolder);

    private static Task<FolderMoveCommandResult> MoveAsync(FabricCoreService service, string? target = null) =>
        service.MoveFolderAsync(WorkspaceId, FolderId, target, TestContext.Current.CancellationToken);
}
