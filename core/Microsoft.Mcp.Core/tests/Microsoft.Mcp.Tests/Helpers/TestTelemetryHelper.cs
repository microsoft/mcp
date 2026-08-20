// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;
using Xunit;

namespace Microsoft.Mcp.Tests.Helpers;

/// <summary>
/// Test helper class for validating telemetry.
/// </summary>
public static class TestTelemetryHelpers
{
    public static object GetAndAssertTagKeyValue(Activity activity, string tagName)
    {
        var matching = activity.TagObjects.Where(x => string.Equals(x.Key, tagName)).ToList();

        Assert.Single(matching);

        var firstValue = matching[0].Value;
        Assert.NotNull(firstValue);
        return firstValue;
    }

    public static void AssertTagDoesNotExist(Activity activity, string tagName)
    {
        var matching = activity.TagObjects.Any(x => string.Equals(x.Key, tagName));
        Assert.False(matching, $"Tag '{tagName}' was found in activity tags but should not exist.");
    }
}
