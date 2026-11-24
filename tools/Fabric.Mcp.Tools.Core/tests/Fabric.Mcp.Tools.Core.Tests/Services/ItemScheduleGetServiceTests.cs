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

public sealed class ItemScheduleGetServiceTests()
{
    [Theory]
    [InlineData(JobType)]
    [InlineData("Future / job?#%+")]
    [InlineData(" /../ ")]
    [InlineData("%2e%2e")]
    [InlineData("\u00e9\U0001F680")]
    public static async Task Get_SendsOneNormalizedAuthenticatedGetAndPreservesTheSnapshot(string jobType)
    {
        using var stream = new OperationTestStream(Encoding.UTF8.GetBytes(ScheduleJson));
        using var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StreamContent(stream) };
        response.Content.Headers.ContentType = new("application/json");
        response.Headers.TryAddWithoutValidation("Location", ["https://example.invalid/private", "/v1/operations/not-a-schedule"]);
        response.Headers.TryAddWithoutValidation("x-ms-operation-id", "private-ignored");
        response.Headers.TryAddWithoutValidation("Retry-After", "0");
        using var handler = new FabricCoreHttpMessageHandler((request, token) =>
        {
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal($"{CollectionUrl(jobType)}/{ScheduleId}", request.RequestUri?.AbsoluteUri);
            Assert.Empty(request.RequestUri!.Query);
            Assert.Empty(request.RequestUri.Fragment);
            Assert.Null(request.Content);
            Assert.Equal("Bearer", request.Headers.Authorization?.Scheme);
            Assert.Equal("job-test-token", request.Headers.Authorization?.Parameter);
            Assert.Equal("Fabric Core MCP", request.Headers.UserAgent.ToString());
            Assert.True(token.CanBeCanceled);
            return Task.FromResult(response);
        });
        using var client = new HttpClient(handler);
        var credential = Credential();

        var result = await new FabricCoreService(client, credential).GetItemScheduleAsync(
            $"{{{WorkspaceId.ToUpperInvariant()}}}", Guid.Parse(ItemId).ToString("N").ToUpperInvariant(),
            jobType, $"{{{ScheduleId.ToUpperInvariant()}}}", TestContext.Current.CancellationToken);

        Assert.Equal(Guid.Parse(WorkspaceId), result.WorkspaceId);
        Assert.Equal(Guid.Parse(ItemId), result.ItemId);
        Assert.Equal(jobType, result.JobType);
        using var expected = JsonDocument.Parse(ScheduleJson);
        Assert.True(JsonElement.DeepEquals(expected.RootElement,
            JsonSerializer.SerializeToElement(result.Schedule, CoreJsonContext.Default.FabricItemSchedule)));
        Assert.Equal(0, result.RetryAfterSeconds);
        var output = JsonSerializer.SerializeToElement(result, CoreJsonContext.Default.ItemScheduleGetCommandResult);
        Assert.Equal(["itemId", "jobType", "retryAfterSeconds", "schedule", "workspaceId"],
            output.EnumerateObject().Select(property => property.Name).Order());
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
    public static async Task Get_RequiresOnlyIdAndEnabledWithoutInventingAbsentOrNullMetadata(bool nullFields)
    {
        var row = new JsonObject { ["id"] = ScheduleId, ["enabled"] = false };
        foreach (var field in new[] { "createdDateTime", "configuration", "owner", "executionData" })
        {
            if (nullFields)
            {
                row[field] = null;
            }
        }
        row["unknown"] = "private-not-projected";
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(Response(row.ToJsonString())));
        using var client = new HttpClient(handler);
        var result = await GetAsync(new FabricCoreService(client, Credential()));
        var schedule = JsonSerializer.SerializeToElement(result.Schedule, CoreJsonContext.Default.FabricItemSchedule);
        Assert.Equal(["enabled", "id"], schedule.EnumerateObject().Select(property => property.Name).Order());
        Assert.False(result.Schedule.Enabled);
        Assert.Null(result.RetryAfterSeconds);
        Assert.Null(result.RetryAfterUtc);
        Assert.Equal(1, handler.CallCount);
    }

    [Theory]
    [InlineData("Cron", """{"interval":1}""")]
    [InlineData("Cron", """{"interval":5270400}""")]
    [InlineData("Daily", """{"times":["00:00","23:59"]}""")]
    [InlineData("Weekly", """{"times":["18:09","17:12"],"weekdays":["Monday","Tuesday"]}""")]
    [InlineData("Monthly", """{"times":["01:30"],"recurrence":12,"occurrence":{"occurrenceType":"DayOfMonth","dayOfMonth":31}}""")]
    [InlineData("Monthly", """{"times":["01:30"],"recurrence":1,"occurrence":{"occurrenceType":"OrdinalWeekday","weekIndex":"Fifth","weekday":"Monday"}}""")]
    [InlineData("Monthly", """{"times":["01:30"],"recurrence":1,"occurrence":{"occurrenceType":"FutureOccurrence","setting":[1,2]}}""")]
    [InlineData("FutureSchedule", """{"interval":"future-value","opaque":{"name":1,"Name":2}}""")]
    public static async Task Get_PreservesExtensibleResponseSettingsWithoutCreationTimePolicy(string type, string fields)
    {
        var row = JsonNode.Parse(ScheduleJson)!.AsObject();
        var configuration = JsonNode.Parse(Config(type, fields))!.AsObject();
        configuration["localTimeZoneId"] = "Future workload time zone";
        configuration["startDateTime"] = "2030-01-01T00:00:00+03:00";
        configuration["endDateTime"] = "2024-04-30T23:59:00.1234567";
        row["configuration"] = configuration;
        row["owner"]!["type"] = "FuturePrincipal";
        row["owner"]!["futureDetails"] = new JsonObject { ["setting"] = true };
        row["executionData"] = new JsonObject();
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(Response(row.ToJsonString())));
        using var client = new HttpClient(handler);
        var result = await GetAsync(new FabricCoreService(client, Credential()));
        using var expected = JsonDocument.Parse(row.ToJsonString());
        Assert.True(JsonElement.DeepEquals(expected.RootElement,
            JsonSerializer.SerializeToElement(result.Schedule, CoreJsonContext.Default.FabricItemSchedule)));
        Assert.Equal("2024-04-28T05:35:20.5366667", result.Schedule.CreatedDateTime);
        Assert.Empty(result.Schedule.ExecutionData!.Value.EnumerateObject());
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
    [InlineData("""{"id":"eeeeeeee-eeee-4eee-8eee-eeeeeeeeeeee","enabled":true,"executionData":{"x":"\uD800"}}""")]
    [InlineData("""{"id":"eeeeeeee-eeee-4eee-8eee-eeeeeeeeeeee","enabled":true,"extra":{"\uDC00":1}}""")]
    public static async Task Get_RejectsUnusableSnapshotsWithoutReturningWrongIdentityOrMutationConfirmation(string json)
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(Response(json, retryAfter: "60")));
        using var client = new HttpClient(handler);
        var error = await Assert.ThrowsAsync<ItemScheduleGetRequestException>(() => GetAsync(new FabricCoreService(client, Credential())));
        Assert.Equal(HttpStatusCode.BadGateway, error.StatusCode);
        Assert.Equal(TimeSpan.FromSeconds(60), error.RetryAfter?.Delta);
        Assert.DoesNotContain("private", error.Message);
        Assert.DoesNotContain("confirmed", error.Message);
        Assert.Equal(1, handler.CallCount);
    }

    [Theory]
    [InlineData((byte)'x', true)]
    [InlineData((byte)0xff, false)]
    public static async Task Get_ValidatesUtf8InRetainedData(byte value, bool valid)
    {
        var prefix = Encoding.UTF8.GetBytes($$"""{"id":"{{ScheduleId}}","enabled":false,"executionData":{"value":""");
        using var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new ByteArrayContent([.. prefix, (byte)'"', value, (byte)'"', (byte)'}', (byte)'}'])
        };
        response.Content.Headers.ContentType = new("application/json");
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, Credential());
        if (valid)
        {
            Assert.Equal("x", (await GetAsync(service)).Schedule.ExecutionData!.Value.GetProperty("value").GetString());
        }
        else
        {
            Assert.Equal(HttpStatusCode.BadGateway, (await Assert.ThrowsAsync<ItemScheduleGetRequestException>(() => GetAsync(service))).StatusCode);
        }
        Assert.Equal(1, handler.CallCount);
    }

    [Theory]
    [InlineData(false, 0)]
    [InlineData(false, 1)]
    [InlineData(true, 0)]
    [InlineData(true, 1)]
    public static async Task Get_EnforcesExactResponseByteBudgetAndDisposes(bool contentLength, int excess)
    {
        const int limit = FabricResponseBodyReader.DefaultMaxJsonBytes;
        var empty = $$$"""{"id":"{{{ScheduleId}}}","enabled":false,"executionData":{"pad":""}}""";
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
            Assert.Equal(limit - empty.Length, (await GetAsync(service)).Schedule.ExecutionData!.Value.GetProperty("pad").GetString()!.Length);
        }
        else
        {
            Assert.Equal(HttpStatusCode.BadGateway, (await Assert.ThrowsAsync<ItemScheduleGetRequestException>(() => GetAsync(service))).StatusCode);
        }
        Assert.Equal(contentLength && excess > 0 ? 0 : limit + excess, stream.BytesRead);
        Assert.True(stream.Disposed);
        Assert.Equal(1, handler.CallCount);
    }

    [Theory]
    [InlineData(64, true)]
    [InlineData(65, false)]
    public static async Task Get_EnforcesRawDepthEvenForIgnoredFields(int depth, bool valid)
    {
        var json = $$"""{"id":"{{ScheduleId}}","enabled":false,"extra":{{new string('[', depth - 1)}}0{{new string(']', depth - 1)}}}""";
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(Response(json)));
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, Credential());
        if (valid)
        {
            Assert.Equal(Guid.Parse(ScheduleId), (await GetAsync(service)).Schedule.Id);
        }
        else
        {
            Assert.Equal(HttpStatusCode.BadGateway, (await Assert.ThrowsAsync<ItemScheduleGetRequestException>(() => GetAsync(service))).StatusCode);
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
    public static async Task Get_PreservesHttpFailuresAndNeverFollowsHeadersOrRetryHints(HttpStatusCode status, HttpStatusCode expected)
    {
        using var response = Response("""{"errorCode":"InsufficientPrivileges","message":"private","isRetriable":true,"moreDetails":[{"message":"private"}]}""", status, "60");
        response.Headers.Location = new Uri("https://example.invalid/private");
        response.Headers.TryAddWithoutValidation("x-ms-operation-id", ScheduleId);
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
        using var client = new HttpClient(handler);
        var error = await Assert.ThrowsAsync<ItemScheduleGetRequestException>(() => GetAsync(new FabricCoreService(client, Credential())));
        Assert.Equal(expected, error.StatusCode);
        Assert.Equal(TimeSpan.FromSeconds(60), error.RetryAfter?.Delta);
        Assert.Equal(response.IsSuccessStatusCode ? null : "InsufficientPrivileges", error.ErrorCode);
        Assert.Equal(1, handler.CallCount);
        Assert.Throws<ObjectDisposedException>(() => response.Content.ReadAsStream(TestContext.Current.CancellationToken));
    }

    [Theory]
    [InlineData("""{"errorCode":"\uD800"}""")]
    [InlineData("""{"\uD800":1}""")]
    [InlineData("""{"errorCode":"ItemNotFound","errorCode":"Other"}""")]
    [InlineData("""{"errorCode":"private invalid"}""")]
    [InlineData("private-invalid-json")]
    public static async Task Get_InvalidErrorMetadataRetainsStatusAndRetryAfter(string json)
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(Response(json, HttpStatusCode.TooManyRequests, "60")));
        using var client = new HttpClient(handler);
        var error = await Assert.ThrowsAsync<ItemScheduleGetRequestException>(() => GetAsync(new FabricCoreService(client, Credential())));
        Assert.Equal(HttpStatusCode.TooManyRequests, error.StatusCode);
        Assert.Equal(TimeSpan.FromSeconds(60), error.RetryAfter?.Delta);
        Assert.True(error.ErrorMetadataUnavailable);
        Assert.Null(error.ErrorCode);
        Assert.Equal(1, handler.CallCount);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public static async Task Get_OversizedErrorMetadataRetainsStatusAndRetryAfter(bool contentLength)
    {
        const int limit = FabricResponseBodyReader.DefaultMaxJsonBytes;
        using var stream = new OperationTestStream(Encoding.UTF8.GetBytes(new string('x', limit + 2)));
        using var response = new HttpResponseMessage(HttpStatusCode.TooManyRequests) { Content = new StreamContent(stream) };
        response.Content.Headers.ContentType = new("application/json");
        response.Headers.TryAddWithoutValidation("Retry-After", "60");
        if (contentLength)
        {
            response.Content.Headers.ContentLength = limit + 2;
        }
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
        using var client = new HttpClient(handler);
        var error = await Assert.ThrowsAsync<ItemScheduleGetRequestException>(() => GetAsync(new FabricCoreService(client, Credential())));
        Assert.Equal(HttpStatusCode.TooManyRequests, error.StatusCode);
        Assert.Equal(TimeSpan.FromSeconds(60), error.RetryAfter?.Delta);
        Assert.True(error.ErrorMetadataUnavailable);
        Assert.Null(error.ErrorCode);
        Assert.Equal(contentLength ? 0 : limit + 1, stream.BytesRead);
        Assert.True(stream.Disposed);
        Assert.Equal(1, handler.CallCount);
    }

    [Theory]
    [InlineData(HttpStatusCode.OK)]
    [InlineData(HttpStatusCode.TooManyRequests)]
    public static async Task Get_NonJsonMediaPreservesRetryAndKnownErrorStatus(HttpStatusCode status)
    {
        using var response = Response(ScheduleJson, status, "60");
        response.Content.Headers.ContentType = new("text/plain");
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
        using var client = new HttpClient(handler);
        var error = await Assert.ThrowsAsync<ItemScheduleGetRequestException>(() => GetAsync(new FabricCoreService(client, Credential())));
        Assert.Equal(status == HttpStatusCode.OK ? HttpStatusCode.BadGateway : status, error.StatusCode);
        Assert.Equal(status != HttpStatusCode.OK, error.ErrorMetadataUnavailable);
        Assert.Equal(TimeSpan.FromSeconds(60), error.RetryAfter?.Delta);
        Assert.Equal(1, handler.CallCount);
    }

    [Theory]
    [InlineData("0", 0L)]
    [InlineData("60", 60L)]
    [InlineData("-1", null)]
    [InlineData("private", null)]
    [InlineData("60, 120", null)]
    [InlineData("999999999999999999999999", null)]
    [InlineData("Tue, 01 Jan 2030 00:00:00 GMT", null)]
    public static async Task Get_UsesOnlyValidatedRetryGuidance(string retry, long? seconds)
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(Response(ScheduleJson, retryAfter: retry)));
        using var client = new HttpClient(handler);
        var result = await GetAsync(new FabricCoreService(client, Credential()));
        Assert.Equal(seconds, result.RetryAfterSeconds);
        Assert.Equal(retry.StartsWith("Tue", StringComparison.Ordinal), result.RetryAfterUtc.HasValue);
        Assert.Equal(1, handler.CallCount);
    }

    [Fact]
    public static async Task Get_DoesNotCombineMultipleRetryAfterValues()
    {
        using var response = Response(ScheduleJson);
        response.Headers.TryAddWithoutValidation("Retry-After", ["60", "120"]);
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
        using var client = new HttpClient(handler);
        var result = await GetAsync(new FabricCoreService(client, Credential()));
        Assert.Null(result.RetryAfterSeconds);
        Assert.Null(result.RetryAfterUtc);
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
    public static async Task Get_ValidatesContextBeforeAuthentication(string option, string value)
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) => throw new InvalidOperationException("No HTTP expected."));
        using var client = new HttpClient(handler);
        var credential = Credential();
        await Assert.ThrowsAsync<FabricItemInputException>(() => new FabricCoreService(client, credential).GetItemScheduleAsync(
            option == "workspace" ? value : WorkspaceId, option == "item" ? value : ItemId,
            option == "job" ? value : JobType, option == "schedule" ? value : ScheduleId, TestContext.Current.CancellationToken));
        Assert.Equal(0, handler.CallCount);
        Assert.Empty(credential.ReceivedCalls());
    }

    [Theory]
    [InlineData(false, 0)]
    [InlineData(false, 1)]
    [InlineData(true, 0)]
    [InlineData(true, 1)]
    public static async Task Get_EnforcesExactUtf8JobTypeBudget(bool multibyte, int excess)
    {
        var jobType = new string(multibyte ? '\u00e9' : 'x', ItemJobRunInput.MaxJobTypeBytes / (multibyte ? 2 : 1)) + new string('x', excess);
        using var handler = new FabricCoreHttpMessageHandler((request, _) =>
        {
            Assert.Equal($"{CollectionUrl(jobType)}/{ScheduleId}", request.RequestUri?.AbsoluteUri);
            return Task.FromResult(Response(ScheduleJson));
        });
        using var client = new HttpClient(handler);
        var credential = Credential();
        var service = new FabricCoreService(client, credential);
        if (excess == 0)
        {
            Assert.Equal(jobType, (await service.GetItemScheduleAsync(WorkspaceId, ItemId, jobType, ScheduleId, TestContext.Current.CancellationToken)).JobType);
        }
        else
        {
            await Assert.ThrowsAsync<FabricItemInputException>(() => service.GetItemScheduleAsync(WorkspaceId, ItemId, jobType, ScheduleId, TestContext.Current.CancellationToken));
            Assert.Empty(credential.ReceivedCalls());
        }
        Assert.Equal(excess == 0 ? 1 : 0, handler.CallCount);
    }

    [Fact]
    public static async Task Get_RejectsUnpairedSurrogatesBeforeAuthentication()
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) => throw new InvalidOperationException("No HTTP expected."));
        using var client = new HttpClient(handler);
        var credential = Credential();
        foreach (var jobType in new[] { "\uD800", "\uDC00", "private\uD800value" })
        {
            await Assert.ThrowsAsync<FabricItemInputException>(() => new FabricCoreService(client, credential)
                .GetItemScheduleAsync(WorkspaceId, ItemId, jobType, ScheduleId, TestContext.Current.CancellationToken));
        }
        Assert.Equal(0, handler.CallCount);
        Assert.Empty(credential.ReceivedCalls());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public static async Task Get_PreSendCancellationStopsBeforeHttp(bool duringAuth)
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
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => GetAsync(new FabricCoreService(client, credential), cancellation.Token));
        Assert.Equal(0, handler.CallCount);
        await credential.Received(duringAuth ? 1 : 0).GetTokenAsync(Arg.Any<TokenRequestContext>(), cancellation.Token);
    }

    [Fact]
    public static async Task Get_AuthenticationFailureStopsBeforeHttp()
    {
        var credential = Credential();
        credential.GetTokenAsync(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>())
            .Returns(ValueTask.FromException<AccessToken>(new AuthenticationFailedException("private")));
        using var handler = new FabricCoreHttpMessageHandler((_, _) => throw new InvalidOperationException("No HTTP expected."));
        using var client = new HttpClient(handler);
        await Assert.ThrowsAsync<AuthenticationFailedException>(() => GetAsync(new FabricCoreService(client, credential)));
        Assert.Equal(0, handler.CallCount);
    }

    [Theory]
    [InlineData("network")]
    [InlineData("timeout")]
    [InlineData("cancellation")]
    public static async Task Get_FailedSendNeverRetries(string failure)
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
        Assert.NotNull(await Record.ExceptionAsync(() => GetAsync(new FabricCoreService(client, Credential()), cancellation.Token)));
        Assert.Equal(1, handler.CallCount);
    }

    [Theory]
    [InlineData("io", HttpStatusCode.OK)]
    [InlineData("network", HttpStatusCode.OK)]
    [InlineData("cancellation", HttpStatusCode.OK)]
    [InlineData("io", HttpStatusCode.TooManyRequests)]
    [InlineData("cancellation", HttpStatusCode.TooManyRequests)]
    public static async Task Get_FailedBodyReadsKeepRetryAndKnownErrorStatusAndDispose(string failure, HttpStatusCode status)
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
        var error = await Assert.ThrowsAsync<ItemScheduleGetRequestException>(() => GetAsync(new FabricCoreService(client, Credential()), cancellation.Token));
        Assert.Equal(status == HttpStatusCode.TooManyRequests ? status :
            failure == "cancellation" ? HttpStatusCode.RequestTimeout : HttpStatusCode.BadGateway, error.StatusCode);
        Assert.Equal(TimeSpan.FromSeconds(60), error.RetryAfter?.Delta);
        Assert.Equal(status == HttpStatusCode.TooManyRequests, error.ErrorMetadataUnavailable);
        Assert.True(stream.Disposed);
        Assert.Equal(1, handler.CallCount);
    }

    [Theory]
    [InlineData(HttpStatusCode.OK, HttpStatusCode.RequestTimeout)]
    [InlineData(HttpStatusCode.TooManyRequests, HttpStatusCode.TooManyRequests)]
    public static async Task Get_CancellationAfterHeadersNeverReturnsASnapshot(HttpStatusCode status, HttpStatusCode expected)
    {
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        using var response = Response(ScheduleJson, status, "60");
        using var handler = new FabricCoreHttpMessageHandler((_, _) =>
        {
            cancellation.Cancel();
            return Task.FromResult(response);
        });
        using var client = new HttpClient(handler);
        var error = await Assert.ThrowsAsync<ItemScheduleGetRequestException>(() => GetAsync(new FabricCoreService(client, Credential()), cancellation.Token));
        Assert.Equal(expected, error.StatusCode);
        Assert.Equal(TimeSpan.FromSeconds(60), error.RetryAfter?.Delta);
        Assert.Equal(1, handler.CallCount);
    }

    [Fact]
    public static async Task Get_IsolatesConcurrentCallerCredentialsTargetsAndSensitiveResults()
    {
        var identity = new AsyncLocal<string>();
        var credential = Credential();
        credential.GetTokenAsync(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>())
            .Returns(_ => new AccessToken(identity.Value!, DateTimeOffset.MaxValue));
        var observed = new ConcurrentBag<string>();
        using var handler = new FabricCoreHttpMessageHandler(async (request, _) =>
        {
            var current = request.Headers.Authorization!.Parameter!;
            observed.Add(current);
            await Task.Yield();
            Assert.Equal($"{FabricEndpoints.GetFabricApiBaseUrl()}/workspaces/{current}/items/{current}/jobs/{current}/schedules/{current}", request.RequestUri?.AbsoluteUri);
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Null(request.Content);
            return Response($$$"""{"id":"{{{current}}}","enabled":false,"executionData":{"caller":"{{{current}}}"}}""");
        });
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, credential);
        await Task.WhenAll(Enumerable.Range(0, 8).Select(async _ =>
        {
            var id = Guid.NewGuid();
            var text = id.ToString();
            identity.Value = text;
            var result = await service.GetItemScheduleAsync(text, text, text, text, TestContext.Current.CancellationToken);
            Assert.Equal(id, result.WorkspaceId);
            Assert.Equal(id, result.ItemId);
            Assert.Equal(text, result.JobType);
            Assert.Equal(id, result.Schedule.Id);
            Assert.Equal(text, result.Schedule.ExecutionData!.Value.GetProperty("caller").GetString());
        }));
        Assert.Equal(8, handler.CallCount);
        Assert.Equal(8, observed.Distinct().Count());
        await credential.Received(8).GetTokenAsync(Arg.Any<TokenRequestContext>(), TestContext.Current.CancellationToken);
    }

    private static Task<ItemScheduleGetCommandResult> GetAsync(FabricCoreService service, CancellationToken? cancellationToken = null) =>
        service.GetItemScheduleAsync(WorkspaceId, ItemId, JobType, ScheduleId, cancellationToken ?? TestContext.Current.CancellationToken);
}
