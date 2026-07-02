// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Reflection;
using Azure.Mcp.Core.Helpers;

namespace Azure.Mcp.Tools.WellArchitectedFramework.Services.UsageGuide;

public sealed class UsageGuideService : IUsageGuideService
{
    private static string? s_usageGuideCache;
    private static readonly object s_lock = new();

    public string GetUsageGuide()
    {
        EnsureUsageGuideLoaded();
        return s_usageGuideCache ?? string.Empty;
    }

    // This double-checked locking pattern is needed to ensure thread safety if two threads call EnsureUsageGuideLoaded
    // at the same time when the cache is not yet initialized.
    //      Thread A: null check → acquire lock → null check again → initialize → release lock
    //      Thread B: null check → wait for lock (while Thread A is working)                    → acquire lock → null check again → see it's initialized by Thread A → return
    private static void EnsureUsageGuideLoaded()
    {
        if (s_usageGuideCache != null)
        {
            return;
        }

        lock (s_lock)
        {
            if (s_usageGuideCache != null)
            {
                return;
            }

            LoadUsageGuide();
        }
    }

    private static void LoadUsageGuide()
    {
        Assembly assembly = typeof(UsageGuideService).Assembly;

        try
        {
            string resourceName = EmbeddedResourceHelper.FindEmbeddedResource(assembly, "usage-guide.md");
            string content = EmbeddedResourceHelper.ReadEmbeddedResource(assembly, resourceName);
            s_usageGuideCache = content;
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            throw new InvalidOperationException("Missing 'usage-guide.md' file", ex);
        }
    }
}
