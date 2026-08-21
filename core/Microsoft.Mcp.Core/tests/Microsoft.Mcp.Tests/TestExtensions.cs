// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;
using System.Text.Json;
using Microsoft.Mcp.Tests.Client.Helpers;
using Microsoft.Mcp.Tests.Helpers;
using Xunit;

namespace Microsoft.Mcp.Tests;

public static class TestExtensions
{
    public const string RunningInNonInteractiveEnvironment =
        "Test skipped when running in a non-interactive environment (dotnet test or DevOps). This test requires interactive environment.";

    private static readonly Lazy<TestMode> s_testMode = new(() =>
    {
        return LiveTestSettings.TryLoadTestSettings(out var settings) ?
            settings.TestMode :
            TestMode.Playback; // Default to Playback if settings file is not found
    });

    public static bool IsRunningInNonInteractiveEnvironment()
    {
        bool isVsCode = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("VSCODE_CLI")) ||
                       !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("VSCODE_PID")) ||
                       !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("VSCODE_CWD"));

        if (isVsCode)
        {
            return false;
        }

        // Check for environment variables that indicate we're running from dotnet test
        bool isDotnetTest = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("VSTEST_HOST_DEBUG")) ||
               !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DOTNET_HOST_PATH")) ||
               !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("TESTINGPLATFORM_TELEMETRY_OPTOUT"));

        if (isDotnetTest)
        {
            return true;
        }

        // Check for environment variables that indicate we're running in a CI environment, which is also non-interactive
        return !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("CI")) ||
            !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("TF_BUILD"));
    }

    public static JsonElement AssertProperty(this JsonElement? element, string propertyName)
    {
        Assert.NotNull(element);
        return element.Value.AssertProperty(propertyName);
    }

    public static JsonElement AssertProperty(this JsonElement element, string propertyName)
    {
        Assert.True(element.TryGetProperty(propertyName, out var property), $"Property '{propertyName}' not found. Full element: '{JsonSerializer.Serialize(element)}'");
        return property;
    }

    public static void AssertPropertyDoesNotExist(this JsonElement element, string propertyName)
    {
        Assert.False(element.TryGetProperty(propertyName, out _), $"Property '{propertyName}' should not exist. Full element: '{JsonSerializer.Serialize(element)}'");
    }

    public static bool IsLiveTestMode() => s_testMode.Value == TestMode.Live;

    /// <summary>
    /// Asserts that a tag with the specified name exists in the activity and has a single match, and that the value of the tag matches the expected value.
    /// </summary>
    /// <param name="activity">The activity to assert on.</param>
    /// <param name="tagName">The tag name to validate existence.</param>
    /// <param name="expectedValue">The expected value of the tag.</param>
    public static void AssertTagEquals(this Activity activity, string tagName, object expectedValue)
    {
        var matching = activity.TagObjects.Where(x => string.Equals(x.Key, tagName)).ToList();
        Assert.Single(matching);
        var firstValue = matching[0].Value;
        Assert.NotNull(firstValue);
        Assert.Equal(expectedValue, firstValue);
    }

    /// <summary>
    /// Asserts that a tag with the specified name exists in the activity and has a single match, and allows for additional assertions on the value of the tag.
    /// </summary>
    /// <param name="activity">The activity to assert on.</param>
    /// <param name="tagName">The tag name to validate existence.</param>
    /// <param name="valueAssertion">An action that performs additional assertions on the value of the tag.</param>
    public static void AssertTagEquals(this Activity activity, string tagName, Action<object> valueAssertion)
    {
        var matching = activity.TagObjects.Where(x => string.Equals(x.Key, tagName)).ToList();
        Assert.Single(matching);
        var firstValue = matching[0].Value;
        Assert.NotNull(firstValue);
        valueAssertion(firstValue);
    }

    /// <summary>
    /// Asserts that a tag with the specified name does not exist in the activity.
    /// </summary>
    /// <param name="activity">The activity to assert on.</param>
    /// <param name="tagName">The tag name to validate it doesn't exist.</param>
    public static void AssertTagDoesNotExist(this Activity activity, string tagName)
    {
        var matching = activity.TagObjects.Any(x => string.Equals(x.Key, tagName));
        Assert.False(matching, $"Tag '{tagName}' was found in activity tags but should not exist.");
    }
}
