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
using Fabric.Mcp.Tools.Core.Validation;
using NSubstitute;
using Xunit;

namespace Fabric.Mcp.Tools.Core.Tests.Services;

public sealed class FabricCoreServiceApiRequestTests
{
    [Theory]
    [InlineData(null, "/v1/connections", "https://api.fabric.microsoft.com/v1/connections", "https://api.fabric.microsoft.com/.default")]
    [InlineData("powerbi", "/v1.0/myorg/gateways", "https://api.powerbi.com/v1.0/myorg/gateways", "https://analysis.windows.net/powerbi/api/.default")]
    public async Task Get_SendsOneAuthenticatedGetToAllowlistedHostWithMatchingScope(
        string? api, string path, string expectedUrl, string expectedScope)
    {
        using var handler = new FabricCoreHttpMessageHandler((request, token) =>
        {
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal(expectedUrl, request.RequestUri?.OriginalString);
            Assert.Null(request.Content);
            Assert.Equal("operation-test-token", request.Headers.Authorization?.Parameter);
            Assert.Equal("application/json", Assert.Single(request.Headers.Accept).MediaType);
            return Task.FromResult(FabricOperationTestData.JsonResponse("""{"value":[{"id":"1"}]}"""));
        });
        using var client = new HttpClient(handler);
        var credential = FabricOperationTestData.Credential();

        var result = await new FabricCoreService(client, credential).GetApiResourceAsync(
            api, path, cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(api ?? "fabric", result.Response.Api);
        Assert.Equal(200, result.Response.Status);
        Assert.True(result.Response.HasBody);
        Assert.Equal("1", result.Response.Value.GetProperty("value")[0].GetProperty("id").GetString());
        Assert.Null(result.Response.NextPath);
        Assert.Equal(1, handler.CallCount);
        await credential.Received(1).GetTokenAsync(
            Arg.Is<TokenRequestContext>(context => context.Scopes.SequenceEqual(new[] { expectedScope })),
            Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("other", "/v1/connections")]
    [InlineData("fabric", "https://evil.example/v1/connections")]
    [InlineData("fabric", "/v1/../admin")]
    [InlineData("powerbi", "/v1/connections")]
    public async Task Get_RejectsInvalidTargetsBeforeAuthentication(string api, string path)
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) => throw new InvalidOperationException("Must not send."));
        using var client = new HttpClient(handler);
        var credential = FabricOperationTestData.Credential();

        await Assert.ThrowsAsync<ApiRequestInputException>(() => new FabricCoreService(client, credential).GetApiResourceAsync(
            api, path, cancellationToken: TestContext.Current.CancellationToken));

        Assert.Equal(0, handler.CallCount);
        Assert.Empty(credential.ReceivedCalls());
    }

    [Fact]
    public async Task Get_SkipsUtf8BomAndReturnsPowerBINextLinkAsRelativePath()
    {
        var json = """{"@odata.nextLink":"https://api.powerbi.com/v1.0/myorg/admin/groups?$skiptoken=abc","value":[]}""";
        var bytes = new byte[] { 0xEF, 0xBB, 0xBF }.Concat(Encoding.UTF8.GetBytes(json)).ToArray();
        var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(bytes) };
        response.Content.Headers.ContentType = new("application/json") { CharSet = "utf-8" };
        response.Headers.TryAddWithoutValidation("RequestId", "5b218778-e7a5-4d73-8187-f10824047715");

        var result = await SendAsync(response, "powerbi", "/v1.0/myorg/admin/groups?$top=100");

        Assert.Equal("/v1.0/myorg/admin/groups?$skiptoken=abc", result.NextPath);
        Assert.Equal("5b218778-e7a5-4d73-8187-f10824047715", result.RequestId);
        Assert.Equal(JsonValueKind.Array, result.Value.GetProperty("value").ValueKind);
    }

    [Theory]
    [InlineData("""{"continuationUri":"https://api.fabric.microsoft.com/v1/connections?continuationToken=abc","value":[]}""", "/v1/connections?continuationToken=abc")]
    [InlineData("""{"continuationUri":"https://evil.example/v1/connections?continuationToken=abc","value":[]}""", null)]
    [InlineData("""{"continuationUri":"https://api.fabric.microsoft.com/v1/../admin","value":[]}""", null)]
    [InlineData("""{"continuationUri":42,"value":[]}""", null)]
    [InlineData("""[{"continuationUri":"https://api.fabric.microsoft.com/v1/connections"}]""", null)]
    public async Task Get_ReturnsOnlySameHostContinuationPaths(string json, string? expected)
    {
        var result = await SendAsync(FabricOperationTestData.JsonResponse(json), "fabric", "/v1/connections");

        Assert.Equal(expected, result.NextPath);
    }

    [Fact]
    public async Task Get_ReturnsEmptyBodyWithoutValue()
    {
        var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent([]) };

        var result = await SendAsync(response, "fabric", "/v1/connections");

        Assert.False(result.HasBody);
        Assert.Equal(JsonValueKind.Undefined, result.Value.ValueKind);
        var output = JsonSerializer.SerializeToElement(new ApiResourceGetCommandResult(result), CoreJsonContext.Default.ApiResourceGetCommandResult);
        Assert.False(output.GetProperty("response").TryGetProperty("value", out _));
    }

    [Fact]
    public async Task Get_ReturnsAcceptedLocationAndRetryAfterWithoutPolling()
    {
        var response = new HttpResponseMessage(HttpStatusCode.Accepted) { Content = new ByteArrayContent([]) };
        response.Headers.TryAddWithoutValidation("Location", "https://api.fabric.microsoft.com/v1/operations/0acd697c-1550-43cd-b998-91bfbfbd47c6");
        response.Headers.TryAddWithoutValidation("Retry-After", "20");
        var count = 0;

        var result = await SendAsync(response, "fabric", "/v1/connections", onSend: () => count++);

        Assert.Equal(202, result.Status);
        Assert.Equal("/v1/operations/0acd697c-1550-43cd-b998-91bfbfbd47c6", result.Location);
        Assert.Equal(20, result.RetryAfterSeconds);
        Assert.Equal(1, count);
    }

    [Fact]
    public async Task Get_DropsAcceptedLocationOnOtherHost()
    {
        var response = new HttpResponseMessage(HttpStatusCode.Accepted) { Content = new ByteArrayContent([]) };
        response.Headers.TryAddWithoutValidation("Location", "https://evil.example/v1/operations/1");

        var result = await SendAsync(response, "fabric", "/v1/connections");

        Assert.Null(result.Location);
    }

    [Theory]
    [InlineData("""{"errorCode":"ItemNotFound","message":"private backend detail","requestId":"abc-123"}""", HttpStatusCode.NotFound, "ItemNotFound", "abc-123")]
    [InlineData("""{"error":{"code":"PowerBIEntityNotFound","message":"private backend detail"}}""", HttpStatusCode.NotFound, "PowerBIEntityNotFound", null)]
    [InlineData("""{"errorCode":"private code with spaces","message":"private"}""", HttpStatusCode.BadRequest, null, null)]
    [InlineData("not json private", HttpStatusCode.InternalServerError, null, null)]
    public async Task Get_ThrowsSanitizedServiceErrors(string json, HttpStatusCode status, string? expectedCode, string? expectedRequestId)
    {
        var exception = await Assert.ThrowsAsync<ApiRequestException>(() =>
            SendAsync(FabricOperationTestData.JsonResponse(json, status), "fabric", "/v1/connections"));

        Assert.Equal(ApiRequestFailure.ServiceError, exception.Failure);
        Assert.Equal(status, exception.StatusCode);
        Assert.Equal((int)status, exception.ResponseStatus);
        Assert.Equal(expectedCode, exception.ErrorCode);
        Assert.Equal(expectedCode is null, exception.ErrorMetadataUnavailable);
        Assert.Equal(expectedRequestId, exception.RequestId);
        Assert.DoesNotContain("private", exception.Message);
    }

    [Fact]
    public async Task Get_ThrowsThrottlingWithRetryAfterAndDoesNotRetry()
    {
        var calls = 0;
        var response = FabricOperationTestData.JsonResponse("""{"errorCode":"RequestBlocked"}""", HttpStatusCode.TooManyRequests, "30");
        response.Headers.TryAddWithoutValidation("x-ms-request-id", "req-1");

        var exception = await Assert.ThrowsAsync<ApiRequestException>(() =>
            SendAsync(response, "fabric", "/v1/connections", onSend: () => calls++));

        Assert.Equal(HttpStatusCode.TooManyRequests, exception.StatusCode);
        Assert.Equal(TimeSpan.FromSeconds(30), exception.RetryAfter?.Delta);
        Assert.Equal("req-1", exception.RequestId);
        Assert.Equal(1, calls);
    }

    [Theory]
    [InlineData(HttpStatusCode.Found)]
    [InlineData(HttpStatusCode.TemporaryRedirect)]
    public async Task Get_DoesNotFollowOrReturnRedirects(HttpStatusCode status)
    {
        var response = new HttpResponseMessage(status);
        response.Headers.Location = new Uri("https://evil.example/private");

        var exception = await Assert.ThrowsAsync<ApiRequestException>(() => SendAsync(response, "fabric", "/v1/connections"));

        Assert.Equal(ApiRequestFailure.Redirect, exception.Failure);
        Assert.Equal(HttpStatusCode.BadGateway, exception.StatusCode);
        Assert.Equal((int)status, exception.ResponseStatus);
    }

    [Fact]
    public async Task Get_DoesNotDownloadNonJsonResponses()
    {
        using var stream = new OperationTestStream(Encoding.UTF8.GetBytes("<html>private</html>"));
        var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StreamContent(stream) };
        response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/html");

        var exception = await Assert.ThrowsAsync<ApiRequestException>(() => SendAsync(response, "fabric", "/v1/connections"));

        Assert.Equal(ApiRequestFailure.UnsupportedMediaType, exception.Failure);
        Assert.Equal(0, stream.BytesRead);
    }

    [Fact]
    public async Task Get_FailsInsteadOfTruncatingLargeResponses()
    {
        var json = "{\"value\":\"" + new string('a', 200) + "\"}";

        var exception = await Assert.ThrowsAsync<ApiRequestException>(() =>
            SendAsync(FabricOperationTestData.JsonResponse(json), "fabric", "/v1/connections", maxResponseBytes: 100));

        Assert.Equal(ApiRequestFailure.ResponseTooLarge, exception.Failure);
        Assert.Equal(HttpStatusCode.BadGateway, exception.StatusCode);
    }

    [Theory]
    [InlineData("{\"value\":")]
    [InlineData("private")]
    public async Task Get_RejectsInvalidJson(string json)
    {
        var exception = await Assert.ThrowsAsync<ApiRequestException>(() =>
            SendAsync(FabricOperationTestData.JsonResponse(json), "fabric", "/v1/connections"));

        Assert.Equal(ApiRequestFailure.InvalidJson, exception.Failure);
    }

    private static async Task<FabricApiResponse> SendAsync(
        HttpResponseMessage response, string api, string path, int? maxResponseBytes = null, Action? onSend = null)
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) =>
        {
            onSend?.Invoke();
            return Task.FromResult(response);
        });
        using var client = new HttpClient(handler);
        var result = await new FabricCoreService(client, FabricOperationTestData.Credential()).GetApiResourceAsync(
            api, path, maxResponseBytes, TestContext.Current.CancellationToken);
        return result.Response;
    }
}
