// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;
using Microsoft.Mcp.Core.Areas.Server.Commands.Runtime;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;

namespace Microsoft.Mcp.Core.Areas.Server.Commands.ToolLoading;

internal static class ToolListCache
{
    internal static bool IsFresh(long cachedAt, TimeSpan? timeToLive) =>
        Stopwatch.GetElapsedTime(cachedAt) < (timeToLive ?? McpRuntime.s_defaultToolListTimeToLive);

    internal static async Task<ListToolsResult> ListRemoteToolsAsync(McpClient client, CancellationToken cancellationToken)
    {
        var result = new ListToolsResult { Tools = [] };
        string? cursor = null;

        do
        {
            var page = await client.ListToolsAsync(new ListToolsRequestParams { Cursor = cursor }, cancellationToken);
            foreach (var tool in page.Tools)
            {
                result.Tools.Add(tool);
            }
            if (page.TimeToLive.HasValue && (result.TimeToLive is null || page.TimeToLive.Value < result.TimeToLive))
            {
                result.TimeToLive = page.TimeToLive.Value;
            }

            cursor = page.NextCursor;
        } while (!string.IsNullOrEmpty(cursor));

        return result;
    }
}
