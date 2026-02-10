// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Reflection;
using Xunit;
using Xunit.v3;

namespace Azure.Mcp.Tests.Helpers;

/// <summary>
/// Resolves the current test method's <see cref="MethodInfo"/> via reflection on xUnit internals.
/// Each call performs the necessary reflection; callers that need the value multiple times within a
/// single test should cache the returned <see cref="MethodInfo"/> themselves.
/// </summary>
internal static class TestMethodResolver
{
    public static MethodInfo? TryResolveCurrentMethodInfo()
    {
        var method = TestContext.Current.Test?.TestCase.TestMethod is IXunitTestMethod testMethod
            ? testMethod.Method
            : null;

        return method;
    }
}
