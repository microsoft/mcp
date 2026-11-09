// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using Azure.Core;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Tools.DeviceProvisioning.Models;
using Azure.Mcp.Tools.DeviceProvisioning.Services;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.DeviceProvisioning.Tests.Services;

public sealed class DeviceProvisioningOnboardingServiceTests
{
    [Fact]
    public async Task ValidateOnboarding_ReturnsPassedForConsistentEnrollment()
    {
        var service = CreateService(request =>
            request.RequestUri!.AbsolutePath.StartsWith("/enrollments/", StringComparison.Ordinal)
                ? JsonResponse(HttpStatusCode.OK, CreateEnrollmentJson())
                : throw new InvalidOperationException($"Unexpected path {request.RequestUri.AbsolutePath}"));

        var result = await service.ValidateOnboarding(
            CreateRequest(includeRegistrationState: false),
            TestContext.Current.CancellationToken);

        Assert.Equal("passed", result.ValidationResult);
        Assert.Equal("complete", result.ValidationCompleteness);
        Assert.Equal(0, result.BlockerCount);
        Assert.Contains(result.Findings, finding =>
            finding.CheckId == "allocation.linkedHubConsistency" &&
            finding.Status == "passed");
    }

    [Fact]
    public async Task ValidateOnboarding_TreatsOmittedPublicNetworkAccessAsEnabledDefault()
    {
        var service = CreateService(
            request =>
                JsonResponse(HttpStatusCode.OK, CreateEnrollmentJson()),
            publicNetworkAccess: string.Empty);

        var result = await service.ValidateOnboarding(
            CreateRequest(includeRegistrationState: false),
            TestContext.Current.CancellationToken);

        Assert.Equal("passed", result.ValidationResult);
        Assert.Equal("complete", result.ValidationCompleteness);
        Assert.Contains(result.Findings, finding =>
            finding.CheckId == "dps.network.publicAccess" &&
            finding.Status == "passed" &&
            finding.Message.Contains("platform default", StringComparison.Ordinal));
    }

    [Fact]
    public async Task ValidateOnboarding_TreatsEnrollmentAuthorizationFailureAsIncomplete()
    {
        var service = CreateService(_ =>
            JsonResponse(HttpStatusCode.Forbidden, """{"error":"forbidden"}"""));

        var result = await service.ValidateOnboarding(
            CreateRequest(includeRegistrationState: false),
            TestContext.Current.CancellationToken);

        Assert.Equal("indeterminate", result.ValidationResult);
        Assert.Equal("partial", result.ValidationCompleteness);
        Assert.Equal(0, result.BlockerCount);
        Assert.Contains(result.ExecutionLimitations, limitation =>
            limitation.Operation == "enrollment.read" &&
            limitation.Reason == "authorizationDenied");
    }

    [Fact]
    public async Task ValidateOnboarding_TreatsAuthorizedNotFoundAsBlocker()
    {
        var service = CreateService(_ =>
            JsonResponse(HttpStatusCode.NotFound, """{"error":"not found"}"""));

        var result = await service.ValidateOnboarding(
            CreateRequest(includeRegistrationState: false),
            TestContext.Current.CancellationToken);

        Assert.Equal("issuesFound", result.ValidationResult);
        Assert.Equal("complete", result.ValidationCompleteness);
        Assert.Equal(1, result.BlockerCount);
        Assert.Empty(result.ExecutionLimitations);
    }

    [Fact]
    public async Task ValidateOnboarding_DetectsUnlinkedHub()
    {
        var service = CreateService(_ =>
            JsonResponse(
                HttpStatusCode.OK,
                CreateEnrollmentJson(iotHub: "unlinked.azure-devices.net")));

        var result = await service.ValidateOnboarding(
            CreateRequest(includeRegistrationState: false),
            TestContext.Current.CancellationToken);

        Assert.Equal("issuesFound", result.ValidationResult);
        Assert.Contains(result.Findings, finding =>
            finding.CheckId == "allocation.linkedHubConsistency" &&
            finding.Status == "blocker");
    }

    [Fact]
    public async Task ValidateOnboarding_RejectsAllocationDisabledLinkedHub()
    {
        var service = CreateService(
            _ => JsonResponse(HttpStatusCode.OK, CreateEnrollmentJson()),
            hub1AllocationEnabled: false);
        var request = CreateRequest(includeRegistrationState: false) with
        {
            IntendedHub = "hub1.azure-devices.net"
        };

        var result = await service.ValidateOnboarding(
            request,
            TestContext.Current.CancellationToken);

        Assert.Equal("issuesFound", result.ValidationResult);
        Assert.Contains(result.Findings, finding =>
            finding.CheckId == "allocation.linkedHubConsistency" &&
            finding.Status == "blocker" &&
            finding.Message.Contains("disabled for DPS allocation", StringComparison.Ordinal));
        Assert.Contains(result.Findings, finding =>
            finding.CheckId == "allocation.intendedHubConsistency" &&
            finding.Status == "blocker" &&
            finding.Message.Contains("disabled for allocation", StringComparison.Ordinal));
    }

    [Fact]
    public async Task ValidateOnboarding_DetectsUnlinkedIntendedHubWithoutRegistrationState()
    {
        var service = CreateService(_ =>
            JsonResponse(HttpStatusCode.OK, CreateEnrollmentJson()));
        var request = CreateRequest(includeRegistrationState: false) with
        {
            IntendedHub = "hub1.azure-devices.net.HOSTNAME"
        };

        var result = await service.ValidateOnboarding(
            request,
            TestContext.Current.CancellationToken);

        Assert.Equal("issuesFound", result.ValidationResult);
        Assert.Contains(result.Findings, finding =>
            finding.CheckId == "allocation.intendedHubConsistency" &&
            finding.Status == "blocker" &&
            finding.Message.Contains("not linked", StringComparison.Ordinal));
    }

    [Fact]
    public async Task ValidateOnboarding_DetectsIntendedHubNotTargetedByEnrollment()
    {
        var service = CreateService(_ =>
            JsonResponse(HttpStatusCode.OK, CreateEnrollmentJson()));
        var request = CreateRequest(includeRegistrationState: false) with
        {
            IntendedHub = "hub2.azure-devices.net"
        };

        var result = await service.ValidateOnboarding(
            request,
            TestContext.Current.CancellationToken);

        Assert.Equal("issuesFound", result.ValidationResult);
        Assert.Contains(result.Findings, finding =>
            finding.CheckId == "allocation.intendedHubConsistency" &&
            finding.Status == "blocker" &&
            finding.Message.Contains("not targeted", StringComparison.Ordinal));
    }

    [Fact]
    public async Task ValidateOnboarding_AcceptsIntendedHubWithoutRegistrationState()
    {
        var service = CreateService(_ =>
            JsonResponse(HttpStatusCode.OK, CreateEnrollmentJson()));
        var request = CreateRequest(includeRegistrationState: false) with
        {
            IntendedHub = "hub1.azure-devices.net"
        };

        var result = await service.ValidateOnboarding(
            request,
            TestContext.Current.CancellationToken);

        Assert.Equal("passed", result.ValidationResult);
        Assert.Contains(result.Findings, finding =>
            finding.CheckId == "allocation.intendedHubConsistency" &&
            finding.Status == "passed");
    }

    [Fact]
    public async Task ValidateOnboarding_RegistrationStateDeniedMakesResultPartial()
    {
        var service = CreateService(request =>
        {
            if (request.RequestUri!.AbsolutePath.StartsWith("/enrollments/", StringComparison.Ordinal))
            {
                return JsonResponse(HttpStatusCode.OK, CreateEnrollmentJson());
            }

            if (request.RequestUri.AbsolutePath.StartsWith("/registrations/", StringComparison.Ordinal))
            {
                return JsonResponse(HttpStatusCode.Forbidden, """{"error":"forbidden"}""");
            }

            throw new InvalidOperationException($"Unexpected path {request.RequestUri.AbsolutePath}");
        });

        var result = await service.ValidateOnboarding(
            CreateRequest(includeRegistrationState: true),
            TestContext.Current.CancellationToken);

        Assert.Equal("passed", result.ValidationResult);
        Assert.Equal("partial", result.ValidationCompleteness);
        Assert.Contains(result.ExecutionLimitations, limitation =>
            limitation.Operation == "registrationState.read");
    }

    [Fact]
    public async Task ValidateOnboarding_AssignedHubOnlyWarnsWhenIntentDiffers()
    {
        var service = CreateService(request =>
            request.RequestUri!.AbsolutePath.StartsWith("/enrollments/", StringComparison.Ordinal)
                ? JsonResponse(
                    HttpStatusCode.OK,
                    CreateEnrollmentJson(iotHub: "hub2.azure-devices.net"))
                : JsonResponse(
                    HttpStatusCode.OK,
                    """
                    {
                      "registrationId": "device1",
                      "assignedHub": "hub1.azure-devices.net",
                      "deviceId": "device1",
                      "status": "assigned",
                      "substatus": "initialAssignment"
                    }
                    """));

        var request = CreateRequest(includeRegistrationState: true) with
        {
            IntendedHub = "hub2.azure-devices.net"
        };
        var result = await service.ValidateOnboarding(
            request,
            TestContext.Current.CancellationToken);

        Assert.Contains(result.Findings, finding =>
            finding.CheckId == "registrationState.assignedHubIntent" &&
            finding.Status == "warning");
        Assert.Contains(
            "| Hub assignment | `hub2.azure-devices.net` | 0/1 matches, 1 mismatches |",
            result.CustomerSummary!,
            StringComparison.Ordinal);
    }

    [Fact]
    public async Task ValidateOnboarding_ValidatesTenDevicesAndAggregatesReadiness()
    {
        var registrationReadCount = 0;
        var service = CreateService(request =>
        {
            if (request.RequestUri!.AbsolutePath.StartsWith("/enrollmentGroups/", StringComparison.Ordinal))
            {
                return JsonResponse(HttpStatusCode.OK, CreateEnrollmentJson());
            }

            if (request.RequestUri.AbsolutePath.StartsWith("/registrations/", StringComparison.Ordinal))
            {
                Interlocked.Increment(ref registrationReadCount);
                return JsonResponse(HttpStatusCode.NotFound, """{"error":"not found"}""");
            }

            throw new InvalidOperationException($"Unexpected path {request.RequestUri.AbsolutePath}");
        });
        var devices = Enumerable.Range(1, 10)
            .Select(index => new OnboardingDeviceEvidence
            {
                RegistrationId = $"device{index}"
            })
            .ToList();
        var request = CreateGroupRequest(devices);

        var result = await service.ValidateOnboarding(
            request,
            TestContext.Current.CancellationToken);

        Assert.Equal(10, registrationReadCount);
        Assert.Equal("passed", result.ValidationResult);
        Assert.Equal("partial", result.ValidationCompleteness);
        Assert.Equal("readyWithWarnings", result.OverallReadiness);
        Assert.Equal(10, result.DeviceCount);
        Assert.Equal(10, result.ReadyWithWarningsDeviceCount);
        Assert.Equal(0, result.BlockedDeviceCount);
        Assert.Equal(10, result.DeviceResults?.Count);
        Assert.Contains(
            "Registration: no stored state for 10/10 devices; expected before first provisioning",
            result.CustomerSummary!,
            StringComparison.Ordinal);
        var serializedResult = JsonSerializer.Serialize(result);
        Assert.True(
            serializedResult.IndexOf("\"CustomerSummary\"", StringComparison.Ordinal) <
            serializedResult.IndexOf("\"DeviceResults\"", StringComparison.Ordinal));
        Assert.All(result.DeviceResults!, device =>
        {
            Assert.Equal("readyWithWarnings", device.OverallReadiness);
            Assert.Contains(device.Findings, finding =>
                finding.CheckId == "device.retryReadiness" &&
                finding.Status == "unverified");
        });
    }

    [Fact]
    public async Task ValidateOnboarding_SummarizesPredictionAgainstAssignedFleet()
    {
        var service = CreateService(request =>
        {
            if (request.RequestUri!.AbsolutePath.StartsWith("/enrollmentGroups/", StringComparison.Ordinal))
            {
                return JsonResponse(
                    HttpStatusCode.OK,
                    CreateEnrollmentJson(iotHub: "hub1.azure-devices.net"));
            }

            if (request.RequestUri.AbsolutePath.StartsWith("/registrations/", StringComparison.Ordinal))
            {
                var registrationId = request.RequestUri.Segments[^1].Trim('/');
                return JsonResponse(
                    HttpStatusCode.OK,
                    $$"""
                    {
                      "registrationId": "{{registrationId}}",
                      "assignedHub": "hub1.azure-devices.net",
                      "deviceId": "{{registrationId}}",
                      "status": "assigned",
                      "substatus": "initialAssignment"
                    }
                    """);
            }

            throw new InvalidOperationException($"Unexpected path {request.RequestUri.AbsolutePath}");
        });
        var request = CreateGroupRequest(
            [
                new OnboardingDeviceEvidence { RegistrationId = "device1" },
                new OnboardingDeviceEvidence { RegistrationId = "device2" }
            ]) with
        {
            IntendedHub = "hub1.azure-devices.net"
        };

        var result = await service.ValidateOnboarding(
            request,
            TestContext.Current.CancellationToken);

        Assert.Contains(
            "| Comparison | Prediction | Actual |",
            result.CustomerSummary,
            StringComparison.Ordinal);
        Assert.Contains(
            "| Registration | Devices complete assignment | 2/2 assigned |",
            result.CustomerSummary,
            StringComparison.Ordinal);
        Assert.Contains(
            "| Hub assignment | `hub1.azure-devices.net` | 2/2 matches, 0 mismatches |",
            result.CustomerSummary,
            StringComparison.Ordinal);
    }

    [Fact]
    public async Task ValidateOnboarding_BlocksCohortWhenAnyDeviceEvidenceIsIncompatible()
    {
        var service = CreateService(request =>
            request.RequestUri!.AbsolutePath.StartsWith("/enrollmentGroups/", StringComparison.Ordinal)
                ? JsonResponse(
                    HttpStatusCode.OK,
                    CreateEnrollmentJson(
                        attestationJson:
                            """{"type":"tpm","tpm":{"endorsementKey":"YWJj"}}"""))
                : JsonResponse(HttpStatusCode.NotFound, """{"error":"not found"}"""));
        var request = CreateGroupRequest(
            [
                new OnboardingDeviceEvidence
                {
                    RegistrationId = "device1",
                    TpmEndorsementKey = "YWJj"
                },
                new OnboardingDeviceEvidence
                {
                    RegistrationId = "device2",
                    TpmEndorsementKey = "ZGVm"
                }
            ]);

        var result = await service.ValidateOnboarding(
            request,
            TestContext.Current.CancellationToken);

        Assert.Equal("issuesFound", result.ValidationResult);
        Assert.Equal("blocked", result.OverallReadiness);
        Assert.Equal(1, result.BlockedDeviceCount);
        Assert.Contains(result.DeviceResults!, device =>
            device.RegistrationId == "device2" &&
            device.OverallReadiness == "blocked" &&
            device.Findings.Any(finding =>
                finding.CheckId == "tpm.selectedEnrollmentCompatibility" &&
                finding.Status == "blocker"));
    }

    [Fact]
    public async Task ValidateOnboarding_RecordsRegistrationAuthorizationPerDevice()
    {
        var service = CreateService(request =>
            request.RequestUri!.AbsolutePath.StartsWith("/enrollmentGroups/", StringComparison.Ordinal)
                ? JsonResponse(HttpStatusCode.OK, CreateEnrollmentJson())
                : JsonResponse(HttpStatusCode.Forbidden, """{"error":"forbidden"}"""));
        var request = CreateGroupRequest(
            [
                new OnboardingDeviceEvidence
                {
                    RegistrationId = "device1"
                }
            ]);

        var result = await service.ValidateOnboarding(
            request,
            TestContext.Current.CancellationToken);

        var deviceResult = Assert.Single(result.DeviceResults!);
        Assert.Equal("readyWithWarnings", deviceResult.OverallReadiness);
        Assert.Equal("partial", deviceResult.ValidationCompleteness);
        Assert.Contains(deviceResult.ExecutionLimitations, limitation =>
            limitation.Operation == "registrationState.read" &&
            limitation.Reason == "authorizationDenied");
    }

    [Fact]
    public async Task ValidateOnboarding_ResolvesVerifiedCaAndTreatsMissingStateAsPreProvisioning()
    {
        var certificates = CreateDirectCertificateChain("device1");
        using var root = certificates.Root;
        using var leaf = certificates.Leaf;
        var certificateDescription = new DeviceProvisioningCertificateDescription(
            "root-ca",
            root.ExportCertificatePem(),
            IsVerified: true,
            root.Subject,
            root.Thumbprint);
        var service = CreateService(
            request =>
                request.RequestUri!.AbsolutePath.StartsWith(
                    "/enrollmentGroups/",
                    StringComparison.Ordinal)
                    ? JsonResponse(
                        HttpStatusCode.OK,
                        CreateEnrollmentJson(
                            attestationJson:
                                """
                                {
                                  "type": "x509",
                                  "x509": {
                                    "caReferences": {
                                      "primary": "root-ca"
                                    }
                                  }
                                }
                                """))
                    : JsonResponse(HttpStatusCode.NotFound, """{"error":"not found"}"""),
            certificate: certificateDescription);
        var request = CreateGroupRequest(
            [
                new OnboardingDeviceEvidence
                {
                    RegistrationId = "device1",
                    CertificateChain = string.Join(
                        Environment.NewLine,
                        leaf.ExportCertificatePem(),
                        root.ExportCertificatePem())
                }
            ]);

        var result = await service.ValidateOnboarding(
            request,
            TestContext.Current.CancellationToken);

        Assert.Equal("passed", result.ValidationResult);
        var deviceResult = Assert.Single(result.DeviceResults!);
        Assert.Equal("readyWithWarnings", deviceResult.OverallReadiness);
        Assert.Contains(deviceResult.Findings, finding =>
            finding.CheckId == "x509.chainIntegrity" &&
            finding.Status == "passed");
        Assert.Contains(deviceResult.Findings, finding =>
            finding.CheckId == "registrationState.available" &&
            finding.Status == "informational" &&
            finding.Message.Contains("normal before", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(
            "Readiness: READY WITH WARNINGS",
            result.CustomerSummary!,
            StringComparison.Ordinal);
        Assert.Contains(
            "Device SDK and firmware retry behavior was not assessed.",
            result.CustomerSummary!,
            StringComparison.Ordinal);
    }

    [Fact]
    public async Task ValidateOnboarding_BlocksConfigurationOnlyX509WhenReferencedCaIsMissing()
    {
        var service = CreateService(
            _ => JsonResponse(
                HttpStatusCode.OK,
                CreateEnrollmentJson(
                    attestationJson:
                        """
                        {
                          "type": "x509",
                          "x509": {
                            "caReferences": {
                              "primary": "missing-ca"
                            }
                          }
                        }
                        """)),
            certificateFailureStatus: HttpStatusCode.NotFound);

        var result = await service.ValidateOnboarding(
            CreateRequest(includeRegistrationState: false),
            TestContext.Current.CancellationToken);

        Assert.Equal("issuesFound", result.ValidationResult);
        Assert.Contains(result.Findings, finding =>
            finding.CheckId == "x509.caReferences.resolved" &&
            finding.Status == "blocker" &&
            finding.Message.Contains("missing-ca", StringComparison.Ordinal));
    }

    [Fact]
    public async Task ValidateOnboarding_BlocksConfigurationOnlyX509WhenReferencedCaIsUnverified()
    {
        var certificate = new DeviceProvisioningCertificateDescription(
            "unverified-ca",
            string.Empty,
            IsVerified: false,
            "CN=unverified-ca",
            "thumbprint");
        var service = CreateService(
            _ => JsonResponse(
                HttpStatusCode.OK,
                CreateEnrollmentJson(
                    attestationJson:
                        """
                        {
                          "type": "x509",
                          "x509": {
                            "caReferences": {
                              "primary": "unverified-ca"
                            }
                          }
                        }
                        """)),
            certificate: certificate);

        var result = await service.ValidateOnboarding(
            CreateRequest(includeRegistrationState: false),
            TestContext.Current.CancellationToken);

        Assert.Equal("issuesFound", result.ValidationResult);
        Assert.Contains(result.Findings, finding =>
            finding.CheckId == "x509.caCertificates.verified" &&
            finding.Status == "blocker" &&
            finding.Message.Contains("unverified-ca", StringComparison.Ordinal));
    }

    private static DeviceProvisioningOnboardingService CreateService(
        Func<HttpRequestMessage, HttpResponseMessage> responseFactory,
        string publicNetworkAccess = "Enabled",
        DeviceProvisioningCertificateDescription? certificate = null,
        bool hub1AllocationEnabled = true,
        HttpStatusCode? certificateFailureStatus = null)
    {
        var managementService = Substitute.For<IDeviceProvisioningService>();
        managementService.GetService(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string?>(),
                Arg.Any<CancellationToken>())
            .Returns(CreateServiceDescription(
                publicNetworkAccess,
                hub1AllocationEnabled));
        if (certificate is not null)
        {
            managementService.GetCertificate(
                    Arg.Any<string>(),
                    Arg.Any<string>(),
                    Arg.Any<string>(),
                    Arg.Any<string>(),
                    Arg.Any<string?>(),
                    Arg.Any<CancellationToken>())
                .Returns(certificate);
        }
        else if (certificateFailureStatus.HasValue)
        {
            managementService.GetCertificate(
                    Arg.Any<string>(),
                    Arg.Any<string>(),
                    Arg.Any<string>(),
                    Arg.Any<string>(),
                    Arg.Any<string?>(),
                    Arg.Any<CancellationToken>())
                .Returns(_ => Task.FromException<DeviceProvisioningCertificateDescription>(
                    new RequestFailedException(
                        (int)certificateFailureStatus.Value,
                        "Certificate read failed.")));
        }

        var credential = Substitute.For<TokenCredential>();
        credential.GetTokenAsync(
                Arg.Any<TokenRequestContext>(),
                Arg.Any<CancellationToken>())
            .Returns(new ValueTask<AccessToken>(
                new AccessToken("test-token", DateTimeOffset.UtcNow.AddHours(1))));

        var azureService = Substitute.For<IAzureService>();
        azureService.GetTokenCredentialAsync(
                Arg.Any<string?>(),
                Arg.Any<CancellationToken>())
            .Returns(credential);

        var httpClientFactory = Substitute.For<IHttpClientFactory>();
        httpClientFactory.CreateClient(Arg.Any<string>())
            .Returns(_ => new HttpClient(
                new DpsTestHttpMessageHandler(responseFactory),
                disposeHandler: true));

        return new DeviceProvisioningOnboardingService(
            managementService,
            httpClientFactory,
            azureService,
            Substitute.For<ILogger<DeviceProvisioningOnboardingService>>());
    }

    private static DeviceProvisioningOnboardingValidationRequest CreateRequest(
        bool includeRegistrationState) =>
        new(
            ServiceName: "dps1",
            ResourceGroup: "rg1",
            Subscription: "sub123",
            Tenant: null,
            EnrollmentId: "device1",
            EnrollmentGroupId: null,
            RegistrationId: "device1",
            IntendedHub: null,
            CertificateChain: null,
            TpmEndorsementKey: null,
            IncludeRegistrationState: includeRegistrationState);

    private static DeviceProvisioningOnboardingValidationRequest CreateGroupRequest(
        List<OnboardingDeviceEvidence> devices) =>
        new(
            ServiceName: "dps1",
            ResourceGroup: "rg1",
            Subscription: "sub123",
            Tenant: null,
            EnrollmentId: null,
            EnrollmentGroupId: "group1",
            RegistrationId: null,
            IntendedHub: null,
            CertificateChain: null,
            TpmEndorsementKey: null,
            IncludeRegistrationState: true,
            Devices: devices);

    private static DeviceProvisioningServiceDescription CreateServiceDescription(
        string publicNetworkAccess,
        bool hub1AllocationEnabled = true) =>
        new(
            Id: "/subscriptions/sub123/resourceGroups/rg1/providers/Microsoft.Devices/provisioningServices/dps1",
            Name: "dps1",
            Location: "eastus",
            ResourceGroup: "rg1",
            SubscriptionId: "sub123",
            Sku: "S1",
            Capacity: 1,
            State: "Active",
            ProvisioningState: "Succeeded",
            ServiceOperationsHostName: "dps1.azure-devices-provisioning.net",
            DeviceProvisioningHostName: "global.azure-devices-provisioning.net",
            IdScope: "0ne00000001",
            AllocationPolicy: "hashed",
            PublicNetworkAccess: publicNetworkAccess,
            EnableDataResidency: false,
            IotHubs:
            [
                new LinkedIoTHubDescription(
                    "hub1.azure-devices.net",
                    "eastus",
                    hub1AllocationEnabled,
                    1),
                new LinkedIoTHubDescription(
                    "hub2.azure-devices.net",
                    "eastus",
                    true,
                    1)
            ]);

    private static string CreateEnrollmentJson(
        string iotHub = "hub1.azure-devices.net",
        string attestationJson = """{"type":"symmetricKey"}""") =>
        $$"""
        {
          "registrationId": "device1",
          "provisioningStatus": "enabled",
          "allocationPolicy": "static",
          "iotHubs": ["{{iotHub}}"],
          "attestation": {{attestationJson}},
          "reprovisionPolicy": {
            "updateHubAssignment": true,
            "migrateDeviceData": true
          }
        }
        """;

    private static HttpResponseMessage JsonResponse(
        HttpStatusCode statusCode,
        string content) =>
        new(statusCode)
        {
            Content = new StringContent(
                content,
                System.Text.Encoding.UTF8,
                "application/json")
        };

    private static (X509Certificate2 Root, X509Certificate2 Leaf)
        CreateDirectCertificateChain(string leafCommonName)
    {
        using var rootKey = RSA.Create(2048);
        var rootRequest = new CertificateRequest(
            "CN=root-ca",
            rootKey,
            HashAlgorithmName.SHA256,
            RSASignaturePadding.Pkcs1);
        rootRequest.CertificateExtensions.Add(
            new X509BasicConstraintsExtension(
                certificateAuthority: true,
                hasPathLengthConstraint: false,
                pathLengthConstraint: 0,
                critical: true));
        rootRequest.CertificateExtensions.Add(
            new X509KeyUsageExtension(
                X509KeyUsageFlags.KeyCertSign | X509KeyUsageFlags.CrlSign,
                critical: true));
        var root = rootRequest.CreateSelfSigned(
            DateTimeOffset.UtcNow.AddDays(-1),
            DateTimeOffset.UtcNow.AddDays(90));

        using var leafKey = RSA.Create(2048);
        var leafRequest = new CertificateRequest(
            $"CN={leafCommonName}",
            leafKey,
            HashAlgorithmName.SHA256,
            RSASignaturePadding.Pkcs1);
        leafRequest.CertificateExtensions.Add(
            new X509BasicConstraintsExtension(
                certificateAuthority: false,
                hasPathLengthConstraint: false,
                pathLengthConstraint: 0,
                critical: true));
        leafRequest.CertificateExtensions.Add(
            new X509KeyUsageExtension(
                X509KeyUsageFlags.DigitalSignature,
                critical: true));
        var serialNumber = RandomNumberGenerator.GetBytes(16);
        serialNumber[0] &= 0x7F;
        var leaf = leafRequest.Create(
            root,
            DateTimeOffset.UtcNow.AddDays(-1),
            DateTimeOffset.UtcNow.AddDays(60),
            serialNumber);

        return (root, leaf);
    }
}
