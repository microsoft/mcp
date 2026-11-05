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
using Fabric.Mcp.Tools.Core.Validation;
using NSubstitute;
using Xunit;
using static Fabric.Mcp.Tools.Core.Tests.TestSupport.ItemJobInstanceTestData;
using static Fabric.Mcp.Tools.Core.Tests.TestSupport.ItemScheduleCreateTestData;
using static Fabric.Mcp.Tools.Core.Tests.TestSupport.ItemScheduleListTestData;

namespace Fabric.Mcp.Tools.Core.Tests.Services;

public sealed class ItemScheduleUpdateServiceTests()
{
    [Theory]
    [InlineData(JobType, false, null)]
    [InlineData("Future / job?#%+", true, ExecutionData)]
    [InlineData(" /../ ", false, "{}")]
    [InlineData("%2e%2e", true, null)]
    [InlineData("\u00e9\U0001F680", false, ExecutionData)]
    public static async Task Update_SendsOneNormalizedAuthenticatedPatchWithExactBody(string jobType, bool enabled, string? execution)
    {
        using var stream = new OperationTestStream(Encoding.UTF8.GetBytes(ScheduleJson));
        using var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StreamContent(stream) };
        response.Content.Headers.ContentType = new("application/json");
        response.Headers.TryAddWithoutValidation("Location", ["https://example.invalid/private", "/v1/operations/not-a-schedule"]);
        response.Headers.TryAddWithoutValidation("x-ms-operation-id", "private-ignored");
        response.Headers.TryAddWithoutValidation("Retry-After", "0");
        using var handler = new FabricCoreHttpMessageHandler(async (request, token) =>
        {
            Assert.Equal(HttpMethod.Patch, request.Method);
            Assert.Equal($"{CollectionUrl(jobType)}/{ScheduleId}", request.RequestUri?.AbsoluteUri);
            Assert.Empty(request.RequestUri!.Query);
            Assert.Equal("Bearer", request.Headers.Authorization?.Scheme);
            Assert.Equal("job-test-token", request.Headers.Authorization?.Parameter);
            Assert.Equal("Fabric Core MCP", request.Headers.UserAgent.ToString());
            Assert.Equal("application/json", request.Content?.Headers.ContentType?.MediaType);
            using var body = JsonDocument.Parse(await request.Content!.ReadAsStringAsync(token));
            Assert.Equal(enabled, body.RootElement.GetProperty("enabled").GetBoolean());
            using var config = JsonDocument.Parse(Configuration);
            Assert.True(JsonElement.DeepEquals(config.RootElement, body.RootElement.GetProperty("configuration")));
            Assert.Equal(execution is not null, body.RootElement.TryGetProperty("executionData", out var data));
            if (execution is not null)
            {
                using var expected = JsonDocument.Parse(execution);
                Assert.True(JsonElement.DeepEquals(expected.RootElement, data));
            }
            Assert.Equal(execution is null ? 2 : 3, body.RootElement.EnumerateObject().Count());
            return response;
        });
        using var client = new HttpClient(handler);
        var credential = Credential();

        var result = await new FabricCoreService(client, credential).UpdateItemScheduleAsync(
            $"{{{WorkspaceId.ToUpperInvariant()}}}", Guid.Parse(ItemId).ToString("N").ToUpperInvariant(),
            jobType, $"{{{ScheduleId.ToUpperInvariant()}}}", enabled, Configuration, execution, TestContext.Current.CancellationToken);

        Assert.Equal(Guid.Parse(WorkspaceId), result.WorkspaceId);
        Assert.Equal(Guid.Parse(ItemId), result.ItemId);
        Assert.Equal(jobType, result.JobType);
        Assert.Equal("Updated", result.UpdateStatus);
        using var expectedSchedule = JsonDocument.Parse(ScheduleJson);
        Assert.True(JsonElement.DeepEquals(expectedSchedule.RootElement,
            JsonSerializer.SerializeToElement(result.Schedule, CoreJsonContext.Default.FabricItemSchedule)));
        Assert.Equal(0, result.RetryAfterSeconds);
        Assert.DoesNotContain("private", JsonSerializer.Serialize(result, CoreJsonContext.Default.ItemScheduleUpdateCommandResult));
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
    public static async Task Update_PreservesOptionalNullOrAbsentFieldsWithoutSynthesizingFromRequest(bool nullFields)
    {
        var row = new JsonObject { ["id"] = ScheduleId, ["enabled"] = false };
        foreach (var name in new[] { "configuration", "owner", "executionData", "createdDateTime" })
        {
            if (nullFields)
            {
                row[name] = null;
            }
        }
        row["unknown"] = "private-not-projected";
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(Response(row.ToJsonString())));
        using var client = new HttpClient(handler);
        var result = await UpdateAsync(new FabricCoreService(client, Credential()));
        var schedule = JsonSerializer.SerializeToElement(result.Schedule, CoreJsonContext.Default.FabricItemSchedule);
        Assert.Equal(["enabled", "id"], schedule.EnumerateObject().Select(property => property.Name).Order());
        Assert.False(result.Schedule.Enabled);
        Assert.Null(result.RetryAfterSeconds);
        Assert.Null(result.RetryAfterUtc);
        Assert.Equal(1, handler.CallCount);
    }

    [Theory]
    [InlineData("")]
    [InlineData("null")]
    [InlineData("[]")]
    [InlineData("{}")]
    [InlineData("{private")]
    [InlineData("""{"id":"eeeeeeee-eeee-4eee-8eee-eeeeeeeeeeee"}""")]
    [InlineData("""{"enabled":true}""")]
    [InlineData("""{"id":"00000000-0000-0000-0000-000000000000","enabled":true}""")]
    [InlineData("""{"id":"private-invalid","enabled":true}""")]
    [InlineData("""{"id":"cccccccc-cccc-4ccc-8ccc-cccccccccccc","enabled":true}""")]
    [InlineData("""{"id":"eeeeeeee-eeee-4eee-8eee-eeeeeeeeeeee","enabled":"true"}""")]
    [InlineData("""{"id":"eeeeeeee-eeee-4eee-8eee-eeeeeeeeeeee","enabled":true,"Enabled":false}""")]
    [InlineData("""{"id":"eeeeeeee-eeee-4eee-8eee-eeeeeeeeeeee","enabled":true,"enabled":false}""")]
    [InlineData("""{"id":"eeeeeeee-eeee-4eee-8eee-eeeeeeeeeeee","enabled":true,"executionData":[]}""")]
    [InlineData("""{"id":"eeeeeeee-eeee-4eee-8eee-eeeeeeeeeeee","enabled":true,"owner":{}}""")]
    [InlineData("""{"id":"eeeeeeee-eeee-4eee-8eee-eeeeeeeeeeee","enabled":true,"configuration":{}}""")]
    [InlineData("""{"id":"eeeeeeee-eeee-4eee-8eee-eeeeeeeeeeee","enabled":true,"createdDateTime":"private-invalid"}""")]
    [InlineData("""{"id":"eeeeeeee-eeee-4eee-8eee-eeeeeeeeeeee","enabled":true,"extra":{"x":1,"x":2}}""")]
    [InlineData("""{"id":"eeeeeeee-eeee-4eee-8eee-eeeeeeeeeeee","enabled":true,"extra":{"x":"\uD800"}}""")]
    [InlineData("""{"id":"eeeeeeee-eeee-4eee-8eee-eeeeeeeeeeee","enabled":true,"extra":{"\uDC00":1}}""")]
    public static async Task Update_RejectsInvalidOrWrongIdentitySnapshotsWithoutLosingConfirmation(string json)
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(Response(json, retryAfter: "60")));
        using var client = new HttpClient(handler);
        var error = await Assert.ThrowsAsync<ItemScheduleUpdateRequestException>(() => UpdateAsync(new FabricCoreService(client, Credential())));
        Assert.True(error.UpdateConfirmed);
        Assert.Equal(HttpStatusCode.BadGateway, error.StatusCode);
        Assert.Equal(TimeSpan.FromSeconds(60), error.RetryAfter?.Delta);
        Assert.DoesNotContain("private", error.Message);
        Assert.Equal(1, handler.CallCount);
    }

    [Theory]
    [InlineData(false, 0)]
    [InlineData(false, 1)]
    [InlineData(true, 0)]
    [InlineData(true, 1)]
    public static async Task Update_EnforcesExactResponseByteBudgetAndDisposes(bool contentLength, int excess)
    {
        const int limit = FabricResponseBodyReader.DefaultMaxJsonBytes;
        var empty = $$"""{"id":"{{ScheduleId}}","enabled":false,"pad":""}""";
        var json = empty.Replace("\"pad\":\"\"", $"\"pad\":\"{new string('x', limit - empty.Length + excess)}\"", StringComparison.Ordinal);
        using var stream = new OperationTestStream(Encoding.UTF8.GetBytes(json));
        using var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StreamContent(stream) };
        response.Content.Headers.ContentType = new("application/json");
        if (contentLength)
        {
            response.Content.Headers.ContentLength = limit + excess;
        }
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, Credential());
        if (excess == 0)
        {
            Assert.Equal(Guid.Parse(ScheduleId), (await UpdateAsync(service)).Schedule.Id);
        }
        else
        {
            Assert.True((await Assert.ThrowsAsync<ItemScheduleUpdateRequestException>(() => UpdateAsync(service))).UpdateConfirmed);
        }
        Assert.Equal(contentLength && excess > 0 ? 0 : limit + excess, stream.BytesRead);
        Assert.True(stream.Disposed);
        Assert.Equal(1, handler.CallCount);
    }

    [Theory]
    [InlineData(64, true)]
    [InlineData(65, false)]
    public static async Task Update_EnforcesDepthForIgnoredResponseFields(int depth, bool valid)
    {
        var json = $$"""{"id":"{{ScheduleId}}","enabled":false,"extension":{{new string('[', depth - 1)}}0{{new string(']', depth - 1)}}}""";
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(Response(json)));
        using var client = new HttpClient(handler);
        if (valid)
        {
            Assert.Equal("Updated", (await UpdateAsync(new FabricCoreService(client, Credential()))).UpdateStatus);
        }
        else
        {
            Assert.True((await Assert.ThrowsAsync<ItemScheduleUpdateRequestException>(() => UpdateAsync(new FabricCoreService(client, Credential())))).UpdateConfirmed);
        }
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
    public static async Task Update_PreservesHttpFailuresAndNeverFollowsHeadersOrRetryHints(HttpStatusCode status, HttpStatusCode expected)
    {
        using var response = Response("""{"errorCode":"InsufficientPrivileges","message":"private","isRetriable":true,"moreDetails":[{"message":"private"}]}""", status, "60");
        response.Headers.Location = new Uri("https://example.invalid/private");
        response.Headers.TryAddWithoutValidation("x-ms-operation-id", ScheduleId);
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
        using var client = new HttpClient(handler);
        var error = await Assert.ThrowsAsync<ItemScheduleUpdateRequestException>(() => UpdateAsync(new FabricCoreService(client, Credential())));
        Assert.Equal(expected, error.StatusCode);
        Assert.False(error.UpdateConfirmed);
        Assert.Equal(TimeSpan.FromSeconds(60), error.RetryAfter?.Delta);
        Assert.Equal(response.IsSuccessStatusCode ? null : "InsufficientPrivileges", error.ErrorCode);
        Assert.Equal(1, handler.CallCount);
    }

    [Theory]
    [InlineData("""{"errorCode":"\uD800"}""")]
    [InlineData("""{"\uD800":1}""")]
    [InlineData("""{"errorCode":"ItemNotFound","errorCode":"Other"}""")]
    [InlineData("""{"errorCode":"private invalid"}""")]
    [InlineData("private-invalid-json")]
    public static async Task Update_InvalidErrorMetadataRetainsStatusAndRetryAfter(string json)
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(Response(json, HttpStatusCode.TooManyRequests, "60")));
        using var client = new HttpClient(handler);
        var error = await Assert.ThrowsAsync<ItemScheduleUpdateRequestException>(() => UpdateAsync(new FabricCoreService(client, Credential())));
        Assert.Equal(HttpStatusCode.TooManyRequests, error.StatusCode);
        Assert.Equal(TimeSpan.FromSeconds(60), error.RetryAfter?.Delta);
        Assert.True(error.ErrorMetadataUnavailable);
        Assert.Null(error.ErrorCode);
        Assert.Equal(1, handler.CallCount);
    }

    [Theory]
    [InlineData(HttpStatusCode.OK)]
    [InlineData(HttpStatusCode.TooManyRequests)]
    public static async Task Update_NonJsonMediaPreservesKnownOutcomeAndRetryAfter(HttpStatusCode status)
    {
        using var response = Response(ScheduleJson, status, "60");
        response.Content.Headers.ContentType = new("text/plain");
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
        using var client = new HttpClient(handler);
        var error = await Assert.ThrowsAsync<ItemScheduleUpdateRequestException>(() => UpdateAsync(new FabricCoreService(client, Credential())));
        Assert.Equal(status == HttpStatusCode.OK, error.UpdateConfirmed);
        Assert.Equal(status == HttpStatusCode.OK ? HttpStatusCode.BadGateway : status, error.StatusCode);
        Assert.Equal(TimeSpan.FromSeconds(60), error.RetryAfter?.Delta);
        Assert.Equal(1, handler.CallCount);
        Assert.Throws<ObjectDisposedException>(() => response.Content.ReadAsStream(TestContext.Current.CancellationToken));
    }

    [Theory]
    [InlineData("0", 0L)]
    [InlineData("60", 60L)]
    [InlineData("-1", null)]
    [InlineData("private", null)]
    [InlineData("60, 120", null)]
    [InlineData("999999999999999999999999", null)]
    [InlineData("Tue, 01 Jan 2030 00:00:00 GMT", null)]
    public static async Task Update_UsesOnlyValidatedRetryGuidance(string retry, long? seconds)
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(Response(ScheduleJson, retryAfter: retry)));
        using var client = new HttpClient(handler);
        var result = await UpdateAsync(new FabricCoreService(client, Credential()));
        Assert.Equal(seconds, result.RetryAfterSeconds);
        Assert.Equal(retry.StartsWith("Tue", StringComparison.Ordinal), result.RetryAfterUtc.HasValue);
        Assert.Equal(1, handler.CallCount);
    }

    [Theory]
    [InlineData("workspace", "private-invalid")]
    [InlineData("item", "00000000-0000-0000-0000-000000000000")]
    [InlineData("schedule", "")]
    [InlineData("schedule", "private-invalid")]
    [InlineData("schedule", "00000000-0000-0000-0000-000000000000")]
    [InlineData("job", "..")]
    [InlineData("job", "private\r\n")]
    [InlineData("configuration", "{}")]
    [InlineData("configuration", """{"enabled":false}""")]
    [InlineData("execution", "[]")]
    [InlineData("execution", "null")]
    public static async Task Update_ValidatesBeforeAuthenticationAndNeverFetchesMissingConfiguration(string option, string value)
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) => throw new InvalidOperationException("No HTTP expected."));
        using var client = new HttpClient(handler);
        var credential = Credential();
        await Assert.ThrowsAsync<FabricItemInputException>(() => new FabricCoreService(client, credential).UpdateItemScheduleAsync(
            option == "workspace" ? value : WorkspaceId, option == "item" ? value : ItemId,
            option == "job" ? value : JobType, option == "schedule" ? value : ScheduleId, false,
            option == "configuration" ? value : Configuration, option == "execution" ? value : null, TestContext.Current.CancellationToken));
        Assert.Equal(0, handler.CallCount);
        Assert.Empty(credential.ReceivedCalls());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public static async Task Update_PreSendCancellationStopsBeforeHttp(bool duringAuth)
    {
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        var credential = Credential();
        credential.GetTokenAsync(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>()).Returns(_ =>
        {
            cancellation.Cancel();
            return new AccessToken("job-test-token", DateTimeOffset.MaxValue);
        });
        if (!duringAuth)
        {
            cancellation.Cancel();
        }
        using var handler = new FabricCoreHttpMessageHandler((_, _) => throw new InvalidOperationException("No HTTP expected."));
        using var client = new HttpClient(handler);
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => UpdateAsync(new FabricCoreService(client, credential), cancellation.Token));
        Assert.Equal(0, handler.CallCount);
        await credential.Received(duringAuth ? 1 : 0).GetTokenAsync(Arg.Any<TokenRequestContext>(), cancellation.Token);
    }

    [Fact]
    public static async Task Update_AuthenticationFailureStopsBeforeHttp()
    {
        var credential = Credential();
        credential.GetTokenAsync(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>())
            .Returns(ValueTask.FromException<AccessToken>(new AuthenticationFailedException("private")));
        using var handler = new FabricCoreHttpMessageHandler((_, _) => throw new InvalidOperationException("No HTTP expected."));
        using var client = new HttpClient(handler);
        await Assert.ThrowsAsync<AuthenticationFailedException>(() => UpdateAsync(new FabricCoreService(client, credential)));
        Assert.Equal(0, handler.CallCount);
    }

    [Theory]
    [InlineData("network")]
    [InlineData("timeout")]
    [InlineData("cancellation")]
    public static async Task Update_UncertainSendNeverRetries(string failure)
    {
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        using var handler = new FabricCoreHttpMessageHandler((_, token) =>
        {
            if (failure == "cancellation")
            {
                cancellation.Cancel();
                return Task.FromCanceled<HttpResponseMessage>(token);
            }
            throw failure == "network" ? new HttpRequestException("private") : new TimeoutException("private");
        });
        using var client = new HttpClient(handler);
        Assert.NotNull(await Record.ExceptionAsync(() => UpdateAsync(new FabricCoreService(client, Credential()), cancellation.Token)));
        Assert.Equal(1, handler.CallCount);
    }

    [Theory]
    [InlineData("io", HttpStatusCode.OK)]
    [InlineData("network", HttpStatusCode.OK)]
    [InlineData("cancellation", HttpStatusCode.OK)]
    [InlineData("io", HttpStatusCode.TooManyRequests)]
    [InlineData("cancellation", HttpStatusCode.TooManyRequests)]
    public static async Task Update_FailedBodyReadsPreserveKnownOutcomeAndDispose(string failure, HttpStatusCode status)
    {
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        using var stream = new OperationTestStream(Encoding.UTF8.GetBytes(ScheduleJson), (_, token) =>
        {
            if (failure == "cancellation")
            {
                cancellation.Cancel();
                return ValueTask.FromCanceled(token);
            }
            return ValueTask.FromException(failure == "io" ? new IOException("private") : new HttpRequestException("private"));
        });
        using var response = new HttpResponseMessage(status) { Content = new StreamContent(stream) };
        response.Content.Headers.ContentType = new("application/json");
        response.Headers.TryAddWithoutValidation("Retry-After", "60");
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
        using var client = new HttpClient(handler);
        var error = await Assert.ThrowsAsync<ItemScheduleUpdateRequestException>(() => UpdateAsync(new FabricCoreService(client, Credential()), cancellation.Token));
        Assert.Equal(status == HttpStatusCode.OK, error.UpdateConfirmed);
        Assert.Equal(status == HttpStatusCode.TooManyRequests ? status :
            failure == "cancellation" ? HttpStatusCode.RequestTimeout : HttpStatusCode.BadGateway, error.StatusCode);
        Assert.Equal(TimeSpan.FromSeconds(60), error.RetryAfter?.Delta);
        Assert.True(stream.Disposed);
        Assert.Equal(1, handler.CallCount);
    }

    [Fact]
    public static async Task Update_CancellationAfterHeadersKeepsConfirmedUpdate()
    {
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        using var response = Response(ScheduleJson);
        using var handler = new FabricCoreHttpMessageHandler((_, _) =>
        {
            cancellation.Cancel();
            return Task.FromResult(response);
        });
        using var client = new HttpClient(handler);
        var error = await Assert.ThrowsAsync<ItemScheduleUpdateRequestException>(() => UpdateAsync(new FabricCoreService(client, Credential()), cancellation.Token));
        Assert.True(error.UpdateConfirmed);
        Assert.Equal(HttpStatusCode.RequestTimeout, error.StatusCode);
        Assert.Equal(1, handler.CallCount);
    }

    [Fact]
    public static async Task Update_IsolatesConcurrentCallerCredentialsTargetsInputsAndResults()
    {
        var identity = new AsyncLocal<string>();
        var credential = Credential();
        credential.GetTokenAsync(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>())
            .Returns(_ => new AccessToken(identity.Value!, DateTimeOffset.MaxValue));
        var observed = new ConcurrentBag<string>();
        using var handler = new FabricCoreHttpMessageHandler(async (request, token) =>
        {
            var current = request.Headers.Authorization!.Parameter!;
            observed.Add(current);
            await Task.Yield();
            using var body = JsonDocument.Parse(await request.Content!.ReadAsStringAsync(token));
            Assert.Equal(current, body.RootElement.GetProperty("executionData").GetProperty("caller").GetString());
            Assert.Equal($"{CollectionUrl(current)}/{current}", request.RequestUri?.AbsoluteUri);
            Assert.Equal(HttpMethod.Patch, request.Method);
            return Response($$"""{"id":"{{current}}","enabled":false}""");
        });
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, credential);
        await Task.WhenAll(Enumerable.Range(0, 8).Select(async _ =>
        {
            var id = Guid.NewGuid();
            identity.Value = id.ToString();
            var result = await service.UpdateItemScheduleAsync(WorkspaceId, ItemId, id.ToString(), id.ToString(), false, Configuration,
                $$"""{"caller":"{{id}}"}""", TestContext.Current.CancellationToken);
            Assert.Equal(id, result.Schedule.Id);
            Assert.Equal(id.ToString(), result.JobType);
        }));
        Assert.Equal(8, handler.CallCount);
        Assert.Equal(8, observed.Distinct().Count());
        await credential.Received(8).GetTokenAsync(Arg.Any<TokenRequestContext>(), TestContext.Current.CancellationToken);
    }

    private static Task<ItemScheduleUpdateCommandResult> UpdateAsync(FabricCoreService service, CancellationToken? cancellationToken = null) =>
        service.UpdateItemScheduleAsync(WorkspaceId, ItemId, JobType, ScheduleId, false, Configuration,
            cancellationToken: cancellationToken ?? TestContext.Current.CancellationToken);
}
