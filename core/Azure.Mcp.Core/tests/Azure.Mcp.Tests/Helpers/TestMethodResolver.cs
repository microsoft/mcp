// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Reflection;
using Xunit;

namespace Azure.Mcp.Tests.Helpers;

/// <summary>
/// Resolves the current test method's <see cref="MethodInfo"/> via reflection on xUnit internals.
/// This helper exists so that the reflection walk happens at most once per test, regardless of
/// how many attribute checks need the result.
/// </summary>
internal static class TestMethodResolver
{
    public static MethodInfo? TryResolveCurrentMethodInfo()
    {
        var test = TestContext.Current?.Test;
        if (test == null)
        {
            return null;
        }

        var testCase = GetPropertyValue(test, "TestCase");
        if (testCase == null)
        {
            return null;
        }

        var testMethod = GetPropertyValue(testCase, "TestMethod");
        if (testMethod == null)
        {
            return null;
        }

        var method = GetPropertyValue(testMethod, "Method") as MethodInfo
                     ?? GetPropertyValue(testMethod, "MethodInfo") as MethodInfo;

        return method;
    }

    private static object? GetPropertyValue(object instance, string propertyName)
    {
        return instance
            .GetType()
            .GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?
            .GetValue(instance);
    }
}
