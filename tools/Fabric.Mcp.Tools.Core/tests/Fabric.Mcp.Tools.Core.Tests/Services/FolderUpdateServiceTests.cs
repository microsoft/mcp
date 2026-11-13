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
using static Fabric.Mcp.Tools.Core.Tests.TestSupport.FolderCreateTestData;
using static Fabric.Mcp.Tools.Core.Tests.TestSupport.FolderListTestData;

namespace Fabric.Mcp.Tools.Core.Tests.Services;

public sealed class FolderUpdateServiceTests()
{
    [Theory]
    [InlineData(null, false)]
    [InlineData(null, true)]
    [InlineData(ParentFolderId, false)]
    public static async Task Update_SendsExactlyOnePatchAndReturnsOnlyObservedMetadata(string? parent, bool explicitNull)
    {
        const string name = "Sales \u00e9\U0001F680";
        var json = Snapshot(parent, name)[..^1] + ",\"extra\":\"private\"" + (explicitNull ? ",\"parentFolderId\":null}" : "}");
        using var stream = new OperationTestStream(Encoding.UTF8.GetBytes(json));
        using var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StreamContent(stream) };
        response.Content.Headers.ContentType = new("application/json");
        response.Headers.TryAddWithoutValidation("Location", ["https://example.invalid/never-follow", "/v1/operations/unused"]);
        response.Headers.TryAddWithoutValidation("x-ms-operation-id", "private");
        response.Headers.TryAddWithoutValidation("Retry-After", "0");
        HttpContent? requestContent = null;
        using var handler = new FabricCoreHttpMessageHandler(async (request, token) =>
        {
            Assert.Equal(HttpMethod.Patch, request.Method);
            Assert.Equal($"{CollectionUrl}/{FolderId}", request.RequestUri?.AbsoluteUri);
            Assert.Equal("application/json", request.Content?.Headers.ContentType?.MediaType);
            Assert.Equal("Bearer", request.Headers.Authorization?.Scheme);
            Assert.Equal("folder-test-token", request.Headers.Authorization?.Parameter);
            Assert.Equal("Fabric Core MCP", request.Headers.UserAgent.ToString());
            requestContent = request.Content;
            using var body = JsonDocument.Parse(await request.Content!.ReadAsStringAsync(token));
            Assert.Equal("displayName", Assert.Single(body.RootElement.EnumerateObject()).Name);
            Assert.Equal(name, body.RootElement.GetProperty("displayName").GetString());
            return response;
        });
        using var client = new HttpClient(handler);
        var credential = Credential();

        var result = await new FabricCoreService(client, credential).UpdateFolderAsync(
            $"{{{WorkspaceId.ToUpperInvariant()}}}", Guid.Parse(FolderId).ToString("N").ToUpperInvariant(), name,
            TestContext.Current.CancellationToken);

        Assert.Equal(Guid.Parse(WorkspaceId), result.Folder.WorkspaceId);
        Assert.Equal(Guid.Parse(FolderId), result.Folder.Id);
        Assert.Equal(parent is null ? null : Guid.Parse(parent), result.Folder.ParentFolderId);
        Assert.Equal(name, result.Folder.DisplayName);
        Assert.Equal("Updated", result.UpdateStatus);
        Assert.Equal(0, result.RetryAfterSeconds);
        var output = JsonSerializer.SerializeToElement(result, CoreJsonContext.Default.FolderUpdateCommandResult);
        Assert.DoesNotContain("private", output.GetRawText());
        Assert.Equal(parent is not null, output.GetProperty("folder").TryGetProperty("parentFolderId", out _));
        Assert.Equal(1, handler.CallCount);
        Assert.True(stream.Disposed);
        Assert.Throws<ObjectDisposedException>(() => response.Content.ReadAsStream(TestContext.Current.CancellationToken));
        Assert.Throws<ObjectDisposedException>(() => requestContent!.ReadAsStream(TestContext.Current.CancellationToken));
        await credential.Received(1).GetTokenAsync(
            Arg.Is<TokenRequestContext>(context => context.Scopes.SequenceEqual(FabricEndpoints.FabricScopes)), TestContext.Current.CancellationToken);
    }

    [Fact]
    public static async Task Update_ReturnsTheServerSnapshotNotSynthesizedRequestMetadata()
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(Response(Snapshot(ParentFolderId, "Server snapshot"))));
        using var client = new HttpClient(handler);
        var result = await UpdateAsync(new FabricCoreService(client, Credential()));
        Assert.Equal("Server snapshot", result.Folder.DisplayName);
        Assert.Equal(Guid.Parse(ParentFolderId), result.Folder.ParentFolderId);
        Assert.Equal(1, handler.CallCount);
    }

    [Fact]
    public static async Task Update_RejectsInvalidInputsBeforeAuthentication()
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) => throw new InvalidOperationException("No HTTP expected."));
        using var client = new HttpClient(handler);
        var credential = Credential();
        var service = new FabricCoreService(client, credential);
        foreach (var name in InvalidNames.Append(null!))
        {
            await Assert.ThrowsAsync<FabricItemInputException>(() => service.UpdateFolderAsync(
                WorkspaceId, FolderId, name, TestContext.Current.CancellationToken));
        }
        foreach (var id in new[] { null!, "", "private-invalid", Guid.Empty.ToString(), "../private" })
        {
            await Assert.ThrowsAsync<FabricItemInputException>(() => service.UpdateFolderAsync(id, FolderId, "Sales", TestContext.Current.CancellationToken));
            await Assert.ThrowsAsync<FabricItemInputException>(() => service.UpdateFolderAsync(WorkspaceId, id, "Sales", TestContext.Current.CancellationToken));
        }
        Assert.Empty(credential.ReceivedCalls());
        Assert.Equal(0, handler.CallCount);
    }

    [Theory]
    [InlineData("credentials", HttpStatusCode.Unauthorized)]
    [InlineData("authentication", HttpStatusCode.Unauthorized)]
    [InlineData("operation-canceled", HttpStatusCode.RequestTimeout)]
    [InlineData("timeout", HttpStatusCode.GatewayTimeout)]
    public static async Task Update_AuthenticationFailuresAreKnownNotSent(string failure, HttpStatusCode expected)
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) => throw new InvalidOperationException("No HTTP expected."));
        using var client = new HttpClient(handler);
        var credential = Credential();
        credential.GetTokenAsync(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>())
            .Returns<ValueTask<AccessToken>>(_ => throw FabricCoreErrorTestData.CreateException(failure));
        var error = await Assert.ThrowsAsync<FolderUpdateRequestException>(() => UpdateAsync(new FabricCoreService(client, credential)));
        Assert.Equal(expected, error.StatusCode);
        Assert.True(error.RequestNotSent);
        Assert.False(error.UpdateConfirmed);
        Assert.Null(error.ResponseStatusCode);
        Assert.Equal(0, handler.CallCount);
        Assert.DoesNotContain("private", error.Message);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public static async Task Update_CancellationBeforeSendNeverClaimsPossibleUpdate(bool duringAuthentication)
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
        var error = await Assert.ThrowsAsync<FolderUpdateRequestException>(() =>
            new FabricCoreService(client, credential).UpdateFolderAsync(WorkspaceId, FolderId, "Sales", cancellation.Token));
        Assert.True(error.RequestNotSent);
        Assert.False(error.UpdateConfirmed);
        Assert.Equal(HttpStatusCode.RequestTimeout, error.StatusCode);
        Assert.Equal(duringAuthentication ? 1 : 0, credential.ReceivedCalls().Count());
        Assert.Equal(0, handler.CallCount);
    }

    [Theory]
    [InlineData("network", HttpStatusCode.ServiceUnavailable)]
    [InlineData("io", HttpStatusCode.ServiceUnavailable)]
    [InlineData("operation-canceled", HttpStatusCode.RequestTimeout)]
    [InlineData("timeout", HttpStatusCode.GatewayTimeout)]
    public static async Task Update_SendFailuresRemainUncertainWithoutRetry(string failure, HttpStatusCode expected)
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) => throw failure switch
        {
            "network" => new HttpRequestException("private"),
            "io" => new IOException("private"),
            _ => FabricCoreErrorTestData.CreateException(failure)
        });
        using var client = new HttpClient(handler);
        var error = await Assert.ThrowsAsync<FolderUpdateRequestException>(() => UpdateAsync(new FabricCoreService(client, Credential())));
        Assert.Equal(expected, error.StatusCode);
        Assert.False(error.RequestNotSent);
        Assert.False(error.UpdateConfirmed);
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
    [InlineData(HttpStatusCode.Conflict, HttpStatusCode.Conflict)]
    [InlineData(HttpStatusCode.TooManyRequests, HttpStatusCode.TooManyRequests)]
    [InlineData(HttpStatusCode.ServiceUnavailable, HttpStatusCode.ServiceUnavailable)]
    public static async Task Update_Requires200AndNeverFollowsUrlsRetriesOrChoosesAnotherName(HttpStatusCode status, HttpStatusCode expected)
    {
        using var response = Response("""{"errorCode":"FolderDisplayNameAlreadyInUse","message":"private","isRetriable":true}""", status, "60");
        response.Headers.Location = new Uri(CollectionUrl + "/" + FolderId);
        response.Headers.TryAddWithoutValidation("x-ms-operation-id", FolderId);
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
        using var client = new HttpClient(handler);
        var error = await Assert.ThrowsAsync<FolderUpdateRequestException>(() => UpdateAsync(new FabricCoreService(client, Credential())));
        Assert.Equal(expected, error.StatusCode);
        Assert.Equal(status, error.ResponseStatusCode);
        Assert.Equal(response.IsSuccessStatusCode ? null : "FolderDisplayNameAlreadyInUse", error.ErrorCode);
        Assert.Equal(TimeSpan.FromSeconds(60), error.RetryAfter?.Delta);
        Assert.False(error.UpdateConfirmed);
        Assert.False(error.RequestNotSent);
        Assert.Equal(1, handler.CallCount);
        Assert.Throws<ObjectDisposedException>(() => response.Content.ReadAsStream(TestContext.Current.CancellationToken));
    }

    [Fact]
    public static async Task Update_RejectsMalformedOrMismatchedMetadataButPreservesConfirmedUpdate()
    {
        var valid = Snapshot(ParentFolderId);
        var invalid = new List<string>
        {
            "", "null", "[]", "{}", "private", valid[..^1],
            valid.Replace(FolderId, ParentFolderId, StringComparison.Ordinal),
            valid.Replace(WorkspaceId, FolderId, StringComparison.Ordinal),
            valid.Replace("\"Sales\"", "null", StringComparison.Ordinal),
            valid.Replace("\"Sales\"", "\"\"", StringComparison.Ordinal),
            valid.Replace("\"Sales\"", "\" \"", StringComparison.Ordinal),
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
            var error = await Assert.ThrowsAsync<FolderUpdateRequestException>(() => UpdateAsync(new FabricCoreService(client, Credential())));
            Assert.True(error.UpdateConfirmed);
            Assert.False(error.RequestNotSent);
            Assert.Equal(HttpStatusCode.BadGateway, error.StatusCode);
            Assert.Equal(HttpStatusCode.OK, error.ResponseStatusCode);
            Assert.Equal(TimeSpan.FromSeconds(60), error.RetryAfter?.Delta);
            Assert.Equal(1, handler.CallCount);
        }
    }

    [Theory]
    [InlineData(HttpStatusCode.OK)]
    [InlineData(HttpStatusCode.TooManyRequests)]
    [InlineData(HttpStatusCode.Conflict)]
    public static async Task Update_UnavailableBodiesPreserveConfirmationOrOriginalFailure(HttpStatusCode status)
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
            var error = await Assert.ThrowsAsync<FolderUpdateRequestException>(() => new FabricCoreService(client, Credential())
                .UpdateFolderAsync(WorkspaceId, FolderId, "Sales", cancellation.Token));
            Assert.Equal(status == HttpStatusCode.OK, error.UpdateConfirmed);
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
    public static async Task Update_UnusableErrorJsonRetainsStatusAndRetryAfter(string json)
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(Response(json, HttpStatusCode.TooManyRequests, "60")));
        using var client = new HttpClient(handler);
        var error = await Assert.ThrowsAsync<FolderUpdateRequestException>(() => UpdateAsync(new FabricCoreService(client, Credential())));
        Assert.Equal(HttpStatusCode.TooManyRequests, error.StatusCode);
        Assert.Equal(TimeSpan.FromSeconds(60), error.RetryAfter?.Delta);
        Assert.True(error.ErrorMetadataUnavailable);
        Assert.Null(error.ErrorCode);
        Assert.False(error.UpdateConfirmed);
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
    public static async Task Update_BoundsSuccessAndErrorBodiesExactly(long length, bool declaredLength, HttpStatusCode status)
    {
        var json = status == HttpStatusCode.OK ? Snapshot(name: "\u00e9\U0001F680") : """{"errorCode":"RequestBlocked"}""";
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
            Assert.Equal("Updated", (await UpdateAsync(service)).UpdateStatus);
        }
        else
        {
            var error = await Assert.ThrowsAsync<FolderUpdateRequestException>(() => UpdateAsync(service));
            Assert.Equal(status == HttpStatusCode.OK ? HttpStatusCode.BadGateway : status, error.StatusCode);
            Assert.Equal(status == HttpStatusCode.OK, error.UpdateConfirmed);
            Assert.Equal(TimeSpan.Zero, error.RetryAfter?.Delta);
            Assert.Equal(status != HttpStatusCode.OK && length > 1048576, error.ErrorMetadataUnavailable);
            Assert.Equal(status != HttpStatusCode.OK && length == 1048576 ? "RequestBlocked" : null, error.ErrorCode);
        }
        Assert.Equal(length > 1048576 && declaredLength ? 0 : length, stream.BytesRead);
        Assert.True(stream.Disposed);
        Assert.Equal(1, handler.CallCount);
    }

    [Fact]
    public static async Task Update_InvalidUtf8PreservesConfirmedUpdate()
    {
        var bytes = Encoding.UTF8.GetBytes(Snapshot());
        bytes[Array.IndexOf(bytes, (byte)'S')] = 0xff;
        using var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(bytes) };
        response.Content.Headers.ContentType = new("application/json");
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
        using var client = new HttpClient(handler);
        var error = await Assert.ThrowsAsync<FolderUpdateRequestException>(() => UpdateAsync(new FabricCoreService(client, Credential())));
        Assert.True(error.UpdateConfirmed);
        Assert.Equal(HttpStatusCode.BadGateway, error.StatusCode);
        Assert.Equal(1, handler.CallCount);
    }

    [Theory]
    [InlineData(64)]
    [InlineData(65)]
    public static async Task Update_BoundsUnknownJsonDepthBeforeProjectingSafeMetadata(int depth)
    {
        var json = Snapshot()[..^1] + ",\"extra\":" + new string('[', depth - 1) + "0" + new string(']', depth - 1) + "}";
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(Response(json)));
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, Credential());
        if (depth == 64)
        {
            Assert.Equal("Updated", (await UpdateAsync(service)).UpdateStatus);
        }
        else
        {
            Assert.True((await Assert.ThrowsAsync<FolderUpdateRequestException>(() => UpdateAsync(service))).UpdateConfirmed);
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
    public static async Task Update_ReturnsOnlyValidatedRetryHints(string retryAfter)
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(Response(Snapshot(), retryAfter: retryAfter)));
        using var client = new HttpClient(handler);
        var result = await UpdateAsync(new FabricCoreService(client, Credential()));
        Assert.Equal(retryAfter is "0" or "120" ? long.Parse(retryAfter) : null, result.RetryAfterSeconds);
        Assert.Equal(retryAfter.StartsWith("Tue", StringComparison.Ordinal), result.RetryAfterUtc.HasValue);
        Assert.Equal(1, handler.CallCount);
    }

    [Fact]
    public static async Task Update_IsolatesConcurrentCallerCredentialsAndInputs()
    {
        var identity = new AsyncLocal<string?>();
        var credential = Credential();
        credential.GetTokenAsync(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>())
            .Returns(_ => new AccessToken(identity.Value!, DateTimeOffset.MaxValue));
        var contexts = Enumerable.Range(0, 12).ToDictionary(index => $"caller-{index}",
            index => (Workspace: Guid.NewGuid(), Folder: Guid.NewGuid(), Parent: index % 2 == 0 ? Guid.NewGuid() : (Guid?)null));
        var observed = new ConcurrentBag<string>();
        using var handler = new FabricCoreHttpMessageHandler(async (request, token) =>
        {
            await Task.Yield();
            using var body = JsonDocument.Parse(await request.Content!.ReadAsStringAsync(token));
            var name = body.RootElement.GetProperty("displayName").GetString()!;
            Assert.Equal(name, request.Headers.Authorization?.Parameter);
            var context = contexts[name];
            Assert.Equal(HttpMethod.Patch, request.Method);
            Assert.Equal($"{FabricEndpoints.GetFabricApiBaseUrl()}/workspaces/{context.Workspace:D}/folders/{context.Folder:D}", request.RequestUri?.AbsoluteUri);
            Assert.Equal("displayName", Assert.Single(body.RootElement.EnumerateObject()).Name);
            observed.Add(name);
            return Response(JsonSerializer.Serialize(new FabricFolder(context.Folder, name, context.Workspace, context.Parent),
                CoreJsonContext.Default.FabricFolder));
        });
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, credential);
        var results = await Task.WhenAll(Enumerable.Range(0, 12).Select(async index =>
        {
            identity.Value = $"caller-{index}";
            var context = contexts[identity.Value];
            var result = await service.UpdateFolderAsync(context.Workspace.ToString(), context.Folder.ToString(), identity.Value,
                TestContext.Current.CancellationToken);
            Assert.Equal(context.Workspace, result.Folder.WorkspaceId);
            Assert.Equal(context.Folder, result.Folder.Id);
            Assert.Equal(context.Parent, result.Folder.ParentFolderId);
            return result;
        }));
        Assert.Equal(12, handler.CallCount);
        Assert.Equal(observed.Order(), results.Select(result => result.Folder.DisplayName).Order());
        Assert.Equal(12, observed.Distinct().Count());
    }

    private static string Snapshot(string? parent = null, string name = "Sales") =>
        JsonSerializer.Serialize(Folder with { DisplayName = name, ParentFolderId = parent is null ? null : Guid.Parse(parent) },
            CoreJsonContext.Default.FabricFolder);

    private static Task<FolderUpdateCommandResult> UpdateAsync(FabricCoreService service) =>
        service.UpdateFolderAsync(WorkspaceId, FolderId, "Sales", TestContext.Current.CancellationToken);
}
