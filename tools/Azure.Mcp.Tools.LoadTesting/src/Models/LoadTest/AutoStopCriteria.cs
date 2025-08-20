// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.LoadTesting.Models.LoadTest;
public class AutoStopCriteria
{
    /// <summary>
    /// Gets or sets whether auto-stop functionality is disabled. Default is false (enabled).
    /// </summary>
    [JsonPropertyName("autoStopDisabled")]
    public bool AutoStopDisabled { get; set; } = false;

    /// <summary>
    /// Gets or sets the error rate percentage threshold (0-100) that triggers auto-stop. Default is 90%.
    /// </summary>
    [JsonPropertyName("errorRate")]
    public int ErrorRate { get; set; } = 90;

    /// <summary>
    /// Gets or sets the time window in seconds for calculating error rate. Default is 60 seconds.
    /// </summary>
    [JsonPropertyName("errorRateTimeWindowInSeconds")]
    public int ErrorRateTimeWindowInSeconds { get; set; } = 60;

    /// <summary>
    /// Gets or sets the maximum virtual users per test engine. Default is 5000.
    /// </summary>
    [JsonPropertyName("maximumVirtualUsersPerEngine")]
    public int MaximumVirtualUsersPerEngine { get; set; } = 5000;
}
