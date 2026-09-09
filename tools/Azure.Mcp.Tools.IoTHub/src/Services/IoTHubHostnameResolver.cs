// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Services.Caching;

namespace Azure.Mcp.Tools.IoTHub.Services;

public class IoTHubHostnameResolver(
    IIoTHubService ioTHubService,
    ICacheService cacheService) : IIoTHubHostnameResolver
{
    private readonly IIoTHubService _ioTHubService = ioTHubService ?? throw new ArgumentNullException(nameof(ioTHubService));
    private readonly ICacheService _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));

    // Resolving the hub hostname requires an ARM control-plane round-trip (a hub GET). Cache the
    // result briefly so repeated data-plane calls against the same hub don't pay that cost every
    // time. Shared across all IoT Hub data-plane commands so they reuse the same cached entries.
    private const string HostnameCacheGroup = "iothub-hostname";
    private static readonly TimeSpan s_cacheDuration = TimeSpan.FromMinutes(15);

    public async Task<string> GetHostnameAsync(
        string hubName,
        string resourceGroup,
        string subscription,
        string? tenant = null,
        CancellationToken cancellationToken = default)
    {
        var cacheKey = $"{subscription}/{resourceGroup}/{hubName}";
        var cached = await _cacheService.GetAsync<string>(HostnameCacheGroup, cacheKey, s_cacheDuration, cancellationToken);
        if (cached is not null)
        {
            return cached;
        }

        var hub = await _ioTHubService.GetIoTHub(
            hubName,
            resourceGroup,
            subscription,
            tenant: tenant,
            cancellationToken: cancellationToken);
        var hostname = hub.HostName ?? throw new InvalidOperationException("IoT Hub hostname is null");

        await _cacheService.SetAsync(HostnameCacheGroup, cacheKey, hostname, s_cacheDuration, cancellationToken);
        return hostname;
    }
}
