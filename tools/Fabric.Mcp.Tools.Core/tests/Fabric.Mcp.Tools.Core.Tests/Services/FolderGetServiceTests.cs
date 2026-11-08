// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
// cspell:words Retriable

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
using static Fabric.Mcp.Tools.Core.Tests.TestSupport.FolderListTestData;

namespace Fabric.Mcp.Tools.Core.Tests.Services;

public sealed class FolderGetServiceTests()
{
    [Fact]
    public static async Task Get_SendsOneNormalizedAuthenticatedGetAndProjectsOnlyFolderMetadata()
    {
        var json = JsonSerializer.SerializeToNode(Folder, CoreJsonContext.Default.FabricFolder)!.AsObject();
        json["privateExtension"] = new JsonObject { ["url"] = "https://example.invalid/private", ["items"] = new JsonArray("private") };
        using var stream = new OperationTestStream(Encoding.UTF8.GetBytes(json.ToJsonString()));
        using var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StreamContent(stream) };
        response.Content.Headers.ContentType = new("application/json");
        response.Headers.Location = new Uri("https://example.invalid/never-follow");
        response.Headers.TryAddWithoutValidation("x-ms-operation-id", FolderId);
        using var handler = new FabricCoreHttpMessageHandler((request, token) =>
        {
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal($"{CollectionUrl}/{FolderId}", request.RequestUri?.AbsoluteUri);
            Assert.Null(request.Content);
            Assert.Equal("Bearer", request.Headers.Authorization?.Scheme);
            Assert.Equal("folder-test-token", request.Headers.Authorization?.Parameter);
            Assert.Equal("Fabric Core MCP", request.Headers.UserAgent.ToString());
            Assert.True(token.CanBeCanceled);
            return Task.FromResult(response);
        });
        using var client = new HttpClient(handler);
        var credential = Credential();

        var result = await new FabricCoreService(client, credential).GetFolderAsync(
            Guid.Parse(WorkspaceId).ToString("B").ToUpperInvariant(),
            Guid.Parse(FolderId).ToString("N").ToUpperInvariant(), TestContext.Current.CancellationToken);

        Assert.Equal(Folder, result.Folder);
        var output = JsonSerializer.SerializeToElement(result, CoreJsonContext.Default.FolderGetCommandResult);
        Assert.Equal("folder", Assert.Single(output.EnumerateObject()).Name);
        Assert.Equal(["displayName", "id", "parentFolderId", "workspaceId"],
            output.GetProperty("folder").EnumerateObject().Select(property => property.Name).Order());
        Assert.DoesNotContain("private", output.GetRawText());
        Assert.Equal(1, handler.CallCount);
        Assert.True(stream.Disposed);
        Assert.Throws<ObjectDisposedException>(() => response.Content.ReadAsStream(TestContext.Current.CancellationToken));
        await credential.Received(1).GetTokenAsync(
            Arg.Is<TokenRequestContext>(context => context.Scopes.SequenceEqual(FabricEndpoints.FabricScopes)),
            TestContext.Current.CancellationToken);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public static async Task Get_PreservesAbsentOrNullParentWithoutInventingARelationship(bool explicitNull)
    {
        var json = JsonSerializer.SerializeToNode(Folder, CoreJsonContext.Default.FabricFolder)!.AsObject();
        json.Remove("parentFolderId");
        if (explicitNull)
        {
            json["parentFolderId"] = null;
        }
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(Response(json.ToJsonString())));
        using var client = new HttpClient(handler);
        var result = await ReadAsync(new FabricCoreService(client, Credential()));
        Assert.Null(result.Folder.ParentFolderId);
        var output = JsonSerializer.SerializeToElement(result, CoreJsonContext.Default.FolderGetCommandResult);
        Assert.False(output.GetProperty("folder").TryGetProperty("parentFolderId", out _));
        Assert.Equal(1, handler.CallCount);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(" Sales ")]
    [InlineData("recycled")]
    [InlineData("legacy/name.with:symbols")]
    [InlineData("\u00e9\U0001F680")]
    [InlineData("line\nbreak")]
    public static async Task Get_ReadsDisplayNamesWithoutApplyingCreationNamingPolicy(string name)
    {
        foreach (var displayName in new[] { name, name + new string('x', 256) })
        {
            var json = JsonSerializer.Serialize(Folder with { DisplayName = displayName }, CoreJsonContext.Default.FabricFolder);
            using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(Response(json)));
            using var client = new HttpClient(handler);
            var result = await ReadAsync(new FabricCoreService(client, Credential()));
            Assert.Equal(displayName, result.Folder.DisplayName);
            Assert.Equal(Guid.Parse(ParentFolderId), result.Folder.ParentFolderId);
            Assert.Equal(1, handler.CallCount);
        }
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("Sales")]
    [InlineData("00000000-0000-0000-0000-000000000000")]
    [InlineData("../private")]
    [InlineData(FolderId + "?query=private")]
    public static async Task Get_ValidatesEachIdBeforeAuthentication(string? value)
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) => throw new InvalidOperationException("No HTTP expected."));
        using var client = new HttpClient(handler);
        var credential = Credential();
        var service = new FabricCoreService(client, credential);
        await Assert.ThrowsAsync<ArgumentException>(() => service.GetFolderAsync(value!, FolderId, TestContext.Current.CancellationToken));
        await Assert.ThrowsAsync<ArgumentException>(() => service.GetFolderAsync(WorkspaceId, value!, TestContext.Current.CancellationToken));
        Assert.Empty(credential.ReceivedCalls());
        Assert.Equal(0, handler.CallCount);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public static async Task Get_CancellationBeforeSendMakesNoRequest(bool afterAuthentication)
    {
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        using var handler = new FabricCoreHttpMessageHandler((_, _) => throw new InvalidOperationException("No HTTP expected."));
        using var client = new HttpClient(handler);
        var credential = Credential();
        if (afterAuthentication)
        {
            credential.GetTokenAsync(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>()).Returns(_ =>
            {
                cancellation.Cancel();
                return new AccessToken("private-token", DateTimeOffset.MaxValue);
            });
        }
        else
        {
            cancellation.Cancel();
        }
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            new FabricCoreService(client, credential).GetFolderAsync(WorkspaceId, FolderId, cancellation.Token));
        Assert.Equal(afterAuthentication ? 1 : 0, credential.ReceivedCalls().Count());
        Assert.Equal(0, handler.CallCount);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public static async Task Get_AuthenticationFailureMakesNoRequest(bool unavailable)
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) => throw new InvalidOperationException("No HTTP expected."));
        using var client = new HttpClient(handler);
        var credential = Credential();
        AuthenticationFailedException failure = unavailable
            ? new CredentialUnavailableException("private")
            : new AuthenticationFailedException("private");
        credential.GetTokenAsync(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>())
            .Returns(ValueTask.FromException<AccessToken>(failure));
        Assert.Same(failure, await Assert.ThrowsAnyAsync<AuthenticationFailedException>(() => ReadAsync(new FabricCoreService(client, credential))));
        Assert.Single(credential.ReceivedCalls());
        Assert.Equal(0, handler.CallCount);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public static async Task Get_DoesNotRetrySendFailures(bool canceled)
    {
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        using var handler = new FabricCoreHttpMessageHandler((_, token) =>
        {
            if (canceled)
            {
                cancellation.Cancel();
                token.ThrowIfCancellationRequested();
            }
            throw new HttpRequestException("private");
        });
        using var client = new HttpClient(handler);
        var credential = Credential();
        var service = new FabricCoreService(client, credential);
        if (canceled)
        {
            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => service.GetFolderAsync(WorkspaceId, FolderId, cancellation.Token));
        }
        else
        {
            await Assert.ThrowsAsync<HttpRequestException>(() => ReadAsync(service));
        }
        Assert.Single(credential.ReceivedCalls());
        Assert.Equal(1, handler.CallCount);
    }

    [Fact]
    public static async Task Get_RejectsMissingWrongIdentityOrMalformedMetadataWithoutAPartialResult()
    {
        var valid = Snapshot();
        var invalid = new List<string>
        {
            "", "null", "[]", "{}", "\"private\"", "private", valid[..^1],
            valid.Replace(FolderId, ParentFolderId, StringComparison.Ordinal),
            valid.Replace(WorkspaceId, ParentFolderId, StringComparison.Ordinal),
            valid.Replace(ParentFolderId, Guid.Empty.ToString(), StringComparison.Ordinal),
            valid.Replace("\"Sales\"", "null", StringComparison.Ordinal),
            valid.Replace("\"Sales\"", "45", StringComparison.Ordinal),
            valid.Replace("\"Sales\"", "\"\\uD800\"", StringComparison.Ordinal),
            valid.Replace("\"Sales\"", "\"\\uDC00\"", StringComparison.Ordinal),
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
            using var response = Response(json, retryAfter: "60");
            using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
            using var client = new HttpClient(handler);
            var error = await Assert.ThrowsAsync<FolderGetRequestException>(() => ReadAsync(new FabricCoreService(client, Credential())));
            Assert.Equal(HttpStatusCode.BadGateway, error.StatusCode);
            Assert.Equal(TimeSpan.FromSeconds(60), error.RetryAfter?.Delta);
            Assert.DoesNotContain("private", error.Message);
            Assert.Equal(1, handler.CallCount);
            Assert.Throws<ObjectDisposedException>(() => response.Content.ReadAsStream(TestContext.Current.CancellationToken));
        }
    }

    [Theory]
    [InlineData(HttpStatusCode.Created)]
    [InlineData(HttpStatusCode.Accepted)]
    [InlineData(HttpStatusCode.NoContent)]
    [InlineData(HttpStatusCode.PartialContent)]
    public static async Task Get_RejectsUnexpectedSuccessWithoutReadingBodiesOrFollowingOperationHeaders(HttpStatusCode status)
    {
        using var stream = new OperationTestStream([1], (_, _) => throw new IOException("Unexpected body read."));
        using var response = new HttpResponseMessage(status) { Content = new StreamContent(stream) };
        response.Headers.Location = new Uri("https://example.invalid/private");
        response.Headers.TryAddWithoutValidation("x-ms-operation-id", FolderId);
        response.Headers.TryAddWithoutValidation("Retry-After", "60");
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
        using var client = new HttpClient(handler);
        var error = await Assert.ThrowsAsync<FolderGetRequestException>(() => ReadAsync(new FabricCoreService(client, Credential())));
        Assert.Equal(HttpStatusCode.BadGateway, error.StatusCode);
        Assert.Equal(TimeSpan.FromSeconds(60), error.RetryAfter?.Delta);
        Assert.Equal(0, stream.BytesRead);
        Assert.True(stream.Disposed);
        Assert.Equal(1, handler.CallCount);
    }

    [Theory]
    [InlineData(HttpStatusCode.BadRequest, "InvalidRequest")]
    [InlineData(HttpStatusCode.Unauthorized, "TokenExpired")]
    [InlineData(HttpStatusCode.Forbidden, "InsufficientPrivileges")]
    [InlineData(HttpStatusCode.NotFound, "FolderNotFound")]
    [InlineData(HttpStatusCode.TooManyRequests, "RequestBlocked")]
    [InlineData(HttpStatusCode.ServiceUnavailable, "ServiceUnavailable")]
    [InlineData(HttpStatusCode.Found, "UnexpectedRedirect")]
    [InlineData(HttpStatusCode.TemporaryRedirect, "UnexpectedRedirect")]
    public static async Task Get_PreservesFailureStatusAndOnlyProjectsASafeCode(HttpStatusCode status, string code)
    {
        var json = new JsonObject
        {
            ["errorCode"] = code,
            ["message"] = "private",
            ["requestId"] = "private",
            ["isRetriable"] = true,
            ["moreDetails"] = new JsonArray(new JsonObject { ["message"] = "private" }),
            ["parameters"] = new JsonArray("private"),
            ["relatedResource"] = new JsonObject { ["resourceId"] = "private" }
        };
        using var response = Response(json.ToJsonString(), status, "60");
        response.Headers.Location = new Uri("https://example.invalid/private");
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
        using var client = new HttpClient(handler);
        var error = await Assert.ThrowsAsync<FolderGetRequestException>(() => ReadAsync(new FabricCoreService(client, Credential())));
        Assert.Equal(status, error.StatusCode);
        Assert.Equal(code, error.ErrorCode);
        Assert.False(error.ErrorMetadataUnavailable);
        Assert.Equal(TimeSpan.FromSeconds(60), error.RetryAfter?.Delta);
        Assert.DoesNotContain("private", error.ToString());
        Assert.Equal(1, handler.CallCount);
        Assert.Throws<ObjectDisposedException>(() => response.Content.ReadAsStream(TestContext.Current.CancellationToken));
    }

    [Theory]
    [InlineData("""{"errorCode":"\uD800"}""")]
    [InlineData("""{"error\uD800Code":"FolderNotFound"}""")]
    [InlineData("""{"errorCode":"FolderNotFound","errorCode":"Other"}""")]
    [InlineData("""{"errorCode":"private invalid"}""")]
    [InlineData("""{"errorCode":null}""")]
    [InlineData("""{"errorCode":4}""")]
    [InlineData("private")]
    [InlineData("[]")]
    public static async Task Get_UnusableErrorJsonRetainsKnownStatusAndRetryAfter(string json)
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(Response(json, HttpStatusCode.TooManyRequests, "60")));
        using var client = new HttpClient(handler);
        var error = await Assert.ThrowsAsync<FolderGetRequestException>(() => ReadAsync(new FabricCoreService(client, Credential())));
        Assert.Equal(HttpStatusCode.TooManyRequests, error.StatusCode);
        Assert.Equal(TimeSpan.FromSeconds(60), error.RetryAfter?.Delta);
        Assert.True(error.ErrorMetadataUnavailable);
        Assert.Null(error.ErrorCode);
        Assert.Equal(1, handler.CallCount);
    }

    [Theory]
    [InlineData(HttpStatusCode.OK)]
    [InlineData(HttpStatusCode.TooManyRequests)]
    public static async Task Get_UnreadableBodiesKeepRetryGuidanceAndKnownErrorStatus(HttpStatusCode status)
    {
        foreach (var failure in new[] { "type", "missing-type", "io", "network", "cancellation", "timeout" })
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
            using var response = new HttpResponseMessage(status) { Content = new StreamContent(stream) };
            response.Content.Headers.ContentType = failure == "missing-type" ? null : new(failure == "type" ? "text/html" : "application/json");
            response.Headers.TryAddWithoutValidation("Retry-After", "60");
            using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
            using var client = new HttpClient(handler);
            var error = await Assert.ThrowsAsync<FolderGetRequestException>(() =>
                new FabricCoreService(client, Credential()).GetFolderAsync(WorkspaceId, FolderId, cancellation.Token));
            var expected = status != HttpStatusCode.OK ? status : failure switch
            {
                "type" or "missing-type" => HttpStatusCode.BadGateway,
                "cancellation" => HttpStatusCode.RequestTimeout,
                "timeout" => HttpStatusCode.GatewayTimeout,
                _ => HttpStatusCode.ServiceUnavailable
            };
            Assert.Equal(expected, error.StatusCode);
            Assert.Equal(TimeSpan.FromSeconds(60), error.RetryAfter?.Delta);
            Assert.Equal(status != HttpStatusCode.OK, error.ErrorMetadataUnavailable);
            Assert.True(stream.Disposed);
            Assert.Equal(1, handler.CallCount);
            Assert.DoesNotContain("private", error.ToString());
        }
    }

    [Theory]
    [InlineData(1048575, false)]
    [InlineData(1048576, false)]
    [InlineData(1048577, false)]
    [InlineData(1048575, true)]
    [InlineData(1048576, true)]
    [InlineData(1048577, true)]
    public static async Task Get_EnforcesExactSuccessAndErrorByteBudgets(long length, bool declaredLength)
    {
        foreach (var status in new[] { HttpStatusCode.OK, HttpStatusCode.TooManyRequests })
        {
            var json = status == HttpStatusCode.OK ? Snapshot() : """{"errorCode":"RequestBlocked"}""";
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
            if (status == HttpStatusCode.OK && length <= 1048576)
            {
                Assert.Equal(Folder, (await ReadAsync(service)).Folder);
            }
            else
            {
                var error = await Assert.ThrowsAsync<FolderGetRequestException>(() => ReadAsync(service));
                Assert.Equal(status == HttpStatusCode.OK ? HttpStatusCode.BadGateway : status, error.StatusCode);
                Assert.Equal(TimeSpan.Zero, error.RetryAfter?.Delta);
                if (status != HttpStatusCode.OK)
                {
                    Assert.Equal(length > 1048576, error.ErrorMetadataUnavailable);
                    Assert.Equal(length > 1048576 ? null : "RequestBlocked", error.ErrorCode);
                }
            }
            Assert.Equal(declaredLength && length > 1048576 ? 0 : length, stream.BytesRead);
            Assert.True(stream.Disposed);
            Assert.Equal(1, handler.CallCount);
        }
    }

    [Theory]
    [InlineData(64)]
    [InlineData(65)]
    public static async Task Get_BoundsIgnoredJsonDepthBeforeProjectingSafeMetadata(int depth)
    {
        foreach (var status in new[] { HttpStatusCode.OK, HttpStatusCode.TooManyRequests })
        {
            var body = status == HttpStatusCode.OK ? Snapshot() : """{"errorCode":"RequestBlocked"}""";
            var json = body[..^1] + ",\"extra\":" + new string('[', depth - 1) + "0" + new string(']', depth - 1) + "}";
            using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(Response(json, status, "60")));
            using var client = new HttpClient(handler);
            var service = new FabricCoreService(client, Credential());
            if (status == HttpStatusCode.OK && depth == 64)
            {
                Assert.Equal(Folder, (await ReadAsync(service)).Folder);
            }
            else
            {
                var error = await Assert.ThrowsAsync<FolderGetRequestException>(() => ReadAsync(service));
                Assert.Equal(status == HttpStatusCode.OK ? HttpStatusCode.BadGateway : status, error.StatusCode);
                Assert.Equal(TimeSpan.FromSeconds(60), error.RetryAfter?.Delta);
                Assert.Equal(status != HttpStatusCode.OK && depth > 64, error.ErrorMetadataUnavailable);
                Assert.Equal(status != HttpStatusCode.OK && depth == 64 ? "RequestBlocked" : null, error.ErrorCode);
            }
            Assert.Equal(1, handler.CallCount);
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    public static async Task Get_CountsUtf8BytesNotCharactersEvenWhenContentLengthUnderstatesTheBody(int extraBytes)
    {
        var prefix = Snapshot()[..^1] + ",\"extra\":\"";
        const string suffix = "\"}";
        var remaining = 1048576 + extraBytes - Encoding.UTF8.GetByteCount(prefix + suffix);
        var json = prefix + new string('\u00e9', remaining / 2) + (remaining % 2 == 0 ? "" : "x") + suffix;
        var bytes = Encoding.UTF8.GetBytes(json);
        Assert.Equal(1048576 + extraBytes, bytes.Length);
        Assert.True(json.Length < 1048576);
        using var stream = new OperationTestStream(bytes);
        using var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StreamContent(stream) };
        response.Content.Headers.ContentType = new("application/json");
        response.Content.Headers.ContentLength = 1;
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, Credential());
        if (extraBytes == 0)
        {
            Assert.Equal(Folder, (await ReadAsync(service)).Folder);
        }
        else
        {
            Assert.Equal(HttpStatusCode.BadGateway, (await Assert.ThrowsAsync<FolderGetRequestException>(() => ReadAsync(service))).StatusCode);
        }
        Assert.Equal(bytes.Length, stream.BytesRead);
        Assert.True(stream.Disposed);
        Assert.Equal(1, handler.CallCount);
    }

    [Fact]
    public static async Task Get_RejectsInvalidUtf8WithoutReturningReplacementCharacters()
    {
        var bytes = Encoding.UTF8.GetBytes(Snapshot());
        bytes[Array.IndexOf(bytes, (byte)'S')] = 0xff;
        using var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(bytes) };
        response.Content.Headers.ContentType = new("application/json");
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
        using var client = new HttpClient(handler);
        var error = await Assert.ThrowsAsync<FolderGetRequestException>(() => ReadAsync(new FabricCoreService(client, Credential())));
        Assert.Equal(HttpStatusCode.BadGateway, error.StatusCode);
        Assert.Equal(1, handler.CallCount);
    }

    [Theory]
    [InlineData("0")]
    [InlineData("120")]
    [InlineData("Tue, 01 Jan 2030 00:00:00 GMT")]
    [InlineData("-1")]
    [InlineData("2147483648")]
    [InlineData("private")]
    [InlineData("1, 2")]
    [InlineData(null)]
    public static async Task Get_ReturnsOnlyValidatedRetryHints(string? retryAfter)
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(Response(Snapshot(), retryAfter: retryAfter)));
        using var client = new HttpClient(handler);
        var result = await ReadAsync(new FabricCoreService(client, Credential()));
        Assert.Equal(retryAfter is "0" or "120" ? long.Parse(retryAfter) : null, result.RetryAfterSeconds);
        Assert.Equal(retryAfter?.StartsWith("Tue", StringComparison.Ordinal) == true, result.RetryAfterUtc.HasValue);
        Assert.Equal(1, handler.CallCount);
    }

    [Fact]
    public static async Task Get_IsolatesConcurrentCallerCredentialsAndResourceIds()
    {
        var identity = new AsyncLocal<string?>();
        var credential = Credential();
        credential.GetTokenAsync(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>())
            .Returns(_ => new AccessToken(identity.Value!, DateTimeOffset.MaxValue));
        var contexts = Enumerable.Range(0, 12).ToDictionary(index => $"caller-{index}",
            index => new FabricFolder(Guid.NewGuid(), $"caller-{index}", Guid.NewGuid(), index % 2 == 0 ? Guid.NewGuid() : null));
        var observed = new ConcurrentBag<string>();
        using var handler = new FabricCoreHttpMessageHandler(async (request, _) =>
        {
            await Task.Yield();
            var caller = request.Headers.Authorization!.Parameter!;
            var folder = contexts[caller];
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Null(request.Content);
            Assert.Equal($"{FabricEndpoints.GetFabricApiBaseUrl()}/workspaces/{folder.WorkspaceId:D}/folders/{folder.Id:D}", request.RequestUri?.AbsoluteUri);
            observed.Add(caller);
            return Response(JsonSerializer.Serialize(folder, CoreJsonContext.Default.FabricFolder));
        });
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, credential);
        var results = await Task.WhenAll(contexts.Select(async pair =>
        {
            identity.Value = pair.Key;
            var folder = pair.Value;
            var result = await service.GetFolderAsync(folder.WorkspaceId.ToString(), folder.Id.ToString(), TestContext.Current.CancellationToken);
            Assert.Equal(folder, result.Folder);
            return result;
        }));
        Assert.Equal(12, handler.CallCount);
        Assert.Equal(observed.Order(), results.Select(result => result.Folder.DisplayName).Order());
        Assert.Equal(12, observed.Distinct().Count());
        Assert.Null(client.DefaultRequestHeaders.Authorization);
    }

    private static string Snapshot() => JsonSerializer.Serialize(Folder, CoreJsonContext.Default.FabricFolder);

    private static Task<FolderGetCommandResult> ReadAsync(FabricCoreService service) =>
        service.GetFolderAsync(WorkspaceId, FolderId, TestContext.Current.CancellationToken);
}
