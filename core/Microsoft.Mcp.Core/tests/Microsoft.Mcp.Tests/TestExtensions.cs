// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Xunit;

namespace Microsoft.Mcp.Tests;

public static class TestExtensions
{
    public const string RunningInNonInteractiveEnvironment =
        "Test skipped when running in a non-interactive environment (dotnet test or DevOps). This test requires interactive environment.";

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
}
