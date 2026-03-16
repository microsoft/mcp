// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Tools.Functions.Commands;
using Azure.Mcp.Tools.Functions.Models;
using Microsoft.Mcp.Tests.Client;
using Microsoft.Mcp.Tests.Client.Helpers;
using Xunit;

namespace Azure.Mcp.Tools.Functions.LiveTests.Template;

/// <summary>
/// Live tests for the TemplateGetCommand. Minimized test set to avoid GitHub API rate limits.
/// Tests focus on: all languages work, basic triggers, caching, and error handling.
/// </summary>
[Trait("Command", "TemplateGetCommand")]
public class TemplateGetCommandLiveTests(
    ITestOutputHelper output,
    TestProxyFixture fixture,
    LiveServerFixture liveServerFixture)
    : BaseFunctionsCommandLiveTests(output, fixture, liveServerFixture)
{
    #region Helper Methods

    private async Task<TemplateListResult> GetTemplateListAsync(string language)
    {
        var result = await CallToolAsync(
            "functions_template_get",
            new() { { "language", language } });

        Assert.NotNull(result);
        var templateResult = JsonSerializer.Deserialize(result.Value, FunctionsJsonContext.Default.TemplateGetCommandResult);
        Assert.NotNull(templateResult?.TemplateList);
        return templateResult.TemplateList;
    }

    private static string? FindTemplateByPattern(TemplateListResult templateList, string pattern)
    {
        return templateList.Triggers?
            .FirstOrDefault(t => t.TemplateName.Contains(pattern, StringComparison.OrdinalIgnoreCase))
            ?.TemplateName;
    }

    #endregion

    #region All Languages - Template List

    [Theory]
    [InlineData("python")]
    [InlineData("typescript")]
    [InlineData("javascript")]
    [InlineData("csharp")]
    [InlineData("java")]
    public async Task ExecuteAsync_ListTemplates_AllLanguages_ReturnsTemplates(string language)
    {
        // Act - List templates for each language (no file download, just manifest read)
        var templateList = await GetTemplateListAsync(language);

        // Assert
        Assert.Equal(language, templateList.Language);
        Assert.NotNull(templateList.Triggers);
        Assert.NotEmpty(templateList.Triggers);
        Output.WriteLine($"{language}: {templateList.Triggers.Count} templates available");
    }

    #endregion

    #region Basic Trigger Tests - One Language Each

    [Fact]
    public async Task ExecuteAsync_HttpTrigger_Python_ReturnsTemplateWithFiles()
    {
        // Arrange
        var templateList = await GetTemplateListAsync("python");
        var httpTemplate = FindTemplateByPattern(templateList, "http-trigger-python");
        Assert.NotNull(httpTemplate);

        // Act
        var result = await CallToolAsync(
            "functions_template_get",
            new()
            {
                { "language", "python" },
                { "template", httpTemplate }
            });

        // Assert
        Assert.NotNull(result);
        var templateResult = JsonSerializer.Deserialize(result.Value, FunctionsJsonContext.Default.TemplateGetCommandResult);
        Assert.NotNull(templateResult?.FunctionTemplate);
        Assert.Equal("python", templateResult.FunctionTemplate.Language);
        Assert.NotNull(templateResult.FunctionTemplate.FunctionFiles);
    }

    [Fact]
    public async Task ExecuteAsync_HttpTrigger_TypeScript_ReturnsTemplateWithFiles()
    {
        // Arrange
        var templateList = await GetTemplateListAsync("typescript");
        var httpTemplate = FindTemplateByPattern(templateList, "http");
        Assert.NotNull(httpTemplate);

        // Act
        var result = await CallToolAsync(
            "functions_template_get",
            new()
            {
                { "language", "typescript" },
                { "template", httpTemplate }
            });

        // Assert
        Assert.NotNull(result);
        var templateResult = JsonSerializer.Deserialize(result.Value, FunctionsJsonContext.Default.TemplateGetCommandResult);
        Assert.NotNull(templateResult?.FunctionTemplate);
        Assert.Equal("typescript", templateResult.FunctionTemplate.Language);
    }

    [Fact]
    public async Task ExecuteAsync_HttpTrigger_CSharp_ReturnsTemplateWithFiles()
    {
        // Arrange
        var templateList = await GetTemplateListAsync("csharp");
        var httpTemplate = FindTemplateByPattern(templateList, "http");
        Assert.NotNull(httpTemplate);

        // Act
        var result = await CallToolAsync(
            "functions_template_get",
            new()
            {
                { "language", "csharp" },
                { "template", httpTemplate }
            });

        // Assert
        Assert.NotNull(result);
        var templateResult = JsonSerializer.Deserialize(result.Value, FunctionsJsonContext.Default.TemplateGetCommandResult);
        Assert.NotNull(templateResult?.FunctionTemplate);
        Assert.Equal("csharp", templateResult.FunctionTemplate.Language);
    }

    #endregion

    #region Caching Tests

    [Fact]
    public async Task ExecuteAsync_SameTemplate_TwoCalls_SecondUsesCache()
    {
        // Arrange - Get template list for Python
        var templateList = await GetTemplateListAsync("python");
        var httpTemplate = FindTemplateByPattern(templateList, "http-trigger-python");
        Assert.NotNull(httpTemplate);

        // Act - First call (fetches from GitHub)
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var result1 = await CallToolAsync(
            "functions_template_get",
            new()
            {
                { "language", "python" },
                { "template", httpTemplate }
            });
        stopwatch.Stop();
        var firstCallMs = stopwatch.ElapsedMilliseconds;

        // Act - Second call (should use cache)
        stopwatch.Restart();
        var result2 = await CallToolAsync(
            "functions_template_get",
            new()
            {
                { "language", "python" },
                { "template", httpTemplate }
            });
        stopwatch.Stop();
        var secondCallMs = stopwatch.ElapsedMilliseconds;

        // Assert - Both return valid results
        Assert.NotNull(result1);
        Assert.NotNull(result2);

        // Second call should be faster (cached) - log times for debugging
        Output.WriteLine($"First call: {firstCallMs}ms, Second call: {secondCallMs}ms");

        // Verify content is identical
        var template1 = JsonSerializer.Deserialize(result1.Value, FunctionsJsonContext.Default.TemplateGetCommandResult);
        var template2 = JsonSerializer.Deserialize(result2.Value, FunctionsJsonContext.Default.TemplateGetCommandResult);
        Assert.NotNull(template1?.FunctionTemplate);
        Assert.NotNull(template2?.FunctionTemplate);
        Assert.Equal(template1.FunctionTemplate.FunctionFiles?.Count, template2.FunctionTemplate.FunctionFiles?.Count);
    }

    #endregion

    #region Runtime Version Replacement

    [Fact]
    public async Task ExecuteAsync_WithRuntimeVersion_ReplacesPlaceholders()
    {
        // Get valid runtime version from language list
        var langResult = await CallToolAsync("functions_language_list", new());
        Assert.NotNull(langResult);
        var langList = JsonSerializer.Deserialize(langResult.Value, FunctionsJsonContext.Default.ListLanguageListResult);
        Assert.NotNull(langList);

        var pythonLang = langList[0].Languages.FirstOrDefault(l => l.Language == "python");
        Assert.NotNull(pythonLang?.RuntimeVersions?.Supported);
        var runtimeVersion = pythonLang.RuntimeVersions.Supported[0];

        // Get a Python template
        var templateList = await GetTemplateListAsync("python");
        var httpTemplate = FindTemplateByPattern(templateList, "http-trigger-python");
        Assert.NotNull(httpTemplate);

        // Act - Request with runtime version
        var result = await CallToolAsync(
            "functions_template_get",
            new()
            {
                { "language", "python" },
                { "template", httpTemplate },
                { "runtime-version", runtimeVersion }
            });

        // Assert
        Assert.NotNull(result);
        var templateResult = JsonSerializer.Deserialize(result.Value, FunctionsJsonContext.Default.TemplateGetCommandResult);
        Assert.NotNull(templateResult?.FunctionTemplate);

        // Verify no unreplaced placeholders
        foreach (var file in templateResult.FunctionTemplate.FunctionFiles ?? [])
        {
            Assert.DoesNotContain("{{pythonVersion}}", file.Content);
        }
    }

    #endregion

    #region Error Handling

    [Fact]
    public async Task ExecuteAsync_InvalidLanguage_ReturnsError()
    {
        // Act - Invalid language returns validation error (no "results" property)
        var result = await CallToolAsync(
            "functions_template_get",
            new() { { "language", "invalid_language" } });

        // Validation errors return null (status 400, no results property)
        Assert.Null(result);
    }

    [Fact]
    public async Task ExecuteAsync_InvalidTemplate_ReturnsError()
    {
        // Act - Invalid template name returns error with details
        var result = await CallToolAsync(
            "functions_template_get",
            new()
            {
                { "language", "python" },
                { "template", "NonExistentTemplate12345" }
            });

        // Service errors include "results" with error details
        Assert.NotNull(result);
        var json = result.Value.ToString();
        Assert.Contains("not found", json);
    }

    #endregion
}
