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

    [Theory]
    [InlineData(null, "POST", "/v1/workspaces/w/items/i/getDefinition", """{"format":"TMDL"}""", "https://api.fabric.microsoft.com/v1/workspaces/w/items/i/getDefinition", "https://api.fabric.microsoft.com/.default")]
    [InlineData("fabric", "patch", "/v1/connections/c", """[ {"op":"x"} ]""", "https://api.fabric.microsoft.com/v1/connections/c", "https://api.fabric.microsoft.com/.default")]
    [InlineData("powerbi", "PUT", "/v1.0/myorg/groups/g/datasets/d", """{"queryScaleOutSettings":{"maxReadOnlyReplicas":-1}}""", "https://api.powerbi.com/v1.0/myorg/groups/g/datasets/d", "https://analysis.windows.net/powerbi/api/.default")]
    [InlineData("powerbi", "DELETE", "/v1.0/myorg/groups/g/datasets/d", null, "https://api.powerbi.com/v1.0/myorg/groups/g/datasets/d", "https://analysis.windows.net/powerbi/api/.default")]
    public async Task Invoke_SendsOneWriteWithUnchangedJsonBodyAndMatchingScope(
        string? api, string method, string path, string? body, string expectedUrl, string expectedScope)
    {
        using var handler = new FabricCoreHttpMessageHandler(async (request, token) =>
        {
            Assert.Equal(method.ToUpperInvariant(), request.Method.Method);
            Assert.Equal(expectedUrl, request.RequestUri?.OriginalString);
            Assert.Equal("operation-test-token", request.Headers.Authorization?.Parameter);
            Assert.Equal("application/json", Assert.Single(request.Headers.Accept).MediaType);
            if (body is null)
            {
                Assert.True(request.Content is null || (await request.Content.ReadAsByteArrayAsync(token)).Length == 0);
            }
            else
            {
                Assert.Equal("application/json", request.Content?.Headers.ContentType?.MediaType);
                Assert.Equal(body, await request.Content!.ReadAsStringAsync(token));
            }
            return FabricOperationTestData.JsonResponse("""{"id":"1"}""");
        });
        using var client = new HttpClient(handler);
        var credential = FabricOperationTestData.Credential();

        var result = await new FabricCoreService(client, credential).InvokeApiRequestAsync(
            api, path, method, body, cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(api ?? "fabric", result.Response?.Api);
        Assert.Equal(200, result.Response?.Status);
        Assert.Equal("1", result.Response?.Value.GetProperty("id").GetString());
        Assert.Null(result.Operation);
        Assert.Null(result.OperationResult);
        Assert.Equal(1, handler.CallCount);
        await credential.Received(1).GetTokenAsync(
            Arg.Is<TokenRequestContext>(context => context.Scopes.SequenceEqual(new[] { expectedScope })),
            Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("fabric", "/v1/connections", "GET", null, false, 120, false)]
    [InlineData("fabric", "/v1/connections", "HEAD", null, false, 120, false)]
    [InlineData("fabric", "https://evil.example/v1/connections", "POST", null, false, 120, false)]
    [InlineData("powerbi", "/v1/connections", "POST", null, false, 120, false)]
    [InlineData("fabric", "/v1/connections", "POST", "42", false, 120, false)]
    [InlineData("fabric", "/v1/connections", "POST", "{", false, 120, false)]
    [InlineData("powerbi", "/v1.0/myorg/gateways", "POST", null, true, 120, false)]
    [InlineData("fabric", "/v1/connections", "POST", null, false, 120, true)]
    [InlineData("fabric", "/v1/connections", "POST", null, true, 601, false)]
    public async Task Invoke_RejectsInvalidInputBeforeAuthentication(
        string api, string path, string method, string? body, bool sync, double maxWaitSeconds, bool operationResult)
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) => throw new InvalidOperationException("Must not send."));
        using var client = new HttpClient(handler);
        var credential = FabricOperationTestData.Credential();

        await Assert.ThrowsAsync<ApiRequestInputException>(() => new FabricCoreService(client, credential).InvokeApiRequestAsync(
            api, path, method, body, null, new(sync, maxWaitSeconds), operationResult, TestContext.Current.CancellationToken));

        Assert.Equal(0, handler.CallCount);
        Assert.Empty(credential.ReceivedCalls());
    }

    [Fact]
    public async Task Invoke_FabricAcceptedWithoutSync_ReturnsReceiptWithoutPolling()
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(FabricOperationTestData.Accepted()));
        using var client = new HttpClient(handler);

        var result = await new FabricCoreService(client, FabricOperationTestData.Credential()).InvokeApiRequestAsync(
            "fabric", "/v1/workspaces/w/lakehouses/l/tables/t/load", "POST", "{}",
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(202, result.Response?.Status);
        Assert.Equal(FabricOperationStatus.Accepted, result.Operation?.Status);
        Assert.Equal(Guid.Parse(FabricOperationTestData.OperationId), result.Operation?.OperationId);
        Assert.Equal(20, result.Operation?.ServerRetryAfterSeconds);
        Assert.Null(result.OperationResult);
        Assert.Equal(1, handler.CallCount);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Invoke_FabricAcceptedWithSync_WaitsAndReadsResultOnlyWhenRequested(bool operationResult)
    {
        var clock = new OperationTestTimeProvider();
        var paths = new List<string>();
        using var handler = new FabricCoreHttpMessageHandler((request, _) =>
        {
            paths.Add($"{request.Method} {request.RequestUri?.AbsolutePath}");
            return Task.FromResult(paths.Count switch
            {
                1 => FabricOperationTestData.Accepted(),
                2 => FabricOperationTestData.JsonResponse("""{"status":"Succeeded"}"""),
                _ => FabricOperationTestData.JsonResponse("""{"definition":{"parts":[]}}""")
            });
        });
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, FabricOperationTestData.Credential(), clock);

        var task = service.InvokeApiRequestAsync(
            "fabric", "/v1/workspaces/w/items/i/getDefinition", "POST", null, null,
            new(Sync: true), operationResult, TestContext.Current.CancellationToken);
        await clock.WaitForTimerAsync(3, TestContext.Current.CancellationToken);
        clock.Advance(3);
        var result = await task.WaitAsync(TestContext.Current.CancellationToken);

        Assert.Equal(FabricOperationStatus.Succeeded, result.Operation?.Status);
        var operationPath = $"/v1/operations/{FabricOperationTestData.OperationId}";
        Assert.Equal(
            operationResult
                ? ["POST /v1/workspaces/w/items/i/getDefinition", $"GET {operationPath}", $"GET {operationPath}/result"]
                : ["POST /v1/workspaces/w/items/i/getDefinition", $"GET {operationPath}"],
            paths);
        if (operationResult)
        {
            Assert.Equal(Guid.Parse(FabricOperationTestData.OperationId), result.OperationResult?.OperationId);
            Assert.Equal(JsonValueKind.Array, result.OperationResult?.Value.GetProperty("definition").GetProperty("parts").ValueKind);
        }
        else
        {
            Assert.Null(result.OperationResult);
        }
    }

    [Fact]
    public async Task Invoke_CallerCancellationDuringWait_ThrowsResumableReceiptWithoutResubmitting()
    {
        var clock = new OperationTestTimeProvider();
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(FabricOperationTestData.Accepted()));
        using var client = new HttpClient(handler);
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        var service = new FabricCoreService(client, FabricOperationTestData.Credential(), clock);

        var task = service.InvokeApiRequestAsync(
            "fabric", "/v1/workspaces/w/items/i/getDefinition", "POST", null, null, new(Sync: true), true, cancellation.Token);
        await clock.WaitForTimerAsync(3, TestContext.Current.CancellationToken);
        await cancellation.CancelAsync();

        var exception = await Assert.ThrowsAsync<FabricOperationCanceledException>(() => task);
        Assert.Equal(Guid.Parse(FabricOperationTestData.OperationId), exception.Operation.OperationId);
        Assert.Equal("CallerCanceled", exception.Operation.Issue?.Code);
        Assert.Equal(1, handler.CallCount);
    }

    [Theory]
    [InlineData("malformed", "InvalidJson")]
    [InlineData("too-deep", "InvalidJson")]
    [InlineData("oversized", "ResponseTooLarge")]
    [InlineData("non-json", "UnsupportedMediaType")]
    [InlineData("unreadable", "ResponseReadFailed")]
    [InlineData("io", "ResponseReadFailed")]
    public async Task Invoke_FabricAcceptedWithUnreadableBody_KeepsResumableReceipt(string body, string expectedCode)
    {
        foreach (var operationId in new[] { FabricOperationTestData.OperationId, null })
        {
            var response = FabricOperationTestData.Accepted(operationId);
            response.Content = AcceptedContent(body);
            using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
            using var client = new HttpClient(handler);

            var exception = await Assert.ThrowsAsync<FabricOperationReceiptException>(() =>
                new FabricCoreService(client, FabricOperationTestData.Credential()).InvokeApiRequestAsync(
                    "fabric", "/v1/workspaces/w/items/i/getDefinition", "POST", null, 150, new(Sync: true), true,
                    TestContext.Current.CancellationToken));

            Assert.Equal(HttpStatusCode.BadGateway, exception.StatusCode);
            Assert.Equal(expectedCode, exception.Operation.Issue?.Code);
            Assert.Equal(502, exception.Operation.Issue?.HttpStatus);
            Assert.Equal(20, exception.Operation.ServerRetryAfterSeconds);
            Assert.DoesNotContain("private", exception.Message);
            if (operationId is null)
            {
                Assert.Equal(FabricOperationStatus.AcceptedWithoutOperationId, exception.Operation.Status);
                Assert.Null(exception.Operation.OperationId);
            }
            else
            {
                Assert.Equal(FabricOperationStatus.TrackingStopped, exception.Operation.Status);
                Assert.Equal(Guid.Parse(operationId), exception.Operation.OperationId);
                Assert.Contains("do not resubmit", exception.Message);
            }
            Assert.Equal(1, handler.CallCount);
        }
    }

    [Theory]
    [InlineData(FabricApiResponseReader.MaxRetainedJsonDepth, true)]
    [InlineData(FabricApiResponseReader.MaxRetainedJsonDepth + 1, false)]
    public async Task Invoke_OperationResult_EnforcesRetainedDepth(int depth, bool returned)
    {
        var clock = new OperationTestTimeProvider();
        var result = new string('[', depth) + new string(']', depth);
        var calls = 0;
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(++calls switch
        {
            1 => FabricOperationTestData.Accepted(),
            2 => FabricOperationTestData.JsonResponse("""{"status":"Succeeded"}"""),
            _ => FabricOperationTestData.JsonResponse(result)
        }));
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, FabricOperationTestData.Credential(), clock);

        var task = service.InvokeApiRequestAsync(
            "fabric", "/v1/workspaces/w/items/i/getDefinition", "POST", null, null,
            new(Sync: true), true, TestContext.Current.CancellationToken);
        await clock.WaitForTimerAsync(3, TestContext.Current.CancellationToken);
        clock.Advance(3);
        var outcome = await task.WaitAsync(TestContext.Current.CancellationToken);

        if (returned)
        {
            Assert.Equal(FabricOperationStatus.Succeeded, outcome.Operation?.Status);
            Assert.Equal(JsonValueKind.Array, outcome.OperationResult?.Value.ValueKind);
        }
        else
        {
            Assert.Equal(FabricOperationStatus.ResultUnavailable, outcome.Operation?.Status);
            Assert.Equal("InvalidJson", outcome.Operation?.Issue?.Code);
            Assert.Null(outcome.OperationResult);
        }
        Assert.Equal(3, handler.CallCount);
    }

    [Fact]
    public async Task Invoke_PowerBIAccepted_ReturnsLocationWithoutPolling()
    {
        var response = new HttpResponseMessage(HttpStatusCode.Accepted) { Content = new ByteArrayContent([]) };
        response.Headers.TryAddWithoutValidation("Location", "https://api.powerbi.com/v1.0/myorg/groups/g/datasets/d/refreshes/r");
        response.Headers.TryAddWithoutValidation("Retry-After", "15");
        response.Headers.TryAddWithoutValidation("x-ms-operation-id", FabricOperationTestData.OperationId);
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
        using var client = new HttpClient(handler);

        var result = await new FabricCoreService(client, FabricOperationTestData.Credential()).InvokeApiRequestAsync(
            "powerbi", "/v1.0/myorg/groups/g/datasets/d/refreshes", "POST", """{"type":"Full"}""",
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(202, result.Response?.Status);
        Assert.Equal("/v1.0/myorg/groups/g/datasets/d/refreshes/r", result.Response?.Location);
        Assert.Equal(15, result.Response?.RetryAfterSeconds);
        Assert.Null(result.Operation);
        Assert.Equal(1, handler.CallCount);
    }

    [Theory]
    [InlineData(HttpStatusCode.Conflict)]
    [InlineData(HttpStatusCode.TooManyRequests)]
    [InlineData(HttpStatusCode.ServiceUnavailable)]
    public async Task Invoke_ThrowsSanitizedErrorsWithoutRetrying(HttpStatusCode status)
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(FabricOperationTestData.JsonResponse(
            """{"errorCode":"Conflict","message":"private backend detail"}""", status, "5")));
        using var client = new HttpClient(handler);

        var exception = await Assert.ThrowsAsync<ApiRequestException>(() => new FabricCoreService(client, FabricOperationTestData.Credential())
            .InvokeApiRequestAsync("fabric", "/v1/connections", "POST", "{}", cancellationToken: TestContext.Current.CancellationToken));

        Assert.Equal(status, exception.StatusCode);
        Assert.Equal("Conflict", exception.ErrorCode);
        Assert.DoesNotContain("private", exception.Message);
        Assert.Equal(1, handler.CallCount);
    }

    private static HttpContent AcceptedContent(string body)
    {
        HttpContent content = body switch
        {
            "malformed" => new StringContent("{\"private\":", Encoding.UTF8, "application/json"),
            "too-deep" => new StringContent(new string('[', 64) + new string(']', 64), Encoding.UTF8, "application/json"),
            "oversized" => new StringContent("{\"value\":\"" + new string('a', 200) + "\"}", Encoding.UTF8, "application/json"),
            "non-json" => new StringContent("private", Encoding.UTF8, "text/plain"),
            "unreadable" => new UnreadableHttpContent(),
            "io" => new StreamContent(FailingStream()),
            _ => throw new ArgumentOutOfRangeException(nameof(body))
        };
        content.Headers.ContentType ??= new MediaTypeHeaderValue("application/json");
        return content;
    }

    private static Stream FailingStream()
    {
        var stream = Substitute.For<Stream>();
        stream.CanRead.Returns(true);
        stream.Read(Arg.Any<byte[]>(), Arg.Any<int>(), Arg.Any<int>()).Returns(_ => throw new IOException("private"));
        stream.ReadAsync(Arg.Any<byte[]>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns<Task<int>>(_ => throw new IOException("private"));
        stream.ReadAsync(Arg.Any<Memory<byte>>(), Arg.Any<CancellationToken>())
            .Returns<ValueTask<int>>(_ => throw new IOException("private"));
        return stream;
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
