// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tests.Commands;
using Azure.Mcp.Tools.DeviceProvisioning.Commands.Onboarding;
using Azure.Mcp.Tools.DeviceProvisioning.Models;
using Azure.Mcp.Tools.DeviceProvisioning.Services;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.DeviceProvisioning.Tests.Onboarding;

public class DeviceProvisioningOnboardingValidateCommandTests
    : SubscriptionCommandUnitTestsBase<DeviceProvisioningOnboardingValidateCommand, IDeviceProvisioningOnboardingService>
{
    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = Command.GetCommand();
        Assert.Equal("validate", command.Name);
        Assert.Contains("existing", command.Description, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("does not perform device registration", command.Description, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("--subscription sub123 --resource-group rg1 --service dps1", false)]
    [InlineData("--subscription sub123 --resource-group rg1 --service dps1 --enrollment-id device1", true)]
    [InlineData("--subscription sub123 --resource-group rg1 --service dps1 --enrollment-group-id group1", true)]
    [InlineData("--subscription sub123 --resource-group rg1 --service dps1 --enrollment-id device1 --enrollment-group-id group1", false)]
    public async Task ExecuteAsync_RequiresExactlyOneTarget(
        string args,
        bool shouldSucceed)
    {
        if (shouldSucceed)
        {
            Service.ValidateOnboarding(
                    Arg.Any<DeviceProvisioningOnboardingValidationRequest>(),
                    Arg.Any<CancellationToken>())
                .Returns(CreateResult());
        }

        var response = await ExecuteCommandAsync(args);

        Assert.Equal(
            shouldSucceed ? HttpStatusCode.OK : HttpStatusCode.BadRequest,
            response.Status);
        if (!shouldSucceed)
        {
            Assert.Contains(
                "exactly one",
                response.Message,
                StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public async Task ExecuteAsync_RejectsPrivateKeyMaterial()
    {
        var response = await ExecuteCommandAsync(
            "--subscription",
            "sub123",
            "--resource-group",
            "rg1",
            "--service",
            "dps1",
            "--enrollment-id",
            "device1",
            "--certificate-chain",
            "-----BEGIN PRIVATE KEY-----");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("private-key", response.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_RejectsInvalidTpmEndorsementKey()
    {
        var response = await ExecuteCommandAsync(
            "--subscription",
            "sub123",
            "--resource-group",
            "rg1",
            "--service",
            "dps1",
            "--enrollment-id",
            "device1",
            "--tpm-endorsement-key",
            "not-base64");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("Base64", response.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_DefaultsRegistrationStateForIndividualEnrollment()
    {
        DeviceProvisioningOnboardingValidationRequest? capturedRequest = null;
        Service.ValidateOnboarding(
                Arg.Do<DeviceProvisioningOnboardingValidationRequest>(request => capturedRequest = request),
                Arg.Any<CancellationToken>())
            .Returns(CreateResult());

        var response = await ExecuteCommandAsync(
            "--subscription",
            "sub123",
            "--resource-group",
            "rg1",
            "--service",
            "dps1",
            "--enrollment-id",
            "device1");

        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(capturedRequest);
        Assert.Equal("device1", capturedRequest.RegistrationId);
        Assert.True(capturedRequest.IncludeRegistrationState);
    }

    [Fact]
    public async Task ExecuteAsync_DeserializesValidationResponse()
    {
        Service.ValidateOnboarding(
                Arg.Any<DeviceProvisioningOnboardingValidationRequest>(),
                Arg.Any<CancellationToken>())
            .Returns(CreateResult());

        var response = await ExecuteCommandAsync(
            "--subscription",
            "sub123",
            "--resource-group",
            "rg1",
            "--service",
            "dps1",
            "--enrollment-group-id",
            "group1");

        var result = ValidateAndDeserializeResponse(
            response,
            Commands.DeviceProvisioningJsonContext.Default.DeviceProvisioningOnboardingValidateCommandResult);
        Assert.Equal("passed", result.Validation.ValidationResult);
        Assert.Equal("complete", result.Validation.ValidationCompleteness);
        Assert.False(result.AreResultsTruncated);
        Assert.False(result.FullDeviceDetailsIncluded);
    }

    [Fact]
    public async Task ExecuteAsync_OmitsRoutineDeviceDetailsByDefault()
    {
        Service.ValidateOnboarding(
                Arg.Any<DeviceProvisioningOnboardingValidationRequest>(),
                Arg.Any<CancellationToken>())
            .Returns(CreateResultWithDeviceDetails());

        var response = await ExecuteCommandAsync(
            "--subscription",
            "sub123",
            "--resource-group",
            "rg1",
            "--service",
            "dps1",
            "--enrollment-group-id",
            "group1");

        var result = ValidateAndDeserializeResponse(
            response,
            Commands.DeviceProvisioningJsonContext.Default.DeviceProvisioningOnboardingValidateCommandResult);
        Assert.Null(result.Validation.DeviceResults);
        Assert.False(result.FullDeviceDetailsIncluded);
    }

    [Fact]
    public async Task ExecuteAsync_IncludesDeviceDetailsWhenRequested()
    {
        Service.ValidateOnboarding(
                Arg.Any<DeviceProvisioningOnboardingValidationRequest>(),
                Arg.Any<CancellationToken>())
            .Returns(CreateResultWithDeviceDetails());

        var response = await ExecuteCommandAsync(
            "--subscription",
            "sub123",
            "--resource-group",
            "rg1",
            "--service",
            "dps1",
            "--enrollment-group-id",
            "group1",
            "--include-device-details",
            "true");

        var result = ValidateAndDeserializeResponse(
            response,
            Commands.DeviceProvisioningJsonContext.Default.DeviceProvisioningOnboardingValidateCommandResult);
        Assert.Equal(2, result.Validation.DeviceResults?.Count);
        Assert.True(result.FullDeviceDetailsIncluded);
    }

    [Fact]
    public async Task ExecuteAsync_KeepsExceptionalDeviceDetailsInCompactResponse()
    {
        var detailedResult = CreateResultWithDeviceDetails();
        var blockedDevice = CreateDeviceResult("device2") with
        {
            OverallReadiness = "blocked",
            ValidationResult = "issuesFound",
            BlockerCount = 1,
            Findings =
            [
                new(
                    "x509.registrationIdentity",
                    "blocker",
                    "The certificate identity does not match the registration ID.")
            ]
        };
        Service.ValidateOnboarding(
                Arg.Any<DeviceProvisioningOnboardingValidationRequest>(),
                Arg.Any<CancellationToken>())
            .Returns(detailedResult with
            {
                OverallReadiness = "blocked",
                BlockerCount = 1,
                ReadyDeviceCount = 1,
                BlockedDeviceCount = 1,
                DeviceResults =
                [
                    CreateDeviceResult("device1"),
                    blockedDevice
                ]
            });

        var response = await ExecuteCommandAsync(
            "--subscription",
            "sub123",
            "--resource-group",
            "rg1",
            "--service",
            "dps1",
            "--enrollment-group-id",
            "group1");

        var result = ValidateAndDeserializeResponse(
            response,
            Commands.DeviceProvisioningJsonContext.Default.DeviceProvisioningOnboardingValidateCommandResult);
        var compactDeviceResult = Assert.Single(result.Validation.DeviceResults!);
        Assert.Equal("device2", compactDeviceResult.RegistrationId);
        Assert.False(result.FullDeviceDetailsIncluded);
    }

    [Fact]
    public async Task ExecuteAsync_ParsesDeviceManifestAndDefaultsRegistrationState()
    {
        DeviceProvisioningOnboardingValidationRequest? capturedRequest = null;
        Service.ValidateOnboarding(
                Arg.Do<DeviceProvisioningOnboardingValidationRequest>(request => capturedRequest = request),
                Arg.Any<CancellationToken>())
            .Returns(CreateResult());

        var response = await ExecuteCommandAsync(
            "--subscription",
            "sub123",
            "--resource-group",
            "rg1",
            "--service",
            "dps1",
            "--enrollment-group-id",
            "group1",
            "--device-manifest",
            """{"devices":[{"registrationId":"device1"},{"registrationId":"device2"}]}""");

        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(capturedRequest);
        Assert.True(capturedRequest.IncludeRegistrationState);
        Assert.Null(capturedRequest.RegistrationId);
        Assert.Equal(2, capturedRequest.Devices?.Count);
        Assert.Equal("device1", capturedRequest.Devices?[0].RegistrationId);
    }

    [Fact]
    public async Task ExecuteAsync_RejectsMoreThanTenManifestDevices()
    {
        var manifest = $$"""{"devices":[{{string.Join(",", Enumerable.Range(1, 11).Select(index => $$"""{"registrationId":"device{{index}}"}"""))}}]}""";

        var response = await ExecuteCommandAsync(
            "--subscription",
            "sub123",
            "--resource-group",
            "rg1",
            "--service",
            "dps1",
            "--enrollment-group-id",
            "group1",
            "--device-manifest",
            manifest);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("Received 11 devices", response.Message, StringComparison.Ordinal);
        Assert.Contains("No devices were assessed", response.Message, StringComparison.Ordinal);
        Assert.Contains("batches of 10 or fewer", response.Message, StringComparison.Ordinal);
        Assert.Contains(
            "azure-device-provisioning-onboarding",
            response.Message,
            StringComparison.Ordinal);
        await Service.DidNotReceive().ValidateOnboarding(
            Arg.Any<DeviceProvisioningOnboardingValidationRequest>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_RejectsManifestCombinedWithSingleDeviceEvidence()
    {
        var response = await ExecuteCommandAsync(
            "--subscription",
            "sub123",
            "--resource-group",
            "rg1",
            "--service",
            "dps1",
            "--enrollment-group-id",
            "group1",
            "--device-manifest",
            """{"devices":[{"registrationId":"device1"}]}""",
            "--registration-id",
            "device1");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("cannot be combined", response.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_RejectsDuplicateManifestRegistrationIds()
    {
        var response = await ExecuteCommandAsync(
            "--subscription",
            "sub123",
            "--resource-group",
            "rg1",
            "--service",
            "dps1",
            "--enrollment-group-id",
            "group1",
            "--device-manifest",
            """{"devices":[{"registrationId":"device1"},{"registrationId":"DEVICE1"}]}""");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("duplicate", response.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_RejectsPrivateKeyInDeviceManifest()
    {
        var response = await ExecuteCommandAsync(
            "--subscription",
            "sub123",
            "--resource-group",
            "rg1",
            "--service",
            "dps1",
            "--enrollment-group-id",
            "group1",
            "--device-manifest",
            """{"devices":[{"registrationId":"device1","certificateChain":"-----BEGIN PRIVATE KEY-----"}]}""");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("private-key", response.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_RequiresIndividualManifestIdentityToMatchEnrollment()
    {
        var response = await ExecuteCommandAsync(
            "--subscription",
            "sub123",
            "--resource-group",
            "rg1",
            "--service",
            "dps1",
            "--enrollment-id",
            "device1",
            "--device-manifest",
            """{"devices":[{"registrationId":"device2"}]}""");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("matches --enrollment-id", response.Message, StringComparison.OrdinalIgnoreCase);
    }

    private static OnboardingValidationResult CreateResult() =>
        new(
            ValidationResult: "passed",
            ValidationCompleteness: "complete",
            TargetType: "individualEnrollment",
            TargetId: "device1",
            ServiceName: "dps1",
            CustomerSummary: "Readiness: READY",
            IdScope: "0ne00000001",
            ServiceState: "Active",
            ProvisioningState: "Succeeded",
            AttestationType: "x509",
            EffectiveAllocationPolicy: "hashed",
            PassedCount: 1,
            BlockerCount: 0,
            WarningCount: 0,
            InformationalCount: 0,
            UnverifiedCount: 0,
            Findings:
            [
                new("enrollment.enabled", "passed", "The selected enrollment is enabled.")
            ],
            ExecutionLimitations: [],
            RegistrationState: null,
            Limitations: []);

    private static OnboardingValidationResult CreateResultWithDeviceDetails() =>
        CreateResult() with
        {
            OverallReadiness = "ready",
            DeviceCount = 2,
            ReadyDeviceCount = 2,
            ReadyWithWarningsDeviceCount = 0,
            BlockedDeviceCount = 0,
            DeviceResults =
            [
                CreateDeviceResult("device1"),
                CreateDeviceResult("device2")
            ]
        };

    private static OnboardingDeviceValidationResult CreateDeviceResult(string registrationId) =>
        new(
            RegistrationId: registrationId,
            OverallReadiness: "ready",
            ValidationResult: "passed",
            ValidationCompleteness: "complete",
            PassedCount: 1,
            BlockerCount: 0,
            WarningCount: 0,
            InformationalCount: 0,
            UnverifiedCount: 0,
            Findings:
            [
                new("device.assessed", "passed", "The device was assessed.")
            ],
            ExecutionLimitations: [],
            RegistrationState: null,
            Limitations: []);
}
