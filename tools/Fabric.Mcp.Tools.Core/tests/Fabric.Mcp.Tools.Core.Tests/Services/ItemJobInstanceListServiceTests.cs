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
using static Fabric.Mcp.Tools.Core.Tests.TestSupport.ItemJobInstanceListTestData;
using static Fabric.Mcp.Tools.Core.Tests.TestSupport.ItemJobInstanceTestData;

namespace Fabric.Mcp.Tools.Core.Tests.Services;

public sealed class ItemJobInstanceListServiceTests()
{
    [Theory]
    [InlineData(null, false)]
    [InlineData(DocumentedToken, false)]
    [InlineData(DocumentedToken, true)]
    [InlineData(" raw+/= &?# ", true)]
    public static async Task List_ReadsOneAuthenticatedPageAndPreservesOpaqueMetadata(string? nextToken, bool includeUri)
    {
        using var stream = new OperationTestStream(Encoding.UTF8.GetBytes(Page(nextToken, includeUri)));
        using var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StreamContent(stream) };
        response.Content.Headers.ContentType = new("application/json");
        response.Headers.Location = new Uri("https://example.invalid/never-follow");
        response.Headers.TryAddWithoutValidation("x-ms-operation-id", "not-a-job-instance-id");
        using var handler = new FabricCoreHttpMessageHandler((request, token) =>
        {
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal(CollectionUrl, request.RequestUri?.AbsoluteUri);
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

        var result = await service.ListItemJobInstancesAsync(
            $"{{{WorkspaceId.ToUpperInvariant()}}}", Guid.Parse(ItemId).ToString("N").ToUpperInvariant(),
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(Instance(), Assert.Single(result.JobInstances));
        Assert.Equal(nextToken, result.ContinuationToken);
        var output = JsonSerializer.Serialize(result, CoreJsonContext.Default.ItemJobInstanceListCommandResult);
        Assert.DoesNotContain("continuationUri", output);
        Assert.DoesNotContain("operationId", output);
        Assert.DoesNotContain("http", output);
        Assert.Equal(1, handler.CallCount);
        Assert.True(stream.Disposed);
        Assert.Throws<ObjectDisposedException>(() => response.Content.ReadAsStream(TestContext.Current.CancellationToken));
        await credential.Received(1).GetTokenAsync(
            Arg.Is<TokenRequestContext>(context => context.Scopes.SequenceEqual(FabricEndpoints.FabricScopes)),
            TestContext.Current.CancellationToken);
    }

    [Theory]
    [InlineData(DocumentedToken, DocumentedToken)]
    [InlineData("raw+/= &?#", "raw%2B%2F%3D%20%26%3F%23")]
    [InlineData(" raw ", "%20raw%20")]
    [InlineData("%3D%2f%26%3F%23%2B%25", "%3D%2f%26%3F%23%2B%25")]
    // cspell:disable-next-line
    [InlineData("x%2F+y%3D?b=two#frag&extra", "x%2F%2By%3D%3Fb%3Dtwo%23frag%26extra")]
    [InlineData("%G1%", "%25G1%25")]
    [InlineData("tail%2", "tail%252")]
    [InlineData("%2541%2526", "%2541%2526")]
    [InlineData("%41%7e%2D%5f%2E", "A~-_.")]
    [InlineData("\u00e9\U0001F680 /", "%C3%A9%F0%9F%9A%80%20%2F")]
    [InlineData("%0D%0A", "%0D%0A")]
    public static async Task List_RequiresAnExplicitSecondCallAndEncodesOnlyRawTokenSpans(string token, string encoded)
    {
        var requests = new List<string>();
        using var handler = new FabricCoreHttpMessageHandler((request, _) =>
        {
            requests.Add(request.RequestUri!.AbsoluteUri);
            Assert.Empty(request.RequestUri.Fragment);
            return Task.FromResult(Response(requests.Count == 1 ? Page(token, includeUri: true) : """{"value":[]}"""));
        });
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, Credential());

        var first = await ReadAsync(service);
        Assert.Equal(token, first.ContinuationToken);
        Assert.Equal([CollectionUrl], requests);
        var second = await ReadAsync(service, first.ContinuationToken);

        Assert.Empty(second.JobInstances);
        Assert.Null(second.ContinuationToken);
        Assert.Equal([CollectionUrl, $"{CollectionUrl}?continuationToken={encoded}"], requests);
        Assert.Equal(2, handler.CallCount);
    }

    [Theory]
    [InlineData("opaque/../cursor", "opaque/../cursor")]
    [InlineData("opaque/./cursor", "opaque/./cursor")]
    // cspell:disable
    [InlineData("opaque/../cursor", "opaque%2F..%2Fcursor")]
    [InlineData("opaque/./cursor", "opaque%2F.%2Fcursor")]
    [InlineData("opaque%2F..%2Fcursor", "opaque/../cursor")]
    [InlineData("opaque%2F.%2Fcursor", "opaque/./cursor")]
    // cspell:enable
    [InlineData("opaque/../cursor", "opaque/%2E%2E/cursor")]
    [InlineData("opaque/./cursor", "opaque/%2E/cursor")]
    public static async Task List_AcceptsOpaqueDotSegmentsInContinuationQueryWithoutChangingTheCollection(
        string token, string queryValue)
    {
        var page = JsonNode.Parse(Page(token))!.AsObject();
        page["continuationUri"] = $"{CollectionUrl}?continuationToken={queryValue}";
        var requests = new List<string>();
        using var handler = new FabricCoreHttpMessageHandler((request, _) =>
        {
            Assert.Equal(new Uri(CollectionUrl).AbsolutePath, request.RequestUri!.AbsolutePath);
            requests.Add(request.RequestUri.AbsoluteUri);
            return Task.FromResult(Response(requests.Count == 1 ? page.ToJsonString() : """{"value":[]}"""));
        });
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, Credential());

        var first = await ReadAsync(service);

        Assert.Equal(token, first.ContinuationToken);
        Assert.Equal([CollectionUrl], requests);
        Assert.DoesNotContain("continuationUri", JsonSerializer.Serialize(first, CoreJsonContext.Default.ItemJobInstanceListCommandResult));

        var second = await ReadAsync(service, first.ContinuationToken);

        Assert.Empty(second.JobInstances);
        Assert.Null(second.ContinuationToken);
        Assert.Equal([CollectionUrl, $"{CollectionUrl}?continuationToken={FabricCoreHttpHelpers.EncodeContinuationToken(token)}"], requests);
        Assert.Equal(2, handler.CallCount);
    }

    [Fact]
    public static async Task List_PreservesMoreThan100ActiveRowsTheirOrderAndRepeatedEntriesWithoutPerJobRequests()
    {
        var instances = Enumerable.Range(1, 101).Select(index => Instance() with
        {
            Id = Guid.Parse($"00000000-0000-4000-8000-{index:000000000000}"),
            Status = "InProgress"
        }).Reverse().ToList();
        instances.Add(instances[0]);
        var values = new JsonArray(instances.Select(instance =>
            JsonSerializer.SerializeToNode(instance, CoreJsonContext.Default.FabricItemJobInstance)).ToArray());
        using var response = Response(new JsonObject { ["value"] = values }.ToJsonString());
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
        using var client = new HttpClient(handler);

        var result = await ReadAsync(new FabricCoreService(client, Credential()));

        Assert.Equal(instances, result.JobInstances);
        Assert.Equal(102, result.JobInstances.Count);
        Assert.Null(result.ContinuationToken);
        Assert.Equal(1, handler.CallCount);
    }

    [Theory]
    [InlineData("NotStarted", "Manual", null, null)]
    [InlineData("InProgress", "Scheduled", "2024-06-22T06:35:00.7812154", null)]
    [InlineData("Completed", "Manual", "2024-06-22T06:35:00Z", "2024-06-22T06:35:00.8033333+00:00")]
    [InlineData("Failed", "Scheduled", "", "")]
    [InlineData("Cancelled", "Manual", null, null)]
    [InlineData("Deduped", "Scheduled", null, null)]
    [InlineData("FutureStatus", "FutureInvocation", null, null)]
    public static async Task List_PreservesFutureStringsAndUtcTextWithOnlySafeFailureFields(
        string status, string invocation, string? start, string? end)
    {
        var instance = JsonNode.Parse(Json)!.AsObject();
        instance["status"] = status;
        instance["invokeType"] = invocation;
        instance["jobType"] = "FutureWorkloadJob";
        instance["startTimeUtc"] = start;
        instance["endTimeUtc"] = end;
        instance["failureReason"] = new JsonObject
        {
            ["errorCode"] = "Job_Failed.1",
            ["requestId"] = ActivityId,
            ["isRetriable"] = false,
            ["message"] = FabricCoreErrorTestData.PrivateDetails,
            ["moreDetails"] = new JsonArray(new JsonObject { ["message"] = "private-details" }),
            ["parameters"] = new JsonArray(new JsonObject { ["value"] = "private-parameter" }),
            ["relatedResource"] = new JsonObject { ["resourceId"] = "private-resource" }
        };
        instance["result"] = "private-result";
        using var response = Response(new JsonObject { ["value"] = new JsonArray(instance) }.ToJsonString());
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
        using var client = new HttpClient(handler);

        var result = await ReadAsync(new FabricCoreService(client, Credential()));
        var snapshot = Assert.Single(result.JobInstances);
        var output = JsonSerializer.Serialize(result, CoreJsonContext.Default.ItemJobInstanceListCommandResult);

        Assert.Equal(status, snapshot.Status);
        Assert.Equal(invocation, snapshot.InvokeType);
        Assert.Equal("FutureWorkloadJob", snapshot.JobType);
        Assert.Equal(start, snapshot.StartTimeUtc);
        Assert.Equal(end, snapshot.EndTimeUtc);
        Assert.Equal(new FabricJobFailure("Job_Failed.1", Guid.Parse(ActivityId), false), snapshot.FailureReason);
        Assert.DoesNotContain("private", output);
        Assert.DoesNotContain("message", output);
        Assert.Equal(1, handler.CallCount);
    }

    [Theory]
    [InlineData("private detail")]
    [InlineData("https://example.invalid/private")]
    [InlineData("")]
    [InlineData(null)]
    public static async Task List_RedactsUnsafeFailureCodesAndEmptyRequestIds(string? code)
    {
        var instance = Instance() with { FailureReason = new(code, Guid.Empty, true) };
        var json = JsonSerializer.Serialize(instance, CoreJsonContext.Default.FabricItemJobInstance);
        using var response = Response($"{{\"value\":[{json}]}}");
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
        using var client = new HttpClient(handler);

        var result = await ReadAsync(new FabricCoreService(client, Credential()));

        Assert.Equal(new FabricJobFailure(IsRetriable: true), Assert.Single(result.JobInstances).FailureReason);
    }

    [Theory]
    [InlineData("")]
    [InlineData("{private")]
    [InlineData("null")]
    [InlineData("[]")]
    [InlineData("{}")]
    [InlineData("""{"value":null}""")]
    [InlineData("""{"value":{}}""")]
    [InlineData("""{"value":"private"}""")]
    [InlineData("""{"value":[null]}""")]
    [InlineData("""{"value":[{}]}""")]
    [InlineData("""{"value":[1]}""")]
    [InlineData("""{"value":[], "value":[]}""")]
    [InlineData("""{"value":[], "Value":[{}]}""")]
    // cspell:disable-next-line
    [InlineData("""{"value":[], "\u0076alue":[]}""")]
    [InlineData("""{"value":[], "extra":{"same":1,"same":2}}""")]
    [InlineData("""{"value":[], "extra":{"text":"\uD800"}}""")]
    [InlineData("""{"value":[], "extra":{"name\uDC00":"private"}}""")]
    public static async Task List_RejectsMalformedMissingAmbiguousAndInvalidUnicodePages(string json) =>
        await AssertInvalidPageAsync(json);

    [Theory]
    [InlineData("id", "\"00000000-0000-0000-0000-000000000000\"")]
    [InlineData("id", "\"private-not-a-uuid\"")]
    [InlineData("itemId", "\"aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa\"")]
    [InlineData("itemId", "\"00000000-0000-0000-0000-000000000000\"")]
    [InlineData("itemId", "null")]
    [InlineData("jobType", "\" \"")]
    [InlineData("invokeType", "null")]
    [InlineData("status", "42")]
    [InlineData("status", "\"\"")]
    [InlineData("rootActivityId", "\"private\"")]
    [InlineData("startTimeUtc", "42")]
    [InlineData("endTimeUtc", "{}")]
    [InlineData("failureReason", "[]")]
    [InlineData("failureReason", """{"requestId":"private"}""")]
    [InlineData("failureReason", """{"errorCode":42}""")]
    [InlineData("failureReason", """{"isRetriable":"true"}""")]
    public static async Task List_RejectsAnInvalidLaterRowWithoutReturningTheValidFirstRow(string name, string value)
    {
        var invalid = JsonNode.Parse(Json)!.AsObject();
        invalid[name] = JsonNode.Parse(value);
        await AssertInvalidPageAsync($"{{\"value\":[{Json},{invalid.ToJsonString()}]}}");
    }

    [Theory]
    [InlineData("id")]
    [InlineData("itemId")]
    [InlineData("jobType")]
    [InlineData("invokeType")]
    [InlineData("status")]
    public static async Task List_RejectsMissingRequiredRowFields(string name)
    {
        var invalid = JsonNode.Parse(Json)!.AsObject();
        invalid.Remove(name);
        await AssertInvalidPageAsync($"{{\"value\":[{Json},{invalid.ToJsonString()}]}}");
    }

    [Theory]
    [InlineData("\"status\":\"InProgress\",\"status\":\"Completed\"")]
    [InlineData("\"Status\":\"Completed\"")]
    [InlineData("\"status\":\"\\uD800\"")]
    [InlineData("\"failureReason\":{\"errorCode\":\"\\uDC00\"}")]
    [InlineData("\"failureReason\":{\"errorCode\":\"one\",\"errorCode\":\"two\"}")]
    public static async Task List_RejectsDuplicateAndInvalidUnicodeRowMetadata(string additional) =>
        await AssertInvalidPageAsync($"{{\"value\":[{Json[..Json.LastIndexOf('}')]},{additional}}}]}}");

    [Theory]
    [InlineData("""{"continuationToken":null}""")]
    [InlineData("""{"continuationToken":""}""")]
    [InlineData("""{"continuationToken":" "}""")]
    [InlineData("""{"continuationToken":42}""")]
    [InlineData("""{"continuationToken":[]}""")]
    [InlineData("""{"continuationToken":"private\r\n"}""")]
    [InlineData("""{"continuationToken":"\uD800"}""")]
    [InlineData("""{"continuationUri":null}""")]
    [InlineData("""{"continuationUri":"https://example.invalid/private"}""")]
    [InlineData("""{"continuationToken":"next","continuationUri":null}""")]
    [InlineData("""{"continuationToken":"next","continuationUri":42}""")]
    [InlineData("""{"continuationToken":"next","continuationToken":"another"}""")]
    [InlineData("""{"continuationToken":"next","ContinuationToken":"another"}""")]
    public static async Task List_RejectsInvalidContinuationInformationInsteadOfImplyingExhaustion(string metadata) =>
        await AssertInvalidPageAsync($"{{\"value\":[],{metadata[1..]}");

    public static TheoryData<string> UntrustedUris => new()
    {
        "",
        "https://example.invalid/private?continuationToken=next",
        CollectionUrl.Replace("https:", "http:", StringComparison.Ordinal) + "?continuationToken=next",
        CollectionUrl.Replace("api.fabric.microsoft.com", "api.fabric.microsoft.com:444", StringComparison.Ordinal) + "?continuationToken=next",
        CollectionUrl.Replace("api.fabric.microsoft.com", "user@api.fabric.microsoft.com", StringComparison.Ordinal) + "?continuationToken=next",
        CollectionUrl.Replace(WorkspaceId, ActivityId, StringComparison.Ordinal) + "?continuationToken=next",
        CollectionUrl.Replace(ItemId, ActivityId, StringComparison.Ordinal) + "?continuationToken=next",
        CollectionUrl.Replace("/jobs/", "/jobs/./", StringComparison.Ordinal) + "?continuationToken=next",
        CollectionUrl.Replace("/jobs/", "/private/../jobs/", StringComparison.Ordinal) + "?continuationToken=next",
        // cspell:disable-next-line
        CollectionUrl.Replace("/jobs/", "/%6aobs/", StringComparison.Ordinal) + "?continuationToken=next",
        CollectionUrl.Replace("/jobs/", "\\jobs\\", StringComparison.Ordinal) + "?continuationToken=next",
        "/v1/workspaces/" + WorkspaceId + "/items/" + ItemId + "/jobs/instances?continuationToken=next",
        CollectionUrl + "/" + JobInstanceId + "?continuationToken=next",
        CollectionUrl,
        CollectionUrl + "?continuationToken=wrong",
        CollectionUrl + "?continuationToken=next#private",
        CollectionUrl + "?continuationToken=next&extra=private",
        CollectionUrl + "?continuationToken=next&continuationToken=next",
        CollectionUrl + "?ContinuationToken=next"
    };

    [Theory]
    [MemberData(nameof(UntrustedUris))]
    public static async Task List_RejectsUntrustedOrContradictoryContinuationUris(string uri)
    {
        var page = JsonNode.Parse(Page("next"))!.AsObject();
        page["continuationUri"] = uri;
        await AssertInvalidPageAsync(page.ToJsonString());
    }

    [Fact]
    public static async Task List_RejectsUriOnlyEvenWhenItPointsToTheExpectedCollection()
    {
        var page = JsonNode.Parse(Page("next", includeUri: true))!.AsObject();
        page.Remove("continuationToken");
        await AssertInvalidPageAsync(page.ToJsonString());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("00000000-0000-0000-0000-000000000000")]
    [InlineData("../jobs/instances")]
    [InlineData("https://example.invalid")]
    [InlineData(ItemId + "?query=private")]
    public static async Task List_ValidatesRouteIdsBeforeAuthentication(string? value)
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) => throw new InvalidOperationException("No HTTP expected."));
        using var client = new HttpClient(handler);
        var credential = Credential();
        var service = new FabricCoreService(client, credential);

        Assert.Equal("workspaceId", (await Assert.ThrowsAsync<ArgumentException>(() =>
            service.ListItemJobInstancesAsync(value!, ItemId, cancellationToken: TestContext.Current.CancellationToken))).ParamName);
        Assert.Equal("itemId", (await Assert.ThrowsAsync<ArgumentException>(() =>
            service.ListItemJobInstancesAsync(WorkspaceId, value!, cancellationToken: TestContext.Current.CancellationToken))).ParamName);
        Assert.Empty(credential.ReceivedCalls());
        Assert.Equal(0, handler.CallCount);
    }

    [Fact]
    public static async Task List_RejectsInvalidTokenCharactersBeforeAuthentication()
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) => throw new InvalidOperationException("No HTTP expected."));
        using var client = new HttpClient(handler);
        var credential = Credential();
        var service = new FabricCoreService(client, credential);
        foreach (var token in new[] { "", " \u00a0", "private\0", "private\r\n", "private\t", "\uD800", "\uDC00" })
        {
            var error = await Assert.ThrowsAsync<ArgumentException>(() => ReadAsync(service, token));
            Assert.Equal("continuationToken", error.ParamName);
            Assert.DoesNotContain("private", error.Message);
        }
        Assert.Empty(credential.ReceivedCalls());
        Assert.Equal(0, handler.CallCount);
    }

    [Theory]
    [InlineData(false, 0)]
    [InlineData(false, 1)]
    [InlineData(true, 0)]
    [InlineData(true, 1)]
    public static async Task List_EnforcesExactUtf8TokenBudgetOnInputAndReturnedTokens(bool multibyte, int excess)
    {
        var token = new string(multibyte ? '\u00e9' : 'a', multibyte ? 4096 : 8192) + new string('x', excess);
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(Response(Page(token))));
        using var client = new HttpClient(handler);
        var credential = Credential();
        var service = new FabricCoreService(client, credential);
        if (excess == 0)
        {
            Assert.Equal(token, (await ReadAsync(service, token)).ContinuationToken);
            Assert.Equal(1, handler.CallCount);
        }
        else
        {
            await Assert.ThrowsAsync<ArgumentException>(() => ReadAsync(service, token));
            Assert.Empty(credential.ReceivedCalls());
            Assert.Equal(0, handler.CallCount);
            await Assert.ThrowsAsync<JsonException>(() => ReadAsync(service));
            Assert.Equal(1, handler.CallCount);
        }
    }

    [Theory]
    [InlineData(false, 0)]
    [InlineData(false, 1)]
    [InlineData(true, 0)]
    [InlineData(true, 1)]
    public static async Task List_EnforcesExactByteLimitWithoutTruncation(bool knownLength, int excess)
    {
        const int limit = 1024 * 1024;
        Assert.Equal(limit, FabricResponseBodyReader.DefaultMaxJsonBytes);
        using var stream = new PaddedJsonTestStream(Encoding.UTF8.GetBytes(Page()), limit + excess);
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
            Assert.Equal(Instance(), Assert.Single((await ReadAsync(service)).JobInstances));
        }
        else
        {
            Assert.Contains("1 MiB", (await Assert.ThrowsAsync<JsonException>(() => ReadAsync(service))).Message);
        }
        Assert.Equal(knownLength && excess > 0 ? 0 : limit + excess, stream.BytesRead);
        Assert.True(stream.Disposed);
        Assert.Equal(1, handler.CallCount);
    }

    [Theory]
    [InlineData(64)]
    [InlineData(65)]
    public static async Task List_EnforcesTheExisting64LevelJsonDepthBudget(int depth)
    {
        var json = "{\"value\":[],\"extra\":" + new string('[', depth - 1) + "0" + new string(']', depth - 1) + "}";
        using var response = Response(json);
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, Credential());

        if (depth == 64)
        {
            Assert.Empty((await ReadAsync(service)).JobInstances);
        }
        else
        {
            await Assert.ThrowsAsync<JsonException>(() => ReadAsync(service));
        }
        Assert.Equal(1, handler.CallCount);
    }

    [Fact]
    public static async Task List_RejectsInvalidUtf8EvenInOmittedFields()
    {
        var body = Encoding.UTF8.GetBytes("{\"value\":[],\"extra\":\"\u00e9\"}");
        body[Array.IndexOf(body, (byte)0xc3)] = 0xff;
        using var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(body) };
        response.Content.Headers.ContentType = new("application/json");
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
        using var client = new HttpClient(handler);

        await Assert.ThrowsAsync<JsonException>(() => ReadAsync(new FabricCoreService(client, Credential())));

        Assert.Throws<ObjectDisposedException>(() => response.Content.ReadAsStream(TestContext.Current.CancellationToken));
        Assert.Equal(1, handler.CallCount);
    }

    [Theory]
    [InlineData(HttpStatusCode.Created)]
    [InlineData(HttpStatusCode.Accepted)]
    [InlineData(HttpStatusCode.NoContent)]
    [InlineData(HttpStatusCode.PartialContent)]
    [InlineData(HttpStatusCode.Found)]
    [InlineData(HttpStatusCode.TemporaryRedirect)]
    [InlineData(HttpStatusCode.BadRequest)]
    [InlineData(HttpStatusCode.Unauthorized)]
    [InlineData(HttpStatusCode.Forbidden)]
    [InlineData(HttpStatusCode.NotFound)]
    [InlineData(HttpStatusCode.TooManyRequests)]
    [InlineData(HttpStatusCode.ServiceUnavailable)]
    public static async Task List_RetainsHttpFailureAndRetryGuidanceWithoutReadingErrorBodiesOrFollowingUrls(HttpStatusCode status)
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
        Assert.True(content.IsDisposed);
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
    [InlineData("45, 60", null)]
    [InlineData("private", null)]
    public static async Task List_ReturnsOnlyValidatedRetrySeconds(string? header, long? expected)
    {
        using var response = Response(Page(), retryAfter: header);
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
        using var client = new HttpClient(handler);

        var result = await ReadAsync(new FabricCoreService(client, Credential()));

        Assert.Equal(expected, result.RetryAfterSeconds);
        Assert.Null(result.RetryAfterUtc);
        Assert.Equal(1, handler.CallCount);
    }

    [Fact]
    public static async Task List_PreservesRetryDateAndRejectsMultipleRetryHeaders()
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) =>
            Task.FromResult(Response(Page(), retryAfter: "Tue, 01 Jan 2030 00:00:00 GMT")));
        using var client = new HttpClient(handler);
        var result = await ReadAsync(new FabricCoreService(client, Credential()));
        Assert.Equal(new DateTimeOffset(2030, 1, 1, 0, 0, 0, TimeSpan.Zero), result.RetryAfterUtc);
        Assert.Null(result.RetryAfterSeconds);

        using var multiple = Response(Page());
        multiple.Headers.TryAddWithoutValidation("Retry-After", ["45", "60"]);
        using var otherHandler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(multiple));
        using var otherClient = new HttpClient(otherHandler);
        var other = await ReadAsync(new FabricCoreService(otherClient, Credential()));
        Assert.Null(other.RetryAfterSeconds);
        Assert.Null(other.RetryAfterUtc);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("text/html")]
    [InlineData("application/octet-stream")]
    public static async Task List_RejectsUnsupportedMediaBeforeReading(string? mediaType)
    {
        using var content = new UnreadableHttpContent();
        content.Headers.ContentType = mediaType is null ? null : new(mediaType);
        using var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = content };
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
        using var client = new HttpClient(handler);

        await Assert.ThrowsAsync<JsonException>(() => ReadAsync(new FabricCoreService(client, Credential())));
        Assert.True(content.IsDisposed);
    }

    [Fact]
    public static async Task List_DoesNotMisclassifyUnrelatedReaderFailuresAsMalformedJson()
    {
        using var stream = new OperationTestStream(Encoding.UTF8.GetBytes(Page()),
            (_, _) => throw new InvalidOperationException("private-reader-failure"));
        using var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StreamContent(stream) };
        response.Content.Headers.ContentType = new("application/json");
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
        using var client = new HttpClient(handler);

        await Assert.ThrowsAsync<InvalidOperationException>(() => ReadAsync(new FabricCoreService(client, Credential())));
        Assert.True(stream.Disposed);
    }

    [Fact]
    public static async Task List_AuthenticationFailureSendsNoHttp()
    {
        var credential = Credential();
        credential.GetTokenAsync(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>())
            .Returns(ValueTask.FromException<AccessToken>(new AuthenticationFailedException("private")));
        using var handler = new FabricCoreHttpMessageHandler((_, _) => throw new InvalidOperationException("No HTTP expected."));
        using var client = new HttpClient(handler);

        await Assert.ThrowsAsync<AuthenticationFailedException>(() => ReadAsync(new FabricCoreService(client, credential)));
        Assert.Equal(0, handler.CallCount);
    }

    [Theory]
    [InlineData("before")]
    [InlineData("authentication")]
    [InlineData("http")]
    [InlineData("headers")]
    [InlineData("body")]
    [InlineData("end-of-body")]
    public static async Task List_PropagatesCancellationAndDisposesAvailableResponses(string phase)
    {
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        var credential = Credential();
        credential.GetTokenAsync(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>()).Returns(_ =>
        {
            if (phase == "authentication")
            {
                cancellation.Cancel();
            }
            return new AccessToken("job-test-token", DateTimeOffset.MaxValue);
        });
        using var stream = new OperationTestStream(Encoding.UTF8.GetBytes(Page()), (count, token) =>
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
        if (phase == "before")
        {
            await cancellation.CancelAsync();
        }

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            ReadAsync(new FabricCoreService(client, credential), cancellationToken: cancellation.Token));

        Assert.Equal(phase is "before" or "authentication" ? 0 : 1, handler.CallCount);
        if (phase is "headers" or "body" or "end-of-body")
        {
            Assert.True(stream.Disposed);
        }
        await credential.Received(phase == "before" ? 0 : 1).GetTokenAsync(Arg.Any<TokenRequestContext>(), cancellation.Token);
    }

    [Fact]
    public static async Task List_IsolatesConcurrentIdentitiesItemsTokensAndPages()
    {
        var identity = new AsyncLocal<string>();
        var credential = Credential();
        credential.GetTokenAsync(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>())
            .Returns(_ => new AccessToken(identity.Value!, DateTimeOffset.MaxValue));
        var requests = new ConcurrentBag<string>();
        using var handler = new FabricCoreHttpMessageHandler(async (request, _) =>
        {
            var item = request.RequestUri!.Segments[5].TrimEnd('/');
            Assert.Equal(item, request.Headers.Authorization?.Parameter);
            Assert.Equal("?continuationToken=" + item, request.RequestUri.Query);
            requests.Add(item);
            await Task.Yield();
            var instance = Instance() with { ItemId = Guid.Parse(item) };
            var json = JsonSerializer.Serialize(instance, CoreJsonContext.Default.FabricItemJobInstance);
            return Response($"{{\"value\":[{json}],\"continuationToken\":\"{item}\"}}");
        });
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, credential);

        await Task.WhenAll(Enumerable.Range(0, 8).Select(async _ =>
        {
            var item = Guid.NewGuid().ToString();
            identity.Value = item;
            var result = await service.ListItemJobInstancesAsync(WorkspaceId, item, item, TestContext.Current.CancellationToken);
            Assert.Equal(Guid.Parse(item), Assert.Single(result.JobInstances).ItemId);
            Assert.Equal(item, result.ContinuationToken);
        }));

        Assert.Equal(8, handler.CallCount);
        Assert.Equal(8, requests.Distinct().Count());
        Assert.Null(client.DefaultRequestHeaders.Authorization);
    }

    private static async Task AssertInvalidPageAsync(string json)
    {
        using var response = Response(json);
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
        using var client = new HttpClient(handler);

        var error = await Assert.ThrowsAsync<JsonException>(() => ReadAsync(new FabricCoreService(client, Credential())));

        Assert.DoesNotContain("private", error.Message);
        Assert.Contains("No partial page", error.Message);
        Assert.Equal(1, handler.CallCount);
        Assert.Throws<ObjectDisposedException>(() => response.Content.ReadAsStream(TestContext.Current.CancellationToken));
    }

    private static Task<ItemJobInstanceListCommandResult> ReadAsync(
        FabricCoreService service, string? token = null, CancellationToken? cancellationToken = null) =>
        service.ListItemJobInstancesAsync(WorkspaceId, ItemId, token, cancellationToken ?? TestContext.Current.CancellationToken);
}
