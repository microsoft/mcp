// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.AzureTerraform.Services;
using Xunit;

namespace Azure.Mcp.Tools.AzureTerraform.UnitTests;

public class AvmDocsServiceParsingTests
{
    [Fact]
    public void SourceFromRepoUrl_ConvertsCorrectly()
    {
        var result = AvmDocsService.SourceFromRepoUrl(
            "https://github.com/Azure/terraform-azurerm-avm-res-storage-storageaccount");

        Assert.Equal("Azure/avm-res-storage-storageaccount/azurerm", result);
    }

    [Fact]
    public void SourceFromRepoUrl_HandlesApiManagement()
    {
        var result = AvmDocsService.SourceFromRepoUrl(
            "https://github.com/Azure/terraform-azurerm-avm-res-apimanagement-service");

        Assert.Equal("Azure/avm-res-apimanagement-service/azurerm", result);
    }

    [Fact]
    public void SourceFromRepoUrl_HandlesPatternModule()
    {
        var result = AvmDocsService.SourceFromRepoUrl(
            "https://github.com/Azure/terraform-azurerm-avm-ptn-virtualnetwork");

        Assert.Equal("Azure/avm-ptn-virtualnetwork/azurerm", result);
    }

    [Fact]
    public void ParseModuleCsv_ParsesValidCsv()
    {
        var csv = """
            ModuleName,Description,ModuleStatus,RepoURL
            avm-res-storage-storageaccount,Storage Account,Available,https://github.com/Azure/terraform-azurerm-avm-res-storage-storageaccount
            avm-res-compute-vm,Virtual Machine,Available,https://github.com/Azure/terraform-azurerm-avm-res-compute-vm
            """;

        var modules = AvmDocsService.ParseModuleCsv(csv);

        Assert.Equal(2, modules.Count);
        Assert.Equal("avm-res-storage-storageaccount", modules[0].ModuleName);
        Assert.Equal("Storage Account", modules[0].Description);
        Assert.Equal("Azure/avm-res-storage-storageaccount/azurerm", modules[0].Source);
    }

    [Fact]
    public void ParseModuleCsv_FiltersProposedModules()
    {
        var csv = """
            ModuleName,Description,ModuleStatus,RepoURL
            avm-res-storage-storageaccount,Storage Account,Available,https://github.com/Azure/terraform-azurerm-avm-res-storage-storageaccount
            avm-res-future-thing,Future Thing,Proposed,https://github.com/Azure/terraform-azurerm-avm-res-future-thing
            """;

        var modules = AvmDocsService.ParseModuleCsv(csv);

        Assert.Single(modules);
        Assert.Equal("avm-res-storage-storageaccount", modules[0].ModuleName);
    }

    [Fact]
    public void ParseModuleCsv_SkipsEmptyLines()
    {
        var csv = "ModuleName,Description,ModuleStatus,RepoURL\n\navm-res-test,Test,Available,https://github.com/Azure/terraform-azurerm-avm-res-test\n\n";

        var modules = AvmDocsService.ParseModuleCsv(csv);

        Assert.Single(modules);
    }

    [Fact]
    public void ParseModuleCsv_HandlesQuotedFields()
    {
        var csv = """
            ModuleName,Description,ModuleStatus,RepoURL
            avm-res-test,"A module for testing, with commas",Available,https://github.com/Azure/terraform-azurerm-avm-res-test
            """;

        var modules = AvmDocsService.ParseModuleCsv(csv);

        Assert.Single(modules);
        Assert.Equal("A module for testing, with commas", modules[0].Description);
    }

    [Fact]
    public void ParseModuleCsv_EmptyContent_ReturnsEmpty()
    {
        var modules = AvmDocsService.ParseModuleCsv("");

        Assert.Empty(modules);
    }

    [Fact]
    public void ParseModuleCsv_HeaderOnly_ReturnsEmpty()
    {
        var modules = AvmDocsService.ParseModuleCsv("ModuleName,Description,ModuleStatus,RepoURL");

        Assert.Empty(modules);
    }

    [Fact]
    public void ParseCsvLine_SimpleValues()
    {
        var values = AvmDocsService.ParseCsvLine("a,b,c");

        Assert.Equal(3, values.Count);
        Assert.Equal("a", values[0]);
        Assert.Equal("b", values[1]);
        Assert.Equal("c", values[2]);
    }

    [Fact]
    public void ParseCsvLine_QuotedComma()
    {
        var values = AvmDocsService.ParseCsvLine("a,\"b,c\",d");

        Assert.Equal(3, values.Count);
        Assert.Equal("a", values[0]);
        Assert.Equal("b,c", values[1]);
        Assert.Equal("d", values[2]);
    }

    [Fact]
    public void ParseModuleCsv_SkipsMissingNameOrUrl()
    {
        var csv = """
            ModuleName,Description,ModuleStatus,RepoURL
            ,Missing Name,Available,https://github.com/Azure/terraform-azurerm-avm-res-test
            avm-res-test,Missing URL,Available,
            avm-res-valid,Valid,Available,https://github.com/Azure/terraform-azurerm-avm-res-valid
            """;

        var modules = AvmDocsService.ParseModuleCsv(csv);

        Assert.Single(modules);
        Assert.Equal("avm-res-valid", modules[0].ModuleName);
    }
}
