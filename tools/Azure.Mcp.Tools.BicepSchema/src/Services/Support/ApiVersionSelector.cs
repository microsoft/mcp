// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.RegularExpressions;
using Azure.Mcp.Tools.BicepSchema.Services.ResourceProperties.Helpers;

namespace Azure.Mcp.Tools.BicepSchema.Services.Support;

public partial class ApiVersionSelector
{
    [GeneratedRegex(@"^\d{4}-\d{2}-\d{2}$")]
    private static partial Regex StableVersionRegex();


    public string ApiVersion { get; set; } = string.Empty;

    public static string SelectLatestStable(IEnumerable<string> apiVersions)
    {
        string[] sortedApiVersions = [.. apiVersions.Order(ApiVersionComparer.Instance)];
        return sortedApiVersions.LastOrDefault(IsStableRelease) // prefer last stable release
            ?? sortedApiVersions.LastOrDefault() // if none, use latest pre-release
            ?? throw new Exception("List of apiVersions should not be empty");
    }

    public static bool IsStableRelease(string apiVersion)
    {
        return StableVersionRegex().IsMatch(apiVersion);
    }

    public static bool IsPrerelease(string apiVersion)
    {
        return !IsStableRelease(apiVersion);
    }
}
