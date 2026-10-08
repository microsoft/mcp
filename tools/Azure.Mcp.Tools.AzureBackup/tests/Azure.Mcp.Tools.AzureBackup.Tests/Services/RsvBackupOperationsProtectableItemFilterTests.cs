// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.AzureBackup.Services;
using Xunit;

namespace Azure.Mcp.Tools.AzureBackup.Tests.Services;

/// <summary>
/// Unit tests for the internal <see cref="RsvBackupOperations.BuildProtectableItemFilter"/> helper
/// that constructs the OData $filter used when listing protectable items.
///
/// Regression coverage for the documented "register container -> inquire -> list" Azure File share
/// flow: an <c>AzureFileShare</c> workload-type filter must route to the <c>AzureStorage</c> backup
/// management type (file shares are not exposed under <c>AzureWorkload</c>), otherwise the flow would
/// return no items.
/// </summary>
public class RsvBackupOperationsProtectableItemFilterTests
{
    [Fact]
    public void BuildProtectableItemFilter_NoWorkloadType_QueriesAzureWorkload()
    {
        var filter = RsvBackupOperations.BuildProtectableItemFilter(null);

        Assert.Equal("backupManagementType eq 'AzureWorkload'", filter);
    }

    [Theory]
    [InlineData("AzureFileShare")]
    [InlineData("FileShare")]
    [InlineData("afs")]
    public void BuildProtectableItemFilter_AzureFileShare_RoutesToAzureStorage(string workloadType)
    {
        var filter = RsvBackupOperations.BuildProtectableItemFilter(workloadType);

        Assert.Equal("backupManagementType eq 'AzureStorage'", filter);
    }

    [Theory]
    [InlineData("SQL", "SQLDataBase")]
    [InlineData("SAPHana", "SAPHanaDatabase")]
    public void BuildProtectableItemFilter_AzureWorkloadType_UsesWorkloadClause(string workloadType, string normalized)
    {
        var filter = RsvBackupOperations.BuildProtectableItemFilter(workloadType);

        Assert.Equal($"backupManagementType eq 'AzureWorkload' and workloadType eq '{normalized}'", filter);
    }
}
