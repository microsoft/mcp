// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.LoadTesting.Models.LoadTestRun;
public class TestRun
{
    /// <summary>
    /// Gets or sets the ID of the test configuration being executed.
    /// </summary>
    [JsonPropertyName("testId")]
    public string TestId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the unique identifier for this test run execution.
    /// </summary>
    [JsonPropertyName("testRunId")]
    public string? TestRunId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the display name for this test run.
    /// </summary>
    [JsonPropertyName("displayName")]
    public string? DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the number of virtual users simulated during the test.
    /// </summary>
    [JsonPropertyName("virtualUsers")]
    public int? VirtualUsers { get; set; } = 0;

    /// <summary>
    /// Gets or sets the current execution status of the test run.
    /// </summary>
    [JsonPropertyName("status")]
    public string? Status { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets when the test run started execution.
    /// </summary>
    [JsonPropertyName("startDateTime")]
    public DateTimeOffset? StartDateTime { get; set; } = null;

    /// <summary>
    /// Gets or sets when the test run completed execution.
    /// </summary>
    [JsonPropertyName("endDateTime")]
    public DateTimeOffset? EndDateTime { get; set; } = null;

    /// <summary>
    /// Gets or sets when the test run was initiated.
    /// </summary>
    [JsonPropertyName("executedDateTime")]
    public DateTimeOffset? ExecutedDateTime { get; set; } = null;

    /// <summary>
    /// Gets or sets the Azure portal URL for viewing test results.
    /// </summary>
    [JsonPropertyName("portalUrl")]
    public string? PortalUrl { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the test execution duration in milliseconds.
    /// </summary>
    [JsonPropertyName("duration")]
    public int? Duration { get; set; } = 0;

    /// <summary>
    /// Gets or sets when the test run was created.
    /// </summary>
    [JsonPropertyName("createdDateTime")]
    public DateTimeOffset? CreatedDateTime { get; set; } = null;

    /// <summary>
    /// Gets or sets who created the test run.
    /// </summary>
    [JsonPropertyName("createdBy")]
    public string? CreatedBy { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets when the test run was last modified.
    /// </summary>
    [JsonPropertyName("lastModifiedDateTime")]
    public DateTimeOffset? LastModifiedDateTime { get; set; } = null;

    /// <summary>
    /// Gets or sets who last modified the test run.
    /// </summary>
    [JsonPropertyName("lastModifiedBy")]
    public string? LastModifiedBy { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the description of the test run.
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the overall test result (PASSED, FAILED, NOT_APPLICABLE).
    /// </summary>
    [JsonPropertyName("testResult")]
    public string? TestResult { get; set; } = string.Empty;
}
