// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.AzureBackup.Commands;
using Microsoft.Mcp.Core.Commands;
using Xunit;

namespace Azure.Mcp.Tools.AzureBackup.Tests.Vault;

public class BackupVaultReferenceParserTests
{
    private const string Sub = "f0c630e0-2995-4853-b056-0b3c09cb673f";
    private const string OtherSub = "11111111-2222-3333-4444-555555555555";
    private const string RsvId = $"/subscriptions/{Sub}/resourceGroups/myRg/providers/Microsoft.RecoveryServices/vaults/myVault";
    private const string DppId = $"/subscriptions/{Sub}/resourceGroups/myRg/providers/Microsoft.DataProtection/backupVaults/myBackupVault";
    private const string RgId = $"/subscriptions/{Sub}/resourceGroups/myRg";

    [Fact]
    public void Normalize_LeavesPlainNamesUnchanged()
    {
        var validation = new ValidationResult();

        var reference = BackupVaultReferenceParser.Normalize(Sub, "myRg", "myVault", "rsv", rsvOnly: false, validation);

        Assert.True(validation.IsValid);
        Assert.Equal(new BackupVaultReference("myRg", "myVault", "rsv"), reference);
    }

    [Theory]
    [InlineData(RsvId, "myVault", "rsv")]
    [InlineData(DppId, "myBackupVault", "dpp")]
    [InlineData($"subscriptions/{Sub}/resourceGroups/myRg/providers/microsoft.recoveryservices/VAULTS/myVault/", "myVault", "rsv")]
    [InlineData($"  /SUBSCRIPTIONS/{Sub}/RESOURCEGROUPS/myRg/PROVIDERS/Microsoft.DataProtection/BackupVaults/myBackupVault  ", "myBackupVault", "dpp")]
    public void Normalize_VaultResourceId_ExtractsNameAndInfersVaultType(string vault, string expectedName, string expectedType)
    {
        var validation = new ValidationResult();

        var reference = BackupVaultReferenceParser.Normalize(Sub, "myRg", vault, null, rsvOnly: false, validation);

        Assert.True(validation.IsValid, string.Join('\n', validation.Errors));
        Assert.Equal(new BackupVaultReference("myRg", expectedName, expectedType), reference);
    }

    [Fact]
    public void Normalize_VaultResourceId_FillsMissingResourceGroup()
    {
        var validation = new ValidationResult();

        var reference = BackupVaultReferenceParser.Normalize(Sub, null, RsvId, null, rsvOnly: false, validation);

        Assert.True(validation.IsValid);
        Assert.Equal("myRg", reference.ResourceGroup);
    }

    [Fact]
    public void Normalize_ResourceGroupResourceId_ExtractsName()
    {
        var validation = new ValidationResult();

        var reference = BackupVaultReferenceParser.Normalize(Sub, RgId, "myVault", null, rsvOnly: false, validation);

        Assert.True(validation.IsValid);
        Assert.Equal(new BackupVaultReference("myRg", "myVault", null), reference);
    }

    [Fact]
    public void Normalize_ResourceGroupComparison_IsCaseInsensitive()
    {
        var validation = new ValidationResult();

        var reference = BackupVaultReferenceParser.Normalize(Sub, "MYRG", RsvId, "RSV", rsvOnly: false, validation);

        Assert.True(validation.IsValid);
        Assert.Equal(new BackupVaultReference("MYRG", "myVault", "RSV"), reference);
    }

    [Theory]
    [InlineData("/subscriptions/not-a-guid/resourceGroups/myRg/providers/Microsoft.RecoveryServices/vaults/myVault")]
    [InlineData($"/subscriptions/{Sub}/resourceGroups/myRg/providers/Microsoft.Storage/storageAccounts/myVault")]
    [InlineData($"/subscriptions/{Sub}/resourceGroups/myRg/providers/Microsoft.RecoveryServices/vaults/myVault/backupFabrics/Azure")]
    [InlineData($"/subscriptions/{Sub}/resourceGroups/myRg/providers/Microsoft.RecoveryServices/vaults")]
    [InlineData($"/subscriptions/{Sub}/resourceGroups/myRg/providers/Microsoft.RecoveryServices/vaults/my%2FVault")]
    [InlineData($"/subscriptions/{Sub}/resourceGroups/myRg/providers/Microsoft.RecoveryServices/vaults/../myVault")]
    [InlineData($"/subscriptions/{Sub}//resourceGroups/myRg/providers/Microsoft.RecoveryServices/vaults/myVault")]
    [InlineData("/myVault")]
    [InlineData("myRg/myVault")]
    public void Normalize_InvalidVaultResourceId_ReturnsTelemetrySafeError(string vault)
    {
        var validation = new ValidationResult();

        BackupVaultReferenceParser.Normalize(Sub, "myRg", vault, null, rsvOnly: false, validation);

        Assert.Equal([BackupVaultReferenceParser.InvalidVaultMessage], validation.Errors);
        Assert.Equal(BackupVaultReferenceParser.InvalidVaultMessage, validation.TelemetrySafeMessage);
    }

    [Theory]
    [InlineData($"/subscriptions/{Sub}")]
    [InlineData($"/subscriptions/{Sub}/resourceGroups/myRg/providers/Microsoft.RecoveryServices/vaults/myVault")]
    [InlineData("/subscriptions/not-a-guid/resourceGroups/myRg")]
    public void Normalize_InvalidResourceGroupResourceId_ReturnsError(string resourceGroup)
    {
        var validation = new ValidationResult();

        BackupVaultReferenceParser.Normalize(Sub, resourceGroup, "myVault", null, rsvOnly: false, validation);

        Assert.Equal([BackupVaultReferenceParser.InvalidResourceGroupMessage], validation.Errors);
    }

    [Theory]
    [InlineData("", BackupVaultReferenceParser.EmptyVaultMessage)]
    [InlineData("   ", BackupVaultReferenceParser.EmptyVaultMessage)]
    public void Normalize_EmptyVault_ReturnsError(string vault, string expected)
    {
        var validation = new ValidationResult();

        BackupVaultReferenceParser.Normalize(Sub, "myRg", vault, null, rsvOnly: false, validation);

        Assert.Equal([expected], validation.Errors);
    }

    [Fact]
    public void Normalize_EmptyResourceGroup_ReturnsErrorWithoutMismatch()
    {
        var validation = new ValidationResult();

        BackupVaultReferenceParser.Normalize(Sub, " ", RsvId, null, rsvOnly: false, validation);

        Assert.Equal([BackupVaultReferenceParser.EmptyResourceGroupMessage], validation.Errors);
    }

    [Fact]
    public void Normalize_ReportsEveryMismatch()
    {
        var validation = new ValidationResult();

        BackupVaultReferenceParser.Normalize(OtherSub, "otherRg", RsvId, "dpp", rsvOnly: false, validation);

        Assert.Equal(
            [
                BackupVaultReferenceParser.VaultTypeMismatchMessage,
                BackupVaultReferenceParser.ResourceGroupMismatchMessage,
                BackupVaultReferenceParser.SubscriptionMismatchMessage
            ],
            validation.Errors);
        Assert.Equal(string.Join('\n', validation.Errors), validation.TelemetrySafeMessage);
    }

    [Fact]
    public void Normalize_ResourceGroupIdSubscriptionMismatch_ReturnsError()
    {
        var validation = new ValidationResult();

        BackupVaultReferenceParser.Normalize(Sub, $"/subscriptions/{OtherSub}/resourceGroups/myRg", "myVault", null, rsvOnly: false, validation);

        Assert.Equal([BackupVaultReferenceParser.SubscriptionMismatchMessage], validation.Errors);
    }

    [Fact]
    public void Normalize_VaultAndResourceGroupIdsInDifferentSubscriptions_ReturnsError()
    {
        var validation = new ValidationResult();

        BackupVaultReferenceParser.Normalize(Sub, $"/subscriptions/{OtherSub}/resourceGroups/myRg", RsvId, null, rsvOnly: false, validation);

        Assert.Equal([BackupVaultReferenceParser.ResourceIdSubscriptionMismatchMessage], validation.Errors);
    }

    [Fact]
    public void Normalize_SubscriptionIdWithDifferentCasing_IsNotAMismatch()
    {
        var validation = new ValidationResult();

        BackupVaultReferenceParser.Normalize("F0C630E0-2995-4853-B056-0B3C09CB673F", "myRg", RsvId, null, rsvOnly: false, validation);

        Assert.True(validation.IsValid);
    }

    [Theory]
    [InlineData("myRg", RsvId)]
    [InlineData(RgId, "myVault")]
    public void Normalize_SubscriptionNameWithResourceId_IsRejected(string resourceGroup, string vault)
    {
        var validation = new ValidationResult();

        BackupVaultReferenceParser.Normalize("My Subscription", resourceGroup, vault, null, rsvOnly: false, validation);

        Assert.Equal([BackupVaultReferenceParser.SubscriptionNameWithResourceIdMessage], validation.Errors);
    }

    [Fact]
    public void Normalize_SubscriptionNameWithPlainNames_IsAllowed()
    {
        var validation = new ValidationResult();

        BackupVaultReferenceParser.Normalize("My Subscription", "myRg", "myVault", null, rsvOnly: false, validation);

        Assert.True(validation.IsValid);
    }

    [Fact]
    public void Normalize_RsvOnly_RejectsBackupVaultResourceId()
    {
        var validation = new ValidationResult();

        BackupVaultReferenceParser.Normalize(Sub, "myRg", DppId, null, rsvOnly: true, validation);

        Assert.Equal([BackupVaultReferenceParser.RsvOnlyVaultMessage], validation.Errors);
    }

    [Fact]
    public void Normalize_RsvOnly_AcceptsRecoveryServicesVaultResourceIdWithoutSettingVaultType()
    {
        var validation = new ValidationResult();

        var reference = BackupVaultReferenceParser.Normalize(Sub, "myRg", RsvId, null, rsvOnly: true, validation);

        Assert.True(validation.IsValid);
        Assert.Equal(new BackupVaultReference("myRg", "myVault", null), reference);
    }

    [Fact]
    public void Normalize_UnknownVaultType_IsLeftForCommandValidation()
    {
        var validation = new ValidationResult();

        var reference = BackupVaultReferenceParser.Normalize(Sub, "myRg", RsvId, "other", rsvOnly: false, validation);

        Assert.True(validation.IsValid);
        Assert.Equal("other", reference.VaultType);
    }

    [Theory]
    [InlineData(null, RsvId, null, Sub)]
    [InlineData("", DppId, null, Sub)]
    [InlineData(null, "myVault", RgId, Sub)]
    [InlineData("explicit", RsvId, RgId, "explicit")]
    [InlineData(null, "myVault", "myRg", null)]
    [InlineData(null, "/bad", "/bad", null)]
    public void ResolveSubscription_UsesResourceIdOnlyWhenSubscriptionOmitted(string? subscription, string vault, string? resourceGroup, string? expected)
    {
        Assert.Equal(expected, BackupVaultReferenceParser.ResolveSubscription(subscription, vault, resourceGroup));
    }
}
