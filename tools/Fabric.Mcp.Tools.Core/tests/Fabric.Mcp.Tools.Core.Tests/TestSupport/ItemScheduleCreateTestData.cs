// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Nodes;
using Fabric.Mcp.Tools.Core.Models;
using static Fabric.Mcp.Tools.Core.Tests.TestSupport.ItemJobInstanceTestData;
using static Fabric.Mcp.Tools.Core.Tests.TestSupport.ItemScheduleListTestData;

namespace Fabric.Mcp.Tools.Core.Tests.TestSupport;

internal static class ItemScheduleCreateTestData
{
    internal const string ToolName = "jobs_create-item-schedule";
    internal const string Configuration = """
        {"type":"Cron","startDateTime":"2024-04-28T00:00:00","endDateTime":"2030-04-30T23:59:00Z","localTimeZoneId":"Central Standard Time","interval":10}
        """;
    internal const string ExecutionData = """{"parameters":{"name":"first","Name":"second"},"future":[null,true,1e30]}""";

    internal static string Config(string type, string fields)
    {
        var config = JsonNode.Parse(Configuration)!.AsObject();
        config["type"] = type;
        config.Remove("interval");
        foreach (var property in JsonNode.Parse(fields)!.AsObject())
        {
            config[property.Key] = property.Value?.DeepClone();
        }
        return config.ToJsonString();
    }

    internal static ItemScheduleCreateCommandResult Created() =>
        new(Guid.Parse(WorkspaceId), Guid.Parse(ItemId), JobType, Schedule());

    internal static string DeepSchedule(int depth, string field)
    {
        var nested = new string('[', depth - 2) + "0" + new string(']', depth - 2);
        var value = field switch
        {
            "configuration" => Configuration[..^1] + ",\"deep\":" + nested + "}",
            "owner" => $$"""{"id":"dddddddd-dddd-4ddd-8ddd-dddddddddddd","type":"FuturePrincipal","deep":{{nested}}}""",
            "executionData" => $$"""{"deep":{{nested}}}""",
            _ => throw new ArgumentOutOfRangeException(nameof(field))
        };
        return $$"""{"id":"{{ScheduleId}}","enabled":false,"{{field}}":{{value}}}""";
    }
}
