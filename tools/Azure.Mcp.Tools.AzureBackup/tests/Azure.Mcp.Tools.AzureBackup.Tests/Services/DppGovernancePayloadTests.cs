// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.AzureBackup.Models;
using Azure.Mcp.Tools.AzureBackup.Services;
using Azure.ResourceManager.DataProtectionBackup.Models;
using Xunit;

namespace Azure.Mcp.Tools.AzureBackup.Tests.Services;

/// <summary>
/// Regression tests locking in the exact payload shape emitted by
/// <see cref="DppBackupOperations.BuildImmutabilitySettings"/> and
/// <see cref="DppBackupOperations.BuildSoftDeleteSettings"/>. DPP's model surface
/// is thinner than RSV — <c>ImmutabilitySettings</c> is not publicly accessible in
/// SDK 1.8.0 — so we assert on the top-level <c>ImmutabilityState</c>. If a future
/// SDK bump exposes the nested type or changes the top-level property, these tests
/// will fail loudly.
/// </summary>
public class DppGovernancePayloadTests
{
    #region BuildImmutabilitySettings

    [Theory]
    [InlineData(AzureBackupImmutabilityState.Disabled, "Disabled")]
    [InlineData(AzureBackupImmutabilityState.Unlocked, "Unlocked")]
    [InlineData(AzureBackupImmutabilityState.Enabled, "Unlocked")]
    [InlineData(AzureBackupImmutabilityState.Locked, "Locked")]
    public void BuildImmutabilitySettings_MapsTopLevelState(
        AzureBackupImmutabilityState input, string expectedTopLevel)
    {
        var settings = DppBackupOperations.BuildImmutabilitySettings(
            input,
            AzureBackupImmutabilityType.AsPerPolicy,
            immutabilityDurationDays: null);

        Assert.NotNull(settings.ImmutabilityState);
        Assert.Equal(expectedTopLevel, settings.ImmutabilityState!.Value.ToString());
    }

    [Fact]
    public void BuildImmutabilitySettings_TimeBased_IsAcceptedButDiscarded()
    {
        // DPP has no ImmutabilityConfiguration; duration/type must be silently accepted
        // (no throw) so the RSV-focused parameters don't blow up DPP calls.
        var settings = DppBackupOperations.BuildImmutabilitySettings(
            AzureBackupImmutabilityState.Unlocked,
            AzureBackupImmutabilityType.TimeBased,
            immutabilityDurationDays: 90);

        Assert.NotNull(settings.ImmutabilityState);
        Assert.Equal("Unlocked", settings.ImmutabilityState!.Value.ToString());
    }

    #endregion

    #region BuildSoftDeleteSettings

    [Theory]
    [InlineData(AzureBackupSoftDeleteState.On, "On")]
    [InlineData(AzureBackupSoftDeleteState.Off, "Off")]
    [InlineData(AzureBackupSoftDeleteState.AlwaysOn, "AlwaysOn")]
    public void BuildSoftDeleteSettings_MapsState(
        AzureBackupSoftDeleteState state, string expected)
    {
        var settings = DppBackupOperations.BuildSoftDeleteSettings(state, softDeleteRetentionDays: 14);

        Assert.NotNull(settings.SoftDeleteSettings);
        Assert.Equal(expected, settings.SoftDeleteSettings!.State?.ToString());
    }

    [Theory]
    [InlineData(14)]
    [InlineData(30)]
    [InlineData(180)]
    public void BuildSoftDeleteSettings_AlwaysForwardsRetentionDays(int retentionDays)
    {
        var settings = DppBackupOperations.BuildSoftDeleteSettings(
            AzureBackupSoftDeleteState.On, retentionDays);

        Assert.Equal(retentionDays, settings.SoftDeleteSettings!.RetentionDurationInDays);
    }

    #endregion
}
