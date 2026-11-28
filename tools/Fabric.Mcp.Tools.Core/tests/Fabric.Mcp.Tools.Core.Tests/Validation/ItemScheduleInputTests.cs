// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Fabric.Mcp.Tools.Core.Models;
using Fabric.Mcp.Tools.Core.Validation;
using Xunit;
using static Fabric.Mcp.Tools.Core.Tests.TestSupport.ItemScheduleCreateTestData;

namespace Fabric.Mcp.Tools.Core.Tests.Validation;

public sealed class ItemScheduleInputTests()
{
    [Theory]
    [InlineData(false, null)]
    [InlineData(true, null)]
    [InlineData(false, "{}")]
    [InlineData(true, ExecutionData)]
    public static void Body_PreservesExplicitEnabledConfigurationAndOpaqueExecutionData(bool enabled, string? execution)
    {
        var body = ItemScheduleInput.CreateBody(enabled, Configuration, execution, TestContext.Current.CancellationToken);
        using var document = JsonDocument.Parse(body);
        var value = document.RootElement;
        Assert.Equal(enabled, value.GetProperty("enabled").GetBoolean());
        using var expected = JsonDocument.Parse(Configuration);
        Assert.True(JsonElement.DeepEquals(expected.RootElement, value.GetProperty("configuration")));
        Assert.Equal(execution is not null, value.TryGetProperty("executionData", out var data));
        if (execution is not null)
        {
            using var expectedExecution = JsonDocument.Parse(execution);
            Assert.True(JsonElement.DeepEquals(expectedExecution.RootElement, data));
        }
        var request = JsonSerializer.Deserialize(body, CoreJsonContext.Default.ItemScheduleCreateRequest);
        Assert.Equal(body, JsonSerializer.Serialize(request, CoreJsonContext.Default.ItemScheduleCreateRequest));
    }

    [Theory]
    [InlineData("Cron", """{"interval":1}""")]
    [InlineData("Cron", """{"interval":5270400}""")]
    [InlineData("Daily", """{"times":["00:00","23:59"]}""")]
    [InlineData("Weekly", """{"times":["09:30"],"weekdays":["Monday","Tuesday"]}""")]
    [InlineData("Monthly", """{"times":["01:30"],"recurrence":12,"occurrence":{"occurrenceType":"DayOfMonth","dayOfMonth":31}}""")]
    [InlineData("Monthly", """{"times":["01:30"],"recurrence":1,"occurrence":{"occurrenceType":"OrdinalWeekday","weekIndex":"Fifth","weekday":"Monday"}}""")]
    [InlineData("Monthly", """{"times":["01:30"],"recurrence":1,"occurrence":{"occurrenceType":"FutureOccurrence","setting":[1,2]}}""")]
    [InlineData("Weekly", """{"times":["09:30"],"weekdays":["FutureDay"]}""")]
    [InlineData("Monthly", """{"times":["01:30"],"recurrence":1,"occurrence":{"occurrenceType":"OrdinalWeekday","weekIndex":"FutureWeek","weekday":"FutureDay"}}""")]
    [InlineData("FutureSchedule", """{"interval":"opaque","future":{"name":1,"Name":2}}""")]
    public static void Body_AcceptsDocumentedBoundsAndExtensibleSettingsWithoutInventingWorkloadRules(string type, string fields)
    {
        var config = JsonNode.Parse(Config(type, fields))!.AsObject();
        config["localTimeZoneId"] = "Future Windows time zone";
        config["extension"] = new JsonArray(1, null, "extra");
        var json = config.ToJsonString();
        using var result = JsonDocument.Parse(ItemScheduleInput.CreateBody(true, json, null, TestContext.Current.CancellationToken));
        using var expected = JsonDocument.Parse(json);
        Assert.True(JsonElement.DeepEquals(expected.RootElement, result.RootElement.GetProperty("configuration")));
    }

    [Theory]
    [InlineData("2024-04-28T00:00:00", "2024-04-28T00:00:01")]
    [InlineData("2024-04-28T00:00:00Z", "2024-04-28T00:00:01Z")]
    [InlineData("2024-04-28T00:00:00.1234567+00:00", "2024-04-28T00:00:00.1234568+00:00")]
    public static void Body_PreservesUtcTextAndAllowsExplicitPastStartWithoutTimeConversion(string start, string end)
    {
        var config = JsonNode.Parse(Configuration)!.AsObject();
        config["startDateTime"] = start;
        config["endDateTime"] = end;
        using var result = JsonDocument.Parse(ItemScheduleInput.CreateBody(true, config.ToJsonString(), null, TestContext.Current.CancellationToken));
        Assert.Equal(start, result.RootElement.GetProperty("configuration").GetProperty("startDateTime").GetString());
        Assert.Equal(end, result.RootElement.GetProperty("configuration").GetProperty("endDateTime").GetString());
    }

    [Theory]
    [InlineData("2024-04-28T00:00:00Z", "2024-04-28T00:00:00Z")]
    [InlineData("2024-04-29T00:00:00Z", "2024-04-28T00:00:00Z")]
    [InlineData("2024-04-28T00:00:00+02:00", "2024-04-29T00:00:00Z")]
    [InlineData("2024-04-28T00:00:00Z", "2024-04-29T00:00:00-03:00")]
    [InlineData("2024-02-31T00:00:00Z", "2024-04-29T00:00:00Z")]
    [InlineData("private-invalid", "2030-04-29T00:00:00Z")]
    public static void Body_RequiresExplicitOrderedUtcBounds(string start, string end)
    {
        var config = JsonNode.Parse(Configuration)!.AsObject();
        config["startDateTime"] = start;
        config["endDateTime"] = end;
        AssertInvalid(config.ToJsonString());
    }

    [Theory]
    [InlineData("type")]
    [InlineData("startDateTime")]
    [InlineData("endDateTime")]
    [InlineData("localTimeZoneId")]
    [InlineData("interval")]
    public static void Body_RejectsMissingNullOrWrongTypeConfigurationFields(string field)
    {
        foreach (var value in new[] { null, "null", "true", "\"\"", "\"private-invalid\"" })
        {
            var config = JsonNode.Parse(Configuration)!.AsObject();
            if (value is null)
            {
                config.Remove(field);
            }
            else
            {
                config[field] = JsonNode.Parse(value);
            }
            // Future type/timezone strings are intentionally open, not OS-local enums.
            if (value == "\"private-invalid\"" && field is "type" or "localTimeZoneId")
            {
                continue;
            }
            AssertInvalid(config.ToJsonString());
        }
    }

    [Theory]
    [InlineData("Cron", """{"interval":0}""")]
    [InlineData("Cron", """{"interval":5270401}""")]
    [InlineData("Cron", """{"interval":1.5}""")]
    [InlineData("Cron", """{"interval":"*/5 * * * *"}""")]
    [InlineData("Daily", """{"times":[]}""")]
    [InlineData("Daily", """{"times":["24:00"]}""")]
    [InlineData("Daily", """{"times":["24:01"]}""")]
    [InlineData("Daily", """{"times":["9:00"]}""")]
    [InlineData("Weekly", """{"times":["24:00"],"weekdays":["Monday"]}""")]
    [InlineData("Weekly", """{"times":["09:00"],"weekdays":[]}""")]
    [InlineData("Weekly", """{"times":["09:00"],"weekdays":[null]}""")]
    [InlineData("Monthly", """{"times":["09:00"],"recurrence":0,"occurrence":{"occurrenceType":"DayOfMonth","dayOfMonth":1}}""")]
    [InlineData("Monthly", """{"times":["24:00"],"recurrence":1,"occurrence":{"occurrenceType":"DayOfMonth","dayOfMonth":1}}""")]
    [InlineData("Monthly", """{"times":["09:00"],"recurrence":13,"occurrence":{"occurrenceType":"DayOfMonth","dayOfMonth":1}}""")]
    [InlineData("Monthly", """{"times":["09:00"],"recurrence":1,"occurrence":{"occurrenceType":"DayOfMonth","dayOfMonth":0}}""")]
    [InlineData("Monthly", """{"times":["09:00"],"recurrence":1,"occurrence":{"occurrenceType":"DayOfMonth","dayOfMonth":32}}""")]
    [InlineData("Monthly", """{"times":["09:00"],"recurrence":1,"occurrence":{"occurrenceType":"OrdinalWeekday","weekday":"Monday"}}""")]
    public static void Body_RejectsInvalidKnownConfigurationShapes(string type, string fields) => AssertInvalid(Config(type, fields));

    [Theory]
    [InlineData(100, 7, true)]
    [InlineData(101, 7, false)]
    [InlineData(100, 8, false)]
    public static void Body_EnforcesExactArrayBoundsWithoutDeduplication(int times, int days, bool valid)
    {
        var fields = new JsonObject
        {
            ["times"] = new JsonArray(Enumerable.Repeat("12:00", times).Select(value => (JsonNode?)JsonValue.Create(value)).ToArray()),
            ["weekdays"] = new JsonArray(Enumerable.Repeat("Monday", days).Select(value => (JsonNode?)JsonValue.Create(value)).ToArray())
        };
        var config = Config("Weekly", fields.ToJsonString());
        if (valid)
        {
            Assert.NotEmpty(ItemScheduleInput.CreateBody(false, config, null, TestContext.Current.CancellationToken));
        }
        else
        {
            AssertInvalid(config);
        }
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("null")]
    [InlineData("[]")]
    [InlineData("true")]
    [InlineData("{private")]
    [InlineData("{}")]
    [InlineData("""{"type":"Cron","type":"Daily"}""")]
    public static void Body_RejectsMissingOrMalformedConfiguration(string? configuration) => AssertInvalid(configuration!);

    [Theory]
    [InlineData("")]
    [InlineData("null")]
    [InlineData("[]")]
    [InlineData("true")]
    [InlineData("""{"x":1,"x":2}""")]
    [InlineData("""{"x":{"p":1,"p":2}}""")]
    [InlineData("""{"private":1,}""")]
    [InlineData("""{"private":/*comment*/1}""")]
    [InlineData("""{"value":"\uD800"}""")]
    [InlineData("""{"\uDC00":1}""")]
    public static void Body_RejectsInvalidOpaqueJsonWithoutEchoingIt(string execution) => AssertInvalid(Configuration, execution);

    [Fact]
    public static void Body_RejectsCaseAmbiguousMetadataAndInvalidUnicodeAnywhere()
    {
        AssertInvalid(Configuration.Replace("\"type\":", "\"Type\":\"Daily\",\"type\":", StringComparison.Ordinal));
        AssertInvalid(Configuration.Replace("\"interval\":10", "\"interval\":10,\"future\":{\"value\":\"\\uD800\"}", StringComparison.Ordinal));
        AssertInvalid(Configuration, "{\"value\":\"" + (char)0xd800 + "\"}");
    }

    [Theory]
    [InlineData(false, 0)]
    [InlineData(false, 1)]
    [InlineData(true, 0)]
    [InlineData(true, 1)]
    public static void Body_EnforcesExactInputByteBudget(bool execution, int excess)
    {
        var json = execution ? "{}" : Configuration;
        json = new string(' ', ItemScheduleInput.MaxRequestBytes - Encoding.UTF8.GetByteCount(json) + excess) + json;
        if (excess == 0)
        {
            Assert.NotEmpty(ItemScheduleInput.CreateBody(false, execution ? Configuration : json, execution ? json : null, TestContext.Current.CancellationToken));
        }
        else
        {
            AssertInvalid(execution ? Configuration : json, execution ? json : null);
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    public static void Body_EnforcesExactSerializedBudgetIncludingEnvelope(int excess)
    {
        var empty = ItemScheduleInput.CreateBody(false, Configuration, """{"pad":""}""", TestContext.Current.CancellationToken);
        var execution = $$"""{"pad":"{{new string('x', ItemScheduleInput.MaxRequestBytes - Encoding.UTF8.GetByteCount(empty) + excess)}}"}""";
        if (excess == 0)
        {
            var body = ItemScheduleInput.CreateBody(false, Configuration, execution, TestContext.Current.CancellationToken);
            Assert.Equal(ItemScheduleInput.MaxRequestBytes, Encoding.UTF8.GetByteCount(body));
        }
        else
        {
            AssertInvalid(Configuration, execution);
        }
    }

    [Theory]
    [InlineData(32, true, false)]
    [InlineData(32, true, true)]
    [InlineData(33, false, false)]
    [InlineData(33, false, true)]
    public static void Body_EnforcesExactDepthAndUtf8SerializationBudget(int depth, bool valid, bool useExecution)
    {
        var nested = new string('[', depth - 1) + "0" + new string(']', depth - 1);
        var configuration = useExecution ? Configuration : Configuration.Replace("\"interval\":10", "\"interval\":10,\"x\":" + nested, StringComparison.Ordinal);
        var execution = useExecution ? "{\"x\":" + nested + "}" : null;
        if (valid)
        {
            Assert.NotEmpty(ItemScheduleInput.CreateBody(false, configuration, execution, TestContext.Current.CancellationToken));
        }
        else
        {
            AssertInvalid(configuration, execution);
        }
        var unicode = $$"""{"pad":"{{new string('\u00e9', 200_000)}}"}""";
        Assert.True(Encoding.UTF8.GetByteCount(unicode) < ItemScheduleInput.MaxRequestBytes);
        AssertInvalid(Configuration, unicode);
    }

    [Fact]
    public static void Body_AppliesOneCombinedBudgetToConfigurationAndExecutionData()
    {
        var config = Configuration.Replace("\"interval\":10", "\"interval\":10,\"pad\":\"" + new string('x', 600_000) + "\"", StringComparison.Ordinal);
        var execution = $$"""{"pad":"{{new string('x', 600_000)}}"}""";
        Assert.True(Encoding.UTF8.GetByteCount(config) < ItemScheduleInput.MaxRequestBytes);
        Assert.True(Encoding.UTF8.GetByteCount(execution) < ItemScheduleInput.MaxRequestBytes);
        AssertInvalid(config, execution);
    }

    [Fact]
    public static void Body_CancelsBeforeParsing()
    {
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        Assert.Throws<OperationCanceledException>(() => ItemScheduleInput.CreateBody(false, Configuration, null, cancellation.Token));
    }

    private static void AssertInvalid(string configuration, string? execution = null)
    {
        var error = Assert.Throws<FabricItemInputException>(() =>
            ItemScheduleInput.CreateBody(false, configuration, execution, TestContext.Current.CancellationToken));
        Assert.DoesNotContain("private", error.Message);
        Assert.DoesNotContain("LineNumber", error.Message);
    }
}
