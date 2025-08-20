// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.LoadTesting.Models.LoadTest;
public class LoadTestConfiguration
{
    /// <summary>
    /// Gets or sets the number of test engine instances to use. Default is 1.
    /// </summary>
    [JsonPropertyName("engineInstances")]
    public int EngineInstances { get; set; } = 1;

    /// <summary>
    /// Gets or sets whether to split all CSV files across engine instances. Default is false.
    /// </summary>
    [JsonPropertyName("splitAllCSVs")]
    public bool SplitAllCSVs { get; set; } = false;

    /// <summary>
    /// Gets or sets whether this is a quick start test (URL-based test). Default is false.
    /// </summary>
    [JsonPropertyName("quickStartTest")]
    public bool QuickStartTest { get; set; } = false;

    /// <summary>
    /// Gets or sets optional load test configuration parameters (virtual users, duration, etc.).
    /// </summary>
    [JsonPropertyName("optionalLoadTestConfig")]
    public OptionalLoadTestConfig? OptionalLoadTestConfig { get; set; } = null;
}
