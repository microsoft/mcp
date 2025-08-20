// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.LoadTesting.Models.LoadTest;
public class InputArtifacts
{
    /// <summary>
    /// Gets or sets the main test script file information for the load test.
    /// </summary>
    [JsonPropertyName("testScriptFileInfo")]
    public TestScriptFileInfo? TestScriptFileInfo { get; set; }

    /// <summary>
    /// Gets or sets additional files required for test execution (config files, test data, etc.).
    /// </summary>
    [JsonPropertyName("additionalFileInfo")]
    public List<AdditionalFileInfo>? AdditionalFileInfo { get; set; } = new();
}
