// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.LoadTesting.Models.LoadTest;

public class InputArtifacts
{
    /// <summary>
    /// Gets or sets the main test script file information for the load test.
    /// </summary>
    public TestScriptFileInfo? TestScriptFileInfo { get; set; }

    /// <summary>
    /// Gets or sets additional files required for test execution (config files, test data, etc.).
    /// </summary>
    public List<AdditionalFileInfo>? AdditionalFileInfo { get; set; } = [];
}
