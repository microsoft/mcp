// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Core;
using Azure.Mcp.Tools.AzureBackup.Services;
using Xunit;

namespace Azure.Mcp.Tools.AzureBackup.UnitTests.Services;

public class DppDatasourceRegistryTests
{
    #region Resolve - MapWorkloadTypeToArmResourceType

    [Theory]
    [InlineData("azuredisk", "Microsoft.Compute/disks")]
    [InlineData("AzureDisk", "Microsoft.Compute/disks")]
    [InlineData("AZUREDISK", "Microsoft.Compute/disks")]
    [InlineData("azureblob", "Microsoft.Storage/storageAccounts/blobServices")]
    [InlineData("postgresqlflexible", "Microsoft.DBforPostgreSQL/flexibleServers")]
    [InlineData("aks", "Microsoft.ContainerService/managedClusters")]
    [InlineData("AKS", "Microsoft.ContainerService/managedClusters")]
    [InlineData("elasticsan", "Microsoft.ElasticSan/elasticSans/volumeGroups")]
    [InlineData("ElasticSan", "Microsoft.ElasticSan/elasticSans/volumeGroups")]
    [InlineData("adls", "Microsoft.Storage/storageAccounts/blobServices")]
    [InlineData("cosmosdb", "Microsoft.DocumentDB/databaseAccounts")]
    public void Resolve_MapsWorkloadTypeToArmResourceType(string workloadType, string expected)
    {
        var profile = DppDatasourceRegistry.Resolve(workloadType);
        Assert.Equal(expected, profile.ArmResourceType);
    }

    [Theory]
    [InlineData("Microsoft.Compute/disks")]
    [InlineData("Microsoft.ContainerService/managedClusters")]
    public void Resolve_MatchesByArmResourceType(string armType)
    {
        var profile = DppDatasourceRegistry.Resolve(armType);
        Assert.Equal(armType, profile.ArmResourceType);
    }

    [Fact]
    public void Resolve_UnknownType_ThrowsArgumentException()
    {
        var ex = Assert.Throws<ArgumentException>(() => DppDatasourceRegistry.Resolve("some.custom/resourceType"));
        Assert.Contains("Unknown DPP workload type", ex.Message);
        Assert.Contains("some.custom/resourceType", ex.Message);
    }

    #endregion

    #region UsesOperationalStore

    [Theory]
    [InlineData("Microsoft.Compute/disks", true)]
    [InlineData("microsoft.compute/disks", true)]
    [InlineData("Microsoft.Storage/storageAccounts/blobServices", true)]
    [InlineData("Microsoft.ContainerService/managedClusters", true)]
    [InlineData("microsoft.containerservice/managedclusters", true)]
    [InlineData("Microsoft.ElasticSan/elasticSans/volumeGroups", true)]
    [InlineData("azuredisk", true)]
    [InlineData("azureblob", true)]
    [InlineData("aks", true)]
    [InlineData("elasticsan", true)]
    [InlineData("Microsoft.DBforPostgreSQL/flexibleServers", false)]
    public void Resolve_UsesOperationalStore_ReturnsExpected(string datasourceType, bool expected)
    {
        var profile = DppDatasourceRegistry.Resolve(datasourceType);
        Assert.Equal(expected, profile.UsesOperationalStore);
    }

    #endregion

    #region IsContinuousBackup (Blob/ADLS)

    [Theory]
    [InlineData("azureblob", true)]
    [InlineData("AzureBlob", true)]
    [InlineData("adls", true)]
    [InlineData("cosmosdb", true)]
    [InlineData("azuredisk", false)]
    [InlineData("aks", false)]
    [InlineData("elasticsan", false)]
    [InlineData("postgresqlflexible", false)]
    public void Resolve_IsContinuousBackup_ReturnsExpected(string datasourceType, bool expected)
    {
        var profile = DppDatasourceRegistry.Resolve(datasourceType);
        Assert.Equal(expected, profile.IsContinuousBackup);
    }

    #endregion

    #region DataSourceSetMode (ESAN & AKS require it)

    [Theory]
    [InlineData("elasticsan", true)]
    [InlineData("Microsoft.ElasticSan/elasticSans/volumeGroups", true)]
    [InlineData("aks", true)]
    [InlineData("Microsoft.ContainerService/managedClusters", true)]
    [InlineData("azuredisk", false)]
    [InlineData("azureblob", false)]
    [InlineData("postgresqlflexible", false)]
    public void Resolve_RequiresDataSourceSetInfo_ReturnsExpected(string datasourceType, bool expected)
    {
        var profile = DppDatasourceRegistry.Resolve(datasourceType);
        var hasDataSourceSet = profile.DataSourceSetMode != DppDataSourceSetMode.None;
        Assert.Equal(expected, hasDataSourceSet);
    }

    [Fact]
    public void RequiresDataSourceSetInfo_IncludesBothEsanAndAks()
    {
        Assert.NotEqual(DppDataSourceSetMode.None, DppDatasourceRegistry.Resolve("elasticsan").DataSourceSetMode);
        Assert.NotEqual(DppDataSourceSetMode.None, DppDatasourceRegistry.Resolve("aks").DataSourceSetMode);
        Assert.NotEqual(DppDataSourceSetMode.None, DppDatasourceRegistry.Resolve("Microsoft.ElasticSan/elasticSans/volumeGroups").DataSourceSetMode);
        Assert.NotEqual(DppDataSourceSetMode.None, DppDatasourceRegistry.Resolve("Microsoft.ContainerService/managedClusters").DataSourceSetMode);

        Assert.Equal(DppDataSourceSetMode.None, DppDatasourceRegistry.Resolve("azuredisk").DataSourceSetMode);
        Assert.Equal(DppDataSourceSetMode.None, DppDatasourceRegistry.Resolve("azureblob").DataSourceSetMode);
        Assert.Equal(DppDataSourceSetMode.None, DppDatasourceRegistry.Resolve("postgresqlflexible").DataSourceSetMode);
    }

    #endregion

    #region GetParentResourceId (ElasticSAN)

    [Fact]
    public void GetParentResourceId_ExtractsParentFromVolumeGroupId()
    {
        var volumeGroupId = new ResourceIdentifier(
            "/subscriptions/00000000-0000-0000-0000-000000000001/resourceGroups/rg1/providers/Microsoft.ElasticSan/elasticSans/mysan/volumeGroups/myvg");

        var parentId = DppDatasourceRegistry.GetParentResourceId(volumeGroupId);

        Assert.Equal(
            "/subscriptions/00000000-0000-0000-0000-000000000001/resourceGroups/rg1/providers/Microsoft.ElasticSan/elasticSans/mysan",
            parentId.ToString());
    }

    [Fact]
    public void GetParentResourceId_HandlesVaryingCase()
    {
        var volumeGroupId = new ResourceIdentifier(
            "/subscriptions/00000000-0000-0000-0000-000000000001/resourceGroups/rg1/providers/Microsoft.ElasticSan/elasticSans/testsan/VolumeGroups/testvg");

        var parentId = DppDatasourceRegistry.GetParentResourceId(volumeGroupId);

        Assert.Equal(
            "/subscriptions/00000000-0000-0000-0000-000000000001/resourceGroups/rg1/providers/Microsoft.ElasticSan/elasticSans/testsan",
            parentId.ToString());
    }

    [Fact]
    public void GetParentResourceId_ReturnsParentForNonVolumeGroup()
    {
        var diskId = new ResourceIdentifier(
            "/subscriptions/00000000-0000-0000-0000-000000000001/resourceGroups/rg1/providers/Microsoft.Compute/disks/mydisk");

        var parentId = DppDatasourceRegistry.GetParentResourceId(diskId);

        Assert.NotNull(parentId);
    }

    #endregion

    #region AllProfiles coverage

    [Fact]
    public void AllProfiles_ContainsExpectedDatasourceTypes()
    {
        var friendlyNames = DppDatasourceRegistry.AllProfiles.Select(p => p.FriendlyName).ToList();

        // Supported DPP types: ESAN, Disk, AKS, Blob, ADLS, PostgreSQL Flexible, CosmosDB
        Assert.Contains("AzureDisk", friendlyNames);
        Assert.Contains("AzureBlob", friendlyNames);
        Assert.Contains("AKS", friendlyNames);
        Assert.Contains("ElasticSAN", friendlyNames);
        Assert.Contains("PostgreSQLFlexible", friendlyNames);
        Assert.Contains("AzureDataLakeStorage", friendlyNames);
        Assert.Contains("CosmosDB", friendlyNames);

        // MySQLFlexible is not a supported datasource type
        Assert.DoesNotContain("MySQLFlexible", friendlyNames);
    }

    #endregion
}
