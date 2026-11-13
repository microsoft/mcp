// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Tools.DeviceProvisioning.Commands.Onboarding;
using Azure.Mcp.Tools.DeviceProvisioning.Commands.Service;
using Azure.Mcp.Tools.DeviceProvisioning.Models;

namespace Azure.Mcp.Tools.DeviceProvisioning.Commands;

[JsonSerializable(typeof(DeviceProvisioningProperties))]
[JsonSerializable(typeof(DeviceProvisioningCertificateProperties))]
[JsonSerializable(typeof(DeviceProvisioningServiceDescription))]
[JsonSerializable(typeof(LinkedIoTHubDescription))]
[JsonSerializable(typeof(List<LinkedIoTHubDescription>))]
[JsonSerializable(typeof(DeviceProvisioningServiceGetCommand.DeviceProvisioningServiceGetCommandResult))]
[JsonSerializable(typeof(OnboardingValidationFinding))]
[JsonSerializable(typeof(List<OnboardingValidationFinding>))]
[JsonSerializable(typeof(OnboardingValidationExecutionLimitation))]
[JsonSerializable(typeof(List<OnboardingValidationExecutionLimitation>))]
[JsonSerializable(typeof(OnboardingRegistrationState))]
[JsonSerializable(typeof(OnboardingDeviceEvidence))]
[JsonSerializable(typeof(List<OnboardingDeviceEvidence?>))]
[JsonSerializable(typeof(OnboardingDeviceManifest))]
[JsonSerializable(typeof(OnboardingDeviceValidationResult))]
[JsonSerializable(typeof(List<OnboardingDeviceValidationResult>))]
[JsonSerializable(typeof(OnboardingValidationResult))]
[JsonSerializable(typeof(DeviceProvisioningOnboardingValidateCommand.DeviceProvisioningOnboardingValidateCommandResult))]
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
internal sealed partial class DeviceProvisioningJsonContext : JsonSerializerContext
{
}
