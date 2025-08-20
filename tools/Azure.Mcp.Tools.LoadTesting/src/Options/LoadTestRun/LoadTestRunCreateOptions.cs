// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.LoadTesting.Options.LoadTestRun;

public class TestRunCreateOptions : BaseLoadTestingOptions
{
    /// <summary>
    /// The ID of the load test run resource.
    /// </summary>
    public string? TestRunId { get; set; }

    /// <summary>
    /// The ID of the load test resource.
    /// </summary>
    public string? TestId { get; set; }

    /// <summary>
    /// The display name for the load test run.
    /// </summary>
    public string? DisplayName { get; set; }

    /// <summary>
    /// The description for the load test run.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// The ID of an existing test run to update. If provided, the command will trigger a rerun of the given test run id.
    /// </summary>
    public string? OldTestRunId { get; set; }
}
