// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.LoadTesting.Options.LoadTest;

public class TestGetOptions : BaseLoadTestingOptions
{
    /// <summary>
    /// The ID of the load test.
    /// </summary>
    public string? TestId { get; set; }
}
