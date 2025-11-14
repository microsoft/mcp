// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Reflection;
using System.Threading;
using Xunit.v3;

namespace Azure.Mcp.Tests.Client.Attributes;

/// <summary>
/// Attribute to customize the test-proxy matcher for a specific test method.
/// Apply this to individual test methods to override default matching behavior for that test only.
///
/// Tests other than what this is applied to will use the default matcher behavior as defined in default test configuration.
/// </summary>
public sealed class CustomMatcherAttribute : BeforeAfterTestAttribute
{
    private static readonly AsyncLocal<CustomMatcherAttribute?> Current = new();

    /// <summary>
    /// When true, the request/response body will be compared during playback matching. Otherwise, body comparison is skipped. Defaults to true.
    /// </summary>
    public bool CompareBodies { get; set; }

    /// <summary>
    /// When true, query parameter ordering will be ignored during playback matching. Defaults to false.
    /// </summary>
    public bool IgnoreQueryOrdering { get; set; }

    public CustomMatcherAttribute(
        bool compareBody = false,
        bool ignoreQueryordering = false)
    {
        CompareBodies = compareBody;
        IgnoreQueryOrdering = ignoreQueryordering;
    }

    public override void Before(MethodInfo methodUnderTest, IXunitTest xunitTest)
    {
        base.Before(methodUnderTest, xunitTest);
        Current.Value = this;
    }

    public override void After(MethodInfo methodUnderTest, IXunitTest xunitTest)
    {
        base.After(methodUnderTest, xunitTest);
        Current.Value = null;
    }

    internal static CustomMatcherAttribute? GetActive() => Current.Value;
}
