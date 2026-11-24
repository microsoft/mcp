// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Nodes;
using Fabric.Mcp.Tools.Core.Models;
using Fabric.Mcp.Tools.Core.Services;
using static Fabric.Mcp.Tools.Core.Tests.TestSupport.ItemJobInstanceTestData;

namespace Fabric.Mcp.Tools.Core.Tests.TestSupport;

internal static class ItemJobInstanceListTestData
{
    // cspell:disable-next-line
    internal const string DocumentedToken = "LDEsMTAwMDAwLDA%3D";
    internal static string CollectionUrl => $"{FabricEndpoints.GetFabricApiBaseUrl()}/workspaces/{WorkspaceId}/items/{ItemId}/jobs/instances";

    internal static string Page(string? token = null, bool includeUri = false)
    {
        var page = new JsonObject { ["value"] = new JsonArray(JsonNode.Parse(Json)) };
        if (token is not null)
        {
            page["continuationToken"] = token;
        }
        if (includeUri)
        {
            page["continuationUri"] = $"{CollectionUrl}?continuationToken={FabricCoreHttpHelpers.EncodeContinuationToken(token!)}";
        }
        return page.ToJsonString();
    }
}
