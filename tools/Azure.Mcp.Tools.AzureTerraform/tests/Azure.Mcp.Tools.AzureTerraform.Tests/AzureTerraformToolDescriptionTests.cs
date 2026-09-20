// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.AzureTerraform.Commands;
using Azure.Mcp.Tools.AzureTerraform.Commands.Conftest;
using Microsoft.Mcp.Core.Commands;
using Xunit;

namespace Azure.Mcp.Tools.AzureTerraform.Tests;

/// <summary>
/// Guards the phrases that keep tools from competing for the same prompts. Tool selection is
/// driven by these descriptions, so rewording them can silently regress which tool a model picks
/// for a prompt. See https://github.com/microsoft/mcp/issues/3553.
/// </summary>
public sealed class AzureTerraformToolDescriptionTests
{
    [Fact]
    public void ResourceExport_IdentifiesResourceIdAndExcludesResourceGroupScope()
    {
        var description = GetDescription<AztfexportResourceCommand>();

        Assert.Contains("resource ID", description, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("/subscriptions/", description);
        // Keeps a full resource ID from selecting the resource group tool.
        Assert.Contains("not the resource group", description, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("does not execute", description, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("azapi", description, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ResourceGroupExport_IdentifiesGroupNameRatherThanResourceId()
    {
        var description = GetDescription<AztfexportResourceGroupCommand>();

        Assert.Contains("resource group", description, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("identifying one resource by its Azure resource ID", description, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("does not execute", description, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("azapi", description, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void AzureRMDocs_KeysOnResourcePrefixAndDisclaimsLiveResourceAccess()
    {
        var description = GetDescription<AzureRMDocsGetCommand>();

        // Separates documentation lookups from the tools that read live Azure resources.
        Assert.Contains("azurerm_", description);
        Assert.Contains("does not query", description, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void AzApiDocs_KeysOnArmNamespaceFormatAndDisclaimsLiveResourceAccess()
    {
        var description = GetDescription<AzApiDocsGetCommand>();

        Assert.Contains("ARM", description);
        Assert.Contains("namespace format", description, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Microsoft.Storage/storageAccounts", description);
        Assert.Contains("does not query", description, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ConftestWorkspace_IdentifiesSourceFilesRatherThanPlanFile()
    {
        var description = GetDescription<ConftestWorkspaceValidationCommand>();

        Assert.Contains(".tf", description);
        Assert.Contains("rather than an already generated Terraform plan JSON file", description, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ConftestPlan_IdentifiesPlanFileRatherThanSourceFiles()
    {
        var description = GetDescription<ConftestPlanValidationCommand>();

        Assert.Contains("tfplan.json", description);
        Assert.Contains("not a folder of Terraform .tf configuration files", description, StringComparison.OrdinalIgnoreCase);
    }

    // The namespace group description is asserted against the live tool listing in
    // AztfexportToolLoadingTests.NamespaceDiscovery_DescribesExportAndFiltersLocalCommands.

    private static string GetDescription<TCommand>()
    {
        var metadata = typeof(TCommand).GetCustomAttributes(typeof(CommandMetadataAttribute), false)
            .Cast<CommandMetadataAttribute>()
            .Single();

        Assert.False(string.IsNullOrWhiteSpace(metadata.Description));
        return metadata.Description;
    }
}
