// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Globalization;
using System.Net.Http.Headers;
using System.Text.Json;
using Azure.Core;
using Azure.Mcp.Tools.Optimization.Models;

namespace Azure.Mcp.Tools.Optimization.Services;

/// <summary>
/// Resolves the current-versus-target VM SKU specifications for a compute resource by calling the
/// ARM resource and Microsoft.Compute Resource SKUs REST APIs.
/// </summary>
internal sealed class OptimizationComputeSkuClient(
    HttpClient httpClient,
    TokenCredential credential,
    string armHost,
    string armScope)
{
    private const string ResourceApiVersion = "2024-07-01";
    private const string SkuApiVersion = "2021-07-01";

    private readonly HttpClient _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    private readonly TokenCredential _credential = credential ?? throw new ArgumentNullException(nameof(credential));
    private readonly string _armHost = armHost.TrimEnd('/');
    private readonly string _armScope = armScope;

    public async Task<ResourceSkuComparison> GetComparisonAsync(
        string resourceId,
        string targetSku,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(targetSku))
        {
            throw new ArgumentException("Target SKU is required.", nameof(targetSku));
        }

        var resource = await GetResourceAsync(resourceId, cancellationToken).ConfigureAwait(false);
        var skus = await GetRegionalSkusAsync(
            resource.SubscriptionId,
            resource.Location,
            cancellationToken).ConfigureAwait(false);

        var current = FindSku(skus, resource.CurrentSku, resource.Location, requireAvailable: false);
        var target = FindSku(skus, targetSku, resource.Location, requireAvailable: true);

        return new ResourceSkuComparison(
            resourceId,
            resource.Location,
            resource.ResourceKind,
            resource.InstanceCount,
            current,
            target);
    }

    private async Task<ResourceMetadata> GetResourceAsync(
        string resourceId,
        CancellationToken cancellationToken)
    {
        var isVmss = resourceId.Contains(
            "/providers/Microsoft.Compute/virtualMachineScaleSets/",
            StringComparison.OrdinalIgnoreCase);
        var isVm = resourceId.Contains(
            "/providers/Microsoft.Compute/virtualMachines/",
            StringComparison.OrdinalIgnoreCase);
        if (!isVm && !isVmss)
        {
            throw new InvalidOperationException(
                "SKU utilization projection supports only Microsoft.Compute virtual machines and virtual machine scale sets.");
        }

        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"{_armHost}{resourceId}?api-version={ResourceApiVersion}");
        using var response = await SendAsync(request, cancellationToken).ConfigureAwait(false);
        await EnsureSuccessAsync(response, "Azure compute resource", cancellationToken).ConfigureAwait(false);
        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken)
            .ConfigureAwait(false);
        var root = document.RootElement;

        var location = root.TryGetProperty("location", out var locationElement)
            ? locationElement.GetString()
            : null;
        string? currentSku;
        var instanceCount = 1;
        if (isVmss)
        {
            if (!root.TryGetProperty("sku", out var skuElement)
                || !skuElement.TryGetProperty("name", out var skuNameElement))
            {
                throw new InvalidOperationException(
                    "The VM scale set does not expose a single top-level SKU. Flexible scale sets with mixed sizes are not supported.");
            }

            currentSku = skuNameElement.GetString();
            if (skuElement.TryGetProperty("capacity", out var capacity)
                && capacity.TryGetInt32(out var parsedCapacity))
            {
                instanceCount = parsedCapacity;
            }
        }
        else
        {
            currentSku = root.TryGetProperty("properties", out var properties)
                && properties.TryGetProperty("hardwareProfile", out var hardwareProfile)
                && hardwareProfile.TryGetProperty("vmSize", out var vmSize)
                    ? vmSize.GetString()
                    : null;
        }

        if (string.IsNullOrWhiteSpace(location) || string.IsNullOrWhiteSpace(currentSku))
        {
            throw new InvalidOperationException("The Azure compute resource did not return its location and current SKU.");
        }

        var segments = resourceId.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (segments.Length < 2 || !string.Equals(segments[0], "subscriptions", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("The resource ID does not contain a subscription ID.");
        }

        return new ResourceMetadata(
            segments[1],
            location,
            isVmss ? "virtualMachineScaleSet" : "virtualMachine",
            currentSku,
            Math.Max(instanceCount, 1));
    }

    private async Task<List<JsonElement>> GetRegionalSkusAsync(
        string subscriptionId,
        string location,
        CancellationToken cancellationToken)
    {
        var filter = Uri.EscapeDataString($"location eq '{location}'");
        var nextLink =
            $"{_armHost}/subscriptions/{subscriptionId}/providers/Microsoft.Compute/skus" +
            $"?api-version={SkuApiVersion}&%24filter={filter}";
        var results = new List<JsonElement>();

        while (!string.IsNullOrWhiteSpace(nextLink))
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, nextLink);
            using var response = await SendAsync(request, cancellationToken).ConfigureAwait(false);
            await EnsureSuccessAsync(response, "Azure compute SKU", cancellationToken).ConfigureAwait(false);
            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
            using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken)
                .ConfigureAwait(false);
            var root = document.RootElement;

            if (root.TryGetProperty("value", out var values))
            {
                results.AddRange(values.EnumerateArray().Select(value => value.Clone()));
            }

            nextLink = root.TryGetProperty("nextLink", out var nextLinkElement)
                ? nextLinkElement.GetString()
                : null;
        }

        return results;
    }

    private static VmSkuSpecifications FindSku(
        IEnumerable<JsonElement> skus,
        string skuName,
        string location,
        bool requireAvailable)
    {
        var sku = skus.FirstOrDefault(item =>
            item.TryGetProperty("name", out var name)
            && item.TryGetProperty("resourceType", out var resourceType)
            && string.Equals(name.GetString(), skuName, StringComparison.OrdinalIgnoreCase)
            && string.Equals(resourceType.GetString(), "virtualMachines", StringComparison.OrdinalIgnoreCase));
        if (sku.ValueKind == JsonValueKind.Undefined)
        {
            throw new InvalidOperationException($"VM SKU '{skuName}' was not found in region '{location}'.");
        }

        if (requireAvailable && IsRestrictedInLocation(sku, location))
        {
            throw new InvalidOperationException(
                $"VM SKU '{skuName}' is restricted for this subscription in region '{location}'.");
        }

        if (!sku.TryGetProperty("capabilities", out var capabilitiesElement)
            || capabilitiesElement.ValueKind != JsonValueKind.Array)
        {
            throw new InvalidOperationException($"VM SKU '{skuName}' did not return capabilities.");
        }

        var capabilities = capabilitiesElement
            .EnumerateArray()
            .Where(capability =>
                capability.TryGetProperty("name", out _)
                && capability.TryGetProperty("value", out _))
            .ToDictionary(
                capability => capability.GetProperty("name").GetString() ?? string.Empty,
                capability => capability.GetProperty("value").GetString() ?? string.Empty,
                StringComparer.OrdinalIgnoreCase);

        var cores = ParsePositiveInt(capabilities, "vCPUsAvailable")
            ?? ParsePositiveInt(capabilities, "vCPUs")
            ?? throw new InvalidOperationException($"VM SKU '{skuName}' did not return a vCPU capability.");
        var memory = ParsePositiveDouble(capabilities, "MemoryGB")
            ?? throw new InvalidOperationException($"VM SKU '{skuName}' did not return a memory capability.");

        return new VmSkuSpecifications(sku.GetProperty("name").GetString()!, cores, memory);
    }

    private static bool IsRestrictedInLocation(JsonElement sku, string location)
    {
        if (!sku.TryGetProperty("restrictions", out var restrictions))
        {
            return false;
        }

        return restrictions.EnumerateArray().Any(restriction =>
        {
            if (!restriction.TryGetProperty("type", out var type)
                || !string.Equals(type.GetString(), "Location", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (restriction.TryGetProperty("values", out var values)
                && values.EnumerateArray().Any(
                    value => string.Equals(value.GetString(), location, StringComparison.OrdinalIgnoreCase)))
            {
                return true;
            }

            return restriction.TryGetProperty("restrictionInfo", out var info)
                && info.TryGetProperty("locations", out var locations)
                && locations.EnumerateArray().Any(
                    value => string.Equals(value.GetString(), location, StringComparison.OrdinalIgnoreCase));
        });
    }

    private static int? ParsePositiveInt(
        IReadOnlyDictionary<string, string> capabilities,
        string name)
    {
        if (!capabilities.TryGetValue(name, out var value)
            || !double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed)
            || parsed <= 0)
        {
            return null;
        }

        return checked((int)parsed);
    }

    private static double? ParsePositiveDouble(
        IReadOnlyDictionary<string, string> capabilities,
        string name) =>
        capabilities.TryGetValue(name, out var value)
        && double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed)
        && parsed > 0
            ? parsed
            : null;

    private async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var token = await _credential
            .GetTokenAsync(new TokenRequestContext(new[] { _armScope }), cancellationToken)
            .ConfigureAwait(false);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.Token);
        return await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
    }

    private static async Task EnsureSuccessAsync(
        HttpResponseMessage response,
        string operation,
        CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        throw new HttpRequestException(
            $"{operation} request failed with status {(int)response.StatusCode}: {body}",
            null,
            response.StatusCode);
    }

    private sealed record ResourceMetadata(
        string SubscriptionId,
        string Location,
        string ResourceKind,
        string CurrentSku,
        int InstanceCount);
}
