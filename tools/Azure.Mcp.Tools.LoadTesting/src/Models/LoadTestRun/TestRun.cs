// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.LoadTesting.Models.LoadTestRun;

public class TestRun
{
    /// <summary>
    /// Gets or sets the ID of the test configuration being executed.
    /// </summary>
    public string TestId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the unique identifier for this test run execution.
    /// </summary>
    public string? TestRunId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the display name for this test run.
    /// </summary>
    public string? DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the number of virtual users simulated during the test.
    /// </summary>
    public int? VirtualUsers { get; set; } = 0;

    /// <summary>
    /// Gets or sets the current execution status of the test run.
    /// </summary>
    public string? Status { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets when the test run started execution.
    /// </summary>
    public DateTimeOffset? StartDateTime { get; set; } = null;

    /// <summary>
    /// Gets or sets when the test run completed execution.
    /// </summary>
    public DateTimeOffset? EndDateTime { get; set; } = null;

    /// <summary>
    /// Gets or sets when the test run was initiated.
    /// </summary>
    public DateTimeOffset? ExecutedDateTime { get; set; } = null;

    /// <summary>
    /// Gets or sets the Azure portal URL for viewing test results.
    /// </summary>
    public string? PortalUrl { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the test execution duration in milliseconds.
    /// </summary>
    public int? Duration { get; set; } = 0;

    /// <summary>
    /// Gets or sets when the test run was created.
    /// </summary>
    public DateTimeOffset? CreatedDateTime { get; set; } = null;

    /// <summary>
    /// Gets or sets who created the test run.
    /// </summary>
    public string? CreatedBy { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets when the test run was last modified.
    /// </summary>
    public DateTimeOffset? LastModifiedDateTime { get; set; } = null;

    /// <summary>
    /// Gets or sets who last modified the test run.
    /// </summary>
    public string? LastModifiedBy { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the description of the test run.
    /// </summary>
    public string? Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the overall test result (PASSED, FAILED, NOT_APPLICABLE).
    /// </summary>
    public string? TestResult { get; set; } = string.Empty;
}
