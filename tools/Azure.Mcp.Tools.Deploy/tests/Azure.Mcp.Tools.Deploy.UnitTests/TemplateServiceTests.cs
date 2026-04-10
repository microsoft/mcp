// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Deploy.Services.Templates;
using Xunit;

namespace Azure.Mcp.Tools.Deploy.UnitTests;

public sealed class TemplateServiceTests
{
    [Fact]
    public void LoadTemplate_ValidTemplate_ReturnsContent()
    {
        // Arrange
        var templateName = "Plan/deployment-plan-base";

        // Act
        var result = TemplateService.LoadTemplate(templateName);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains("{{Title}}", result);
        Assert.Contains("{{Goal}}", result);
        Assert.Contains("{{SampleMermaid}}", result);
        Assert.Contains("{{ResourceInfo}}", result);
        Assert.Contains("{{ExecutionSteps}}", result);
    }

    [Fact]
    public void LoadTemplate_InvalidTemplate_ThrowsFileNotFoundException()
    {
        // Arrange
        var templateName = "Plan/non-existent-template";

        // Act & Assert
        Assert.Throws<FileNotFoundException>(() => TemplateService.LoadTemplate(templateName));
    }

    [Fact]
    public void ProcessTemplate_WithReplacements_ReplacesPlaceholders()
    {
        // Arrange
        var templateName = "Plan/deployment-plan-base";
        var replacements = new Dictionary<string, string>
        {
            { "Title", "Test Deployment Plan" },
            { "Goal", "Deploy TestProject to Azure" },
            { "SampleMermaid", "<resource-diagram>" },
            { "ResourceInfo", "Azure existing resources" },
            { "ExecutionSteps", "<execution-steps>" }
        };

        // Act
        var result = TemplateService.ProcessTemplate(templateName, replacements);

        // Assert
        Assert.Contains("Test Deployment Plan", result);
        Assert.Contains("TestProject", result);
        Assert.Contains("<resource-diagram>", result);
        Assert.Contains("Azure existing resources", result);
        Assert.Contains("<execution-steps>", result);
        Assert.DoesNotContain("{{Title}}", result);
        Assert.DoesNotContain("{{Goal}}", result);
        Assert.DoesNotContain("{{SampleMermaid}}", result);
        Assert.DoesNotContain("{{ResourceInfo}}", result);
        Assert.DoesNotContain("{{ExecutionSteps}}", result);
    }

    [Fact]
    public void ProcessTemplateContent_WithReplacements_ReplacesPlaceholders()
    {
        // Arrange
        var templateContent = "Hello {{Name}}, welcome to {{Project}}!";
        var replacements = new Dictionary<string, string>
        {
            { "Name", "John" },
            { "Project", "Azure MCP" }
        };

        // Act
        var result = TemplateService.ProcessTemplateContent(templateContent, replacements);

        // Assert
        Assert.Equal("Hello John, welcome to Azure MCP!", result);
    }

}
