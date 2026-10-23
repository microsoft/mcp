// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;
using Azure.Mcp.Tools.AzureBackup.Models;
using Xunit;

namespace Azure.Mcp.Tools.AzureBackup.Tests.Models;

public class AzureBackupTelemetryTagsTests
{
    [Theory]
    [InlineData(null, "auto")]
    [InlineData("", "auto")]
    [InlineData("  ", "auto")]
    [InlineData("RSV", "rsv")]
    [InlineData("DPP", "dpp")]
    [InlineData("Rsv", "rsv")]
    public void NormalizeVaultType_ReturnsExpectedValue(string? input, string expected)
    {
        Assert.Equal(expected, AzureBackupTelemetryTags.NormalizeVaultType(input));
    }

    [Theory]
    [InlineData(null, "unspecified")]
    [InlineData("", "unspecified")]
    [InlineData("  ", "unspecified")]
    [InlineData("VM", "vm")]
    [InlineData("SqlDatabase", "sqldatabase")]
    [InlineData("AzureFileShare", "azurefileshare")]
    public void NormalizeWorkloadType_ReturnsExpectedValue(string? input, string expected)
    {
        Assert.Equal(expected, AzureBackupTelemetryTags.NormalizeWorkloadType(input));
    }

    [Fact]
    public void AddVaultTags_NullActivity_DoesNotThrow()
    {
        AzureBackupTelemetryTags.AddVaultTags(null, "rsv");
    }

    [Fact]
    public void AddVaultTags_NullVaultType_SetsAutoTag()
    {
        using var listener = new ActivityListener
        {
            ShouldListenTo = _ => true,
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllData,
        };
        ActivitySource.AddActivityListener(listener);

        using var source = new ActivitySource("test");
        using var activity = source.StartActivity("test-op");
        Assert.NotNull(activity);

        AzureBackupTelemetryTags.AddVaultTags(activity, null);

        var tag = activity.GetTagItem(AzureBackupTelemetryTags.VaultType);
        Assert.Equal("auto", tag);
    }

    [Fact]
    public void AddVaultTags_WithVaultType_SetsNormalizedTag()
    {
        using var listener = new ActivityListener
        {
            ShouldListenTo = _ => true,
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllData,
        };
        ActivitySource.AddActivityListener(listener);

        using var source = new ActivitySource("test");
        using var activity = source.StartActivity("test-op");
        Assert.NotNull(activity);

        AzureBackupTelemetryTags.AddVaultTags(activity, "RSV");

        var tag = activity.GetTagItem(AzureBackupTelemetryTags.VaultType);
        Assert.Equal("rsv", tag);
    }

    [Fact]
    public void AddVaultAndWorkloadTags_SetsAllTags()
    {
        using var listener = new ActivityListener
        {
            ShouldListenTo = _ => true,
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllData,
        };
        ActivitySource.AddActivityListener(listener);

        using var source = new ActivitySource("test");
        using var activity = source.StartActivity("test-op");
        Assert.NotNull(activity);

        AzureBackupTelemetryTags.AddVaultAndWorkloadTags(activity, null, null);

        Assert.Equal("auto", activity.GetTagItem(AzureBackupTelemetryTags.VaultType));
        Assert.Equal("unspecified", activity.GetTagItem(AzureBackupTelemetryTags.WorkloadType));
    }

    [Fact]
    public void TagConstants_HaveCorrectPrefix()
    {
        Assert.Equal("azurebackup/VaultType", AzureBackupTelemetryTags.VaultType);
        Assert.Equal("azurebackup/WorkloadType", AzureBackupTelemetryTags.WorkloadType);
        Assert.Equal("azurebackup/DatasourceType", AzureBackupTelemetryTags.DatasourceType);
        Assert.Equal("azurebackup/OperationScope", AzureBackupTelemetryTags.OperationScope);
    }

    [Fact]
    public void SubscriptionGuid_MatchesGlobalAzureTagName()
    {
        // Must stay in sync with Microsoft.Mcp.Core's internal AzureTagName.SubscriptionGuid.
        Assert.Equal("AzSubscriptionGuid", AzureBackupTelemetryTags.SubscriptionGuid);
    }

    [Fact]
    public void AddSubscriptionTag_NullActivity_DoesNotThrow()
    {
        AzureBackupTelemetryTags.AddSubscriptionTag(null, "00000000-0000-0000-0000-000000000000");
    }

    [Fact]
    public void AddSubscriptionTag_NullSubscription_DoesNotEmitTag()
    {
        using var listener = new ActivityListener
        {
            ShouldListenTo = _ => true,
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllData,
        };
        ActivitySource.AddActivityListener(listener);

        using var source = new ActivitySource("test");
        using var activity = source.StartActivity("test-op");
        Assert.NotNull(activity);

        AzureBackupTelemetryTags.AddSubscriptionTag(activity, null);

        Assert.Null(activity.GetTagItem(AzureBackupTelemetryTags.SubscriptionGuid));
    }

    [Theory]
    [InlineData("00000000-0000-0000-0000-000000000000")]
    [InlineData("my-subscription-name")]
    [InlineData("")]
    public void AddSubscriptionTag_NonNullValue_EmitsTag(string subscription)
    {
        using var listener = new ActivityListener
        {
            ShouldListenTo = _ => true,
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllData,
        };
        ActivitySource.AddActivityListener(listener);

        using var source = new ActivitySource("test");
        using var activity = source.StartActivity("test-op");
        Assert.NotNull(activity);

        AzureBackupTelemetryTags.AddSubscriptionTag(activity, subscription);

        Assert.Equal(subscription, activity.GetTagItem(AzureBackupTelemetryTags.SubscriptionGuid));
    }
}
