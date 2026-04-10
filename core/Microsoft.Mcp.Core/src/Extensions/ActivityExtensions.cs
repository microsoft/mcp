// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;

namespace Microsoft.Mcp.Core.Extensions;

public static class ActivityExtensions
{
    /// <summary>
    /// Sets a tag in the activity if, and only if, the tag does not already exist.
    /// </summary>
    /// <param name="name">The name of the tag.</param>
    /// <param name="value">The value of the tag.</param>
    public static Activity SetTagIfNotExists(this Activity? activity, string name, object? value)
    {
        if (activity == null)
        {
            return activity!;
        }

        if (activity.GetTagItem(name) is null)
        {
            activity.SetTag(name, value);
        }

        return activity;
    }
}
