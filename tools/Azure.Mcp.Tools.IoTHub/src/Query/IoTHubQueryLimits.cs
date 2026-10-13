// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.IoTHub.Query;

/// <summary>
/// Shared limits for IoT Hub query paging. Centralizing these values keeps the page-size hint
/// returned by <c>iothub query compile</c> aligned with the page size enforced by <c>iothub query run</c>.
/// </summary>
public static class IoTHubQueryLimits
{
    /// <summary>
    /// The maximum number of items returned in a single query page. Requests for a larger page size
    /// are capped to this value, so a single page is always at most this many items.
    /// </summary>
    public const int MaxPageSize = 100;
}
