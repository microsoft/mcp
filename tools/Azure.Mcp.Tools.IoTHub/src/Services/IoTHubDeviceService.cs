// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Web;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Core.Services.Azure.Authentication;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.Mcp.Core.Services.Http;
using Azure.Mcp.Tools.IoTHub.Commands;
using Azure.Mcp.Tools.IoTHub.Models;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.IoTHub.Services;

public class IoTHubDeviceService(
    IIoTHubService ioTHubService,
    IAzureTokenCredentialProvider credentialProvider,
    IHttpClientService httpClientService,
    ITenantService tenantService,
    ILogger<IoTHubDeviceService> logger)
    : BaseAzureService(tenantService), IIoTHubDeviceService
{
    private readonly IIoTHubService _ioTHubService = ioTHubService ?? throw new ArgumentNullException(nameof(ioTHubService));
    private readonly IAzureTokenCredentialProvider _credentialProvider = credentialProvider ?? throw new ArgumentNullException(nameof(credentialProvider));
    private readonly IHttpClientService _httpClientService = httpClientService ?? throw new ArgumentNullException(nameof(httpClientService));
    private readonly ITenantService _tenantService = tenantService ?? throw new ArgumentNullException(nameof(tenantService));
    private readonly ILogger<IoTHubDeviceService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<List<DeviceIdentity>> ListDevices(
        string name,
        string resourceGroup,
        string subscription,
        int? maxCount = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(name), name),
            (nameof(resourceGroup), resourceGroup),
            (nameof(subscription), subscription));

        var hubDetails = await _ioTHubService.GetIoTHub(name, resourceGroup, subscription, retryPolicy, cancellationToken);
        if (hubDetails.Count == 0)
        {
            throw new InvalidOperationException($"IoT Hub '{name}' not found in resource group '{resourceGroup}'");
        }

        var hostname = hubDetails[0].HostName ?? throw new InvalidOperationException("IoT Hub hostname is null");
        var keys = await _ioTHubService.GetIoTHubKeys(name, resourceGroup, subscription, retryPolicy, cancellationToken);
        var key = keys.FirstOrDefault(k => k.Rights?.Contains("RegistryRead") == true)
            ?? throw new InvalidOperationException("No key with RegistryRead rights found");

        using var httpClient = _httpClientService.CreateClient();
        var apiVersion = "2021-04-12";
        var maxCountParam = maxCount.HasValue ? $"&top={maxCount.Value}" : string.Empty;
        var requestUri = $"https://{hostname}/devices?api-version={apiVersion}{maxCountParam}";

        var token = GetSasToken(hostname, key.KeyName, key.PrimaryKey);
        httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("SharedAccessSignature", token);

        var response = await httpClient.GetAsync(requestUri, cancellationToken);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize(content, IoTHubJsonContext.Default.ListDeviceIdentity) ?? [];
    }

    public async Task<DeviceTwin> GetDeviceTwin(
        string deviceId,
        string name,
        string resourceGroup,
        string subscription,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(deviceId), deviceId),
            (nameof(name), name),
            (nameof(resourceGroup), resourceGroup),
            (nameof(subscription), subscription));

        var hubDetails = await _ioTHubService.GetIoTHub(name, resourceGroup, subscription, retryPolicy, cancellationToken);
        if (hubDetails.Count == 0)
        {
            throw new InvalidOperationException($"IoT Hub '{name}' not found in resource group '{resourceGroup}'");
        }

        var hostname = hubDetails[0].HostName ?? throw new InvalidOperationException("IoT Hub hostname is null");
        var keys = await _ioTHubService.GetIoTHubKeys(name, resourceGroup, subscription, retryPolicy, cancellationToken);
        var key = keys.FirstOrDefault(k => k.Rights?.Contains("RegistryRead") == true)
            ?? throw new InvalidOperationException("No key with RegistryRead rights found");

        using var httpClient = _httpClientService.CreateClient();
        var apiVersion = "2021-04-12";
        var requestUri = $"https://{hostname}/twins/{Uri.EscapeDataString(deviceId)}?api-version={apiVersion}";

        var token = GetSasToken(hostname, key.KeyName, key.PrimaryKey);
        httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("SharedAccessSignature", token);

        var response = await httpClient.GetAsync(requestUri, cancellationToken);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize(content, IoTHubJsonContext.Default.DeviceTwin) ?? throw new InvalidOperationException("Failed to deserialize device twin");
    }

    public async Task<DeviceTwin> UpdateDeviceTwin(
        string deviceId,
        TwinPatch patch,
        string name,
        string resourceGroup,
        string subscription,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(deviceId), deviceId),
            (nameof(name), name),
            (nameof(resourceGroup), resourceGroup),
            (nameof(subscription), subscription));

        if (patch == null)
        {
            throw new ArgumentNullException(nameof(patch));
        }

        var hubDetails = await _ioTHubService.GetIoTHub(name, resourceGroup, subscription, retryPolicy, cancellationToken);
        if (hubDetails.Count == 0)
        {
            throw new InvalidOperationException($"IoT Hub '{name}' not found in resource group '{resourceGroup}'");
        }

        var hostname = hubDetails[0].HostName ?? throw new InvalidOperationException("IoT Hub hostname is null");
        var keys = await _ioTHubService.GetIoTHubKeys(name, resourceGroup, subscription, retryPolicy, cancellationToken);
        var key = keys.FirstOrDefault(k => k.Rights?.Contains("RegistryWrite") == true)
            ?? throw new InvalidOperationException("No key with RegistryWrite rights found");

        using var httpClient = _httpClientService.CreateClient();
        var apiVersion = "2021-04-12";
        var requestUri = $"https://{hostname}/twins/{Uri.EscapeDataString(deviceId)}?api-version={apiVersion}";

        var token = GetSasToken(hostname, key.KeyName, key.PrimaryKey);
        httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("SharedAccessSignature", token);

        var patchJson = JsonSerializer.Serialize(patch, IoTHubJsonContext.Default.TwinPatch);
        var content = new StringContent(patchJson, Encoding.UTF8, "application/json");

        var response = await httpClient.PatchAsync(requestUri, content, cancellationToken);
        response.EnsureSuccessStatusCode();

        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize(responseContent, IoTHubJsonContext.Default.DeviceTwin) ?? throw new InvalidOperationException("Failed to deserialize updated device twin");
    }

    public async Task<List<DeviceTwin>> QueryTwins(
        string query,
        string name,
        string resourceGroup,
        string subscription,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(query), query),
            (nameof(name), name),
            (nameof(resourceGroup), resourceGroup),
            (nameof(subscription), subscription));

        var hubDetails = await _ioTHubService.GetIoTHub(name, resourceGroup, subscription, retryPolicy, cancellationToken);
        if (hubDetails.Count == 0)
        {
            throw new InvalidOperationException($"IoT Hub '{name}' not found in resource group '{resourceGroup}'");
        }

        var hostname = hubDetails[0].HostName ?? throw new InvalidOperationException("IoT Hub hostname is null");
        var keys = await _ioTHubService.GetIoTHubKeys(name, resourceGroup, subscription, retryPolicy, cancellationToken);
        var key = keys.FirstOrDefault(k => k.Rights?.Contains("RegistryRead") == true)
            ?? throw new InvalidOperationException("No key with RegistryRead rights found");

        using var httpClient = _httpClientService.CreateClient();
        var apiVersion = "2021-04-12";
        var requestUri = $"https://{hostname}/devices/query?api-version={apiVersion}";

        var token = GetSasToken(hostname, key.KeyName, key.PrimaryKey);
        httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("SharedAccessSignature", token);

        var queryObject = new { query };
        var queryJson = JsonSerializer.Serialize(queryObject, IoTHubJsonContext.Default.Object);
        var content = new StringContent(queryJson, Encoding.UTF8, "application/json");

        var response = await httpClient.PostAsync(requestUri, content, cancellationToken);
        response.EnsureSuccessStatusCode();

        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize(responseContent, IoTHubJsonContext.Default.ListDeviceTwin) ?? [];
    }

    private static string GetSasToken(string hostname, string policyName, string sharedAccessKey)
    {
        // Generate SAS token for IoT Hub authentication
        var expiry = DateTimeOffset.UtcNow.AddHours(1).ToUnixTimeSeconds();
        var resourceUri = Uri.EscapeDataString(hostname);
        var toSign = $"{resourceUri}\n{expiry}";

        var keyBytes = Convert.FromBase64String(sharedAccessKey);
        using var hmac = new HMACSHA256(keyBytes);
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(toSign));
        var signature = Uri.EscapeDataString(Convert.ToBase64String(hash));

        return $"sr={resourceUri}&sig={signature}&se={expiry}&skn={policyName}";
    }
}
