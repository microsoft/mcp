// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.AzureBackup.Services;
using Microsoft.Mcp.Core.Commands;
using Xunit;

namespace Azure.Mcp.Tools.AzureBackup.Tests.Services;

public class BackupJobIdParserTests
{
    private const string JobGuid = "9f1c2a3b-0000-4000-8000-123456789abc";
    private const string Sub = "00000000-0000-0000-0000-000000000001";
    private const string OtherSub = "11111111-1111-1111-1111-111111111111";
    private const string RsvJobId = $"/subscriptions/{Sub}/resourceGroups/rg1/providers/Microsoft.RecoveryServices/vaults/vault1/backupJobs/{JobGuid}";
    private const string DppJobId = $"/subscriptions/{Sub}/resourceGroups/rg1/providers/Microsoft.DataProtection/backupVaults/vault1/backupJobs/{JobGuid}";

    [Theory]
    [InlineData(JobGuid)]
    [InlineData(" " + JobGuid + " ")]
    [InlineData(JobGuid + "/")]
    [InlineData("/" + JobGuid)]
    [InlineData("backupJobs/" + JobGuid)]
    [InlineData("BackupJobs/" + JobGuid + "/")]
    public void Parse_AcceptsJobNameForms(string job)
    {
        var result = new ValidationResult();

        var reference = BackupJobIdParser.Parse(job, Sub, "vault1", "rg1", null, result);

        Assert.True(result.IsValid);
        Assert.NotNull(reference);
        Assert.Equal(JobGuid, reference.JobName);
        Assert.Null(reference.VaultType);
        Assert.Null(reference.SubscriptionId);
    }

    [Theory]
    [InlineData(RsvJobId, "rsv")]
    [InlineData(RsvJobId + "/", "rsv")]
    [InlineData(DppJobId, "dpp")]
    public void Parse_FullArmJobId_ReturnsNameSubscriptionAndVaultType(string job, string expectedVaultType)
    {
        var result = new ValidationResult();

        var reference = BackupJobIdParser.Parse(job, Sub, "VAULT1", "RG1", null, result);

        Assert.True(result.IsValid);
        Assert.NotNull(reference);
        Assert.Equal(JobGuid, reference.JobName);
        Assert.Equal(expectedVaultType, reference.VaultType);
        Assert.Equal(Sub, reference.SubscriptionId);
    }

    [Fact]
    public void Parse_FullArmJobId_WithMatchingVaultType_Succeeds()
    {
        var result = new ValidationResult();

        var reference = BackupJobIdParser.Parse(DppJobId, Sub, "vault1", "rg1", "DPP", result);

        Assert.True(result.IsValid);
        Assert.Equal("dpp", reference!.VaultType);
    }

    [Theory]
    [InlineData(OtherSub, "vault1", "rg1", null, "--subscription", BackupJobIdParser.SubscriptionMismatchTelemetryMessage)]
    [InlineData(Sub, "otherVault", "rg1", null, "--vault", BackupJobIdParser.VaultMismatchTelemetryMessage)]
    [InlineData(Sub, "vault1", "otherRg", null, "--resource-group", BackupJobIdParser.ResourceGroupMismatchTelemetryMessage)]
    [InlineData(Sub, "vault1", "rg1", "dpp", "--vault-type", BackupJobIdParser.VaultTypeMismatchTelemetryMessage)]
    public void Parse_FullArmJobId_SingleMismatch_AddsErrorWithTelemetrySafeMessage(
        string subscription, string vault, string resourceGroup, string? vaultType, string option, string telemetryMessage)
    {
        var result = new ValidationResult();

        var reference = BackupJobIdParser.Parse(RsvJobId, subscription, vault, resourceGroup, vaultType, result);

        Assert.Null(reference);
        var error = Assert.Single(result.Errors);
        Assert.Contains(option, error);
        Assert.Equal(telemetryMessage, result.TelemetrySafeMessage);
    }

    [Fact]
    public void Parse_FullArmJobId_EveryMismatch_ReportsAllErrorsAtOnce()
    {
        var result = new ValidationResult();

        var reference = BackupJobIdParser.Parse(RsvJobId, OtherSub, "otherVault", "otherRg", "dpp", result);

        Assert.Null(reference);
        Assert.Equal(4, result.Errors.Count);
        Assert.Contains(result.Errors, e => e.Contains("--subscription", StringComparison.Ordinal));
        Assert.Contains(result.Errors, e => e.Contains("--resource-group", StringComparison.Ordinal));
        Assert.Contains(result.Errors, e => e.Contains("--vault '", StringComparison.Ordinal));
        Assert.Contains(result.Errors, e => e.Contains("--vault-type", StringComparison.Ordinal));

        // Telemetry gets only the static messages, never the user-supplied names.
        Assert.Equal(
            string.Join('\n',
                BackupJobIdParser.SubscriptionMismatchTelemetryMessage,
                BackupJobIdParser.ResourceGroupMismatchTelemetryMessage,
                BackupJobIdParser.VaultMismatchTelemetryMessage,
                BackupJobIdParser.VaultTypeMismatchTelemetryMessage),
            result.TelemetrySafeMessage);
        Assert.DoesNotContain("otherVault", result.TelemetrySafeMessage);
        Assert.DoesNotContain("otherRg", result.TelemetrySafeMessage);
        Assert.DoesNotContain(OtherSub, result.TelemetrySafeMessage);
    }

    [Fact]
    public void Parse_FullArmJobId_WithSubscriptionName_DefersSubscriptionCheck()
    {
        // Names are resolved to GUIDs later and checked with ValidateSubscription.
        var result = new ValidationResult();

        var reference = BackupJobIdParser.Parse(RsvJobId, "My Subscription", "vault1", "rg1", null, result);

        Assert.True(result.IsValid);
        Assert.Equal(Sub, reference!.SubscriptionId);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("/")]
    [InlineData("a/b/c")]
    [InlineData("vaults/vault1/backupJobs/" + JobGuid)]
    [InlineData($"/subscriptions/{Sub}/resourceGroups/rg1/providers/Microsoft.Compute/virtualMachines/vm1")]
    [InlineData($"/subscriptions/{Sub}/resourceGroups/rg1/providers/Microsoft.RecoveryServices/vaults/vault1")]
    public void Parse_RejectsMalformedValues(string? job)
    {
        var result = new ValidationResult();

        var reference = BackupJobIdParser.Parse(job, Sub, "vault1", "rg1", null, result);

        Assert.Null(reference);
        Assert.Equal(BackupJobIdParser.InvalidFormatMessage, Assert.Single(result.Errors));
        Assert.Equal(BackupJobIdParser.InvalidFormatMessage, result.TelemetrySafeMessage);
    }

    [Fact]
    public void Parse_InvalidVaultType_DoesNotThrow()
    {
        var result = new ValidationResult();

        var reference = BackupJobIdParser.Parse(RsvJobId, Sub, "vault1", "rg1", "not-a-type", result);

        Assert.True(result.IsValid);
        Assert.Equal("rsv", reference!.VaultType);
    }

    [Theory]
    [InlineData(Sub, true)]
    [InlineData(OtherSub, false)]
    public void ValidateSubscription_ComparesResolvedSubscription(string resolvedSubscription, bool expectedValid)
    {
        var reference = BackupJobIdParser.Parse(RsvJobId, null, "vault1", "rg1", null, new ValidationResult());
        var result = new ValidationResult();

        BackupJobIdParser.ValidateSubscription(reference!, resolvedSubscription, result);

        Assert.Equal(expectedValid, result.IsValid);
        if (!expectedValid)
        {
            Assert.Equal(BackupJobIdParser.SubscriptionMismatchTelemetryMessage, result.TelemetrySafeMessage);
        }
    }

    [Fact]
    public void ValidateSubscription_JobNameOnly_AlwaysValid()
    {
        var reference = BackupJobIdParser.Parse(JobGuid, null, "vault1", "rg1", null, new ValidationResult());
        var result = new ValidationResult();

        BackupJobIdParser.ValidateSubscription(reference!, OtherSub, result);

        Assert.True(result.IsValid);
    }
}
