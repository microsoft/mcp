// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.AzureBackup.Models;
using Azure.Mcp.Tools.AzureBackup.Services;
using Azure.ResourceManager.RecoveryServices.Models;
using Xunit;

namespace Azure.Mcp.Tools.AzureBackup.Tests.Services;

/// <summary>
/// Regression tests locking in the exact payload shape emitted by
/// <see cref="RsvBackupOperations.BuildImmutabilitySettings"/> and
/// <see cref="RsvBackupOperations.BuildSoftDeleteSettings"/>. These payloads were
/// silently broken by the SDK 1.2.0 -> 1.3.0 bump (PR #3279) which introduced
/// new required fields on api-version 2026-02-01+ / 2026-05-01+:
///   * ImmutabilitySettings.Configuration.Type (whenever State != Disabled)
///   * RecoveryServicesSoftDeleteSettings.SoftDeleteRetentionPeriodInDays
///   * RecoveryServicesSoftDeleteSettings.EnhancedSecurityState
/// Recording-based playback tests will pass a stale payload without catching these
/// regressions; the assertions below run against the SDK model classes directly so
/// any future SDK bump that changes payload requirements will fail here first.
/// </summary>
public class RsvGovernancePayloadTests
{
    #region BuildImmutabilitySettings

    [Fact]
    public void BuildImmutabilitySettings_Disabled_OmitsConfiguration()
    {
        var settings = RsvBackupOperations.BuildImmutabilitySettings(
            AzureBackupImmutabilityState.Disabled,
            AzureBackupImmutabilityType.AsPerPolicy,
            immutabilityDurationDays: null);

        Assert.NotNull(settings.ImmutabilitySettings);
        Assert.Equal(ImmutabilityState.Disabled, settings.ImmutabilitySettings!.State);
        // Regression guard: for Disabled we must NOT send Configuration; RP rejects it.
        Assert.Null(settings.ImmutabilitySettings.Configuration);
    }

    [Fact]
    public void BuildImmutabilitySettings_Unlocked_AsPerPolicy_SetsConfigurationTypeAndOmitsDuration()
    {
        var settings = RsvBackupOperations.BuildImmutabilitySettings(
            AzureBackupImmutabilityState.Unlocked,
            AzureBackupImmutabilityType.AsPerPolicy,
            immutabilityDurationDays: null);

        Assert.Equal(ImmutabilityState.Unlocked, settings.ImmutabilitySettings!.State);
        // Regression guard: api-version 2026-05-01+ requires Configuration.Type when state != Disabled.
        Assert.NotNull(settings.ImmutabilitySettings.Configuration);
        Assert.Equal(ImmutabilityType.AsPerPolicy, settings.ImmutabilitySettings.Configuration!.Type);
        // AsPerPolicy takes duration from the policy, not the vault; do not send DurationInDays.
        Assert.Null(settings.ImmutabilitySettings.Configuration.DurationInDays);
    }

    [Fact]
    public void BuildImmutabilitySettings_Locked_AsPerPolicy_SetsConfigurationType()
    {
        var settings = RsvBackupOperations.BuildImmutabilitySettings(
            AzureBackupImmutabilityState.Locked,
            AzureBackupImmutabilityType.AsPerPolicy,
            immutabilityDurationDays: null);

        Assert.Equal(ImmutabilityState.Locked, settings.ImmutabilitySettings!.State);
        Assert.NotNull(settings.ImmutabilitySettings.Configuration);
        Assert.Equal(ImmutabilityType.AsPerPolicy, settings.ImmutabilitySettings.Configuration!.Type);
    }

    [Fact]
    public void BuildImmutabilitySettings_TimeBased_ForwardsDuration()
    {
        var settings = RsvBackupOperations.BuildImmutabilitySettings(
            AzureBackupImmutabilityState.Unlocked,
            AzureBackupImmutabilityType.TimeBased,
            immutabilityDurationDays: 90);

        Assert.NotNull(settings.ImmutabilitySettings!.Configuration);
        Assert.Equal(ImmutabilityType.TimeBased, settings.ImmutabilitySettings.Configuration!.Type);
        Assert.Equal(90, settings.ImmutabilitySettings.Configuration.DurationInDays);
    }

    [Fact]
    public void BuildImmutabilitySettings_EnabledAlias_MapsToUnlockedState()
    {
        // Enabled is a backward-compat alias; dispatcher normalises to Unlocked, but
        // BuildImmutabilitySettings must be defensive and map Enabled -> Unlocked as well.
        var settings = RsvBackupOperations.BuildImmutabilitySettings(
            AzureBackupImmutabilityState.Enabled,
            AzureBackupImmutabilityType.AsPerPolicy,
            immutabilityDurationDays: null);

        Assert.Equal(ImmutabilityState.Unlocked, settings.ImmutabilitySettings!.State);
    }

    #endregion

    #region BuildSoftDeleteSettings

    [Theory]
    [InlineData(AzureBackupSoftDeleteState.On, "Enabled", "Enabled")]
    [InlineData(AzureBackupSoftDeleteState.Off, "Disabled", "Disabled")]
    [InlineData(AzureBackupSoftDeleteState.AlwaysOn, "AlwaysON", "AlwaysON")]
    public void BuildSoftDeleteSettings_MirrorsEnhancedSecurityState(
        AzureBackupSoftDeleteState state, string expectedSoftDelete, string expectedEnhanced)
    {
        var settings = RsvBackupOperations.BuildSoftDeleteSettings(state, softDeleteRetentionDays: 14);

        Assert.NotNull(settings.SoftDeleteSettings);
        Assert.Equal(expectedSoftDelete, settings.SoftDeleteSettings!.SoftDeleteState?.ToString());
        // Regression guard: api-version 2026-02-01+ rejects patches missing EnhancedSecurityState.
        Assert.NotNull(settings.SoftDeleteSettings.EnhancedSecurityState);
        Assert.Equal(expectedEnhanced, settings.SoftDeleteSettings.EnhancedSecurityState!.Value.ToString());
    }

    [Theory]
    [InlineData(14)]
    [InlineData(30)]
    [InlineData(180)]
    public void BuildSoftDeleteSettings_AlwaysForwardsRetentionDays(int retentionDays)
    {
        var settings = RsvBackupOperations.BuildSoftDeleteSettings(
            AzureBackupSoftDeleteState.On, retentionDays);

        // Regression guard: api-version 2026-02-01+ rejects patches missing retention.
        Assert.Equal(retentionDays, settings.SoftDeleteSettings!.SoftDeleteRetentionPeriodInDays);
    }

    #endregion
}
