// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using System.Text.Json.Nodes;
using Fabric.Mcp.Tools.Core.Models;
using Fabric.Mcp.Tools.Core.Services;
using static Fabric.Mcp.Tools.Core.Tests.TestSupport.ItemJobInstanceTestData;

namespace Fabric.Mcp.Tools.Core.Tests.TestSupport;

internal static class ItemScheduleListTestData
{
    internal const string JobType = "DefaultJob";
    internal const string ScheduleId = "eeeeeeee-eeee-4eee-8eee-eeeeeeeeeeee";
    // cspell:disable-next-line
    internal const string DocumentedToken = "ABCsMTAwMDAwLDA%3D";
    internal const string ScheduleJson = """
        {
          "id": "eeeeeeee-eeee-4eee-8eee-eeeeeeeeeeee",
          "enabled": true,
          "createdDateTime": "2024-04-28T05:35:20.5366667",
          "configuration": {
            "type": "Cron",
            "startDateTime": "2024-04-28T00:00:00",
            "endDateTime": "2024-04-30T23:59:00Z",
            "localTimeZoneId": "Central Standard Time",
            "interval": 10
          },
          "owner": {
            "id": "dddddddd-dddd-4ddd-8ddd-dddddddddddd",
            "type": "User",
            "displayName": "Test owner",
            "userDetails": {"userPrincipalName": "owner@example.invalid"}
          },
          "executionData": {"parameters": {"name": "first", "Name": "second"}, "future": [null, true, 1]}
        }
        """;

    internal static string CollectionUrl(string jobType = JobType) =>
        $"{FabricEndpoints.GetFabricApiBaseUrl()}/workspaces/{WorkspaceId}/items/{ItemId}/jobs/{Uri.EscapeDataString(jobType)}/schedules";

    internal static FabricItemSchedule Schedule() =>
        JsonSerializer.Deserialize(ScheduleJson, CoreJsonContext.Default.FabricItemSchedule)!;

    internal static string Page(string? token = null, bool includeUri = false, string jobType = JobType)
    {
        var page = new JsonObject { ["value"] = new JsonArray(JsonNode.Parse(ScheduleJson)) };
        if (token is not null)
        {
            page["continuationToken"] = token;
        }
        if (includeUri)
        {
            page["continuationUri"] = $"{CollectionUrl(jobType)}?continuationToken={FabricCoreHttpHelpers.EncodeContinuationToken(token!)}";
        }
        return page.ToJsonString();
    }
}
