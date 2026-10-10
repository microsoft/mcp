// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Azure.Core;
using Fabric.Mcp.Tools.Core.Models;
using Fabric.Mcp.Tools.Core.Services;
using Fabric.Mcp.Tools.Core.Tests.TestSupport;
using NSubstitute;
using Xunit;

namespace Fabric.Mcp.Tools.Core.Tests.Services;

public sealed class FabricOperationServiceTests()
{
    [Theory]
    [InlineData("Undefined")]
    [InlineData("NotStarted")]
    [InlineData("Running")]
    [InlineData("Succeeded")]
    [InlineData("Failed")]
    [InlineData("FutureState")]
    public async Task State_PreservesStatusesAndOptionalMetadata_InOneAuthenticatedRead(string status)
    {
        using var response = FabricOperationTestData.JsonResponse($$"""{"status":"{{status}}"}""", retryAfter: "20");
        response.Headers.Location = new Uri("https://example.invalid/never-follow");
        using var handler = new FabricCoreHttpMessageHandler((request, _) =>
        {
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal($"{FabricEndpoints.FabricApiBaseUrl}/operations/{FabricOperationTestData.OperationId}", request.RequestUri?.AbsoluteUri);
            Assert.Null(request.Content);
            Assert.Equal("operation-test-token", request.Headers.Authorization?.Parameter);
            return Task.FromResult(response);
        });
        using var client = new HttpClient(handler);
        var credential = FabricOperationTestData.Credential();
        var service = new FabricCoreService(client, credential);

        var result = await service.GetOperationStateAsync(
            $"{{{FabricOperationTestData.OperationId.ToUpperInvariant()}}}", TestContext.Current.CancellationToken);

        Assert.Equal(Guid.Parse(FabricOperationTestData.OperationId), result.OperationId);
        Assert.Equal(status, result.State.Status);
        Assert.Null(result.State.CreatedTimeUtc);
        Assert.Null(result.State.LastUpdatedTimeUtc);
        Assert.Null(result.State.PercentComplete);
        Assert.Equal(20, result.ServerRetryAfterSeconds);
        Assert.Equal(20, result.RecommendedPollAfterSeconds);
        Assert.Equal(1, handler.CallCount);
        Assert.Throws<ObjectDisposedException>(() => response.Content.ReadAsStream(TestContext.Current.CancellationToken));
        await credential.Received(1).GetTokenAsync(
            Arg.Is<TokenRequestContext>(context => context.Scopes.SequenceEqual(FabricEndpoints.FabricScopes)),
            TestContext.Current.CancellationToken);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(100)]
    public async Task State_ReturnsProgressTimestampsAndOnlySafeFailureFields(int progress)
    {
        using var response = FabricOperationTestData.JsonResponse($$$"""
            {"status":"Failed","percentComplete":{{{progress}}},
             "createdTimeUtc":"2030-01-01T00:00:00Z","lastUpdatedTimeUtc":"2030-01-01T00:00:20Z",
             "error":{"errorCode":"Operation_Failed.1","requestId":"{{{FabricOperationTestData.OperationId}}}",
                      "message":"private-backend-detail","moreDetails":[{"message":"private-backend-detail"}]}}
            """);
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, FabricOperationTestData.Credential());

        var result = await service.GetOperationStateAsync(FabricOperationTestData.OperationId, TestContext.Current.CancellationToken);
        var json = JsonSerializer.Serialize(result, CoreJsonContext.Default.OperationStateResult);

        Assert.Equal(progress, result.State.PercentComplete);
        Assert.Equal(FabricOperationTestData.Epoch, result.State.CreatedTimeUtc);
        Assert.Equal(FabricOperationTestData.Epoch.AddSeconds(20), result.State.LastUpdatedTimeUtc);
        Assert.Equal("Operation_Failed.1", result.State.Error?.ErrorCode);
        Assert.Equal(Guid.Parse(FabricOperationTestData.OperationId), result.State.Error?.RequestId);
        Assert.DoesNotContain("private-backend-detail", json);
        Assert.DoesNotContain("message", json);
        Assert.Equal(1, handler.CallCount);
    }

    [Fact]
    public async Task State_DoesNotEchoNonIdentifierErrorCode()
    {
        using var response = FabricOperationTestData.JsonResponse("""{"status":"Failed","error":{"errorCode":"Bearer private-backend-detail"}}""");
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, FabricOperationTestData.Credential());

        var result = await service.GetOperationStateAsync(FabricOperationTestData.OperationId, TestContext.Current.CancellationToken);

        Assert.Equal("Failed", result.State.Status);
        Assert.Null(result.State.Error?.ErrorCode);
    }

    [Theory]
    [InlineData("")]
    [InlineData("{")]
    [InlineData("null")]
    [InlineData("[]")]
    [InlineData("{}")]
    [InlineData("""{"status":""}""")]
    [InlineData("""{"status":1}""")]
    [InlineData("""{"status":"Running","percentComplete":-1}""")]
    [InlineData("""{"status":"Running","percentComplete":101}""")]
    [InlineData("""{"status":"Running","percentComplete":0.5}""")]
    [InlineData("""{"status":"Running","createdTimeUtc":"private-backend-detail"}""")]
    public async Task State_RejectsInvalidPayloadsWithoutExposingBackendDetails(string json)
    {
        using var stream = new OperationTestStream(Encoding.UTF8.GetBytes(json));
        using var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StreamContent(stream) };
        response.Content.Headers.ContentType = new("application/json");
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, FabricOperationTestData.Credential());

        var error = await Assert.ThrowsAsync<FabricOperationException>(() =>
            service.GetOperationStateAsync(FabricOperationTestData.OperationId, TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.BadGateway, error.StatusCode);
        Assert.DoesNotContain("private-backend-detail", error.Message);
        Assert.True(stream.Disposed);
        Assert.Equal(1, handler.CallCount);
    }

    [Theory]
    [InlineData(false, null)]
    [InlineData(true, null)]
    [InlineData(false, "")]
    [InlineData(true, " ")]
    [InlineData(false, "operation-name")]
    [InlineData(true, "operation-name")]
    [InlineData(false, "00000000-0000-0000-0000-000000000000")]
    [InlineData(true, "00000000-0000-0000-0000-000000000000")]
    [InlineData(false, "../operations?token=private")]
    [InlineData(true, "https://example.invalid/operations")]
    public async Task Reads_ValidateIdsBeforeCredentialsOrNetwork(bool result, string? operationId)
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) => throw new InvalidOperationException("No HTTP expected."));
        using var client = new HttpClient(handler);
        var credential = FabricOperationTestData.Credential();
        var service = new FabricCoreService(client, credential);

        await Assert.ThrowsAsync<ArgumentException>(() => ReadAsync(service, result, operationId!, TestContext.Current.CancellationToken));

        Assert.Empty(credential.ReceivedCalls());
        Assert.Equal(0, handler.CallCount);
    }

    [Theory]
    [InlineData(false, "invalid")]
    [InlineData(true, "invalid")]
    [InlineData(false, FabricOperationTestData.ItemId)]
    [InlineData(true, FabricOperationTestData.ItemId)]
    [InlineData(false, "multiple")]
    [InlineData(true, "multiple")]
    public async Task Reads_DoNotAcceptChangedOrAmbiguousResponseOperationId(bool result, string header)
    {
        using var response = FabricOperationTestData.JsonResponse(result ? "{}" : """{"status":"Running"}""");
        response.Headers.TryAddWithoutValidation("x-ms-operation-id", header == "multiple"
            ? [FabricOperationTestData.OperationId, FabricOperationTestData.OperationId]
            : [header]);
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, FabricOperationTestData.Credential());

        var error = await Assert.ThrowsAsync<FabricOperationException>(() =>
            ReadAsync(service, result, FabricOperationTestData.OperationId, TestContext.Current.CancellationToken));

        Assert.Equal("InvalidOperationId", error.Code);
        Assert.Equal(1, handler.CallCount);
    }

    [Theory]
    [InlineData("", false, null)]
    [InlineData("null", true, JsonValueKind.Null)]
    [InlineData("{}", true, JsonValueKind.Object)]
    [InlineData("[1,true]", true, JsonValueKind.Array)]
    [InlineData("\"text\"", true, JsonValueKind.String)]
    [InlineData("42", true, JsonValueKind.Number)]
    public async Task Result_PreservesArbitraryJsonAndDistinguishesNullFromEmpty(string json, bool hasBody, JsonValueKind? kind)
    {
        using var response = FabricOperationTestData.JsonResponse(json);
        response.Headers.TryAddWithoutValidation("x-ms-operation-id", FabricOperationTestData.OperationId);
        response.Headers.Location = new Uri("https://example.invalid/private");
        using var handler = new FabricCoreHttpMessageHandler((request, _) =>
        {
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal($"{FabricEndpoints.FabricApiBaseUrl}/operations/{FabricOperationTestData.OperationId}/result", request.RequestUri?.AbsoluteUri);
            Assert.Equal("operation-test-token", request.Headers.Authorization?.Parameter);
            return Task.FromResult(response);
        });
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, FabricOperationTestData.Credential());

        var result = await service.GetOperationResultAsync(FabricOperationTestData.OperationId, TestContext.Current.CancellationToken);
        var roundTrip = JsonSerializer.Deserialize(JsonSerializer.Serialize(result, CoreJsonContext.Default.OperationResult), CoreJsonContext.Default.OperationResult);

        Assert.Equal(Guid.Parse(FabricOperationTestData.OperationId), result.OperationId);
        Assert.Equal(hasBody, result.HasBody);
        Assert.Equal(kind ?? JsonValueKind.Undefined, result.Value.ValueKind);
        Assert.Equal(kind ?? JsonValueKind.Undefined, roundTrip?.Value.ValueKind);
        Assert.Equal(1, handler.CallCount);
    }

    [Theory]
    [InlineData("application/octet-stream", "")]
    [InlineData("application/octet-stream", "binary")]
    [InlineData("text/html", "<html>private</html>")]
    [InlineData(null, "{}")]
    [InlineData("application/json", "{private")]
    [InlineData("application/json", " ")]
    public async Task Result_RejectsUnsupportedMediaAndInvalidJson(string? mediaType, string body)
    {
        using var stream = new OperationTestStream(Encoding.UTF8.GetBytes(body));
        using var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StreamContent(stream) };
        response.Content.Headers.ContentType = mediaType is null ? null : new MediaTypeHeaderValue(mediaType);
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, FabricOperationTestData.Credential());

        var error = await Assert.ThrowsAsync<FabricOperationException>(() =>
            service.GetOperationResultAsync(FabricOperationTestData.OperationId, TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.BadGateway, error.StatusCode);
        Assert.DoesNotContain("private", error.Message);
        Assert.True(stream.Disposed);
        Assert.Equal(1, handler.CallCount);
    }

    [Theory]
    [InlineData(false, 0)]
    [InlineData(false, 1)]
    [InlineData(true, 0)]
    [InlineData(true, 1)]
    public async Task Result_EnforcesExactByteLimit_WithOrWithoutContentLength(bool contentLength, int excess)
    {
        var body = Encoding.UTF8.GetBytes("\"" + new string('x', FabricOperationHttp.MaxJsonBytes - 2 + excess) + "\"");
        using var stream = new OperationTestStream(body);
        using var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StreamContent(stream) };
        response.Content.Headers.ContentType = new("application/json");
        if (contentLength)
        {
            response.Content.Headers.ContentLength = body.Length;
        }
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, FabricOperationTestData.Credential());

        if (excess == 0)
        {
            var result = await service.GetOperationResultAsync(FabricOperationTestData.OperationId, TestContext.Current.CancellationToken);
            Assert.Equal(FabricOperationHttp.MaxJsonBytes - 2, result.Value.GetString()?.Length);
        }
        else
        {
            var error = await Assert.ThrowsAsync<FabricOperationException>(() =>
                service.GetOperationResultAsync(FabricOperationTestData.OperationId, TestContext.Current.CancellationToken));
            Assert.Equal("ResultTooLarge", error.Code);
            Assert.True(stream.BytesRead <= FabricOperationHttp.MaxJsonBytes + 1);
            if (contentLength)
            {
                Assert.Equal(0, stream.BytesRead);
            }
        }
        Assert.True(stream.Disposed);
        Assert.Equal(1, handler.CallCount);
    }

    [Theory]
    [InlineData(false, HttpStatusCode.Created)]
    [InlineData(true, HttpStatusCode.Accepted)]
    [InlineData(false, HttpStatusCode.NoContent)]
    [InlineData(true, HttpStatusCode.NoContent)]
    [InlineData(false, HttpStatusCode.Forbidden)]
    [InlineData(true, HttpStatusCode.Forbidden)]
    [InlineData(false, HttpStatusCode.NotFound)]
    [InlineData(true, HttpStatusCode.NotFound)]
    [InlineData(false, HttpStatusCode.TooManyRequests)]
    [InlineData(true, HttpStatusCode.TooManyRequests)]
    [InlineData(false, HttpStatusCode.ServiceUnavailable)]
    [InlineData(true, HttpStatusCode.ServiceUnavailable)]
    [InlineData(false, HttpStatusCode.TemporaryRedirect)]
    [InlineData(true, HttpStatusCode.PermanentRedirect)]
    public async Task Reads_EnforceEndpointStatusWithoutRetriesOrRedirects(bool result, HttpStatusCode status)
    {
        using var response = FabricOperationTestData.JsonResponse("private-backend-detail", status, "20");
        response.Headers.Location = new Uri("https://example.invalid/private");
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, FabricOperationTestData.Credential());

        var error = await Assert.ThrowsAnyAsync<HttpRequestException>(() =>
            ReadAsync(service, result, FabricOperationTestData.OperationId, TestContext.Current.CancellationToken));

        Assert.Equal(response.IsSuccessStatusCode ? HttpStatusCode.BadGateway : status, error.StatusCode);
        Assert.DoesNotContain("private", error.Message);
        Assert.Equal(1, handler.CallCount);
        Assert.Throws<ObjectDisposedException>(() => response.Content.ReadAsStream(TestContext.Current.CancellationToken));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Reads_PreCancellationSkipsAuthenticationAndNetwork(bool result)
    {
        using var caller = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        await caller.CancelAsync();
        using var handler = new FabricCoreHttpMessageHandler((_, _) => throw new InvalidOperationException("No HTTP expected."));
        using var client = new HttpClient(handler);
        var credential = FabricOperationTestData.Credential();
        var service = new FabricCoreService(client, credential);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            ReadAsync(service, result, FabricOperationTestData.OperationId, caller.Token));

        Assert.Equal(0, handler.CallCount);
        Assert.Empty(credential.ReceivedCalls());
    }

    [Fact]
    public async Task State_UsesSameStreamingByteLimitAsResult()
    {
        using var response = FabricOperationTestData.JsonResponse(
            "{\"status\":\"Running\",\"extra\":\"" + new string('x', FabricOperationHttp.MaxJsonBytes) + "\"}");
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, FabricOperationTestData.Credential());

        var error = await Assert.ThrowsAsync<FabricOperationException>(() =>
            service.GetOperationStateAsync(FabricOperationTestData.OperationId, TestContext.Current.CancellationToken));

        Assert.Equal("ResultTooLarge", error.Code);
        Assert.Equal(1, handler.CallCount);
    }

    private static async Task ReadAsync(FabricCoreService service, bool result, string id, CancellationToken cancellationToken)
    {
        if (result)
        {
            await service.GetOperationResultAsync(id, cancellationToken);
        }
        else
        {
            await service.GetOperationStateAsync(id, cancellationToken);
        }
    }
}
