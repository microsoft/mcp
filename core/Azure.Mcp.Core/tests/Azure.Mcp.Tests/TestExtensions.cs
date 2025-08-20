// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Xunit;

namespace Azure.Mcp.Tests;

public static class TestExtensions
{
    public const string RunningFromDotnetTestReason =
        "Test skipped when running from dotnet test. This test requires interactive environment.";

    public static bool IsRunningFromDotnetTest()
    {
        bool isVsCode = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("VSCODE_CLI")) ||
                       !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("VSCODE_PID")) ||
                       !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("VSCODE_CWD"));

        if (isVsCode)
        {
            return false;
        }

        // Check for environment variables that indicate we're running from dotnet test
        return !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("VSTEST_HOST_DEBUG")) ||
               !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DOTNET_HOST_PATH"));
    }

    public static JsonElement AssertProperty(this JsonElement? element, string propertyName)
    {
        Assert.NotNull(element);
        return element.Value.AssertProperty(propertyName);
    }
    public static JsonElement AssertProperty(this JsonElement element, string propertyName)
    {
        Assert.True(element.TryGetProperty(propertyName, out var property), $"Property '{propertyName}' not found.");
        return property;
    }
}
