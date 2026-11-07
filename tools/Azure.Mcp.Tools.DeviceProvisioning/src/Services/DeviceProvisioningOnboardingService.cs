// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Core;
using Azure.Core.Pipeline;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Tools.DeviceProvisioning.Models;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.DeviceProvisioning.Services;

public sealed class DeviceProvisioningOnboardingService(
    IDeviceProvisioningService deviceProvisioningService,
    IHttpClientFactory httpClientFactory,
    IAzureService azureService,
    ILogger<DeviceProvisioningOnboardingService> logger)
    : BaseAzureService(azureService), IDeviceProvisioningOnboardingService
{
    private const string ApiVersion = "2021-10-01";
    private static readonly TimeSpan s_operationTimeout = TimeSpan.FromSeconds(100);
    private readonly IDeviceProvisioningService _deviceProvisioningService =
        deviceProvisioningService ?? throw new ArgumentNullException(nameof(deviceProvisioningService));
    private readonly IHttpClientFactory _httpClientFactory =
        httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
    private readonly ILogger<DeviceProvisioningOnboardingService> _logger =
        logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<OnboardingValidationResult> ValidateOnboarding(
        DeviceProvisioningOnboardingValidationRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ValidateRequiredParameters(
            (nameof(request.ServiceName), request.ServiceName),
            (nameof(request.ResourceGroup), request.ResourceGroup),
            (nameof(request.Subscription), request.Subscription));

        var targetType = string.IsNullOrWhiteSpace(request.EnrollmentId)
            ? "enrollmentGroup"
            : "individualEnrollment";
        var targetId = request.EnrollmentId ?? request.EnrollmentGroupId ?? string.Empty;
        var result = new OnboardingValidationBuilder(
            targetType,
            targetId,
            request.ServiceName,
            request.IntendedHub);

        try
        {
            return await ExecuteWithTimeoutAsync(
                ct => ValidateCoreAsync(request, targetType, targetId, result, ct),
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error validating onboarding configuration for DPS '{ServiceName}', target type '{TargetType}', target '{TargetId}'.",
                request.ServiceName,
                targetType,
                targetId);
            throw;
        }
    }

    private async Task<OnboardingValidationResult> ValidateCoreAsync(
        DeviceProvisioningOnboardingValidationRequest request,
        string targetType,
        string targetId,
        OnboardingValidationBuilder result,
        CancellationToken cancellationToken)
    {
        DeviceProvisioningServiceDescription service;
        try
        {
            service = await _deviceProvisioningService.GetService(
                request.ServiceName,
                request.ResourceGroup,
                request.Subscription,
                request.Tenant,
                cancellationToken);
        }
        catch (RequestFailedException ex) when (ex.Status == (int)HttpStatusCode.NotFound)
        {
            result.AddFinding(
                "dps.resource.exists",
                "blocker",
                "The selected DPS ARM resource was not found.",
                "Verify the subscription, resource group, and DPS service name.");
            return result.Build();
        }
        catch (RequestFailedException ex) when (ex.Status is
            (int)HttpStatusCode.Unauthorized or
            (int)HttpStatusCode.Forbidden)
        {
            result.AddExecutionLimitation(
                "dps.arm.read",
                "authorizationDenied",
                "The DPS ARM resource could not be read with the current identity.",
                "Microsoft.Devices/provisioningServices/read");
            return result.Build(essentialCheckIncomplete: true);
        }

        result.AddFinding(
            "dps.resource.exists",
            "passed",
            "The selected DPS ARM resource exists and was readable.");

        AccessToken accessToken;
        try
        {
            var credential = await GetCredential(request.Tenant, cancellationToken);
            accessToken = await credential.GetTokenAsync(
                new TokenRequestContext([GetTokenScope(service.ServiceOperationsHostName)]),
                cancellationToken);
        }
        catch (Exception ex)
        {
            result.AddExecutionLimitation(
                "dps.service.authentication",
                "authenticationFailed",
                $"A DPS service-plane token could not be acquired: {ex.Message}");
            return result.Build(essentialCheckIncomplete: true);
        }

        var targetPath = targetType == "individualEnrollment"
            ? $"/enrollments/{Uri.EscapeDataString(targetId)}"
            : $"/enrollmentGroups/{Uri.EscapeDataString(targetId)}";
        var (targetStatus, targetContent) = await SendGetAsync(
            service.ServiceOperationsHostName,
            targetPath,
            accessToken.Token,
            cancellationToken);

        if (targetStatus == HttpStatusCode.NotFound)
        {
            result.AddFinding(
                "enrollment.exists",
                "blocker",
                $"The selected {targetType} '{targetId}' was not found.",
                "Verify the target identifier or create the required enrollment.");
            return result.Build();
        }

        if (targetStatus is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
        {
            result.AddExecutionLimitation(
                "enrollment.read",
                "authorizationDenied",
                $"The selected {targetType} could not be read with the current identity.",
                targetType == "individualEnrollment"
                    ? "Microsoft.Devices/provisioningServices/enrollments/read"
                    : "Microsoft.Devices/provisioningServices/enrollmentGroups/read");
            return result.Build(essentialCheckIncomplete: true);
        }

        EnsureSuccessOrThrow(targetStatus, "read the selected enrollment");
        result.AddFinding(
            "enrollment.exists",
            "passed",
            $"The selected {targetType} exists and was readable.");

        var enrollment = DpsResponseParser.ParseEnrollment(targetContent, targetId);
        enrollment = await ResolveCertificateAuthoritiesAsync(
            request,
            enrollment,
            result,
            cancellationToken);
        OnboardingConfigurationValidator.Validate(
            service,
            enrollment,
            request.IntendedHub,
            result);
        var registrationId = request.RegistrationId ??
            (targetType == "individualEnrollment" ? targetId : null);

        if (request.Devices is { Count: > 0 })
        {
            var deviceResults = await ValidateDevicesAsync(
                service,
                enrollment,
                request.Devices,
                request.IntendedHub,
                request.IncludeRegistrationState,
                accessToken.Token,
                cancellationToken);
            result.AddDeviceResults(deviceResults);
        }
        else if (!string.IsNullOrWhiteSpace(request.CertificateChain))
        {
            PublicIdentityEvidenceValidator.ValidateCertificateChain(
                request.CertificateChain,
                registrationId,
                enrollment,
                result);
        }

        if (request.Devices is not { Count: > 0 } &&
            !string.IsNullOrWhiteSpace(request.TpmEndorsementKey))
        {
            PublicIdentityEvidenceValidator.ValidateTpmEndorsementKey(
                request.TpmEndorsementKey,
                enrollment,
                result);
        }

        if (request.Devices is not { Count: > 0 } &&
            request.IncludeRegistrationState &&
            !string.IsNullOrWhiteSpace(registrationId))
        {
            result.RegistrationState = await AddRegistrationStateAsync(
                service,
                registrationId,
                request.IntendedHub,
                accessToken.Token,
                result,
                cancellationToken);
        }

        return result.Build();
    }

    private async Task<DpsEnrollmentSnapshot> ResolveCertificateAuthoritiesAsync(
        DeviceProvisioningOnboardingValidationRequest request,
        DpsEnrollmentSnapshot enrollment,
        IOnboardingValidationSink result,
        CancellationToken cancellationToken)
    {
        if (!enrollment.UsesX509CertificateAuthorityReferences)
        {
            return enrollment;
        }

        var resolvedAuthorities = new List<DpsCertificateAuthoritySnapshot>(
            enrollment.X509CertificateAuthorities.Count);
        foreach (var authority in enrollment.X509CertificateAuthorities)
        {
            try
            {
                var certificate = await _deviceProvisioningService.GetCertificate(
                    request.ServiceName,
                    authority.Name,
                    request.ResourceGroup,
                    request.Subscription,
                    request.Tenant,
                    cancellationToken);
                resolvedAuthorities.Add(authority with
                {
                    ResolutionStatus = "resolved",
                    Certificate = certificate
                });
            }
            catch (RequestFailedException ex) when (ex.Status == (int)HttpStatusCode.NotFound)
            {
                resolvedAuthorities.Add(authority with
                {
                    ResolutionStatus = "notFound"
                });
            }
            catch (RequestFailedException ex) when (ex.Status is
                (int)HttpStatusCode.Unauthorized or
                (int)HttpStatusCode.Forbidden)
            {
                resolvedAuthorities.Add(authority with
                {
                    ResolutionStatus = "authorizationDenied"
                });
                result.AddExecutionLimitation(
                    "x509.caCertificate.read",
                    "authorizationDenied",
                    $"The {authority.Role} DPS certificate-authority reference '{authority.Name}' could not be read with the current identity.",
                    "Microsoft.Devices/provisioningServices/certificates/read");
            }
        }

        return enrollment with
        {
            X509CertificateAuthorities = resolvedAuthorities
        };
    }

    private Task<OnboardingRegistrationState?> AddRegistrationStateAsync(
        DeviceProvisioningServiceDescription service,
        string registrationId,
        string? intendedHub,
        string accessToken,
        IOnboardingValidationSink result,
        CancellationToken cancellationToken) =>
        AddRegistrationStateCoreAsync(
            service,
            registrationId,
            intendedHub,
            accessToken,
            result,
            cancellationToken);

    private async Task<OnboardingRegistrationState?> AddRegistrationStateCoreAsync(
        DeviceProvisioningServiceDescription service,
        string registrationId,
        string? intendedHub,
        string accessToken,
        IOnboardingValidationSink result,
        CancellationToken cancellationToken)
    {
        var (status, content) = await SendGetAsync(
            service.ServiceOperationsHostName,
            $"/registrations/{Uri.EscapeDataString(registrationId)}",
            accessToken,
            cancellationToken);

        if (status == HttpStatusCode.NotFound)
        {
            result.AddFinding(
                "registrationState.available",
                "informational",
                $"No stored registration state was found for '{registrationId}', which is normal before its first successful provisioning attempt.");
            return null;
        }

        if (status is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
        {
            result.AddExecutionLimitation(
                "registrationState.read",
                "authorizationDenied",
                "The current stored registration state could not be read with the current identity.",
                "Microsoft.Devices/provisioningServices/registrationStates/read");
            return null;
        }

        EnsureSuccessOrThrow(status, "read the stored registration state");
        var state = DpsResponseParser.ParseRegistrationState(content);
        var registrationState = new OnboardingRegistrationState(
            state.RegistrationId,
            state.AssignedHub,
            state.DeviceId,
            state.Status,
            state.Substatus,
            state.ErrorCode,
            state.ErrorMessage,
            state.CreatedDateTimeUtc,
            state.LastUpdatedDateTimeUtc);
        result.AddFinding(
            "registrationState.available",
            state.ErrorCode.HasValue ||
            string.Equals(state.Status, "failed", StringComparison.OrdinalIgnoreCase)
                ? "warning"
                : "informational",
            $"Stored registration state is available with status '{state.Status}' and assigned Hub '{state.AssignedHub}'.");

        if (!string.IsNullOrWhiteSpace(intendedHub))
        {
            var assignedHubMatchesIntent = string.Equals(
                state.AssignedHub,
                intendedHub,
                StringComparison.OrdinalIgnoreCase);
            result.AddFinding(
                "registrationState.assignedHubIntent",
                assignedHubMatchesIntent ? "passed" : "warning",
                assignedHubMatchesIntent
                    ? $"The stored assigned Hub matches the intended Hub '{intendedHub}'."
                    : $"The stored assigned Hub '{state.AssignedHub}' does not match the intended Hub '{intendedHub}'.",
                assignedHubMatchesIntent
                    ? null
                    : "Confirm whether reprovisioning should retain or update the existing Hub assignment.");
        }

        return registrationState;
    }

    private async Task<List<OnboardingDeviceValidationResult>> ValidateDevicesAsync(
        DeviceProvisioningServiceDescription service,
        DpsEnrollmentSnapshot enrollment,
        List<OnboardingDeviceEvidence> devices,
        string? intendedHub,
        bool includeRegistrationState,
        string accessToken,
        CancellationToken cancellationToken)
    {
        var tasks = devices.Select(device =>
            ValidateDeviceAsync(
                service,
                enrollment,
                device,
                intendedHub,
                includeRegistrationState,
                accessToken,
                cancellationToken));
        var results = await Task.WhenAll(tasks);
        return [.. results];
    }

    private async Task<OnboardingDeviceValidationResult> ValidateDeviceAsync(
        DeviceProvisioningServiceDescription service,
        DpsEnrollmentSnapshot enrollment,
        OnboardingDeviceEvidence device,
        string? intendedHub,
        bool includeRegistrationState,
        string accessToken,
        CancellationToken cancellationToken)
    {
        var result = new OnboardingDeviceValidationBuilder(device.RegistrationId);
        if (!string.IsNullOrWhiteSpace(device.CertificateChain))
        {
            PublicIdentityEvidenceValidator.ValidateCertificateChain(
                device.CertificateChain,
                device.RegistrationId,
                enrollment,
                result);
        }
        else if (!string.IsNullOrWhiteSpace(device.TpmEndorsementKey))
        {
            PublicIdentityEvidenceValidator.ValidateTpmEndorsementKey(
                device.TpmEndorsementKey,
                enrollment,
                result);
        }
        else
        {
            result.AddFinding(
                "device.identityEvidence",
                "unverified",
                $"No public device identity evidence was supplied for '{device.RegistrationId}'.",
                "Supply a public X.509 certificate chain or TPM endorsement key when applicable.");
        }

        result.AddFinding(
            "device.retryReadiness",
            "unverified",
            "Device SDK and firmware retry readiness was not assessed.",
            "Review the device SDK retry, backoff, timeout, and reprovisioning behavior before production rollout.");

        if (includeRegistrationState)
        {
            result.RegistrationState = await AddRegistrationStateCoreAsync(
                service,
                device.RegistrationId,
                intendedHub,
                accessToken,
                result,
                cancellationToken);
        }

        return result.Build();
    }

    private async Task<(HttpStatusCode Status, string Content)> SendGetAsync(
        string hostname,
        string path,
        string accessToken,
        CancellationToken cancellationToken)
    {
        var requestUri = CreateValidatedDataPlaneUri(hostname, path);
        using var httpClient = _httpClientFactory.CreateClient();
        var pipeline = BuildDataPlanePipeline(httpClient);
        using var request = pipeline.CreateRequest();
        request.Method = RequestMethod.Get;
        request.Uri.Reset(requestUri);
        request.Headers.Add("Authorization", $"Bearer {accessToken}");
        request.Headers.Add("Accept", "application/json");

        using var response = await pipeline.SendRequestAsync(request, cancellationToken);
        return ((HttpStatusCode)response.Status, response.Content.ToString());
    }

    private Uri CreateValidatedDataPlaneUri(string hostname, string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(hostname);
        var endpoint = new Uri(
            $"https://{hostname}{path}?api-version={ApiVersion}",
            UriKind.Absolute);
        AzureService.ValidateAzureServiceEndpoint(
            endpoint.AbsoluteUri,
            "deviceprovisioning");
        return endpoint;
    }

    private static HttpPipeline BuildDataPlanePipeline(HttpClient httpClient)
    {
        var options = AddDefaultPolicies(new DeviceProvisioningClientOptions());
        options.Transport = new HttpClientTransport(httpClient);
        return HttpPipelineBuilder.Build(options);
    }

    private static string GetTokenScope(string hostname)
    {
        if (hostname.EndsWith(".azure-devices-provisioning.cn", StringComparison.OrdinalIgnoreCase))
        {
            return "https://azure-devices-provisioning.cn/.default";
        }

        if (hostname.EndsWith(".azure-devices-provisioning.us", StringComparison.OrdinalIgnoreCase))
        {
            return "https://azure-devices-provisioning.us/.default";
        }

        return "https://azure-devices-provisioning.net/.default";
    }

    private static void EnsureSuccessOrThrow(
        HttpStatusCode statusCode,
        string operation)
    {
        if ((int)statusCode is >= 200 and <= 299)
        {
            return;
        }

        throw new HttpRequestException(
            $"DPS failed to {operation} with status {(int)statusCode} ({statusCode}).",
            inner: null,
            statusCode);
    }

    private static async Task<T> ExecuteWithTimeoutAsync<T>(
        Func<CancellationToken, Task<T>> operation,
        CancellationToken cancellationToken)
    {
        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(s_operationTimeout);
        try
        {
            return await operation(timeoutCts.Token);
        }
        catch (OperationCanceledException) when (
            timeoutCts.IsCancellationRequested &&
            !cancellationToken.IsCancellationRequested)
        {
            throw new TimeoutException(
                $"The DPS onboarding validation operation timed out after {s_operationTimeout.TotalSeconds:N0} seconds.");
        }
    }
}
