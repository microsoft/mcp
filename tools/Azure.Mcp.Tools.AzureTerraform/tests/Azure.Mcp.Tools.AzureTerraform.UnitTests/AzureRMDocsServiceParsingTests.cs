// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.AzureTerraform.Services;
using Xunit;

namespace Azure.Mcp.Tools.AzureTerraform.UnitTests;

public class AzureRMDocsServiceParsingTests
{
    private const string SampleMarkdown = """
        ---
        subcategory: "Resource Group"
        layout: "azurerm"
        page_title: "Azure Resource Manager: azurerm_resource_group"
        description: Manages a Resource Group.
        ---

        # azurerm_resource_group

        Manages a Resource Group.

        ## Example Usage

        ```hcl
        resource "azurerm_resource_group" "example" {
          name     = "example"
          location = "West Europe"
        }
        ```

        ## Argument Reference

        The following arguments are supported:

        * `name` - (Required) The Name which should be used for this Resource Group. Changing this forces a new Resource Group to be created.
        * `location` - (Required) The Azure Region where the Resource Group should exist. Changing this forces a new Resource Group to be created.
        * `tags` - (Optional) A mapping of tags which should be assigned to the Resource Group.

        ## Attributes Reference

        In addition to the Arguments listed above - the following Attributes are exported:

        * `id` - The ID of the Resource Group.

        -> **NOTE:** Some note about the resource.
        """;

    [Fact]
    public void ExtractSummary_ReturnsSummaryFromMarkdown()
    {
        string summary = AzureRMDocsService.ExtractSummary(SampleMarkdown, "azurerm_resource_group", false);

        Assert.Equal("Manages a Resource Group.", summary);
    }

    [Fact]
    public void ExtractArguments_ReturnsArgumentsFromMarkdown()
    {
        var args = AzureRMDocsService.ExtractArguments(SampleMarkdown, false);

        Assert.Equal(3, args.Count);
        Assert.Equal("name", args[0].Name);
        Assert.True(args[0].Required);
        Assert.Equal("location", args[1].Name);
        Assert.True(args[1].Required);
        Assert.Equal("tags", args[2].Name);
        Assert.False(args[2].Required);
    }

    [Fact]
    public void ExtractAttributes_ReturnsAttributesFromMarkdown()
    {
        var attrs = AzureRMDocsService.ExtractAttributes(SampleMarkdown);

        Assert.Contains(attrs, a => a.Name == "id");
    }

    [Fact]
    public void ExtractExamples_ReturnsExamplesFromMarkdown()
    {
        var examples = AzureRMDocsService.ExtractExamples(SampleMarkdown, "resource_group", false);

        Assert.NotEmpty(examples);
        Assert.Contains("azurerm_resource_group", examples[0]);
    }

    [Fact]
    public void ExtractNotes_ReturnsNotesFromMarkdown()
    {
        var notes = AzureRMDocsService.ExtractNotes(SampleMarkdown);

        Assert.NotEmpty(notes);
        Assert.Contains(notes, n => n.Contains("note about the resource", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void ExtractSummary_WithDataSource_ReturnsDefaultSummary()
    {
        const string markdown = """
            ---
            layout: "azurerm"
            ---
            Short line
            """;

        string summary = AzureRMDocsService.ExtractSummary(markdown, "azurerm_resource_group", true);

        Assert.Contains("data source", summary, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ExtractArguments_WithNoArguments_ReturnsDefaults()
    {
        const string markdown = """
            ---
            layout: "azurerm"
            ---
            Some resource description that is long enough.
            """;

        var args = AzureRMDocsService.ExtractArguments(markdown, false);

        Assert.NotEmpty(args);
        Assert.Contains(args, a => a.Name == "name");
        Assert.Contains(args, a => a.Name == "location");
    }

    [Fact]
    public void ExtractBlockDefinitions_ParsesBlockArguments()
    {
        const string markdown = """
            ## Argument Reference

            * `identity` - (Optional) An identity block as defined below.

            An `identity` block supports the following:

            * `type` - (Required) The type of identity.
            * `identity_ids` - (Optional) A list of identity IDs.

            ## Attributes Reference
            """;

        var blocks = AzureRMDocsService.ExtractBlockDefinitions(markdown);

        Assert.True(blocks.ContainsKey("identity"));
        Assert.Equal(2, blocks["identity"].Count);
        Assert.Equal("type", blocks["identity"][0].Name);
        Assert.True(blocks["identity"][0].Required);
    }
}
