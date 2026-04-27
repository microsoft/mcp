// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.AzureTerraform.Services;
using Xunit;

namespace Azure.Mcp.Tools.AzureTerraform.UnitTests;

public class AztfexportServiceTests
{
    private readonly AztfexportService _service = new();

    [Fact]
    public void GenerateResourceCommand_BasicArgs_ReturnsCorrectCommand()
    {
        var result = _service.GenerateResourceCommand(
            "/subscriptions/sub-id/resourceGroups/rg/providers/Microsoft.Storage/storageAccounts/sa");

        Assert.True(result.AztfexportFound);
        Assert.Equal("aztfexport", result.Command);
        Assert.Contains("resource", result.Args);
        Assert.Contains("--non-interactive", result.Args);
        Assert.Contains("--plain-ui", result.Args);
        Assert.Contains("--continue", result.Args);
        Assert.Contains("/subscriptions/sub-id/resourceGroups/rg/providers/Microsoft.Storage/storageAccounts/sa", result.Args);
    }

    [Fact]
    public void GenerateResourceCommand_AllOptions_IncludesAllArgs()
    {
        var result = _service.GenerateResourceCommand(
            "/subscriptions/sub/resourceGroups/rg/providers/Microsoft.Compute/virtualMachines/vm",
            outputFolderName: "my-output",
            provider: "azapi",
            resourceName: "my_vm",
            includeRoleAssignment: true,
            parallelism: 20,
            continueOnError: false);

        Assert.Contains("--provider-name", result.Args);
        Assert.Contains("azapi", result.Args);
        Assert.Contains("--name", result.Args);
        Assert.Contains("my_vm", result.Args);
        Assert.Contains("--include-role-assignment", result.Args);
        Assert.Contains("--parallelism", result.Args);
        Assert.Contains("20", result.Args);
        Assert.DoesNotContain("--continue", result.Args);
        Assert.Equal("my-output", result.OutputFolderName);
        Assert.Equal("my-output", result.WorkingDirectory);
    }

    [Fact]
    public void GenerateResourceCommand_DefaultProvider_DoesNotIncludeProviderFlag()
    {
        var result = _service.GenerateResourceCommand(
            "/subscriptions/sub/resourceGroups/rg/providers/Microsoft.Storage/storageAccounts/sa",
            provider: "azurerm");

        Assert.DoesNotContain("--provider-name", result.Args);
    }

    [Fact]
    public void GenerateResourceGroupCommand_BasicArgs_ReturnsCorrectCommand()
    {
        var result = _service.GenerateResourceGroupCommand("my-resource-group");

        Assert.True(result.AztfexportFound);
        Assert.Equal("aztfexport", result.Command);
        Assert.Contains("resource-group", result.Args);
        Assert.Contains("--non-interactive", result.Args);
        Assert.Contains("--plain-ui", result.Args);
        Assert.Contains("--continue", result.Args);
        Assert.Contains("my-resource-group", result.Args);
    }

    [Fact]
    public void GenerateResourceGroupCommand_WithNamePattern_IncludesPattern()
    {
        var result = _service.GenerateResourceGroupCommand(
            "my-rg",
            namePattern: "res_*",
            provider: "azapi",
            includeRoleAssignment: true);

        Assert.Contains("--name-pattern", result.Args);
        Assert.Contains("res_*", result.Args);
        Assert.Contains("--provider-name", result.Args);
        Assert.Contains("azapi", result.Args);
        Assert.Contains("--include-role-assignment", result.Args);
    }

    [Fact]
    public void GenerateQueryCommand_BasicArgs_ReturnsCorrectCommand()
    {
        var query = "type =~ 'Microsoft.Storage/storageAccounts'";
        var result = _service.GenerateQueryCommand(query);

        Assert.True(result.AztfexportFound);
        Assert.Equal("aztfexport", result.Command);
        Assert.Contains("query", result.Args);
        Assert.Contains("--non-interactive", result.Args);
        Assert.Contains("--plain-ui", result.Args);
        Assert.Contains(query, result.Args);
    }

    [Fact]
    public void GenerateQueryCommand_LongQuery_TruncatesInDescription()
    {
        var longQuery = "type =~ 'Microsoft.Storage/storageAccounts' and resourceGroup =~ 'my-very-long-resource-group-name-for-testing'";
        var result = _service.GenerateQueryCommand(longQuery);

        Assert.StartsWith("Export Azure resources by query:", result.Description, StringComparison.Ordinal);
        Assert.Contains("...", result.Description);
    }

    [Fact]
    public void GenerateQueryCommand_ShortQuery_DoesNotTruncate()
    {
        var shortQuery = "type =~ 'Microsoft.Compute/VMs'";
        var result = _service.GenerateQueryCommand(shortQuery);

        Assert.DoesNotContain("...", result.Description);
    }

    [Fact]
    public void GenerateQueryCommand_AllOptions_IncludesAllArgs()
    {
        var result = _service.GenerateQueryCommand(
            "type =~ 'Microsoft.Storage/storageAccounts'",
            outputFolderName: "output",
            provider: "azapi",
            namePattern: "storage_*",
            includeRoleAssignment: true,
            parallelism: 5,
            continueOnError: true);

        Assert.Contains("--provider-name", result.Args);
        Assert.Contains("azapi", result.Args);
        Assert.Contains("--name-pattern", result.Args);
        Assert.Contains("storage_*", result.Args);
        Assert.Contains("--include-role-assignment", result.Args);
        Assert.Contains("--parallelism", result.Args);
        Assert.Contains("5", result.Args);
        Assert.Contains("--continue", result.Args);
    }

    [Fact]
    public void NotFoundResult_ReturnsInstallationHelp()
    {
        var result = AztfexportService.NotFoundResult("Test description");

        Assert.False(result.AztfexportFound);
        Assert.Empty(result.Command);
        Assert.Equal("Test description", result.Description);
        Assert.NotNull(result.InstallationHelp);
        Assert.Equal("aztfexport", result.InstallationHelp.ToolName);
        Assert.NotEmpty(result.InstallationHelp.InstallationMethods);
        Assert.NotEmpty(result.InstallationHelp.DocumentationUrl);
        Assert.NotEmpty(result.InstallationHelp.VerifyCommand);
        Assert.NotEmpty(result.InstallationHelp.AdditionalNotes);
    }

    [Fact]
    public void GetInstallationHelp_ContainsAllPlatforms()
    {
        var help = AztfexportService.GetAztfexportInstallationHelp();

        Assert.Contains(help.InstallationMethods, m => m.Platform == "windows");
        Assert.Contains(help.InstallationMethods, m => m.Platform == "macos");
        Assert.Contains(help.InstallationMethods, m => m.Platform == "linux");
    }

    [Fact]
    public void GetInstallationHelp_ContainsMultipleMethods()
    {
        var help = AztfexportService.GetAztfexportInstallationHelp();

        Assert.Contains(help.InstallationMethods, m => m.Method == "winget");
        Assert.Contains(help.InstallationMethods, m => m.Method == "brew");
        Assert.Contains(help.InstallationMethods, m => m.Method == "apt");
        Assert.Contains(help.InstallationMethods, m => m.Method == "manual");
    }

    [Fact]
    public void ResourceCommand_LastArgIsResourceId()
    {
        var resourceId = "/subscriptions/sub/resourceGroups/rg/providers/Microsoft.Storage/storageAccounts/sa";
        var result = _service.GenerateResourceCommand(resourceId);

        Assert.Equal(resourceId, result.Args[^1]);
    }

    [Fact]
    public void ResourceGroupCommand_LastArgIsGroupName()
    {
        var result = _service.GenerateResourceGroupCommand("my-rg");

        Assert.Equal("my-rg", result.Args[^1]);
    }

    [Fact]
    public void QueryCommand_LastArgIsQuery()
    {
        var query = "type =~ 'Microsoft.Storage/storageAccounts'";
        var result = _service.GenerateQueryCommand(query);

        Assert.Equal(query, result.Args[^1]);
    }
}
