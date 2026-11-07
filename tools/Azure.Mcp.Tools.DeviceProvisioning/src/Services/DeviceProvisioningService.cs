// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text;
using Azure.Core;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Tools.DeviceProvisioning.Commands;
using Azure.Mcp.Tools.DeviceProvisioning.Models;
using Azure.ResourceManager;
using Azure.ResourceManager.Resources;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.DeviceProvisioning.Services;

public class DeviceProvisioningService(
    IAzureService azureService,
    ILogger<DeviceProvisioningService> logger)
    : BaseAzureService(azureService), IDeviceProvisioningService
{
    private const string CertificateApiVersion = "2022-12-12";
    private static readonly ResourceType s_certificateResourceType =
        new("Microsoft.Devices/provisioningServices/certificates");
    private readonly ILogger<DeviceProvisioningService> _logger =
        logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<DeviceProvisioningServiceDescription> GetService(
        string serviceName,
        string resourceGroup,
        string subscription,
        string? tenant = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(subscription), subscription),
            (nameof(resourceGroup), resourceGroup),
            (nameof(serviceName), serviceName));

        try
        {
            var subscriptionResource = await AzureService.GetSubscription(
                subscription,
                tenant,
                cancellationToken: cancellationToken);
            var armClient = await CreateArmClientAsync(
                tenant,
                cancellationToken: cancellationToken);
            var serviceResourceId = new ResourceIdentifier(
                $"/subscriptions/{subscriptionResource.Data.SubscriptionId}/resourceGroups/{resourceGroup}/providers/Microsoft.Devices/provisioningServices/{serviceName}");
            var service = await armClient
                .GetGenericResource(serviceResourceId)
                .GetAsync(cancellationToken);

            return ConvertToDescription(service.Value.Data);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error retrieving Device Provisioning Service '{ServiceName}' in resource group '{ResourceGroup}' and subscription '{Subscription}'.",
                serviceName,
                resourceGroup,
                subscription);
            throw;
        }
    }

    public async Task<DeviceProvisioningCertificateDescription> GetCertificate(
        string serviceName,
        string certificateName,
        string resourceGroup,
        string subscription,
        string? tenant = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(subscription), subscription),
            (nameof(resourceGroup), resourceGroup),
            (nameof(serviceName), serviceName),
            (nameof(certificateName), certificateName));

        try
        {
            var subscriptionResource = await AzureService.GetSubscription(
                subscription,
                tenant,
                cancellationToken: cancellationToken);
            var armClientOptions = new ArmClientOptions();
            armClientOptions.SetApiVersion(
                s_certificateResourceType,
                CertificateApiVersion);
            var armClient = await CreateArmClientAsync(
                tenant,
                armClientOptions,
                cancellationToken: cancellationToken);
            var certificateResourceId = new ResourceIdentifier(
                $"/subscriptions/{subscriptionResource.Data.SubscriptionId}/resourceGroups/{resourceGroup}/providers/Microsoft.Devices/provisioningServices/{serviceName}/certificates/{certificateName}");
            var certificate = await armClient
                .GetGenericResource(certificateResourceId)
                .GetAsync(cancellationToken);
            var properties = certificate.Value.Data.Properties?.ToObjectFromJson(
                DeviceProvisioningJsonContext.Default.DeviceProvisioningCertificateProperties);

            return new DeviceProvisioningCertificateDescription(
                certificate.Value.Data.Name,
                DecodeCertificate(properties?.Certificate),
                properties?.IsVerified ?? false,
                properties?.Subject ?? string.Empty,
                properties?.Thumbprint ?? string.Empty);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error retrieving certificate '{CertificateName}' from Device Provisioning Service '{ServiceName}' in resource group '{ResourceGroup}' and subscription '{Subscription}'.",
                certificateName,
                serviceName,
                resourceGroup,
                subscription);
            throw;
        }
    }

    private static DeviceProvisioningServiceDescription ConvertToDescription(
        GenericResourceData service)
    {
        var properties = service.Properties?.ToObjectFromJson(
            DeviceProvisioningJsonContext.Default.DeviceProvisioningProperties);
        var linkedHubs = properties?.IotHubs?
            .Select(hub => new LinkedIoTHubDescription(
                hub.Name ?? string.Empty,
                hub.Location ?? string.Empty,
                hub.ApplyAllocationPolicy,
                hub.AllocationWeight))
            .ToList() ?? [];

        return new DeviceProvisioningServiceDescription(
            service.Id.ToString(),
            service.Name,
            service.Location.ToString(),
            service.Id?.ResourceGroupName ?? string.Empty,
            service.Id?.SubscriptionId ?? string.Empty,
            service.Sku?.Name ?? string.Empty,
            service.Sku?.Capacity ?? 0,
            properties?.State ?? string.Empty,
            properties?.ProvisioningState ?? string.Empty,
            properties?.ServiceOperationsHostName ?? string.Empty,
            properties?.DeviceProvisioningHostName ?? string.Empty,
            properties?.IdScope ?? string.Empty,
            properties?.AllocationPolicy ?? string.Empty,
            properties?.PublicNetworkAccess ?? string.Empty,
            properties?.EnableDataResidency,
            linkedHubs);
    }

    private static string DecodeCertificate(string? certificate)
    {
        if (string.IsNullOrWhiteSpace(certificate) ||
            certificate.Contains(
                "-----BEGIN CERTIFICATE-----",
                StringComparison.Ordinal))
        {
            return certificate ?? string.Empty;
        }

        try
        {
            var decoded = Encoding.UTF8.GetString(
                Convert.FromBase64String(certificate));
            return decoded.Contains(
                "-----BEGIN CERTIFICATE-----",
                StringComparison.Ordinal)
                ? decoded
                : certificate;
        }
        catch (FormatException)
        {
            return certificate;
        }
    }
}
