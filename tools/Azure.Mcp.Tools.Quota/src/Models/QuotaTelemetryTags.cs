// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Quota.Models;

public static class QuotaTelemetryTags
{
    private static string AddPrefix(string tagName) => $"quota/{tagName}";

    public static readonly string ResourceTypes = AddPrefix("ResourceTypes");
    public static readonly string Region = AddPrefix("Region");
}
