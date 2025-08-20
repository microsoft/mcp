// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;

namespace Azure.Mcp.Tools.LoadTesting.Options;
public class BaseLoadTestingOptions : SubscriptionOptions
{
    /// <summary>
    /// The name of the test resource.
    /// </summary>
    public string? TestResourceName { get; set; }
}
