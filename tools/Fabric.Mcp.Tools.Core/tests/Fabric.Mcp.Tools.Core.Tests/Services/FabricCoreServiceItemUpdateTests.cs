// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;
using Azure.Core;
using Fabric.Mcp.Tools.Core.Commands;
using Fabric.Mcp.Tools.Core.Models;
using Fabric.Mcp.Tools.Core.Options;
using Fabric.Mcp.Tools.Core.Services;
using Fabric.Mcp.Tools.Core.Tests.TestSupport;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Mcp.Core.Models.Command;
using NSubstitute;
using Xunit;

namespace Fabric.Mcp.Tools.Core.Tests.Services;

public sealed class FabricCoreServiceItemUpdateTests()
{
    [Theory]
    [InlineData("Renamed", null, """{"displayName":"Renamed"}""")]
    [InlineData(null, "Updated", """{"description":"Updated"}""")]
    [InlineData(null, "", """{"description":""}""")]
    [InlineData("Renamed", "", """{"displayName":"Renamed","description":""}""")]
    [InlineData(null, "  ", """{"description":"  "}""")]
    public static async Task UpdateItemAsync_SendsExactlyOneNormalizedPatch(string? name, string? description, string expectedJson)
    {
        var credential = ItemUpdateTestData.Credential();
        using var response = ItemUpdateTestData.Response();
        HttpRequestMessage? captured = null;
        using var handler = new FabricCoreHttpMessageHandler(async (request, token) =>
        {
            captured = request;
            Assert.True(token.CanBeCanceled);
            Assert.Equal(HttpMethod.Patch, request.Method);
            Assert.Equal(
                $"https://api.fabric.microsoft.com/v1/workspaces/{ItemUpdateTestData.WorkspaceId}/items/{ItemUpdateTestData.ItemId}",
                request.RequestUri!.AbsoluteUri);
            Assert.Equal("Bearer", request.Headers.Authorization!.Scheme);
            Assert.Equal(ItemUpdateTestData.Token, request.Headers.Authorization.Parameter);
            Assert.Equal("Fabric Core MCP", request.Headers.UserAgent.ToString());
            Assert.Equal("application/json", request.Content!.Headers.ContentType!.MediaType);
            Assert.Equal(expectedJson, await request.Content.ReadAsStringAsync(token));
            return response;
        });
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, credential);

        var item = await service.UpdateItemAsync(
            $"{{{ItemUpdateTestData.WorkspaceId.ToUpperInvariant()}}}",
            ItemUpdateTestData.ItemId.Replace("-", "").ToUpperInvariant(),
            new(name, description),
            TestContext.Current.CancellationToken);

        Assert.Equal(ItemUpdateTestData.Metadata(), item);
        Assert.Equal(1, handler.CallCount);
        await credential.Received(1).GetTokenAsync(
            Arg.Is<TokenRequestContext>(r => r.Scopes.SequenceEqual(FabricEndpoints.FabricScopes)),
            TestContext.Current.CancellationToken);
        Assert.Throws<ObjectDisposedException>(() => response.Content.ReadAsStream(TestContext.Current.CancellationToken));
        Assert.NotNull(captured);
        Assert.Throws<ObjectDisposedException>(() => captured.Content!.ReadAsStream(TestContext.Current.CancellationToken));
    }

    [Theory]
    [InlineData("", ItemUpdateTestData.ItemId)]
    [InlineData("workspace-name", ItemUpdateTestData.ItemId)]
    [InlineData("https://example.com", ItemUpdateTestData.ItemId)]
    [InlineData("00000000-0000-0000-0000-000000000000", ItemUpdateTestData.ItemId)]
    [InlineData(ItemUpdateTestData.WorkspaceId, "")]
    [InlineData(ItemUpdateTestData.WorkspaceId, "../other")]
    [InlineData(ItemUpdateTestData.WorkspaceId, "00000000-0000-0000-0000-000000000000")]
    public static async Task UpdateItemAsync_RejectsInvalidIdsBeforeCredentialsOrHttp(string workspaceId, string itemId)
    {
        var credential = ItemUpdateTestData.Credential();
        using var handler = new FabricCoreHttpMessageHandler((_, _) => throw new InvalidOperationException("No HTTP expected."));
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, credential);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.UpdateItemAsync(workspaceId, itemId, new(Description: ""), TestContext.Current.CancellationToken));

        Assert.Empty(credential.ReceivedCalls());
        Assert.Equal(0, handler.CallCount);
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData("", null)]
    [InlineData(" ", "description")]
    public static async Task UpdateItemAsync_RejectsInvalidBodyBeforeCredentialsOrHttp(string? name, string? description)
    {
        await AssertInvalidRequestAsync(new(name, description));
    }

    [Fact]
    public static async Task UpdateItemAsync_RejectsOversizedDescriptionAndNullRequest()
    {
        await AssertInvalidRequestAsync(new(Description: new string('d', 257)));
        await AssertInvalidRequestAsync(null);
    }

    [Fact]
    public static async Task UpdateItemAsync_AcceptsTheMaximumDescriptionLength()
    {
        var description = new string('d', 256);
        using var handler = new FabricCoreHttpMessageHandler(async (request, token) =>
        {
            using var json = JsonDocument.Parse(await request.Content!.ReadAsStringAsync(token));
            Assert.Equal(description, json.RootElement.GetProperty("description").GetString());
            return ItemUpdateTestData.Response();
        });
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, ItemUpdateTestData.Credential());

        await service.UpdateItemAsync(
            ItemUpdateTestData.WorkspaceId, ItemUpdateTestData.ItemId, new(Description: description), TestContext.Current.CancellationToken);

        Assert.Equal(1, handler.CallCount);
    }

    [Theory]
    [InlineData("")]
    [InlineData("null")]
    [InlineData("{}")]
    [InlineData("[]")]
    [InlineData("{malformed")]
    [InlineData("""{"id":123}""")]
    public static async Task UpdateItemAsync_RejectsMalformedSuccessAndDisposesResponse(string json)
    {
        using var response = ItemUpdateTestData.Response(json: json);
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, ItemUpdateTestData.Credential());

        var error = await Assert.ThrowsAsync<InvalidDataException>(() => UpdateAsync(service, TestContext.Current.CancellationToken));

        Assert.Equal("Fabric returned an invalid Update Item response.", error.Message);
        Assert.Null(error.InnerException);
        Assert.Equal(1, handler.CallCount);
        Assert.Throws<ObjectDisposedException>(() => response.Content.ReadAsStream(TestContext.Current.CancellationToken));
    }

    [Theory]
    [InlineData("id", null)]
    [InlineData("id", "00000000-0000-0000-0000-000000000000")]
    [InlineData("id", ItemUpdateTestData.WorkspaceId)]
    [InlineData("workspaceId", ItemUpdateTestData.ItemId)]
    [InlineData("workspaceId", "malformed")]
    [InlineData("displayName", "")]
    [InlineData("displayName", null)]
    [InlineData("type", " ")]
    [InlineData("type", null)]
    public static async Task UpdateItemAsync_RejectsInvalidOrMismatchedMetadata(string property, string? value)
    {
        var json = JsonSerializer.SerializeToNode(ItemUpdateTestData.Metadata(), CoreJsonContext.Default.ItemUpdateMetadata)!;
        json[property] = value;
        using var response = ItemUpdateTestData.Response(json: json.ToJsonString());
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
        using var client = new HttpClient(handler);

        await Assert.ThrowsAsync<InvalidDataException>(() => UpdateAsync(new(client, ItemUpdateTestData.Credential()), TestContext.Current.CancellationToken));

        Assert.Equal(1, handler.CallCount);
    }

    [Theory]
    [InlineData("id")]
    [InlineData("workspaceId")]
    [InlineData("displayName")]
    [InlineData("type")]
    public static async Task UpdateItemAsync_RejectsMissingRequiredMetadata(string property)
    {
        var json = JsonSerializer.SerializeToNode(ItemUpdateTestData.Metadata(), CoreJsonContext.Default.ItemUpdateMetadata)!.AsObject();
        json.Remove(property);
        using var response = ItemUpdateTestData.Response(json: json.ToJsonString());
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
        using var client = new HttpClient(handler);

        await Assert.ThrowsAsync<InvalidDataException>(() => UpdateAsync(new(client, ItemUpdateTestData.Credential()), TestContext.Current.CancellationToken));
    }

    [Theory]
    [InlineData(HttpStatusCode.Created)]
    [InlineData(HttpStatusCode.Accepted)]
    [InlineData(HttpStatusCode.NoContent)]
    public static async Task UpdateItemAsync_RejectsUndocumentedSuccessWithoutPolling(HttpStatusCode status)
    {
        using var response = ItemUpdateTestData.Response(status);
        response.Headers.Location = new Uri("https://example.com/do-not-follow");
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
        using var client = new HttpClient(handler);

        await Assert.ThrowsAsync<InvalidDataException>(() => UpdateAsync(new(client, ItemUpdateTestData.Credential()), TestContext.Current.CancellationToken));

        Assert.Equal(1, handler.CallCount);
        Assert.Throws<ObjectDisposedException>(() => response.Content.ReadAsStream(TestContext.Current.CancellationToken));
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
    public static async Task UpdateItemAsync_PreservesFailuresWithoutReadingBodiesOrRetrying(HttpStatusCode status)
    {
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        using var stream = new ItemUpdateCancellationStream(cancellation);
        using var response = new HttpResponseMessage(status) { Content = new StreamContent(stream) };
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
        using var client = new HttpClient(handler);

        var error = await Assert.ThrowsAnyAsync<HttpRequestException>(() =>
            UpdateAsync(new(client, ItemUpdateTestData.Credential()), cancellation.Token));

        Assert.Equal(status, error.StatusCode);
        Assert.Null(error.InnerException);
        Assert.False(cancellation.IsCancellationRequested);
        Assert.True(stream.IsDisposed);
        Assert.Equal(1, handler.CallCount);
    }

    [Theory]
    [InlineData("0", 0)]
    [InlineData("12", 12)]
    [InlineData("00012", 12)]
    [InlineData(" 12 ", 12)]
    [InlineData("\t12\t", 12)]
    [InlineData("2147483647", int.MaxValue)]
    [InlineData(null, null)]
    [InlineData("-1", null)]
    [InlineData("-0", null)]
    [InlineData("+12", null)]
    [InlineData("", null)]
    [InlineData("1e1", null)]
    [InlineData("1.5", null)]
    [InlineData("2147483648", null)]
    [InlineData("9223372036854775807", null)]
    [InlineData("12,13", null)]
    [InlineData("Wed, 21 Oct 2026 07:28:00 GMT", null)]
    [InlineData("Wed, 21 Oct 2015 07:28:00 GMT", null)]
    [InlineData(ItemUpdateTestData.PrivateDetails, null)]
    public static async Task UpdateItemAsync_ExposesOnlyValidatedRetryAfterSeconds(string? header, int? expected)
    {
        using var response = ItemUpdateTestData.Response(HttpStatusCode.TooManyRequests, ItemUpdateTestData.PrivateDetails);
        if (header is not null)
        {
            response.Headers.TryAddWithoutValidation("Retry-After", header);
        }
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
        using var client = new HttpClient(handler);
        var command = new ItemUpdateCommand(NullLogger<ItemUpdateCommand>.Instance, new FabricCoreService(client, ItemUpdateTestData.Credential()));

        var result = await command.ExecuteAsync(new(), UpdateOptions(), TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.TooManyRequests, result.Status);
        if (expected is { } seconds)
        {
            Assert.Contains($"Wait at least {seconds} seconds", result.Message);
        }
        else
        {
            Assert.DoesNotContain("Wait at least", result.Message);
        }
        Assert.DoesNotContain(ItemUpdateTestData.PrivateDetails, result.Message);
        Assert.Null(result.Results);
        Assert.Equal(1, handler.CallCount);
    }

    [Fact]
    public static async Task UpdateItemAsync_RejectsMultipleRetryAfterValues()
    {
        using var response = ItemUpdateTestData.Response(HttpStatusCode.TooManyRequests);
        response.Headers.TryAddWithoutValidation("Retry-After", ["2", "3"]);
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
        using var client = new HttpClient(handler);
        var command = new ItemUpdateCommand(NullLogger<ItemUpdateCommand>.Instance, new FabricCoreService(client, ItemUpdateTestData.Credential()));

        var result = await command.ExecuteAsync(new(), UpdateOptions(), TestContext.Current.CancellationToken);

        Assert.DoesNotContain("Wait at least", result.Message);
        Assert.Equal(1, handler.CallCount);
    }

    [Fact]
    public static async Task UpdateItemAsync_PropagatesCredentialCancellation()
    {
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        var credential = ItemUpdateTestData.Credential();
        credential.GetTokenAsync(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>()).Returns(_ =>
        {
            cancellation.Cancel();
            return ValueTask.FromCanceled<AccessToken>(cancellation.Token);
        });
        using var handler = new FabricCoreHttpMessageHandler((_, _) => throw new InvalidOperationException("No HTTP expected."));
        using var client = new HttpClient(handler);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => UpdateAsync(new(client, credential), cancellation.Token));

        Assert.Equal(0, handler.CallCount);
    }

    [Fact]
    public static async Task UpdateItemAsync_PropagatesHttpCancellation()
    {
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        using var handler = new FabricCoreHttpMessageHandler((_, token) =>
        {
            cancellation.Cancel();
            Assert.True(token.IsCancellationRequested);
            return Task.FromCanceled<HttpResponseMessage>(token);
        });
        using var client = new HttpClient(handler);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            UpdateAsync(new(client, ItemUpdateTestData.Credential()), cancellation.Token));

        Assert.Equal(1, handler.CallCount);
    }

    [Fact]
    public static async Task UpdateItemAsync_PropagatesDeserializationCancellationAndDisposesResponse()
    {
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        using var stream = new ItemUpdateCancellationStream(cancellation);
        using var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StreamContent(stream) };
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
        using var client = new HttpClient(handler);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            UpdateAsync(new(client, ItemUpdateTestData.Credential()), cancellation.Token));

        Assert.True(stream.IsDisposed);
        Assert.Equal(1, handler.CallCount);
    }

    [Fact]
    public static async Task UpdateItemAsync_KeepsConcurrentCredentialsAndBodiesIsolated()
    {
        var identity = new AsyncLocal<string>();
        var credential = ItemUpdateTestData.Credential();
        credential.GetTokenAsync(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>())
            .Returns(_ => new AccessToken(identity.Value!, DateTimeOffset.MaxValue));
        using var handler = new FabricCoreHttpMessageHandler(async (request, token) =>
        {
            await Task.Yield();
            using var body = JsonDocument.Parse(await request.Content!.ReadAsStringAsync(token));
            Assert.Equal(body.RootElement.GetProperty("description").GetString(), request.Headers.Authorization!.Parameter);
            return ItemUpdateTestData.Response();
        });
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, credential);

        await Task.WhenAll(UpdateForIdentityAsync("first-user"), UpdateForIdentityAsync("second-user"));

        Assert.Equal(2, handler.CallCount);
        await credential.Received(2).GetTokenAsync(Arg.Any<TokenRequestContext>(), TestContext.Current.CancellationToken);

        async Task UpdateForIdentityAsync(string token)
        {
            identity.Value = token;
            await service.UpdateItemAsync(
                ItemUpdateTestData.WorkspaceId, ItemUpdateTestData.ItemId, new(Description: token), TestContext.Current.CancellationToken);
        }
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public static async Task ExistingPostOperations_PreserveRequestsAndResults(bool create)
    {
        using var response = create
            ? ItemUpdateTestData.Response()
            : ItemUpdateTestData.Response(json: """{"value":[],"continuationToken":"next"}""");
        using var handler = new FabricCoreHttpMessageHandler(async (request, token) =>
        {
            Assert.Equal(HttpMethod.Post, request.Method);
            Assert.Equal(
                create ? $"/v1/workspaces/{ItemUpdateTestData.WorkspaceId}/items" : "/v1/catalog/search",
                request.RequestUri!.AbsolutePath);
            using var body = JsonDocument.Parse(await request.Content!.ReadAsStringAsync(token));
            Assert.Equal("Existing behavior", body.RootElement.GetProperty(create ? "displayName" : "search").GetString());
            return response;
        });
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, ItemUpdateTestData.Credential());

        if (create)
        {
            var result = await service.CreateItemAsync(
                ItemUpdateTestData.WorkspaceId,
                new() { DisplayName = "Existing behavior", Type = "Lakehouse" },
                TestContext.Current.CancellationToken);
            Assert.Equal(ItemUpdateTestData.ItemId, result.Id);
        }
        else
        {
            var result = await service.SearchCatalogAsync(new() { Search = "Existing behavior" }, TestContext.Current.CancellationToken);
            Assert.Empty(result.Value);
            Assert.Equal("next", result.ContinuationToken);
        }
        Assert.Equal(1, handler.CallCount);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public static async Task ExistingPostOperations_PreserveErrorBehavior(bool create)
    {
        using var response = ItemUpdateTestData.Response(HttpStatusCode.BadRequest, "existing error");
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, ItemUpdateTestData.Credential());

        var error = await Assert.ThrowsAsync<HttpRequestException>(async () =>
        {
            if (create)
            {
                await service.CreateItemAsync(ItemUpdateTestData.WorkspaceId, new(), TestContext.Current.CancellationToken);
            }
            else
            {
                await service.SearchCatalogAsync(new(), TestContext.Current.CancellationToken);
            }
        });

        Assert.Contains("existing error", error.Message);
        Assert.Equal(HttpStatusCode.BadRequest, error.StatusCode);
        Assert.Equal(1, handler.CallCount);
    }

    private static async Task AssertInvalidRequestAsync(UpdateItemRequest? request)
    {
        var credential = ItemUpdateTestData.Credential();
        using var handler = new FabricCoreHttpMessageHandler((_, _) => throw new InvalidOperationException("No HTTP expected."));
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, credential);

        await Assert.ThrowsAnyAsync<ArgumentException>(() => service.UpdateItemAsync(
            ItemUpdateTestData.WorkspaceId, ItemUpdateTestData.ItemId, request!, TestContext.Current.CancellationToken));

        Assert.Empty(credential.ReceivedCalls());
        Assert.Equal(0, handler.CallCount);
    }

    private static Task<ItemUpdateMetadata> UpdateAsync(FabricCoreService service, CancellationToken cancellationToken) =>
        service.UpdateItemAsync(ItemUpdateTestData.WorkspaceId, ItemUpdateTestData.ItemId, new(Description: ""), cancellationToken);

    private static ItemUpdateOptions UpdateOptions() =>
        new() { WorkspaceId = ItemUpdateTestData.WorkspaceId, ItemId = ItemUpdateTestData.ItemId, Description = "" };
}
