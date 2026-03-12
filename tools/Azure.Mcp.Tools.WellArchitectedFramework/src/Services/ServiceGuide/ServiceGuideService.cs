// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Reflection;
using System.Text.Json;
using Azure.Mcp.Core.Helpers;
using Azure.Mcp.Tools.WellArchitectedFramework.Commands;
using ServiceGuideModel = Azure.Mcp.Tools.WellArchitectedFramework.Models.ServiceGuide;

namespace Azure.Mcp.Tools.WellArchitectedFramework.Services.ServiceGuide;

public sealed class ServiceGuideService : IServiceGuideService
{
    private static Dictionary<string, ServiceGuideModel>? s_serviceGuidesCache;
    // Reverse lookup: normalized variation → ServiceGuideUrl, built once from s_serviceGuidesCache.
    // Multiple variations per service all point to the same URL, so each variation is a unique key.
    private static Dictionary<string, string>? s_variationToUrlCache;
    private static readonly object s_lock = new();

    public string? GetServiceGuideUrl(string serviceName)
    {
        EnsureServiceGuidesLoaded();

        if (s_variationToUrlCache == null)
        {
            return null;
        }

        var serviceNameNormalized = NormalizeServiceName(serviceName);
        return s_variationToUrlCache.TryGetValue(serviceNameNormalized, out var url) ? url : null;
    }

    public string GetAllServiceNamesAsCommaSeparatedList()
    {
        EnsureServiceGuidesLoaded();

        if (s_serviceGuidesCache == null || s_serviceGuidesCache.Count == 0)
        {
            return string.Empty;
        }

        return string.Join(", ", s_serviceGuidesCache.Keys);
    }

    // This double-checked locking pattern is needed to ensure thread safety if two threads call EnsureServiceGuidesLoaded
    // at the same time when the cache is not yet initialized.
    //      Thread A: null check → acquire lock → null check again → initialize → release lock
    //      Thread B: null check → wait for lock (while Thread A is working)                    → acquire lock → null check again → see it's initialized by Thread A → return
    private static void EnsureServiceGuidesLoaded()
    {
        if (s_variationToUrlCache != null)
        {
            return;
        }

        lock (s_lock)
        {
            if (s_variationToUrlCache != null)
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

            // Build reverse lookup: each normalized variation maps to its service's URL.
            // A service may have many variations, but each variation points to exactly one URL.
            var totalVariations = s_serviceGuidesCache.Sum(kvp => kvp.Value.ServiceNameVariationsNormalized.Length);
            var variationToUrl = new Dictionary<string, string>(totalVariations, StringComparer.Ordinal);
            foreach (var kvp in s_serviceGuidesCache)
            {
                foreach (var variation in kvp.Value.ServiceNameVariationsNormalized)
                {
                    variationToUrl[variation] = kvp.Value.ServiceGuideUrl;
                }
            }
            s_variationToUrlCache = variationToUrl;

            return;
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            // If loading fails, set to empty dictionaries to prevent repeated attempts
            s_serviceGuidesCache = new Dictionary<string, ServiceGuideModel>();
            s_variationToUrlCache = new Dictionary<string, string>(StringComparer.Ordinal);

            throw new InvalidOperationException("Missing 'service-guides.json' file", ex);
        }
        catch (JsonException ex)
        {
            // If loading fails, set to empty dictionaries to prevent repeated attempts
            s_serviceGuidesCache = new Dictionary<string, ServiceGuideModel>();
            s_variationToUrlCache = new Dictionary<string, string>(StringComparer.Ordinal);

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
