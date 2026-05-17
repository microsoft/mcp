// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.AzureBackup.Services;
using Xunit;

namespace Azure.Mcp.Tools.AzureBackup.UnitTests.Services;

public class RsvDatasourceRegistryTests
{
    #region Resolve - FriendlyName and Alias matching

    [Theory]
    [InlineData("VM", "VM")]
    [InlineData("vm", "VM")]
    [InlineData("iaasvm", "VM")]
    [InlineData("azurevm", "VM")]
    [InlineData("virtualmachine", "VM")]
    [InlineData("SQL", "SQL")]
    [InlineData("sql", "SQL")]
    [InlineData("sqldatabase", "SQL")]
    [InlineData("sqldb", "SQL")]
    [InlineData("SAPHANA", "SAPHANA")]
    [InlineData("saphana", "SAPHANA")]
    [InlineData("hana", "SAPHANA")]
    [InlineData("SAPASE", "SAPASE")]
    [InlineData("sapase", "SAPASE")]
    [InlineData("ase", "SAPASE")]
    [InlineData("sybase", "SAPASE")]
    [InlineData("AzureFileShare", "AzureFileShare")]
    [InlineData("azurefileshare", "AzureFileShare")]
    [InlineData("fileshare", "AzureFileShare")]
    [InlineData("afs", "AzureFileShare")]
    public void Resolve_MatchesFriendlyNameAndAliases(string input, string expectedFriendlyName)
    {
        var profile = RsvDatasourceRegistry.Resolve(input);
        Assert.NotNull(profile);
        Assert.Equal(expectedFriendlyName, profile!.FriendlyName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Resolve_NullOrEmpty_ReturnsNull(string? input)
    {
        var profile = RsvDatasourceRegistry.Resolve(input);
        Assert.Null(profile);
    }

    [Fact]
    public void Resolve_UnknownType_ReturnsNull()
    {
        var profile = RsvDatasourceRegistry.Resolve("unknown");
        Assert.Null(profile);
    }

    #endregion

    #region ResolveOrDefault

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void ResolveOrDefault_NullOrEmpty_ReturnsIaasVm(string? input)
    {
        var profile = RsvDatasourceRegistry.ResolveOrDefault(input);
        Assert.Equal(RsvDatasourceRegistry.IaasVm, profile);
    }

    [Fact]
    public void ResolveOrDefault_ValidType_ReturnsProfile()
    {
        var profile = RsvDatasourceRegistry.ResolveOrDefault("sql");
        Assert.Equal(RsvDatasourceRegistry.SqlDatabase, profile);
    }

    [Fact]
    public void ResolveOrDefault_UnknownType_ThrowsArgumentException()
    {
        var ex = Assert.Throws<ArgumentException>(() => RsvDatasourceRegistry.ResolveOrDefault("unknown"));
        Assert.Contains("Unknown RSV workload type", ex.Message);
        Assert.Contains("unknown", ex.Message);
    }

    #endregion

    #region ResolveFromProtectedItemName

    [Theory]
    [InlineData("sqldatabase;mssqlserver;mydb", null, "SQL")]
    [InlineData("SQLDatabase;MSSQLSERVER;DB1", null, "SQL")]
    [InlineData("sqlinstance;mssqlserver", null, "SQL")]
    [InlineData("saphanadatabase;hdb;mydb", null, "SAPHANA")]
    [InlineData("SAPHanaDatabase;HDB;testdb", null, "SAPHANA")]
    [InlineData("saphanainstance;hdb", null, "SAPHANA")]
    [InlineData("sapasedatabase;ase;mydb", null, "SAPASE")]
    [InlineData("azurefileshare;myshare", null, "AzureFileShare")]
    [InlineData("AzureFileShare;TestShare", null, "AzureFileShare")]
    [InlineData("someitem", "storagecontainer;storage;myaccount", "AzureFileShare")]
    [InlineData("someitem", "vmappcontainer;compute;myvm", "SQL")]
    [InlineData("vm;iaasvmcontainerv2;rg;myvm", null, "VM")]
    [InlineData("unknownformat", null, "VM")]
    [InlineData("unknownformat", "unknowncontainer", "VM")]
    public void ResolveFromProtectedItemName_ResolvesCorrectly(string itemName, string? containerName, string expectedFriendlyName)
    {
        var profile = RsvDatasourceRegistry.ResolveFromProtectedItemName(itemName, containerName);
        Assert.Equal(expectedFriendlyName, profile.FriendlyName);
    }

    #endregion

    #region Profile properties

    [Theory]
    [InlineData("VM", false)]
    [InlineData("SQL", true)]
    [InlineData("SAPHANA", true)]
    [InlineData("SAPASE", true)]
    [InlineData("AzureFileShare", false)]
    public void Profile_IsWorkloadType_ReturnsExpected(string friendlyName, bool expected)
    {
        var profile = RsvDatasourceRegistry.Resolve(friendlyName);
        Assert.NotNull(profile);
        Assert.Equal(expected, profile!.IsWorkloadType);
    }

    [Theory]
    [InlineData("SQL", "SQLDataBase")]
    [InlineData("SAPHANA", "SAPHanaDatabase")]
    [InlineData("SAPASE", "SAPAseDatabase")]
    public void Profile_ApiWorkloadType_ReturnsExpected(string friendlyName, string expectedApiType)
    {
        var profile = RsvDatasourceRegistry.Resolve(friendlyName);
        Assert.NotNull(profile);
        Assert.Equal(expectedApiType, profile!.ApiWorkloadType);
    }

    [Fact]
    public void Profile_IaasVm_HasNoApiWorkloadType()
    {
        Assert.Null(RsvDatasourceRegistry.IaasVm.ApiWorkloadType);
    }

    [Theory]
    [InlineData("VM", true)]
    [InlineData("SQL", true)]
    [InlineData("SAPHANA", true)]
    [InlineData("SAPASE", true)]
    [InlineData("AzureFileShare", true)]
    public void Profile_SupportsPolicyUpdate_ReturnsExpected(string friendlyName, bool expected)
    {
        var profile = RsvDatasourceRegistry.Resolve(friendlyName);
        Assert.NotNull(profile);
        Assert.Equal(expected, profile!.SupportsPolicyUpdate);
    }

    #endregion

    #region AllProfiles coverage

    [Fact]
    public void AllProfiles_ContainsExpectedDatasourceTypes()
    {
        var friendlyNames = RsvDatasourceRegistry.AllProfiles.Select(p => p.FriendlyName).ToList();

        Assert.Contains("VM", friendlyNames);
        Assert.Contains("SQL", friendlyNames);
        Assert.Contains("SAPHANA", friendlyNames);
        Assert.Contains("SAPASE", friendlyNames);
        Assert.Contains("AzureFileShare", friendlyNames);
        Assert.Equal(5, friendlyNames.Count);
    }

    [Fact]
    public void KnownTypeNames_ContainsAllFriendlyNamesAndAliases()
    {
        var knownNames = RsvDatasourceRegistry.KnownTypeNames;

        // Friendly names
        Assert.Contains("VM", knownNames);
        Assert.Contains("SQL", knownNames);
        Assert.Contains("SAPHANA", knownNames);
        Assert.Contains("SAPASE", knownNames);
        Assert.Contains("AzureFileShare", knownNames);

        // Some aliases
        Assert.Contains("vm", knownNames);
        Assert.Contains("iaasvm", knownNames);
        Assert.Contains("sql", knownNames);
        Assert.Contains("hana", knownNames);
        Assert.Contains("ase", knownNames);
        Assert.Contains("fileshare", knownNames);
    }

    #endregion
}
