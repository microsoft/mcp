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
using static Fabric.Mcp.Tools.Core.Tests.TestSupport.ItemMoveTestData;

namespace Fabric.Mcp.Tools.Core.Tests.Services;

public sealed class ItemMoveServiceTests()
{
    [Theory]
    [InlineData(null)]
    [InlineData(FolderId)]
    [InlineData(ItemId)]
    public static async Task Move_OnePostPreservesParentAndChildMetadataWithoutAFolderSelfTargetRule(string? target)
    {
        using var stream = new OperationTestStream(Encoding.UTF8.GetBytes(Collection(target)));
        using var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StreamContent(stream) };
        response.Content.Headers.ContentType = new("application/json");
        response.Headers.TryAddWithoutValidation("Location", ["https://example.invalid/never-follow", "/v1/operations/unused"]);
        response.Headers.TryAddWithoutValidation("x-ms-operation-id", "private");
        response.Headers.TryAddWithoutValidation("Retry-After", "0");
        HttpContent? requestContent = null;
        using var handler = new FabricCoreHttpMessageHandler(async (request, token) =>
        {
            Assert.Equal(HttpMethod.Post, request.Method);
            Assert.Equal(ItemUrl + "/move", request.RequestUri?.AbsoluteUri);
            Assert.Equal("application/json", request.Content?.Headers.ContentType?.MediaType);
            Assert.Equal("Bearer", request.Headers.Authorization?.Scheme);
            Assert.Equal("item-move-test-token", request.Headers.Authorization?.Parameter);
            Assert.Equal("Fabric Core MCP", request.Headers.UserAgent.ToString());
            Assert.True(token.CanBeCanceled);
            requestContent = request.Content;
            Assert.Equal(target is null ? "{}" : $"{{\"targetFolderId\":\"{target}\"}}", await request.Content!.ReadAsStringAsync(token));
            return response;
        });
        using var client = new HttpClient(handler);
        var credential = Credential();

        var result = await new FabricCoreService(client, credential).MoveItemAsync(
            $"{{{WorkspaceId.ToUpperInvariant()}}}", Guid.Parse(ItemId).ToString("N").ToUpperInvariant(),
            target is null ? null : Guid.Parse(target).ToString("B").ToUpperInvariant(), TestContext.Current.CancellationToken);

        Assert.Equal([Guid.Parse(ItemId), Guid.Parse(ChildId)], result.Items.Select(item => item.Id));
        Assert.Equal(["Lakehouse", "SQLEndpoint"], result.Items.Select(item => item.Type));
        Assert.All(result.Items, item =>
        {
            Assert.Equal(Guid.Parse(WorkspaceId), item.WorkspaceId);
            Assert.Equal(target is null ? null : Guid.Parse(target), item.FolderId);
        });
        Assert.Equal(Item(target), result.Items[0]);
        Assert.Null(result.Items[1].Description);
        Assert.Equal("Moved", result.MoveStatus);
        Assert.Equal(0, result.RetryAfterSeconds);
        Assert.Equal(1, handler.CallCount);
        Assert.True(stream.Disposed);
        Assert.Throws<ObjectDisposedException>(() => response.Content.ReadAsStream(TestContext.Current.CancellationToken));
        Assert.Throws<ObjectDisposedException>(() => requestContent!.ReadAsStream(TestContext.Current.CancellationToken));
        await credential.Received(1).GetTokenAsync(
            Arg.Is<TokenRequestContext>(context => context.Scopes.SequenceEqual(FabricEndpoints.FabricScopes)), TestContext.Current.CancellationToken);
    }

    [Theory]
    [InlineData("""{"value":[]}""", 0)]
    [InlineData("""{"value":[{"type":"FutureItem"}]}""", 1)]
    [InlineData("""{"value":[{"type":"FutureItem","id":null,"displayName":null,"description":null,"workspaceId":null,"folderId":null,"logicalId":null,"tags":null,"sensitivityLabel":null}]}""", 1)]
    public static async Task Move_AcceptsSchemaPermittedEmptyAndSparseCollectionsWithoutInventingMetadata(string json, int count)
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(Response(json)));
        using var client = new HttpClient(handler);
        var result = await MoveAsync(new FabricCoreService(client, Credential()), FolderId);
        Assert.Equal(count, result.Items.Count);
        Assert.All(result.Items, item => Assert.Equal(new FabricMovedItem("FutureItem"), item));
        Assert.DoesNotContain(ItemId, JsonSerializer.Serialize(result, CoreJsonContext.Default.ItemMoveCommandResult));
        Assert.Equal(1, handler.CallCount);
    }

    [Fact]
    public static async Task Move_PreservesOrderDuplicateRowsFutureTypesAndApprovedMetadataOnly()
    {
        var item = Item(FolderId) with
        {
            Type = "FutureWorkload",
            DisplayName = " legacy /\u00e9\U0001F680 ",
            Description = "",
            LogicalId = Guid.Parse(ChildId),
            Tags = [new(Guid.Parse(FolderId), "tag")],
            SensitivityLabel = new(Guid.Parse(WorkspaceId))
        };
        var snapshot = JsonSerializer.Serialize(item, CoreJsonContext.Default.FabricMovedItem);
        var extra = snapshot[..^1] + ",\"defaultIdentity\":{\"userDetails\":{\"userPrincipalName\":\"private\"}},\"definition\":\"private\",\"extra\":\"private\"}";
        var child = JsonSerializer.Serialize(Item(FolderId) with { Id = Guid.Parse(ChildId), Type = "SQLEndpoint" },
            CoreJsonContext.Default.FabricMovedItem);
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(Response($"{{\"value\":[{child},{extra},{extra}]}}")));
        using var client = new HttpClient(handler);

        var result = await MoveAsync(new FabricCoreService(client, Credential()), FolderId);

        Assert.Equal([Guid.Parse(ChildId), Guid.Parse(ItemId), Guid.Parse(ItemId)], result.Items.Select(value => value.Id));
        Assert.Equal(item.Type, result.Items[1].Type);
        Assert.Equal(item.DisplayName, result.Items[1].DisplayName);
        Assert.Equal("", result.Items[1].Description);
        Assert.Equal(item.LogicalId, result.Items[1].LogicalId);
        Assert.Equal(item.Tags, result.Items[1].Tags);
        Assert.Equal(item.SensitivityLabel, result.Items[1].SensitivityLabel);
        var output = JsonSerializer.SerializeToElement(result, CoreJsonContext.Default.ItemMoveCommandResult);
        Assert.Equal(["guidance", "items", "moveStatus"], output.EnumerateObject().Select(property => property.Name).Order());
        Assert.DoesNotContain("private", output.GetRawText());
        Assert.DoesNotContain("defaultIdentity", output.GetRawText());
        Assert.Equal(1, handler.CallCount);
    }

    [Fact]
    public static async Task Move_DoesNotRequireTheRequestedItemToBeIdentifiedOrImposeARowLimit()
    {
        var child = JsonSerializer.Serialize(Item() with { Id = Guid.Parse(ChildId) }, CoreJsonContext.Default.FabricMovedItem);
        var json = "{\"value\":[" + string.Join(',', Enumerable.Repeat(child, 1001)) + "]}";
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(Response(json)));
        using var client = new HttpClient(handler);
        var result = await MoveAsync(new FabricCoreService(client, Credential()));
        Assert.Equal(1001, result.Items.Count);
        Assert.All(result.Items, item => Assert.Equal(Guid.Parse(ChildId), item.Id));
        Assert.Equal(1, handler.CallCount);
    }

    [Fact]
    public static async Task Move_RejectsInvalidInputsBeforeAuthentication()
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) => throw new InvalidOperationException("No HTTP expected."));
        using var client = new HttpClient(handler);
        var credential = Credential();
        var service = new FabricCoreService(client, credential);
        foreach (var id in new[] { null!, "", " ", "private-invalid", Guid.Empty.ToString(), "../private" })
        {
            await Assert.ThrowsAsync<FabricItemInputException>(() => service.MoveItemAsync(id, ItemId, cancellationToken: TestContext.Current.CancellationToken));
            await Assert.ThrowsAsync<FabricItemInputException>(() => service.MoveItemAsync(WorkspaceId, id, cancellationToken: TestContext.Current.CancellationToken));
            if (id is not null)
            {
                await Assert.ThrowsAsync<FabricItemInputException>(() => MoveAsync(service, id));
            }
        }
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
        var error = await Assert.ThrowsAsync<ItemMoveRequestException>(() => MoveAsync(new FabricCoreService(client, credential)));
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
        var error = await Assert.ThrowsAsync<ItemMoveRequestException>(() =>
            new FabricCoreService(client, credential).MoveItemAsync(WorkspaceId, ItemId, cancellationToken: cancellation.Token));
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
        var error = await Assert.ThrowsAsync<ItemMoveRequestException>(() => MoveAsync(new FabricCoreService(client, Credential())));
        Assert.Equal(expected, error.StatusCode);
        Assert.False(error.RequestNotSent);
        Assert.False(error.MoveConfirmed);
        Assert.Null(error.ResponseStatusCode);
        Assert.Equal(1, handler.CallCount);
    }

    [Theory]
    [InlineData(HttpStatusCode.Created)]
    [InlineData(HttpStatusCode.Accepted)]
    [InlineData(HttpStatusCode.NoContent)]
    [InlineData(HttpStatusCode.TemporaryRedirect)]
    [InlineData(HttpStatusCode.BadRequest)]
    [InlineData(HttpStatusCode.Unauthorized)]
    [InlineData(HttpStatusCode.Forbidden)]
    [InlineData(HttpStatusCode.NotFound)]
    [InlineData(HttpStatusCode.RequestTimeout)]
    [InlineData(HttpStatusCode.Conflict)]
    [InlineData(HttpStatusCode.TooManyRequests)]
    [InlineData(HttpStatusCode.InternalServerError)]
    public static async Task Move_Requires200AndNeverFollowsUrlsRetriesOrChoosesAnotherDestination(HttpStatusCode status)
    {
        using var response = Response("""{"errorCode":"CannotMoveChildOnly","message":"private","isRetriable":true}""", status, "60");
        response.Headers.Location = new Uri(ItemUrl);
        response.Headers.TryAddWithoutValidation("x-ms-operation-id", ItemId);
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
        using var client = new HttpClient(handler);
        var error = await Assert.ThrowsAsync<ItemMoveRequestException>(() => MoveAsync(new FabricCoreService(client, Credential())));
        Assert.Equal(response.IsSuccessStatusCode ? HttpStatusCode.BadGateway : status, error.StatusCode);
        Assert.Equal(status, error.ResponseStatusCode);
        Assert.Equal(response.IsSuccessStatusCode ? null : "CannotMoveChildOnly", error.ErrorCode);
        Assert.Equal(TimeSpan.FromSeconds(60), error.RetryAfter?.Delta);
        Assert.False(error.MoveConfirmed);
        Assert.False(error.RequestNotSent);
        Assert.Equal(1, handler.CallCount);
        Assert.Throws<ObjectDisposedException>(() => response.Content.ReadAsStream(TestContext.Current.CancellationToken));
    }

    [Fact]
    public static async Task Move_RejectsMalformedCollectionsAndConflictingMetadataWithoutPartialResults()
    {
        var valid = Collection(FolderId);
        var invalid = new List<string>
        {
            "", "null", "[]", "{}", "private", valid[..^1], "{\"value\":null}", "{\"value\":{}}",
            "{\"value\":[null]}", "{\"value\":[{}]}", "{\"value\":[1]}", "{\"value\":[\"private\"]}",
            valid.Replace(WorkspaceId, ChildId, StringComparison.Ordinal),
            valid.Replace(FolderId, ChildId, StringComparison.Ordinal),
            valid.Replace("\"Lakehouse\"", "null", StringComparison.Ordinal),
            valid.Replace("\"Lakehouse\"", "\" \"", StringComparison.Ordinal),
            valid.Replace("\"Lakehouse\"", "12", StringComparison.Ordinal),
            valid.Replace("\"MyLakehouse\"", "\"\\uD800\"", StringComparison.Ordinal),
            valid.Replace("\"MyLakehouse\"", "\"\\uDC00\"", StringComparison.Ordinal),
            valid[..^1] + ",\"value\":[]}", valid[..^1] + ",\"Value\":[]}", valid[..^1] + ",\"\\u0076\\u0061\\u006c\\u0075\\u0065\":[]}",
            valid[..^1] + ",\"unknown\":\"\\uD800\"}", valid[..^1] + ",\"unknown\\uD800\":\"private\"}",
            valid[..^1] + ",\"unknown\":{\"name\":1,\"Name\":2}}",
            valid[..^1] + ",\"unknown\":[{\"name\":\"\\uDC00\"}]}"
        };
        foreach (var field in new[] { "id", "workspaceId", "folderId", "logicalId" })
        {
            foreach (var value in new JsonNode[] { JsonValue.Create(""), JsonValue.Create("private"),
                JsonValue.Create(Guid.Empty), JsonValue.Create(3), new JsonObject() })
            {
                var node = JsonNode.Parse(valid)!;
                node["value"]![1]![field] = value;
                invalid.Add(node.ToJsonString());
            }
        }
        foreach (var (field, json) in new[]
        {
            ("tags", "{}"), ("tags", "[null]"), ("tags", "[{}]"), ("tags", "[{\"id\":\"" + FolderId + "\"}]"),
            ("sensitivityLabel", "{}"), ("sensitivityLabel", "[]"), ("displayName", "3"), ("description", "{}")
        })
        {
            var node = JsonNode.Parse(valid)!;
            node["value"]![1]![field] = JsonNode.Parse(json);
            invalid.Add(node.ToJsonString());
        }
        var duplicate = JsonSerializer.Serialize(Item(), CoreJsonContext.Default.FabricMovedItem);
        invalid.Add("{\"value\":[" + duplicate[..^1] + ",\"ID\":\"" + ItemId + "\"}]}");
        foreach (var json in invalid)
        {
            using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(Response(json, retryAfter: "60")));
            using var client = new HttpClient(handler);
            var error = await Assert.ThrowsAsync<ItemMoveRequestException>(() => MoveAsync(new FabricCoreService(client, Credential()), FolderId));
            Assert.True(error.MoveConfirmed);
            Assert.False(error.RequestNotSent);
            Assert.Equal(HttpStatusCode.BadGateway, error.StatusCode);
            Assert.Equal(HttpStatusCode.OK, error.ResponseStatusCode);
            Assert.Equal(TimeSpan.FromSeconds(60), error.RetryAfter?.Delta);
            Assert.Equal(1, handler.CallCount);
        }
    }

    [Fact]
    public static async Task Move_ToRootRejectsANonNullReturnedFolderWithoutFallingBack()
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(Response(Collection(FolderId))));
        using var client = new HttpClient(handler);
        var error = await Assert.ThrowsAsync<ItemMoveRequestException>(() => MoveAsync(new FabricCoreService(client, Credential())));
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
            var error = await Assert.ThrowsAsync<ItemMoveRequestException>(() => new FabricCoreService(client, Credential())
                .MoveItemAsync(WorkspaceId, ItemId, cancellationToken: cancellation.Token));
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
    [InlineData("""{"error\uD800Code":"ItemNotFound"}""")]
    [InlineData("""{"errorCode":"ItemNotFound","errorCode":"Other"}""")]
    [InlineData("""{"errorCode":"private invalid"}""")]
    [InlineData("""{"errorCode":null}""")]
    [InlineData("private")]
    [InlineData("[]")]
    public static async Task Move_UnusableErrorJsonRetainsStatusAndRetryAfter(string json)
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(Response(json, HttpStatusCode.TooManyRequests, "60")));
        using var client = new HttpClient(handler);
        var error = await Assert.ThrowsAsync<ItemMoveRequestException>(() => MoveAsync(new FabricCoreService(client, Credential())));
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
    public static async Task Move_BoundsCompleteSuccessAndErrorBodiesExactly(long length, bool declaredLength, HttpStatusCode status)
    {
        var json = status == HttpStatusCode.OK ? Collection().Replace("MyLakehouse", "\u00e9\U0001F680", StringComparison.Ordinal)
            : """{"errorCode":"RequestBlocked"}""";
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
            Assert.Equal(2, (await MoveAsync(service)).Items.Count);
        }
        else
        {
            var error = await Assert.ThrowsAsync<ItemMoveRequestException>(() => MoveAsync(service));
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
        var bytes = Encoding.UTF8.GetBytes(Collection());
        bytes[Array.IndexOf(bytes, (byte)'M')] = 0xff;
        using var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(bytes) };
        response.Content.Headers.ContentType = new("application/json");
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
        using var client = new HttpClient(handler);
        Assert.True((await Assert.ThrowsAsync<ItemMoveRequestException>(() => MoveAsync(new FabricCoreService(client, Credential())))).MoveConfirmed);
        Assert.Equal(1, handler.CallCount);
    }

    [Theory]
    [InlineData(64)]
    [InlineData(65)]
    public static async Task Move_BoundsUnknownJsonDepthBeforeProjectingSafeMetadata(int depth)
    {
        var json = Collection()[..^1] + ",\"extra\":" + new string('[', depth - 1) + "0" + new string(']', depth - 1) + "}";
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(Response(json)));
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, Credential());
        if (depth == 64)
        {
            Assert.Equal(2, (await MoveAsync(service)).Items.Count);
        }
        else
        {
            Assert.True((await Assert.ThrowsAsync<ItemMoveRequestException>(() => MoveAsync(service))).MoveConfirmed);
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
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(Response(Collection(), retryAfter: retryAfter)));
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
        using var response = Response(status == HttpStatusCode.OK ? Collection() : """{"errorCode":"RequestBlocked"}""", status);
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
            var error = await Assert.ThrowsAsync<ItemMoveRequestException>(() => MoveAsync(service));
            Assert.Equal(status, error.StatusCode);
            Assert.Null(error.RetryAfter);
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
            index => (Workspace: Guid.NewGuid(), Item: Guid.NewGuid(), Target: index % 2 == 0 ? Guid.NewGuid() : (Guid?)null));
        var observed = new ConcurrentBag<string>();
        using var handler = new FabricCoreHttpMessageHandler(async (request, token) =>
        {
            await Task.Yield();
            var caller = request.Headers.Authorization!.Parameter!;
            var context = contexts[caller];
            Assert.Equal(HttpMethod.Post, request.Method);
            Assert.Equal($"{FabricEndpoints.GetFabricApiBaseUrl()}/workspaces/{context.Workspace:D}/items/{context.Item:D}/move", request.RequestUri?.AbsoluteUri);
            Assert.Equal(context.Target is { } target ? $"{{\"targetFolderId\":\"{target:D}\"}}" : "{}", await request.Content!.ReadAsStringAsync(token));
            observed.Add(caller);
            var item = Item() with { Id = context.Item, WorkspaceId = context.Workspace, FolderId = context.Target, DisplayName = caller };
            return Response("{\"value\":[" + JsonSerializer.Serialize(item, CoreJsonContext.Default.FabricMovedItem) + "]}");
        });
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, credential);
        var results = await Task.WhenAll(Enumerable.Range(0, 12).Select(async index =>
        {
            identity.Value = $"caller-{index}";
            var context = contexts[identity.Value];
            var result = await service.MoveItemAsync(context.Workspace.ToString(), context.Item.ToString(), context.Target?.ToString(),
                TestContext.Current.CancellationToken);
            var item = Assert.Single(result.Items);
            Assert.Equal(context.Workspace, item.WorkspaceId);
            Assert.Equal(context.Item, item.Id);
            Assert.Equal(context.Target, item.FolderId);
            return item;
        }));
        Assert.Equal(12, handler.CallCount);
        Assert.Equal(observed.Order(), results.Select(item => item.DisplayName).Order());
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
            Assert.Equal(ItemUrl, request.RequestUri?.AbsoluteUri);
            return Task.FromResult(Response(JsonSerializer.Serialize(Item(FolderId), CoreJsonContext.Default.FabricMovedItem)));
        });
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, Credential());
        var error = await Assert.ThrowsAsync<ItemMoveRequestException>(() => MoveAsync(service, FolderId));
        Assert.Null(error.ResponseStatusCode);
        Assert.Equal(1, handler.CallCount);

        var snapshot = await service.GetItemAsync(WorkspaceId, ItemId, TestContext.Current.CancellationToken);
        Assert.Equal(Guid.Parse(ItemId), snapshot.Id);
        Assert.Equal(2, handler.CallCount);
    }

    private static Task<ItemMoveCommandResult> MoveAsync(FabricCoreService service, string? target = null) =>
        service.MoveItemAsync(WorkspaceId, ItemId, target, TestContext.Current.CancellationToken);
}
