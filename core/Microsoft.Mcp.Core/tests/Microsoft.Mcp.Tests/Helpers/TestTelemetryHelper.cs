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
        var matching = activity.TagObjects.SingleOrDefault(x => string.Equals(x.Key, tagName, StringComparison.OrdinalIgnoreCase));

        Assert.False(matching.Equals(default(KeyValuePair<string, object?>)), $"Tag '{tagName}' was not found in activity tags.");
        Assert.NotNull(matching.Value);

        return matching.Value;
    }

    public static void AssertTagDoesNotExist(Activity activity, string tagName)
    {
        var matching = activity.TagObjects.SingleOrDefault(x => string.Equals(x.Key, tagName, StringComparison.OrdinalIgnoreCase));
        Assert.True(matching.Equals(default(KeyValuePair<string, object?>)), $"Tag '{tagName}' was found in activity tags but should not exist.");
    }
}
