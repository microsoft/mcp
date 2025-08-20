// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.LoadTesting.Options.LoadTestRun;

public class TestRunGetOptions : BaseLoadTestingOptions
{
    /// <summary>
    /// The ID of the load test run resource.
    /// </summary>
    public string? TestRunId { get; set; }
}
