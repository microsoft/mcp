// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Functions.Services;

/// <summary>
/// Cache duration constants for Azure Functions template services.
/// </summary>
internal static class FunctionsCacheDurations
{
    /// <summary>
    /// 12-hour cache for manifest, tree, and template files.
    /// </summary>
    public static readonly TimeSpan TemplateCacheDuration = TimeSpan.FromHours(12);
}
