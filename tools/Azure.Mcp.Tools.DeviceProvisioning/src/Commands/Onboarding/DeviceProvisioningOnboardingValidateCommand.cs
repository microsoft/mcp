// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.DeviceProvisioning.Models;
using Azure.Mcp.Tools.DeviceProvisioning.Options.Onboarding;
using Azure.Mcp.Tools.DeviceProvisioning.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.DeviceProvisioning.Commands.Onboarding;

[CommandMetadata(
    Id = "40bf6778-de05-4781-858e-ec61f89b81db",
    Name = "validate",
    Title = "Validate Device Provisioning Onboarding Configuration",
    Description = """
        Validate an existing Azure Device Provisioning Service individual enrollment or enrollment group,
        optionally assessing a bounded manifest of up to 10 devices against the selected target.
        Checks the DPS resource, selected enrollment status, attestation configuration, allocation policy,
        linked IoT Hub consistency, reprovisioning settings, optional stored registration state, and optional
        public X.509 or TPM identity evidence. X.509 checks resolve verified DPS CA references and validate
        certificate signatures, ordering, validity, constraints, key usage, registration identity, trust-anchor
        matching, and RSA or ECDSA compatibility. Reports blockers, warnings, unverified conditions, and access
        limitations separately. This command does not perform device registration, retrieve keys, validate every
        device in an enrollment group, or establish which enrollment DPS would authoritatively select.
        Responses are compact by default: aggregate results and exceptional devices are returned while repetitive
        per-device evidence is omitted. Set include-device-details only when full per-device findings are required.
        Present results as a compact aggregate readiness summary with short warnings and exception-only device
        details. For a current-state request, return validation.customerSummary exactly with no added narrative.
        When the user explicitly asks to compare against an earlier preflight result available in the conversation,
        use one compact aggregate Preflight-versus-Current table instead. Do not add detailed timestamps, discarded
        attempts, temporary-file cleanup, wide tables, repeated identical per-device findings, or internal count
        deltas unless explicitly requested.
        """,
    OperationPlane = ToolOperationPlane.Data,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class DeviceProvisioningOnboardingValidateCommand(
    ILogger<DeviceProvisioningOnboardingValidateCommand> logger,
    IDeviceProvisioningOnboardingService service,
    ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<DeviceProvisioningOnboardingValidateOptions, DeviceProvisioningOnboardingValidateCommand.DeviceProvisioningOnboardingValidateCommandResult>(subscriptionResolver)
{
    private const int MaximumCertificateChainLength = 64 * 1024;
    private const int MaximumTpmEndorsementKeyLength = 16 * 1024;
    private const int MaximumDeviceManifestLength = 1024 * 1024;
    private const int MaximumDeviceCount = 10;
    private readonly ILogger<DeviceProvisioningOnboardingValidateCommand> _logger = logger;
    private readonly IDeviceProvisioningOnboardingService _service = service;

    public override void ValidateOptions(
        DeviceProvisioningOnboardingValidateOptions options,
        ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);

        if (!IsValidServiceName(options.Service))
        {
            validationResult.Errors.Add(
                "--service must be 3-64 characters long, contain only letters, numbers, or hyphens, and start and end with a letter or number.");
        }

        var hasEnrollmentId = !string.IsNullOrWhiteSpace(options.EnrollmentId);
        var hasEnrollmentGroupId = !string.IsNullOrWhiteSpace(options.EnrollmentGroupId);
        if (hasEnrollmentId == hasEnrollmentGroupId)
        {
            validationResult.Errors.Add(
                "Specify exactly one of --enrollment-id or --enrollment-group-id.");
        }

        ValidateIdentifier("--enrollment-id", options.EnrollmentId, validationResult);
        ValidateIdentifier("--enrollment-group-id", options.EnrollmentGroupId, validationResult);
        ValidateIdentifier("--registration-id", options.RegistrationId, validationResult);
        if (hasEnrollmentId &&
            !string.IsNullOrWhiteSpace(options.RegistrationId) &&
            !string.Equals(
                options.RegistrationId,
                options.EnrollmentId,
                StringComparison.OrdinalIgnoreCase))
        {
            validationResult.Errors.Add(
                "--registration-id must match --enrollment-id when validating an individual enrollment.");
        }

        if (!string.IsNullOrWhiteSpace(options.CertificateChain) &&
            PublicIdentityEvidenceValidator.ContainsPrivateKey(options.CertificateChain))
        {
            validationResult.Errors.Add(
                "--certificate-chain must contain public certificates only and must not contain private-key material.");
        }

        if (options.CertificateChain?.Length > MaximumCertificateChainLength)
        {
            validationResult.Errors.Add(
                $"--certificate-chain must not exceed {MaximumCertificateChainLength} characters.");
        }

        if (!string.IsNullOrWhiteSpace(options.TpmEndorsementKey))
        {
            if (options.TpmEndorsementKey.Length > MaximumTpmEndorsementKeyLength)
            {
                validationResult.Errors.Add(
                    $"--tpm-endorsement-key must not exceed {MaximumTpmEndorsementKeyLength} characters.");
                return;
            }

            try
            {
                _ = Convert.FromBase64String(options.TpmEndorsementKey);
            }
            catch (FormatException)
            {
                validationResult.Errors.Add(
                    "--tpm-endorsement-key must be a valid Base64-encoded public endorsement key.");
            }
        }

        ValidateDeviceManifest(options, validationResult);
    }

    public override async Task<CommandResponse> ExecuteAsync(
        CommandContext context,
        DeviceProvisioningOnboardingValidateOptions options,
        CancellationToken cancellationToken)
    {
        try
        {
            var manifest = ParseDeviceManifest(options.DeviceManifest);
            var registrationId = manifest is null
                ? string.IsNullOrWhiteSpace(options.RegistrationId)
                    ? options.EnrollmentId
                    : options.RegistrationId
                : null;
            var includeRegistrationState =
                options.IncludeRegistrationState ??
                (manifest is not null || !string.IsNullOrWhiteSpace(registrationId));
            var validation = await _service.ValidateOnboarding(
                new DeviceProvisioningOnboardingValidationRequest(
                    options.Service,
                    options.ResourceGroup,
                    options.Subscription!,
                    options.Tenant,
                    options.EnrollmentId,
                    options.EnrollmentGroupId,
                    registrationId,
                    options.IntendedHub,
                    options.CertificateChain,
                    options.TpmEndorsementKey,
                    includeRegistrationState,
                    manifest?.Devices!
                        .Select(device => device!)
                        .ToList()),
                cancellationToken);
            var includeDeviceDetails = options.IncludeDeviceDetails == true;
            var responseValidation = includeDeviceDetails
                ? validation
                : CreateCompactValidation(validation);

            context.Response.Results = ResponseResult.Create(
                new DeviceProvisioningOnboardingValidateCommandResult(
                    responseValidation,
                    AreResultsTruncated: false,
                    FullDeviceDetailsIncluded: includeDeviceDetails),
                DeviceProvisioningJsonContext.Default.DeviceProvisioningOnboardingValidateCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error validating DPS onboarding configuration for service '{ServiceName}'.",
                options.Service);
            HandleException(context, ex);
        }

        return context.Response;
    }

    public record DeviceProvisioningOnboardingValidateCommandResult(
        OnboardingValidationResult Validation,
        bool AreResultsTruncated,
        bool FullDeviceDetailsIncluded);

    private static OnboardingValidationResult CreateCompactValidation(
        OnboardingValidationResult validation)
    {
        if (validation.DeviceResults is not { Count: > 0 } deviceResults)
        {
            return validation;
        }

        var warningOccurrences = deviceResults
            .SelectMany(result => result.Findings)
            .Where(finding => string.Equals(
                finding.Status,
                "warning",
                StringComparison.OrdinalIgnoreCase))
            .GroupBy(
                finding => (finding.CheckId, finding.Message),
                EqualityComparer<(string CheckId, string Message)>.Default)
            .ToDictionary(group => group.Key, group => group.Count());
        var exceptionalDeviceResults = deviceResults
            .Where(result =>
                result.BlockerCount > 0 ||
                result.ExecutionLimitations.Count > 0 ||
                result.Findings.Any(finding =>
                    string.Equals(
                        finding.CheckId,
                        "registrationState.assignedHubIntent",
                        StringComparison.Ordinal) &&
                    string.Equals(
                        finding.Status,
                        "warning",
                        StringComparison.OrdinalIgnoreCase)) ||
                result.Findings.Any(finding =>
                    string.Equals(
                        finding.Status,
                        "warning",
                        StringComparison.OrdinalIgnoreCase) &&
                    warningOccurrences[(finding.CheckId, finding.Message)] == 1))
            .ToList();

        return validation with
        {
            DeviceResults = exceptionalDeviceResults.Count == 0
                ? null
                : exceptionalDeviceResults
        };
    }

    private static void ValidateIdentifier(
        string optionName,
        string? value,
        ValidationResult validationResult)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return;
        }

        if (value.Length > 128 ||
            !IsAlphaNumeric(value[0]) ||
            !IsAlphaNumeric(value[^1]) ||
            value.Any(ch =>
                !IsAlphaNumeric(ch) &&
                ch is not '.' and not '_' and not '-' and not ':'))
        {
            validationResult.Errors.Add(
                $"{optionName} must be at most 128 characters, use letters, numbers, '.', '_', '-', or ':', and start and end with a letter or number.");
        }
    }

    private static bool IsAlphaNumeric(char value) =>
        value is >= 'a' and <= 'z' or
            >= 'A' and <= 'Z' or
            >= '0' and <= '9';

    private static bool IsValidServiceName(string value)
    {
        if (value.Length is < 3 or > 64 ||
            value[0] == '-' ||
            value[^1] == '-')
        {
            return false;
        }

        return value.All(ch => IsAlphaNumeric(ch) || ch == '-');
    }

    private static void ValidateDeviceManifest(
        DeviceProvisioningOnboardingValidateOptions options,
        ValidationResult validationResult)
    {
        if (string.IsNullOrWhiteSpace(options.DeviceManifest))
        {
            return;
        }

        if (options.DeviceManifest.Length > MaximumDeviceManifestLength)
        {
            validationResult.Errors.Add(
                $"--device-manifest must not exceed {MaximumDeviceManifestLength} characters.");
            return;
        }

        if (!string.IsNullOrWhiteSpace(options.RegistrationId) ||
            !string.IsNullOrWhiteSpace(options.CertificateChain) ||
            !string.IsNullOrWhiteSpace(options.TpmEndorsementKey))
        {
            validationResult.Errors.Add(
                "--device-manifest cannot be combined with --registration-id, --certificate-chain, or --tpm-endorsement-key.");
        }

        OnboardingDeviceManifest? manifest;
        try
        {
            manifest = ParseDeviceManifest(options.DeviceManifest);
        }
        catch (JsonException)
        {
            validationResult.Errors.Add(
                "--device-manifest must be a valid JSON object with a devices array.");
            return;
        }

        if (manifest?.Devices is not { Count: >= 1 })
        {
            validationResult.Errors.Add(
                $"--device-manifest must contain between 1 and {MaximumDeviceCount} devices.");
            return;
        }

        if (manifest.Devices.Count > MaximumDeviceCount)
        {
            validationResult.Errors.Add(
                $"Received {manifest.Devices.Count} devices; this request supports at most {MaximumDeviceCount}. " +
                "No devices were assessed. Submit the fleet in batches of 10 or fewer, or use the " +
                "azure-device-provisioning-onboarding skill for automatic batching and aggregation.");
            return;
        }

        if (!string.IsNullOrWhiteSpace(options.EnrollmentId) &&
            (manifest.Devices.Count != 1 ||
             manifest.Devices[0] is null ||
             !string.Equals(
                 manifest.Devices[0]!.RegistrationId,
                 options.EnrollmentId,
                 StringComparison.OrdinalIgnoreCase)))
        {
            validationResult.Errors.Add(
                "An individual enrollment manifest must contain exactly one device whose registrationId matches --enrollment-id.");
        }

        var registrationIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var device in manifest.Devices)
        {
            if (device is null)
            {
                validationResult.Errors.Add(
                    "--device-manifest devices must not contain null entries.");
                continue;
            }

            if (string.IsNullOrWhiteSpace(device.RegistrationId))
            {
                validationResult.Errors.Add(
                    "--device-manifest registrationId is required for every device.");
                continue;
            }

            ValidateIdentifier(
                "--device-manifest registrationId",
                device.RegistrationId,
                validationResult);
            if (!registrationIds.Add(device.RegistrationId))
            {
                validationResult.Errors.Add(
                    $"--device-manifest contains duplicate registrationId '{device.RegistrationId}'.");
            }

            var hasCertificate = !string.IsNullOrWhiteSpace(device.CertificateChain);
            var hasTpm = !string.IsNullOrWhiteSpace(device.TpmEndorsementKey);
            if (hasCertificate && hasTpm)
            {
                validationResult.Errors.Add(
                    $"Device '{device.RegistrationId}' must provide at most one of certificateChain or tpmEndorsementKey.");
            }

            if (hasCertificate &&
                PublicIdentityEvidenceValidator.ContainsPrivateKey(device.CertificateChain!))
            {
                validationResult.Errors.Add(
                    $"Device '{device.RegistrationId}' certificateChain must not contain private-key material.");
            }

            if (device.CertificateChain?.Length > MaximumCertificateChainLength)
            {
                validationResult.Errors.Add(
                    $"Device '{device.RegistrationId}' certificateChain must not exceed {MaximumCertificateChainLength} characters.");
            }

            if (device.TpmEndorsementKey?.Length > MaximumTpmEndorsementKeyLength)
            {
                validationResult.Errors.Add(
                    $"Device '{device.RegistrationId}' tpmEndorsementKey must not exceed {MaximumTpmEndorsementKeyLength} characters.");
            }
            else if (hasTpm)
            {
                try
                {
                    _ = Convert.FromBase64String(device.TpmEndorsementKey!);
                }
                catch (FormatException)
                {
                    validationResult.Errors.Add(
                        $"Device '{device.RegistrationId}' tpmEndorsementKey must be valid Base64.");
                }
            }
        }
    }

    private static OnboardingDeviceManifest? ParseDeviceManifest(string? json) =>
        string.IsNullOrWhiteSpace(json)
            ? null
            : JsonSerializer.Deserialize(
                json,
                DeviceProvisioningJsonContext.Default.OnboardingDeviceManifest);
}
