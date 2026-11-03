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
using static Fabric.Mcp.Tools.Core.Tests.TestSupport.ItemJobInstanceTestData;

namespace Fabric.Mcp.Tools.Core.Tests.Services;

public sealed class ItemJobInstanceGetServiceTests()
{
    [Theory]
    [InlineData("NotStarted", "Manual")]
    [InlineData("InProgress", "Scheduled")]
    [InlineData("Completed", "Manual")]
    [InlineData("Failed", "Scheduled")]
    [InlineData("Cancelled", "Manual")]
    [InlineData("Deduped", "Scheduled")]
    [InlineData("FutureStatus", "FutureInvocation")]
    public static async Task Get_PreservesSnapshotInOneAuthenticatedNormalizedRead(string status, string invokeType)
    {
        var json = JsonNode.Parse(Json)!.AsObject();
        json["status"] = status;
        json["invokeType"] = invokeType;
        json["jobType"] = "FutureWorkloadJob";
        using var stream = new OperationTestStream(Encoding.UTF8.GetBytes(json.ToJsonString()));
        using var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StreamContent(stream) };
        response.Content.Headers.ContentType = new("application/json");
        response.Headers.Location = new Uri("https://example.invalid/never-follow");
        response.Headers.TryAddWithoutValidation("x-ms-operation-id", "not-a-job-instance-id");
        using var handler = new FabricCoreHttpMessageHandler((request, token) =>
        {
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal($"{FabricEndpoints.GetFabricApiBaseUrl()}/workspaces/{WorkspaceId}/items/{ItemId}/jobs/instances/{JobInstanceId}",
                request.RequestUri?.AbsoluteUri);
            Assert.Null(request.Content);
            Assert.Equal("Bearer", request.Headers.Authorization?.Scheme);
            Assert.Equal("job-test-token", request.Headers.Authorization?.Parameter);
            Assert.Equal("Fabric Core MCP", request.Headers.UserAgent.ToString());
            Assert.True(token.CanBeCanceled);
            return Task.FromResult(response);
        });
        using var client = new HttpClient(handler);
        var credential = Credential();
        var service = new FabricCoreService(client, credential);

        var result = await service.GetItemJobInstanceAsync(
            $"{{{WorkspaceId.ToUpperInvariant()}}}", Guid.Parse(ItemId).ToString("N").ToUpperInvariant(),
            $"({JobInstanceId.ToUpperInvariant()})", TestContext.Current.CancellationToken);

        Assert.Equal(Guid.Parse(JobInstanceId), result.JobInstance.Id);
        Assert.Equal(Guid.Parse(ItemId), result.JobInstance.ItemId);
        Assert.Equal("FutureWorkloadJob", result.JobInstance.JobType);
        Assert.Equal(invokeType, result.JobInstance.InvokeType);
        Assert.Equal(status, result.JobInstance.Status);
        Assert.Equal(Guid.Parse(ActivityId), result.JobInstance.RootActivityId);
        Assert.Equal("2023-04-22T06:35:00.7812154", result.JobInstance.StartTimeUtc);
        Assert.Equal("2023-04-22T06:35:00.8033333", result.JobInstance.EndTimeUtc);
        Assert.Null(result.JobInstance.FailureReason);
        Assert.Null(result.RetryAfterSeconds);
        Assert.Null(result.RetryAfterUtc);
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
    public static async Task Get_AllowsAbsentTimingTraceAndFailureWithoutInventingValues(bool explicitNull)
    {
        var json = JsonNode.Parse(Json)!.AsObject();
        json["status"] = "NotStarted";
        foreach (var name in new[] { "rootActivityId", "startTimeUtc", "endTimeUtc", "failureReason" })
        {
            if (explicitNull)
            {
                json[name] = null;
            }
            else
            {
                json.Remove(name);
            }
        }
        using var response = Response(json.ToJsonString());
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
        using var client = new HttpClient(handler);

        var result = await ReadAsync(new FabricCoreService(client, Credential()));
        var output = JsonSerializer.SerializeToElement(result, CoreJsonContext.Default.ItemJobInstanceGetCommandResult);

        Assert.Equal(["id", "invokeType", "itemId", "jobType", "status"],
            output.GetProperty("jobInstance").EnumerateObject().Select(property => property.Name).Order());
        Assert.Equal("NotStarted", result.JobInstance.Status);
        Assert.Equal(1, handler.CallCount);
    }

    [Theory]
    [InlineData("2023-04-22T06:35:00.7812154", "2023-04-22T06:35:00.8033333")]
    [InlineData("2023-04-22T06:35:00Z", "2023-04-22T06:35:00.8033333+00:00")]
    [InlineData("", null)]
    public static async Task Get_RoundTripsDocumentedStringTimestampsWithoutHostTimeZoneSemantics(string start, string? end)
    {
        var json = JsonNode.Parse(Json)!.AsObject();
        json["startTimeUtc"] = start;
        json["endTimeUtc"] = end;
        using var response = Response(json.ToJsonString());
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
        using var client = new HttpClient(handler);

        var result = await ReadAsync(new FabricCoreService(client, Credential()));
        var roundTrip = JsonSerializer.Deserialize(JsonSerializer.Serialize(result, CoreJsonContext.Default.ItemJobInstanceGetCommandResult),
            CoreJsonContext.Default.ItemJobInstanceGetCommandResult);

        Assert.Equal(start, roundTrip?.JobInstance.StartTimeUtc);
        Assert.Equal(end, roundTrip?.JobInstance.EndTimeUtc);
        Assert.Equal(1, handler.CallCount);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    [InlineData(null)]
    public static async Task Get_ExposesOnlySafeFailureFieldsAndKeepsFailedStatus(bool? isRetriable)
    {
        var json = JsonNode.Parse(Json)!.AsObject();
        json["status"] = "Failed";
        json["failureReason"] = new JsonObject
        {
            ["errorCode"] = "Job_Failed.1",
            ["requestId"] = ActivityId,
            ["isRetriable"] = isRetriable,
            ["message"] = FabricCoreErrorTestData.PrivateDetails,
            ["moreDetails"] = new JsonArray(new JsonObject { ["message"] = "private-details" }),
            ["parameters"] = new JsonArray(new JsonObject { ["value"] = "private-parameter" }),
            ["relatedResource"] = new JsonObject { ["resourceId"] = "private-resource" }
        };
        json["result"] = "private-result";
        using var response = Response(json.ToJsonString());
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
        using var client = new HttpClient(handler);

        var result = await ReadAsync(new FabricCoreService(client, Credential()));
        var output = JsonSerializer.Serialize(result, CoreJsonContext.Default.ItemJobInstanceGetCommandResult);

        Assert.Equal("Failed", result.JobInstance.Status);
        Assert.Equal(new FabricJobFailure("Job_Failed.1", Guid.Parse(ActivityId), isRetriable), result.JobInstance.FailureReason);
        Assert.DoesNotContain("private", output);
        Assert.DoesNotContain("message", output);
        Assert.DoesNotContain("stackTrace", output);
        Assert.Equal(1, handler.CallCount);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("private detail")]
    [InlineData("private\r\nheader")]
    [InlineData("https://example.invalid/private")]
    [InlineData(null)]
    public static async Task Get_RedactsNonIdentifierFailureCodesAndEmptyRequestIds(string? code)
    {
        var json = JsonNode.Parse(Json)!.AsObject();
        json["status"] = "Failed";
        json["failureReason"] = new JsonObject { ["errorCode"] = code, ["requestId"] = Guid.Empty.ToString() };
        using var response = Response(json.ToJsonString());
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
        using var client = new HttpClient(handler);

        var result = await ReadAsync(new FabricCoreService(client, Credential()));

        Assert.Equal("Failed", result.JobInstance.Status);
        Assert.Equal(new FabricJobFailure(), result.JobInstance.FailureReason);
        Assert.Equal(1, handler.CallCount);
    }

    [Theory]
    [InlineData("0", 0L)]
    [InlineData("45", 45L)]
    [InlineData("2147483647", 2147483647L)]
    [InlineData(null, null)]
    [InlineData("", null)]
    [InlineData("-1", null)]
    [InlineData("1.5", null)]
    [InlineData("2147483648", null)]
    [InlineData("9223372036854775807", null)]
    [InlineData("45, 60", null)]
    [InlineData("private-invalid", null)]
    public static async Task Get_ReturnsOnlyValidatedRetrySecondsWithoutFallbackOrPolling(string? header, long? expected)
    {
        using var response = Response(retryAfter: header);
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
        using var client = new HttpClient(handler);

        var result = await ReadAsync(new FabricCoreService(client, Credential()));

        Assert.Equal(expected, result.RetryAfterSeconds);
        Assert.Null(result.RetryAfterUtc);
        Assert.Equal(1, handler.CallCount);
    }

    [Fact]
    public static async Task Get_PreservesHttpDateRetryGuidanceWithoutInventingARelativeDelay()
    {
        using var response = Response(retryAfter: "Tue, 01 Jan 2030 00:00:00 GMT");
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
        using var client = new HttpClient(handler);

        var result = await ReadAsync(new FabricCoreService(client, Credential()));

        Assert.Null(result.RetryAfterSeconds);
        Assert.Equal(new DateTimeOffset(2030, 1, 1, 0, 0, 0, TimeSpan.Zero), result.RetryAfterUtc);
        Assert.Equal(1, handler.CallCount);
    }

    [Theory]
    [InlineData(HttpStatusCode.OK)]
    [InlineData(HttpStatusCode.TooManyRequests)]
    [InlineData(HttpStatusCode.ServiceUnavailable)]
    public static async Task Get_RejectsAmbiguousRetryHeaders(HttpStatusCode status)
    {
        using var response = Response(status: status);
        response.Headers.TryAddWithoutValidation("Retry-After", ["45", "60"]);
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, Credential());

        if (status == HttpStatusCode.OK)
        {
            var result = await ReadAsync(service);
            Assert.Null(result.RetryAfterSeconds);
            Assert.Null(result.RetryAfterUtc);
        }
        else
        {
            var error = await Assert.ThrowsAsync<ItemJobInstanceRequestException>(() => ReadAsync(service));
            Assert.Equal(status, error.StatusCode);
            Assert.Null(error.RetryAfter);
        }
        Assert.Equal(1, handler.CallCount);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("00000000-0000-0000-0000-000000000000")]
    [InlineData("../jobs/instances")]
    [InlineData("https://example.invalid")]
    [InlineData(JobInstanceId + "?query=private")]
    [InlineData(JobInstanceId + "/result")]
    public static async Task Get_ValidatesEveryRouteIdBeforeAuthentication(string? value)
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) => throw new InvalidOperationException("No HTTP expected."));
        using var client = new HttpClient(handler);
        var credential = Credential();
        var service = new FabricCoreService(client, credential);

        Assert.Equal("workspaceId", (await Assert.ThrowsAsync<ArgumentException>(() =>
            service.GetItemJobInstanceAsync(value!, ItemId, JobInstanceId, TestContext.Current.CancellationToken))).ParamName);
        Assert.Equal("itemId", (await Assert.ThrowsAsync<ArgumentException>(() =>
            service.GetItemJobInstanceAsync(WorkspaceId, value!, JobInstanceId, TestContext.Current.CancellationToken))).ParamName);
        Assert.Equal("jobInstanceId", (await Assert.ThrowsAsync<ArgumentException>(() =>
            service.GetItemJobInstanceAsync(WorkspaceId, ItemId, value!, TestContext.Current.CancellationToken))).ParamName);
        Assert.Empty(credential.ReceivedCalls());
        Assert.Equal(0, handler.CallCount);
    }

    [Theory]
    [InlineData(HttpStatusCode.Created)]
    [InlineData(HttpStatusCode.Accepted)]
    [InlineData(HttpStatusCode.NoContent)]
    [InlineData(HttpStatusCode.PartialContent)]
    [InlineData(HttpStatusCode.MovedPermanently)]
    [InlineData(HttpStatusCode.Found)]
    [InlineData(HttpStatusCode.TemporaryRedirect)]
    [InlineData(HttpStatusCode.PermanentRedirect)]
    [InlineData(HttpStatusCode.BadRequest)]
    [InlineData(HttpStatusCode.Unauthorized)]
    [InlineData(HttpStatusCode.Forbidden)]
    [InlineData(HttpStatusCode.NotFound)]
    [InlineData(HttpStatusCode.TooManyRequests)]
    [InlineData(HttpStatusCode.InternalServerError)]
    [InlineData(HttpStatusCode.ServiceUnavailable)]
    public static async Task Get_RejectsOtherStatusesWithoutReadingBodiesOrFollowingLocations(HttpStatusCode status)
    {
        using var content = new UnreadableHttpContent();
        using var response = new HttpResponseMessage(status) { Content = content };
        response.Headers.Location = new Uri("https://example.invalid/private");
        response.Headers.TryAddWithoutValidation("Retry-After", "45");
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
        using var client = new HttpClient(handler);

        var error = await Assert.ThrowsAsync<ItemJobInstanceRequestException>(() => ReadAsync(new FabricCoreService(client, Credential())));

        Assert.Equal(response.IsSuccessStatusCode ? HttpStatusCode.BadGateway : status, error.StatusCode);
        Assert.Equal(TimeSpan.FromSeconds(45), error.RetryAfter?.Delta);
        Assert.DoesNotContain("private", error.Message);
        Assert.Equal(1, handler.CallCount);
        Assert.True(content.IsDisposed);
    }

    [Theory]
    [InlineData("")]
    [InlineData("{private")]
    [InlineData("null")]
    [InlineData("[]")]
    [InlineData("{}")]
    [InlineData("123")]
    public static async Task Get_RejectsMalformedOrEmptySnapshots(string json)
    {
        using var response = Response(json);
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
        using var client = new HttpClient(handler);

        var error = await Assert.ThrowsAsync<JsonException>(() => ReadAsync(new FabricCoreService(client, Credential())));

        Assert.DoesNotContain("private", error.Message);
        Assert.Equal(1, handler.CallCount);
        Assert.Throws<ObjectDisposedException>(() => response.Content.ReadAsStream(TestContext.Current.CancellationToken));
    }

    [Theory]
    [InlineData("id", "\"aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa\"")]
    [InlineData("itemId", "\"aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa\"")]
    [InlineData("id", "\"00000000-0000-0000-0000-000000000000\"")]
    [InlineData("itemId", "\"private-not-a-uuid\"")]
    [InlineData("id", "null")]
    [InlineData("jobType", "\" \"")]
    [InlineData("invokeType", "null")]
    [InlineData("status", "\"\"")]
    [InlineData("status", "42")]
    [InlineData("startTimeUtc", "42")]
    [InlineData("endTimeUtc", "{}")]
    [InlineData("rootActivityId", "\"private-invalid\"")]
    [InlineData("failureReason", "[]")]
    [InlineData("failureReason", """{"requestId":"private-invalid"}""")]
    [InlineData("failureReason", """{"errorCode":42}""")]
    [InlineData("failureReason", """{"isRetriable":"true"}""")]
    public static async Task Get_RejectsMismatchedIdsAndInvalidFieldTypes(string name, string value)
    {
        var json = JsonNode.Parse(Json)!.AsObject();
        json[name] = JsonNode.Parse(value);
        using var response = Response(json.ToJsonString());
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
        using var client = new HttpClient(handler);

        var error = await Assert.ThrowsAsync<JsonException>(() => ReadAsync(new FabricCoreService(client, Credential())));

        Assert.DoesNotContain("private", error.Message);
        Assert.Equal(1, handler.CallCount);
        Assert.Throws<ObjectDisposedException>(() => response.Content.ReadAsStream(TestContext.Current.CancellationToken));
    }

    [Theory]
    [InlineData("id")]
    [InlineData("itemId")]
    [InlineData("jobType")]
    [InlineData("invokeType")]
    [InlineData("status")]
    public static async Task Get_RejectsSnapshotsWithoutIdentityOrState(string name)
    {
        var json = JsonNode.Parse(Json)!.AsObject();
        json.Remove(name);
        using var response = Response(json.ToJsonString());
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
        using var client = new HttpClient(handler);

        await Assert.ThrowsAsync<JsonException>(() => ReadAsync(new FabricCoreService(client, Credential())));

        Assert.Equal(1, handler.CallCount);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("text/html")]
    [InlineData("application/octet-stream")]
    public static async Task Get_RejectsUnsupportedMediaBeforeReading(string? mediaType)
    {
        using var content = new UnreadableHttpContent();
        content.Headers.ContentType = mediaType is null ? null : new(mediaType);
        using var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = content };
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
        using var client = new HttpClient(handler);

        await Assert.ThrowsAsync<JsonException>(() => ReadAsync(new FabricCoreService(client, Credential())));

        Assert.True(content.IsDisposed);
        Assert.Equal(1, handler.CallCount);
    }

    [Theory]
    [InlineData(false, 0)]
    [InlineData(false, 1)]
    [InlineData(true, 0)]
    [InlineData(true, 1)]
    public static async Task Get_EnforcesExactByteLimitWithAndWithoutContentLength(bool knownLength, int excess)
    {
        const int limit = 1024 * 1024;
        Assert.Equal(limit, FabricResponseBodyReader.DefaultMaxJsonBytes);
        using var stream = new PaddedJsonTestStream(Encoding.UTF8.GetBytes(Json), limit + excess);
        using var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StreamContent(stream) };
        response.Content.Headers.ContentType = new("application/json");
        if (knownLength)
        {
            response.Content.Headers.ContentLength = limit + excess;
        }
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, Credential());

        if (excess == 0)
        {
            Assert.Equal(Guid.Parse(JobInstanceId), (await ReadAsync(service)).JobInstance.Id);
        }
        else
        {
            var error = await Assert.ThrowsAsync<JsonException>(() => ReadAsync(service));
            Assert.Contains("1 MiB", error.Message);
        }
        Assert.Equal(knownLength && excess > 0 ? 0 : limit + excess, stream.BytesRead);
        Assert.True(stream.Disposed);
        Assert.Equal(1, handler.CallCount);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public static async Task Get_RejectsInvalidUtf8AndExcessiveDepth(bool deep)
    {
        var body = deep
            ? Encoding.UTF8.GetBytes(Json[..Json.LastIndexOf('}')] + ",\"extra\":" + new string('[', 65) + "0" + new string(']', 65) + "}")
            : Encoding.UTF8.GetBytes(Json.Replace("DefaultJob", "\u00e9", StringComparison.Ordinal));
        if (!deep)
        {
            body[Array.IndexOf(body, (byte)0xc3)] = 0xff;
        }
        using var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(body) };
        response.Content.Headers.ContentType = new("application/json");
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
        using var client = new HttpClient(handler);

        await Assert.ThrowsAsync<JsonException>(() => ReadAsync(new FabricCoreService(client, Credential())));

        Assert.Equal(1, handler.CallCount);
    }

    [Fact]
    public static async Task Get_PropagatesAuthenticationFailureBeforeHttp()
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) => throw new InvalidOperationException("No HTTP expected."));
        using var client = new HttpClient(handler);
        var credential = Credential();
        credential.GetTokenAsync(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>())
            .Returns(ValueTask.FromException<AccessToken>(new AuthenticationFailedException(FabricCoreErrorTestData.PrivateDetails)));

        await Assert.ThrowsAsync<AuthenticationFailedException>(() => ReadAsync(new FabricCoreService(client, credential)));

        Assert.Equal(0, handler.CallCount);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public static async Task Get_CancelsBeforeHttpIncludingCredentialsThatIgnoreCancellation(bool duringAuth)
    {
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        var credential = Credential();
        credential.GetTokenAsync(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>())
            .Returns(_ =>
            {
                cancellation.Cancel();
                return new ValueTask<AccessToken>(new AccessToken("job-test-token", DateTimeOffset.MaxValue));
            });
        if (!duringAuth)
        {
            await cancellation.CancelAsync();
        }
        using var handler = new FabricCoreHttpMessageHandler((_, _) => throw new InvalidOperationException("No HTTP expected."));
        using var client = new HttpClient(handler);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            ReadAsync(new FabricCoreService(client, credential), cancellation.Token));

        Assert.Equal(0, handler.CallCount);
        await credential.Received(duringAuth ? 1 : 0).GetTokenAsync(Arg.Any<TokenRequestContext>(), cancellation.Token);
    }

    [Theory]
    [InlineData("http")]
    [InlineData("headers")]
    [InlineData("body")]
    [InlineData("end-of-body")]
    public static async Task Get_CancelsInFlightAndDisposesAvailableResponse(string phase)
    {
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        using var stream = new OperationTestStream(Encoding.UTF8.GetBytes(Json), (count, token) =>
        {
            if (phase == "body" || (phase == "end-of-body" && count == 0))
            {
                cancellation.Cancel();
                if (phase == "body")
                {
                    token.ThrowIfCancellationRequested();
                }
            }
            return ValueTask.CompletedTask;
        });
        using var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StreamContent(stream) };
        response.Content.Headers.ContentType = new("application/json");
        using var handler = new FabricCoreHttpMessageHandler((_, token) =>
        {
            if (phase is "http" or "headers")
            {
                cancellation.Cancel();
            }
            return phase == "http" ? Task.FromCanceled<HttpResponseMessage>(token) : Task.FromResult(response);
        });
        using var client = new HttpClient(handler);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            ReadAsync(new FabricCoreService(client, Credential()), cancellation.Token));

        Assert.Equal(1, handler.CallCount);
        if (phase != "http")
        {
            Assert.True(stream.Disposed);
            Assert.Throws<ObjectDisposedException>(() => response.Content.ReadAsStream(TestContext.Current.CancellationToken));
        }
    }

    [Fact]
    public static async Task Get_KeepsConcurrentIdentitiesRequestsAndSnapshotsIsolated()
    {
        var identity = new AsyncLocal<string>();
        var credential = Credential();
        credential.GetTokenAsync(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>())
            .Returns(_ => new AccessToken(identity.Value!, DateTimeOffset.MaxValue));
        var requests = new ConcurrentBag<string>();
        using var handler = new FabricCoreHttpMessageHandler(async (request, _) =>
        {
            var id = request.RequestUri!.Segments[^1];
            Assert.Equal(id, request.Headers.Authorization?.Parameter);
            requests.Add(id);
            await Task.Yield();
            var json = JsonNode.Parse(Json)!.AsObject();
            json["id"] = id;
            return Response(json.ToJsonString());
        });
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, credential);

        await Task.WhenAll(Enumerable.Range(0, 8).Select(async _ =>
        {
            var id = Guid.NewGuid();
            identity.Value = id.ToString();
            var result = await service.GetItemJobInstanceAsync(WorkspaceId, ItemId, id.ToString(), TestContext.Current.CancellationToken);
            Assert.Equal(id, result.JobInstance.Id);
        }));

        Assert.Equal(8, handler.CallCount);
        Assert.Equal(8, requests.Distinct().Count());
        Assert.Null(client.DefaultRequestHeaders.Authorization);
        await credential.Received(8).GetTokenAsync(Arg.Any<TokenRequestContext>(), TestContext.Current.CancellationToken);
    }

    private static Task<ItemJobInstanceGetCommandResult> ReadAsync(FabricCoreService service, CancellationToken? cancellationToken = null) =>
        service.GetItemJobInstanceAsync(WorkspaceId, ItemId, JobInstanceId, cancellationToken ?? TestContext.Current.CancellationToken);
}
