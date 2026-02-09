// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Reflection;
using Azure.Mcp.Tests.Client;
using Azure.Mcp.Tests.Client.Helpers;
using Azure.Mcp.Tests.Helpers;
using Xunit;
using Xunit.v3;

namespace Azure.Mcp.Tests.Client.Attributes;

/// <summary>
/// Attribute to mark tests that can only run in Live or Record mode and should be skipped in Playback mode.
/// This is used for tests that cannot be recorded due to technical limitations (e.g., WebSocket connections, 
/// direct TCP connections, or client inheritance bugs).
/// </summary>
/// <remarks>
/// <para>
/// Use this attribute on test methods that inherit from <see cref="RecordedCommandTestsBase"/> when those
/// tests cannot be recorded due to technical limitations such as:
/// </para>
/// <list type="bullet">
/// <item>Tests using WebSocket connections (not supported by the test proxy)</item>
/// <item>Tests using direct TCP connections (e.g., PostgreSQL, MySQL)</item>
/// <item>Tests with client inheritance bugs that prevent proper recording</item>
/// </list>
/// <para>
/// When the test mode is Playback, tests marked with this attribute will be skipped automatically.
/// Tests will still run in Live and Record modes.
/// </para>
/// <example>
/// <code>
/// public class MyCommandTests(ITestOutputHelper output, TestProxyFixture fixture) : RecordedCommandTestsBase(output, fixture)
/// {
///     [LiveTestOnly]
///     [Fact]
///     public async Task TestWebSocketConnection()
///     {
///         // This test uses WebSocket which cannot be recorded
///         // It will be skipped in Playback mode
///     }
/// }
/// </code>
/// </example>
/// </remarks>
public sealed class LiveTestOnlyAttribute : BeforeAfterTestAttribute
{
    public override void Before(MethodInfo methodUnderTest, IXunitTest xunitTest)
    {
        base.Before(methodUnderTest, xunitTest);

        // Get the test mode from the test instance
        var testMode = GetTestMode();

        // Skip the test if we're in Playback mode
        if (testMode == TestMode.Playback)
        {
            Assert.Skip("This test can only run in Live or Record mode due to technical limitations that prevent recording.");
        }
    }

    private static TestMode GetTestMode()
    {
        // Try to get the test mode from the current test context
        var test = TestContext.Current?.Test;
        if (test == null)
        {
            // Default to Live if we can't determine the mode
            return TestMode.Live;
        }

        // Try to get the test class instance to access TestMode property
        var testCase = GetPropertyValue(test, "TestCase");
        if (testCase == null)
        {
            return TestMode.Live;
        }

        var testMethod = GetPropertyValue(testCase, "TestMethod");
        if (testMethod == null)
        {
            return TestMode.Live;
        }

        var testClass = GetPropertyValue(testMethod, "TestClass");
        if (testClass == null)
        {
            return TestMode.Live;
        }

        var type = GetPropertyValue(testClass, "Class") as Type;
        if (type == null)
        {
            return TestMode.Live;
        }

        // Check if the test class inherits from RecordedCommandTestsBase
        if (!typeof(RecordedCommandTestsBase).IsAssignableFrom(type))
        {
            // Not a recorded test, so it's Live
            return TestMode.Live;
        }

        // Try to get the TestMode from LiveTestSettings
        if (LiveTestSettings.TryLoadTestSettings(out var settings))
        {
            return settings.TestMode;
        }

        return TestMode.Live;
    }

    private static object? GetPropertyValue(object instance, string propertyName)
    {
        return instance
            .GetType()
            .GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?
            .GetValue(instance);
    }
}
