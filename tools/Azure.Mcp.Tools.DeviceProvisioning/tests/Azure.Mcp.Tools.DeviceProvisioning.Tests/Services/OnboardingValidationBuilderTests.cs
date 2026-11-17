// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.DeviceProvisioning.Models;
using Azure.Mcp.Tools.DeviceProvisioning.Services;
using Xunit;

namespace Azure.Mcp.Tools.DeviceProvisioning.Tests.Services;

public sealed class OnboardingValidationBuilderTests
{
    [Fact]
    public void Build_PreservesIndeterminateSummaryWhenDeviceResultsExist()
    {
        var builder = new OnboardingValidationBuilder(
            "enrollmentGroup",
            "group1",
            "dps1");
        builder.AddDeviceResults(
        [
            new OnboardingDeviceValidationResult(
                "device1",
                "readyWithWarnings",
                "passed",
                "partial",
                0,
                0,
                0,
                0,
                1,
                [new("device.identityEvidence", "unverified", "Evidence was not supplied.")],
                [],
                null,
                [])
        ]);

        var result = builder.Build(essentialCheckIncomplete: true);

        Assert.Equal("indeterminate", result.ValidationResult);
        Assert.StartsWith(
            "Readiness: INDETERMINATE",
            result.CustomerSummary,
            StringComparison.Ordinal);
    }

    [Fact]
    public void Build_PreservesCertificateValidityBlockerReason()
    {
        var builder = new OnboardingValidationBuilder(
            "individualEnrollment",
            "device1",
            "dps1");
        builder.AddFinding(
            "x509.certificateValidity",
            "blocker",
            "The leaf certificate is not valid before tomorrow.");

        var result = builder.Build();

        Assert.Contains(
            "The leaf certificate is not valid before tomorrow.",
            result.CustomerSummary,
            StringComparison.Ordinal);
        Assert.DoesNotContain(
            "expire within 30 days",
            result.CustomerSummary,
            StringComparison.Ordinal);
    }
}
