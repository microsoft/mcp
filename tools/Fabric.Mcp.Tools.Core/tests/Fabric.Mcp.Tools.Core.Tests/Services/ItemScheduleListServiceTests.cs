// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

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
using static Fabric.Mcp.Tools.Core.Tests.TestSupport.ItemScheduleListTestData;

namespace Fabric.Mcp.Tools.Core.Tests.Services;

public sealed class ItemScheduleListServiceTests()
{
    [Theory]
    [InlineData(JobType)]
    [InlineData("Future / job?#%+")]
    [InlineData(" /../ ")]
    [InlineData("%2e%2e")]
    [InlineData("\u00e9\U0001F680")]
    public static async Task List_UsesOneNormalizedAuthenticatedGetAndPreservesCompleteSettings(string jobType)
    {
        using var stream = new OperationTestStream(Encoding.UTF8.GetBytes(Page(DocumentedToken, true, jobType)));
        using var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StreamContent(stream) };
        response.Content.Headers.ContentType = new("application/json");
        response.Headers.Location = new Uri("https://example.invalid/never-follow");
        response.Headers.TryAddWithoutValidation("x-ms-operation-id", "not-a-schedule-id");
        using var handler = new FabricCoreHttpMessageHandler((request, token) =>
        {
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal(CollectionUrl(jobType), request.RequestUri?.AbsoluteUri);
            Assert.Null(request.Content);
            Assert.Equal("Bearer", request.Headers.Authorization?.Scheme);
            Assert.Equal("job-test-token", request.Headers.Authorization?.Parameter);
            Assert.Equal("Fabric Core MCP", request.Headers.UserAgent.ToString());
            Assert.True(token.CanBeCanceled);
            return Task.FromResult(response);
        });
        using var client = new HttpClient(handler);
        var credential = Credential();

        var result = await new FabricCoreService(client, credential).ListItemSchedulesAsync(
            $"{{{WorkspaceId.ToUpperInvariant()}}}", Guid.Parse(ItemId).ToString("N").ToUpperInvariant(), jobType,
            cancellationToken: TestContext.Current.CancellationToken);

        var schedule = Assert.Single(result.Schedules);
        var serialized = JsonSerializer.SerializeToElement(schedule, CoreJsonContext.Default.FabricItemSchedule);
        using var expected = JsonDocument.Parse(ScheduleJson);
        Assert.True(JsonElement.DeepEquals(expected.RootElement, serialized));
        Assert.Equal(DocumentedToken, result.ContinuationToken);
        var output = JsonSerializer.Serialize(result, CoreJsonContext.Default.ItemScheduleListCommandResult);
        Assert.DoesNotContain("continuationUri", output);
        Assert.DoesNotContain("operationId", output);
        Assert.Equal(1, handler.CallCount);
        Assert.True(stream.Disposed);
        Assert.Throws<ObjectDisposedException>(() => response.Content.ReadAsStream(TestContext.Current.CancellationToken));
        await credential.Received(1).GetTokenAsync(
            Arg.Is<TokenRequestContext>(context => context.Scopes.SequenceEqual(FabricEndpoints.FabricScopes)),
            TestContext.Current.CancellationToken);
    }

    [Theory]
    [InlineData(false, false, false)]
    [InlineData(false, false, true)]
    [InlineData(false, true, false)]
    [InlineData(false, true, true)]
    [InlineData(true, false, false)]
    [InlineData(true, false, true)]
    [InlineData(true, true, false)]
    [InlineData(true, true, true)]
    public static async Task List_AcceptsAbsentOrNullContinuationOnEmptyAndNonemptyFinalPages(bool nullToken, bool nullUri, bool empty)
    {
        var page = JsonNode.Parse(Page())!.AsObject();
        if (nullToken)
        {
            page["continuationToken"] = null;
        }
        if (nullUri)
        {
            page["continuationUri"] = null;
        }
        if (empty)
        {
            page["value"] = new JsonArray();
        }
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(Response(page.ToJsonString())));
        using var client = new HttpClient(handler);
        var result = await ReadAsync(new FabricCoreService(client, Credential()));
        Assert.Equal(empty ? 0 : 1, result.Schedules.Count);
        Assert.Null(result.ContinuationToken);
        Assert.Equal(1, handler.CallCount);
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public static async Task List_ContinuesEmptyOrNonemptyPagesOnlyOnExplicitCallWithTokenAndAbsentOrNullUri(bool nullUri, bool empty)
    {
        var page = JsonNode.Parse(Page(DocumentedToken))!.AsObject();
        if (nullUri)
        {
            page["continuationUri"] = null;
        }
        if (empty)
        {
            page["value"] = new JsonArray();
        }
        var requests = new List<string>();
        using var handler = new FabricCoreHttpMessageHandler((request, _) =>
        {
            requests.Add(request.RequestUri!.AbsoluteUri);
            return Task.FromResult(Response(requests.Count == 1 ? page.ToJsonString() : """{"value":[]}"""));
        });
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, Credential());
        var first = await ReadAsync(service);
        Assert.Equal(empty ? 0 : 1, first.Schedules.Count);
        Assert.Equal(DocumentedToken, first.ContinuationToken);
        Assert.Equal([CollectionUrl()], requests);

        var second = await ReadAsync(service, first.ContinuationToken);

        Assert.Empty(second.Schedules);
        Assert.Null(second.ContinuationToken);
        Assert.Equal([CollectionUrl(), $"{CollectionUrl()}?continuationToken={DocumentedToken}"], requests);
        Assert.Equal(2, handler.CallCount);
    }

    [Theory]
    [InlineData(DocumentedToken, DocumentedToken)]
    [InlineData(" raw+/= &?# ", "%20raw%2B%2F%3D%20%26%3F%23%20")]
    [InlineData("%3D%2f%26%3F%23%2B%25", "%3D%2f%26%3F%23%2B%25")]
    [InlineData("%G1%", "%25G1%25")]
    [InlineData("%2541%2526", "%2541%2526")]
    [InlineData("%41%7e%2D%5f%2E", "A~-_.")]
    [InlineData("\u00e9\U0001F680 /", "%C3%A9%F0%9F%9A%80%20%2F")]
    [InlineData("%0D%0A", "%0D%0A")]
    public static async Task List_EncodesRawTokenSpansOnlyOnAnExplicitSecondCall(string token, string encoded)
    {
        var requests = new List<string>();
        using var handler = new FabricCoreHttpMessageHandler((request, _) =>
        {
            requests.Add(request.RequestUri!.AbsoluteUri);
            Assert.Empty(request.RequestUri.Fragment);
            return Task.FromResult(Response(requests.Count == 1 ? Page(token, true) : """{"value":[]}"""));
        });
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, Credential());
        Assert.Equal(token, (await ReadAsync(service)).ContinuationToken);
        Assert.Equal([CollectionUrl()], requests);
        Assert.Empty((await ReadAsync(service, token)).Schedules);
        Assert.Equal([CollectionUrl(), $"{CollectionUrl()}?continuationToken={encoded}"], requests);
    }

    [Theory]
    [InlineData("opaque/../cursor", "opaque/../cursor")]
    [InlineData("opaque/./cursor", "opaque/./cursor")]
    // cspell:disable
    [InlineData("opaque/../cursor", "opaque%2F..%2Fcursor")]
    [InlineData("opaque%2F.%2Fcursor", "opaque/./cursor")]
    // cspell:enable
    public static async Task List_KeepsDotSegmentsInQueryDataSeparateFromTheEscapedJobType(string token, string query)
    {
        const string jobType = "Future / job";
        var page = JsonNode.Parse(Page(token))!.AsObject();
        page["continuationUri"] = $"{CollectionUrl(jobType)}?continuationToken={query}";
        using var handler = new FabricCoreHttpMessageHandler((request, _) =>
        {
            Assert.Equal(new Uri(CollectionUrl(jobType)).AbsolutePath, request.RequestUri!.AbsolutePath);
            return Task.FromResult(Response(page.ToJsonString()));
        });
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, Credential());
        Assert.Equal(token, (await ReadAsync(service, jobType: jobType)).ContinuationToken);
        Assert.Equal(1, handler.CallCount);
        Assert.Equal(token, (await ReadAsync(service, token, jobType)).ContinuationToken);
        Assert.Equal(2, handler.CallCount);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public static async Task List_OnlyRequiresIdAndEnabledAndDoesNotInventOptionalFields(bool nullFields)
    {
        var row = new JsonObject { ["id"] = ScheduleId, ["enabled"] = false };
        foreach (var name in new[] { "createdDateTime", "configuration", "owner", "executionData" })
        {
            if (nullFields)
            {
                row[name] = null;
            }
        }
        using var handler = new FabricCoreHttpMessageHandler((_, _) =>
            Task.FromResult(Response(new JsonObject { ["value"] = new JsonArray(row) }.ToJsonString())));
        using var client = new HttpClient(handler);
        var schedule = Assert.Single((await ReadAsync(new FabricCoreService(client, Credential()))).Schedules);
        Assert.False(schedule.Enabled);
        Assert.Null(schedule.CreatedDateTime);
        Assert.Null(schedule.Configuration);
        Assert.Null(schedule.Owner);
        Assert.Null(schedule.ExecutionData);
        var json = JsonSerializer.SerializeToElement(schedule, CoreJsonContext.Default.FabricItemSchedule);
        Assert.Equal(["enabled", "id"], json.EnumerateObject().Select(property => property.Name).Order());
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
    [InlineData("FutureSchedule", """{"interval":"not-an-integer","opaque":{"name":1,"Name":2}}""")]
    public static async Task List_PreservesDocumentedAndFutureConfigurationsWithoutSchedulingInterpretation(string type, string fields)
    {
        var row = JsonNode.Parse(ScheduleJson)!.AsObject();
        var config = row["configuration"]!.AsObject();
        config.Remove("interval");
        config["type"] = type;
        config["localTimeZoneId"] = "Future workload time zone";
        config["endDateTime"] = "2024-04-30T23:59:00.1234567+00:00";
        foreach (var property in JsonNode.Parse(fields)!.AsObject())
        {
            config[property.Key] = property.Value?.DeepClone();
        }
        row["owner"]!["type"] = "FuturePrincipal";
        row["owner"]!["futureDetails"] = new JsonObject { ["setting"] = true };
        row["extra"] = "private-unknown-top-level";
        using var handler = new FabricCoreHttpMessageHandler((_, _) =>
            Task.FromResult(Response(new JsonObject { ["value"] = new JsonArray(row) }.ToJsonString())));
        using var client = new HttpClient(handler);
        var result = await ReadAsync(new FabricCoreService(client, Credential()));
        var schedule = Assert.Single(result.Schedules);
        Assert.Equal("2024-04-28T05:35:20.5366667", schedule.CreatedDateTime);
        Assert.Equal(config.ToJsonString(), JsonNode.Parse(schedule.Configuration!.Value.GetRawText())!.ToJsonString());
        Assert.Equal("FuturePrincipal", schedule.Owner!.Value.GetProperty("type").GetString());
        Assert.True(schedule.Owner.Value.GetProperty("futureDetails").GetProperty("setting").GetBoolean());
        Assert.DoesNotContain("private-unknown-top-level", JsonSerializer.Serialize(result, CoreJsonContext.Default.ItemScheduleListCommandResult));
        Assert.Equal(1, handler.CallCount);
    }

    [Fact]
    public static async Task List_DoesNotApplyJobHistoryOrScheduleCreationLimitsOrDeduplicateRows()
    {
        var rows = Enumerable.Range(1, 101).Reverse().Select(index => new FabricItemSchedule
        {
            Id = Guid.Parse($"00000000-0000-4000-8000-{index:000000000000}"),
            Enabled = index % 2 == 0
        }).ToList();
        rows.Add(rows[0]);
        var page = new JsonObject
        {
            ["value"] = new JsonArray(rows.Select(row => JsonSerializer.SerializeToNode(row, CoreJsonContext.Default.FabricItemSchedule)).ToArray())
        };
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(Response(page.ToJsonString())));
        using var client = new HttpClient(handler);
        var result = await ReadAsync(new FabricCoreService(client, Credential()));
        Assert.Equal(rows, result.Schedules);
        Assert.Equal(102, result.Schedules.Count);
        Assert.Equal(1, handler.CallCount);
    }

    [Theory]
    [InlineData("")]
    [InlineData("{private")]
    [InlineData("null")]
    [InlineData("[]")]
    [InlineData("{}")]
    [InlineData("""{"value":null}""")]
    [InlineData("""{"value":{}}""")]
    [InlineData("""{"value":[null]}""")]
    [InlineData("""{"value":[{}]}""")]
    [InlineData("""{"value":[1]}""")]
    [InlineData("""{"value":[], "value":[]}""")]
    [InlineData("""{"value":[], "Value":[{}]}""")]
    [InlineData("""{"value":[], "extra":{"same":1,"same":2}}""")]
    [InlineData("""{"value":[], "extra":{"text":"\uD800"}}""")]
    [InlineData("""{"value":[], "extra":{"name\uDC00":"private"}}""")]
    [InlineData("""{"value":[{"id":"eeeeeeee-eeee-4eee-8eee-eeeeeeeeeeee","enabled":true,"executionData":{"value":"\uD800"}}]}""")]
    [InlineData("""{"value":[{"id":"eeeeeeee-eeee-4eee-8eee-eeeeeeeeeeee","enabled":true,"enabled":false}]}""")]
    [InlineData("""{"value":[{"id":"eeeeeeee-eeee-4eee-8eee-eeeeeeeeeeee","enabled":true,"Enabled":false}]}""")]
    [InlineData("""{"value":[{"id":"eeeeeeee-eeee-4eee-8eee-eeeeeeeeeeee","enabled":true,"executionData":{"value":1,"value":2}}]}""")]
    public static async Task List_RejectsMalformedAmbiguousAndInvalidUnicodePages(string json) => await AssertInvalidAsync(json);

    [Theory]
    [InlineData("id", "\"00000000-0000-0000-0000-000000000000\"")]
    [InlineData("id", "\"private-not-a-uuid\"")]
    [InlineData("enabled", "null")]
    [InlineData("enabled", "\"true\"")]
    [InlineData("createdDateTime", "42")]
    [InlineData("createdDateTime", "\"not-a-date\"")]
    [InlineData("configuration", "[]")]
    [InlineData("configuration", "{}")]
    [InlineData("owner", "[]")]
    [InlineData("owner", "{}")]
    [InlineData("owner", """{"id":"00000000-0000-0000-0000-000000000000","type":"User"}""")]
    [InlineData("owner", """{"id":"dddddddd-dddd-4ddd-8ddd-dddddddddddd","type":42}""")]
    [InlineData("executionData", "\"private\"")]
    [InlineData("executionData", "[]")]
    public static async Task List_RejectsAnInvalidLaterRowWithoutReturningTheFirst(string name, string value)
    {
        var row = JsonNode.Parse(ScheduleJson)!.AsObject();
        row[name] = JsonNode.Parse(value);
        await AssertInvalidAsync($"{{\"value\":[{ScheduleJson},{row.ToJsonString()}]}}");
    }

    [Theory]
    [InlineData("id")]
    [InlineData("enabled")]
    public static async Task List_RejectsMissingRequiredFields(string name)
    {
        var row = JsonNode.Parse(ScheduleJson)!.AsObject();
        row.Remove(name);
        await AssertInvalidAsync($"{{\"value\":[{row.ToJsonString()}]}}");
    }

    [Theory]
    [InlineData("type", "null")]
    [InlineData("type", "\" \"")]
    [InlineData("localTimeZoneId", "42")]
    [InlineData("startDateTime", "\"not-a-date\"")]
    [InlineData("endDateTime", "null")]
    [InlineData("interval", "0")]
    [InlineData("interval", "5270401")]
    [InlineData("interval", "1.5")]
    [InlineData("interval", "\"10\"")]
    [InlineData("interval", "null")]
    public static async Task List_RejectsMalformedConfigurationFields(string name, string value)
    {
        var row = JsonNode.Parse(ScheduleJson)!.AsObject();
        row["configuration"]![name] = JsonNode.Parse(value);
        await AssertInvalidAsync($"{{\"value\":[{row.ToJsonString()}]}}");
    }

    [Theory]
    [InlineData("type")]
    [InlineData("startDateTime")]
    [InlineData("endDateTime")]
    [InlineData("localTimeZoneId")]
    [InlineData("interval")]
    public static async Task List_RejectsMissingRequiredConfigurationFields(string name)
    {
        var row = JsonNode.Parse(ScheduleJson)!.AsObject();
        row["configuration"]!.AsObject().Remove(name);
        await AssertInvalidAsync($"{{\"value\":[{row.ToJsonString()}]}}");
    }

    [Theory]
    [InlineData("Daily", """{"times":[]}""")]
    [InlineData("Daily", """{"times":["24:00"]}""")]
    [InlineData("Daily", """{"times":["24:01"]}""")]
    [InlineData("Daily", """{"times":[42]}""")]
    [InlineData("Weekly", """{"times":["24:00"],"weekdays":["Monday"]}""")]
    [InlineData("Weekly", """{"times":["12:30"],"weekdays":[]}""")]
    [InlineData("Weekly", """{"times":["12:30"],"weekdays":[null]}""")]
    [InlineData("Monthly", """{"times":["12:30"],"recurrence":0,"occurrence":{"occurrenceType":"DayOfMonth","dayOfMonth":1}}""")]
    [InlineData("Monthly", """{"times":["24:00"],"recurrence":1,"occurrence":{"occurrenceType":"DayOfMonth","dayOfMonth":1}}""")]
    [InlineData("Monthly", """{"times":["12:30"],"recurrence":13,"occurrence":{"occurrenceType":"DayOfMonth","dayOfMonth":1}}""")]
    [InlineData("Monthly", """{"times":["12:30"],"recurrence":1,"occurrence":{"occurrenceType":"DayOfMonth","dayOfMonth":0}}""")]
    [InlineData("Monthly", """{"times":["12:30"],"recurrence":1,"occurrence":{"occurrenceType":"DayOfMonth","dayOfMonth":32}}""")]
    [InlineData("Monthly", """{"times":["12:30"],"recurrence":1,"occurrence":null}""")]
    [InlineData("Monthly", """{"times":["12:30"],"recurrence":1,"occurrence":{"occurrenceType":"OrdinalWeekday","weekday":"Monday"}}""")]
    public static async Task List_RejectsMalformedKnownConfigurationShapes(string type, string fields)
    {
        var row = JsonNode.Parse(ScheduleJson)!.AsObject();
        var config = row["configuration"]!.AsObject();
        config["type"] = type;
        foreach (var property in JsonNode.Parse(fields)!.AsObject())
        {
            config[property.Key] = property.Value?.DeepClone();
        }
        await AssertInvalidAsync($"{{\"value\":[{row.ToJsonString()}]}}");
    }

    [Theory]
    [InlineData("times", 100, true)]
    [InlineData("times", 101, false)]
    [InlineData("weekdays", 7, true)]
    [InlineData("weekdays", 8, false)]
    public static async Task List_EnforcesDocumentedArrayBoundsWithoutReorderingOrDeduplication(string field, int count, bool valid)
    {
        var row = JsonNode.Parse(ScheduleJson)!.AsObject();
        var config = row["configuration"]!.AsObject();
        config["type"] = "Weekly";
        config["times"] = new JsonArray("12:30");
        config["weekdays"] = new JsonArray("Monday");
        config[field] = new JsonArray(Enumerable.Range(0, count)
            .Select(_ => (JsonNode?)JsonValue.Create(field == "times" ? "12:30" : "Monday")).ToArray());
        var json = $"{{\"value\":[{row.ToJsonString()}]}}";
        if (!valid)
        {
            await AssertInvalidAsync(json);
            return;
        }
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(Response(json)));
        using var client = new HttpClient(handler);
        var result = await ReadAsync(new FabricCoreService(client, Credential()));
        Assert.Equal(count, Assert.Single(result.Schedules).Configuration!.Value.GetProperty(field).GetArrayLength());
        Assert.Equal(1, handler.CallCount);
    }

    [Fact]
    public static async Task List_RejectsInvalidUtf8EvenInIgnoredFields()
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
    [InlineData("""{"continuationToken":""}""")]
    [InlineData("""{"continuationToken":" "}""")]
    [InlineData("""{"continuationToken":false}""")]
    [InlineData("""{"continuationToken":42}""")]
    [InlineData("""{"continuationToken":[]}""")]
    [InlineData("""{"continuationToken":{}}""")]
    [InlineData("""{"continuationToken":"private\r\n"}""")]
    [InlineData("""{"continuationToken":"\uD800"}""")]
    [InlineData("""{"continuationUri":""}""")]
    [InlineData("""{"continuationUri":false}""")]
    [InlineData("""{"continuationUri":0}""")]
    [InlineData("""{"continuationUri":[]}""")]
    [InlineData("""{"continuationUri":{}}""")]
    [InlineData("""{"continuationToken":null,"continuationUri":"https://example.invalid/private"}""")]
    [InlineData("""{"continuationToken":false,"continuationUri":null}""")]
    [InlineData("""{"continuationToken":"next","continuationUri":""}""")]
    [InlineData("""{"continuationToken":"next","continuationUri":42}""")]
    [InlineData("""{"continuationToken":"next","continuationToken":"another"}""")]
    [InlineData("""{"continuationToken":"next","ContinuationToken":"another"}""")]
    public static async Task List_RejectsInvalidNonNullContinuationInsteadOfImplyingExhaustion(string metadata) =>
        await AssertInvalidAsync($"{{\"value\":[],{metadata[1..]}");

    [Fact]
    public static async Task List_RejectsWrongOriginScopeJobTypeRouteAndTokenWithoutFollowingUris()
    {
        var collection = CollectionUrl();
        foreach (var uri in new[]
        {
            "https://example.invalid/private?continuationToken=next",
            collection.Replace("https:", "http:", StringComparison.Ordinal) + "?continuationToken=next",
            collection.Replace("api.fabric.microsoft.com", "api.fabric.microsoft.com:444", StringComparison.Ordinal) + "?continuationToken=next",
            collection.Replace("api.fabric.microsoft.com", "user@api.fabric.microsoft.com", StringComparison.Ordinal) + "?continuationToken=next",
            collection.Replace(WorkspaceId, ActivityId, StringComparison.Ordinal) + "?continuationToken=next",
            collection.Replace(ItemId, ActivityId, StringComparison.Ordinal) + "?continuationToken=next",
            collection.Replace(JobType, "OtherJob", StringComparison.Ordinal) + "?continuationToken=next",
            collection.Replace(JobType, JobType.ToLowerInvariant(), StringComparison.Ordinal) + "?continuationToken=next",
            collection.Replace("/jobs/", "/jobs/./", StringComparison.Ordinal) + "?continuationToken=next",
            collection.Replace("/jobs/", "/private/../jobs/", StringComparison.Ordinal) + "?continuationToken=next",
            // cspell:disable-next-line
            collection.Replace("/jobs/", "/%6aobs/", StringComparison.Ordinal) + "?continuationToken=next",
            collection.Replace("/jobs/", "\\jobs\\", StringComparison.Ordinal) + "?continuationToken=next",
            new Uri(collection).AbsolutePath + "?continuationToken=next",
            collection + "/" + ScheduleId + "?continuationToken=next",
            collection,
            collection + "?continuationToken=wrong",
            collection + "?continuationToken=next#private",
            collection + "?continuationToken=next&extra=private",
            collection + "?continuationToken=next&continuationToken=next",
            collection + "?ContinuationToken=next"
        })
        {
            var page = JsonNode.Parse(Page("next"))!.AsObject();
            page["continuationUri"] = uri;
            await AssertInvalidAsync(page.ToJsonString());
        }
        foreach (var nullToken in new[] { false, true })
        {
            var page = JsonNode.Parse(Page("next", true))!.AsObject();
            page.Remove("continuationToken");
            if (nullToken)
            {
                page["continuationToken"] = null;
            }
            await AssertInvalidAsync(page.ToJsonString());
        }
    }

    [Fact]
    public static async Task List_ValidatesIdsJobTypeAndTokenBeforeCredentialsOrHttp()
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) => throw new InvalidOperationException("No HTTP expected."));
        using var client = new HttpClient(handler);
        var credential = Credential();
        var service = new FabricCoreService(client, credential);
        foreach (var invalid in new[] { null, "", " ", "private", Guid.Empty.ToString(), "../jobs", ItemId + "?private" })
        {
            await Assert.ThrowsAsync<FabricItemInputException>(() => service.ListItemSchedulesAsync(
                invalid!, ItemId, JobType, cancellationToken: TestContext.Current.CancellationToken));
            await Assert.ThrowsAsync<FabricItemInputException>(() => service.ListItemSchedulesAsync(
                WorkspaceId, invalid!, JobType, cancellationToken: TestContext.Current.CancellationToken));
        }
        foreach (var invalid in new[] { null, "", " ", ".", "..", "private\r\n", "\uD800", "\uDC00" })
        {
            await Assert.ThrowsAsync<FabricItemInputException>(() => ReadAsync(service, jobType: invalid!));
        }
        foreach (var invalid in new[] { "", " ", "private\r\n", "\uD800", "\uDC00" })
        {
            await Assert.ThrowsAsync<ArgumentException>(() => ReadAsync(service, invalid));
        }
        Assert.Empty(credential.ReceivedCalls());
        Assert.Equal(0, handler.CallCount);
    }

    [Theory]
    [InlineData(false, 0)]
    [InlineData(false, 1)]
    [InlineData(true, 0)]
    [InlineData(true, 1)]
    public static async Task List_EnforcesExactUtf8JobTypeAndTokenBudgets(bool multibyte, int excess)
    {
        var token = new string(multibyte ? '\u00e9' : 'x', multibyte ? 4096 : 8192) + new string('x', excess);
        var jobType = new string(multibyte ? '\u00e9' : 'x', multibyte ? 512 : 1024) + new string('x', excess);
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(Response(Page(token))));
        using var client = new HttpClient(handler);
        var credential = Credential();
        var service = new FabricCoreService(client, credential);
        if (excess == 0)
        {
            Assert.Equal(token, (await ReadAsync(service, token, jobType)).ContinuationToken);
        }
        else
        {
            await Assert.ThrowsAsync<ArgumentException>(() => ReadAsync(service, token));
            await Assert.ThrowsAsync<FabricItemInputException>(() => ReadAsync(service, jobType: jobType));
            Assert.Empty(credential.ReceivedCalls());
            Assert.Equal(0, handler.CallCount);
            await Assert.ThrowsAsync<JsonException>(() => ReadAsync(service));
        }
        Assert.Equal(1, handler.CallCount);
    }

    [Theory]
    [InlineData(false, 0)]
    [InlineData(false, 1)]
    [InlineData(true, 0)]
    [InlineData(true, 1)]
    public static async Task List_EnforcesExactByteLimitWithoutReturningAPartialPage(bool knownLength, int excess)
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
            Assert.Single((await ReadAsync(service)).Schedules);
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
    public static async Task List_EnforcesExactDepthBudget(int depth)
    {
        var json = "{\"value\":[],\"extra\":" + new string('[', depth - 1) + "0" + new string(']', depth - 1) + "}";
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(Response(json)));
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, Credential());
        if (depth == 64)
        {
            Assert.Empty((await ReadAsync(service)).Schedules);
        }
        else
        {
            await Assert.ThrowsAsync<JsonException>(() => ReadAsync(service));
        }
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
    public static async Task List_PreservesStatusAndRetryWithoutReadingErrorBodiesOrFollowingUrls(HttpStatusCode status)
    {
        using var content = new UnreadableHttpContent();
        using var response = new HttpResponseMessage(status) { Content = content };
        response.Headers.Location = new Uri("https://example.invalid/private");
        response.Headers.TryAddWithoutValidation("Retry-After", "45");
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
        using var client = new HttpClient(handler);
        var error = await Assert.ThrowsAsync<ItemScheduleRequestException>(() => ReadAsync(new FabricCoreService(client, Credential())));
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
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(Response(Page(), retryAfter: header)));
        using var client = new HttpClient(handler);
        var result = await ReadAsync(new FabricCoreService(client, Credential()));
        Assert.Equal(expected, result.RetryAfterSeconds);
        Assert.Null(result.RetryAfterUtc);
        Assert.Equal(1, handler.CallCount);
    }

    [Fact]
    public static async Task List_PreservesHttpDatesAndRejectsMultipleRetryValues()
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
    public static async Task List_RejectsNonJsonWithoutReading(string? mediaType)
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
    public static async Task List_DoesNotMistakeUnrelatedReaderFailuresForMalformedJson()
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
    public static async Task List_AuthenticationFailureSendsNoRequest()
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
    public static async Task List_IsolatesConcurrentIdentitiesJobTypesAndTokens()
    {
        var identity = new AsyncLocal<string>();
        var credential = Credential();
        credential.GetTokenAsync(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>())
            .Returns(_ => new AccessToken(identity.Value!, DateTimeOffset.MaxValue));
        var requests = new ConcurrentBag<string>();
        using var handler = new FabricCoreHttpMessageHandler(async (request, _) =>
        {
            var type = request.RequestUri!.Segments[7].TrimEnd('/');
            Assert.Equal(type, request.Headers.Authorization?.Parameter);
            Assert.Equal("?continuationToken=" + type, request.RequestUri.Query);
            requests.Add(type);
            await Task.Yield();
            return Response(Page(type));
        });
        using var client = new HttpClient(handler);
        var service = new FabricCoreService(client, credential);
        async Task ReadForIdentityAsync(string caller)
        {
            identity.Value = caller;
            var result = await ReadAsync(service, caller, caller);
            Assert.Equal(caller, result.ContinuationToken);
        }
        await Task.WhenAll(ReadForIdentityAsync("CallerOne"), ReadForIdentityAsync("CallerTwo"));
        Assert.Equal(["CallerOne", "CallerTwo"], requests.Order());
        Assert.Equal(2, handler.CallCount);
        Assert.Null(client.DefaultRequestHeaders.Authorization);
    }

    private static Task<ItemScheduleListCommandResult> ReadAsync(
        FabricCoreService service, string? token = null, string jobType = JobType, CancellationToken? cancellationToken = null) =>
        service.ListItemSchedulesAsync(WorkspaceId, ItemId, jobType, token, cancellationToken ?? TestContext.Current.CancellationToken);

    private static async Task AssertInvalidAsync(string json)
    {
        using var response = Response(json);
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
        using var client = new HttpClient(handler);
        var exception = await Assert.ThrowsAsync<JsonException>(() => ReadAsync(new FabricCoreService(client, Credential())));
        Assert.Contains("No partial page", exception.Message);
        Assert.DoesNotContain("private", exception.Message);
        Assert.Equal(1, handler.CallCount);
        Assert.Throws<ObjectDisposedException>(() => response.Content.ReadAsStream(TestContext.Current.CancellationToken));
    }
}
