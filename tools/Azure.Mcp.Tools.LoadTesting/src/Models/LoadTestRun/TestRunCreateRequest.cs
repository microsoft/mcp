// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.LoadTesting.Models.LoadTestRun;

public class TestRunCreateRequest
{
    /// <summary>
    /// Gets or sets the test run request details.
    /// </summary>
    public TestRunRequest TestRunRequest { get; set; } = new();
}
