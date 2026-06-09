// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.LoadTesting.Models.LoadTest;

namespace Azure.Mcp.Tools.LoadTesting.Models.LoadTestRun;

public class TestRunRequest
{
    /// <summary>
    /// Gets or sets the ID of the test configuration to execute.
    /// </summary>
    public string TestId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the display name for this test run execution.
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets secrets used during test execution (passwords, API keys, etc.).
    /// </summary>
    public IDictionary<string, string> Secrets { get; set; } = new Dictionary<string, string>();

    /// <summary>
    /// Gets or sets the client certificate for authentication.
    /// </summary>
    public string? Certificate { get; set; } = null;

    /// <summary>
    /// Gets or sets environment variables available during test execution.
    /// </summary>
    public IDictionary<string, string> EnvironmentVariables { get; set; } = new Dictionary<string, string>();

    /// <summary>
    /// Gets or sets the description of this test run.
    /// </summary>
    public string? Description { get; set; } = null;

    /// <summary>
    /// Gets or sets the load test execution configuration.
    /// </summary>
    public LoadTestConfiguration LoadTestConfiguration { get; set; } = new();

    /// <summary>
    /// Gets or sets whether debug logging is enabled. Default is false.
    /// </summary>
    public bool? DebugLogsEnabled { get; set; } = false;

    /// <summary>
    /// Gets or sets the level of request data to capture during execution.
    /// </summary>
    public RequestDataLevel? RequestDataLevel { get; set; }
}
