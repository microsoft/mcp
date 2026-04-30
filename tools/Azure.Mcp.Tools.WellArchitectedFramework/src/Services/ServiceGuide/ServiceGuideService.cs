// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Reflection;
using System.Text.Json;
using Azure.Mcp.Tools.WellArchitectedFramework.Commands;
using Microsoft.Mcp.Core.Helpers;
using ServiceGuideModel = Azure.Mcp.Tools.WellArchitectedFramework.Models.ServiceGuide;

namespace Azure.Mcp.Tools.WellArchitectedFramework.Services.ServiceGuide;

public sealed class ServiceGuideService : IServiceGuideService
{
    private static Dictionary<string, ServiceGuideModel>? s_serviceGuidesCache;
    private static readonly object s_lock = new();

    public string? GetServiceGuideUrl(string serviceName)
    {
        EnsureServiceGuidesLoaded();

        if (s_serviceGuidesCache == null)
        {
            return null;
        }

        var serviceNameNormalized = NormalizeServiceName(serviceName);
        foreach (var kvp in s_serviceGuidesCache)
        {
            if (kvp.Value.ServiceNameVariationsNormalized.Contains(serviceNameNormalized))
            {
                return kvp.Value.ServiceGuideUrl;
            }
        }

        return null;
    }

    public List<string> GetAllServiceNames()
    {
        EnsureServiceGuidesLoaded();

        if (s_serviceGuidesCache == null || s_serviceGuidesCache.Count == 0)
        {
            return [];
        }

        return [.. s_serviceGuidesCache.Keys.OrderBy(k => k)];
    }

    // This double-checked locking pattern is needed to ensure thread safety if two threads call EnsureServiceGuidesLoaded
    // at the same time when the cache is not yet initialized.
    //      Thread A: null check → acquire lock → null check again → initialize → release lock
    //      Thread B: null check → wait for lock (while Thread A is working)                    → acquire lock → null check again → see it's initialized by Thread A → return
    private static void EnsureServiceGuidesLoaded()
    {
        if (s_serviceGuidesCache != null)
        {
            return;
        }

        lock (s_lock)
        {
            if (s_serviceGuidesCache != null)
            {
                return;
            }

            LoadServiceGuides();
        }
    }

    private static void LoadServiceGuides()
    {
        Assembly assembly = typeof(ServiceGuideService).Assembly;

        try
        {
            string resourceName = EmbeddedResourceHelper.FindEmbeddedResource(assembly, "service-guides.json");
            string jsonContent = EmbeddedResourceHelper.ReadEmbeddedResource(assembly, resourceName);
            var serviceGuides = JsonSerializer.Deserialize(
                jsonContent,
                WellArchitectedFrameworkJsonContext.Default.DictionaryStringServiceGuide);
            s_serviceGuidesCache = serviceGuides ?? new Dictionary<string, ServiceGuideModel>();

            return;
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            throw new InvalidOperationException("Missing 'service-guides.json' file", ex);
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException("Failed to parse 'service-guides.json' file", ex);
        }
    }

    private static string NormalizeServiceName(string serviceName)
    {
        return serviceName
            .ToLowerInvariant()
            .Trim(new char[] { ' ', '"', '\'' })
            .Replace("-", string.Empty)
            .Replace("_", string.Empty)
            .Replace(" ", string.Empty);
    }
}
