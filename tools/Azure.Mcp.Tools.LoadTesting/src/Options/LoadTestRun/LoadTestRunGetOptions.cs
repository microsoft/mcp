// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.LoadTesting.Options.LoadTestRun;

public class TestRunGetOptions : BaseLoadTestingOptions
{
    /// <summary>
    /// The ID of the load test run resource. If provided, returns a single test run.
    /// </summary>
    public string? TestRunId { get; set; }

    /// <summary>
    /// The ID of the load test resource. If provided (and TestRunId is not), returns all test runs for this test.
    /// </summary>
    public string? TestId { get; set; }
}
