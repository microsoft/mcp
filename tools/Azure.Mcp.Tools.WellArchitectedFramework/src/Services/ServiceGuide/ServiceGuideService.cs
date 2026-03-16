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
    private static Dictionary<string, ServiceGuideModel>? s_serviceGuidesMetadataCache;
    private static readonly object s_lock = new();

    public string? GetServiceGuideUrl(string serviceName)
    {
        EnsureServiceGuidesMetadataLoaded();

        if (s_serviceGuidesMetadataCache == null)
        {
            return null;
        }

        var serviceNameNormalized = NormalizeServiceName(serviceName);
        foreach (var kvp in s_serviceGuidesMetadataCache)
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
        EnsureServiceGuidesMetadataLoaded();

        if (s_serviceGuidesMetadataCache == null || s_serviceGuidesMetadataCache.Count == 0)
        {
            return [];
        }

        return [.. s_serviceGuidesMetadataCache.Keys.OrderBy(k => k)];
    }

    // This double-checked locking pattern is needed to ensure thread safety if two threads call EnsureServiceGuidesMetadataLoaded
    // at the same time when the cache is not yet initialized.
    //      Thread A: null check → acquire lock → null check again → initialize → release lock
    //      Thread B: null check → wait for lock (while Thread A is working)                    → acquire lock → null check again → see it's initialized by Thread A → return
    private static void EnsureServiceGuidesMetadataLoaded()
    {
        if (s_serviceGuidesMetadataCache != null)
        {
            return;
        }

        lock (s_lock)
        {
            if (s_serviceGuidesMetadataCache != null)
            {
                return;
            }

            LoadServiceGuidesMetadata();
        }
    }

    private static void LoadServiceGuidesMetadata()
    {
        Assembly assembly = typeof(ServiceGuideService).Assembly;

        try
        {
            string resourceName = EmbeddedResourceHelper.FindEmbeddedResource(assembly, "service-guides-metadata.json");
            string jsonContent = EmbeddedResourceHelper.ReadEmbeddedResource(assembly, resourceName);
            var serviceGuidesMetadata = JsonSerializer.Deserialize(
                jsonContent,
                WellArchitectedFrameworkJsonContext.Default.DictionaryStringServiceGuide);
            s_serviceGuidesMetadataCache = serviceGuidesMetadata ?? new Dictionary<string, ServiceGuideModel>();

            return;
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            throw new InvalidOperationException("Missing 'service-guides-metadata.json' file", ex);
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException("Failed to parse 'service-guides-metadata.json' file", ex);
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
