// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tests.Client.Attributes;

/// <summary>
/// Attribute to customize the test-proxy matcher for a specific test method.
/// Apply this to individual test methods to override default matching behavior for that test only.
/// 
/// Tests other than what this is applied to will use the default matcher behavior as defined in default test configuration.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class CustomMatcherAttribute : Attribute
{
    /// <summary>
    /// When true, the request/response body will be compared during playback matching. Will not be otherwise.
    /// </summary>
    public bool CompareBodies { get; set; }

    /// <summary>
    /// When true, query parameter ordering will be ignored during playback matching.
    /// </summary>
    public bool IgnoreQueryOrdering { get; set; }


    public CustomMatcherAttribute(
        bool compareBody = false,
        bool ignoreQueryordering = false)
    {
        CompareBodies = compareBody;
        IgnoreQueryOrdering = ignoreQueryordering;
    }
}
