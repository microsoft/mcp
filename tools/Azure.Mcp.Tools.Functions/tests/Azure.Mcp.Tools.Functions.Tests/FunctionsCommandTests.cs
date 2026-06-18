// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Tools.Functions.Commands;
using Azure.Mcp.Tools.Functions.Models;
using Microsoft.Mcp.Tests.Attributes;
using Microsoft.Mcp.Tests.Client;
using Microsoft.Mcp.Tests.Client.Helpers;
using Xunit;

namespace Azure.Mcp.Tools.Functions.Tests;

/// <summary>
/// Live tests for the LanguageListCommand which fetches supported languages and runtime versions from the
/// Azure Functions manifest and TemplateGetCommand which performs template listing and file retrieval for all
/// supported languages.
/// 
/// Note: Most tests are marked [LiveTestOnly] because they depend on in-memory cached
/// CDN manifest data. Only the first test (ExecuteAsync_ReturnsAllSupportedLanguages)
/// fetches from CDN and can be recorded. Recorded tests with non-deterministic execution order would fail.
/// </summary>
[Trait("Command", "LanguageListCommand")]
[Trait("Command", "TemplateGetCommand")]
public class FunctionsCommandTests(ITestOutputHelper output, TestProxyFixture fixture, LiveServerFixture liveServerFixture)
    : RecordedCommandTestsBase(output, fixture, liveServerFixture)
{
    private static readonly string[] s_expectedLanguages = ["python", "typescript", "javascript", "csharp", "java", "powershell"];

    /// <summary>
    /// Disable default sanitizer additions since Functions tests don't have
    /// Azure resources (no ResourceBaseName or SubscriptionId environment variables).
    /// Using empty strings in regex sanitizers causes test proxy 400 errors.
    /// </summary>
    public override bool EnableDefaultSanitizerAdditions => false;

    #region Helper Methods

    private async Task<LanguageListResult> GetLanguageListAsync()
    {
        var result = await CallToolAsync("functions_language_list", new());
        Assert.NotNull(result);
        var languageResults = JsonSerializer.Deserialize(result.Value, FunctionsJsonContext.Default.ListLanguageListResult);
        Assert.NotNull(languageResults);
        Assert.Single(languageResults);
        return languageResults[0];
    }

    private static LanguageDetails GetLanguage(LanguageListResult languageList, string languageKey)
    {
        var language = languageList.Languages.FirstOrDefault(l => l.Language == languageKey);
        Assert.NotNull(language);
        return language;
    }

    #endregion

    #region Core Language List Tests

    /// <summary>
    /// Primary test that fetches from CDN and can be recorded.
    /// All other tests depend on cached manifest and are marked [LiveTestOnly].
    /// </summary>
    [Fact]
    public async Task ExecuteAsync_ReturnsAllSupportedLanguages()
    {
        // Act
        var languageList = await GetLanguageListAsync();

        // Assert
        Assert.NotEmpty(languageList.FunctionsRuntimeVersion);
        Assert.NotEmpty(languageList.ExtensionBundleVersion);

        // Verify all 6 expected languages are present
        Assert.Equal(6, languageList.Languages.Count);
        var languageNames = languageList.Languages.Select(l => l.Language).ToList();
        foreach (var expected in s_expectedLanguages)
        {
            Assert.Contains(expected, languageNames);
        }
    }

    [Fact]
    [LiveTestOnly]
    public async Task ExecuteAsync_ReturnsCorrectLanguageInfo_AllLanguages()
    {
        // Act
        var languageList = await GetLanguageListAsync();

        // Assert - Verify each language has correct metadata
        var expectations = new Dictionary<string, (string Name, string Runtime, string Model)>
        {
            ["python"] = ("Python", "python", "v2 (Decorator-based)"),
            ["typescript"] = ("Node.js - TypeScript", "node", "v4 (Schema-based)"),
            ["javascript"] = ("Node.js - JavaScript", "node", "v4 (Schema-based)"),
            ["java"] = ("Java", "java", "Annotations-based"),
            ["csharp"] = ("dotnet-isolated - C#", "dotnet", "Isolated worker process"),
            ["powershell"] = ("PowerShell", "powershell", "Script-based")
        };

        foreach (var (languageKey, expected) in expectations)
        {
            var language = GetLanguage(languageList, languageKey);
            Assert.Equal(expected.Name, language.Info.Name);
            Assert.Equal(expected.Runtime, language.Info.Runtime);
            Assert.Equal(expected.Model, language.Info.ProgrammingModel);
            Assert.NotEmpty(language.Info.Prerequisites);
            Assert.NotEmpty(language.Info.DevelopmentTools);
            Assert.NotEmpty(language.Info.InitCommand);
            Assert.NotEmpty(language.Info.RunCommand);
            Assert.NotEmpty(language.Info.InitInstructions);
            Assert.NotEmpty(language.Info.ProjectStructure);
        }
    }

    [Fact]
    [LiveTestOnly]
    public async Task ExecuteAsync_ReturnsRuntimeVersions_AllLanguages()
    {
        // Act
        var languageList = await GetLanguageListAsync();

        // Assert - All languages have valid runtime versions
        foreach (var languageKey in s_expectedLanguages)
        {
            var language = GetLanguage(languageList, languageKey);

            Assert.NotNull(language.RuntimeVersions);
            Assert.NotEmpty(language.RuntimeVersions.Supported);
            Assert.NotEmpty(language.RuntimeVersions.Default);

            // Default should be one of the supported versions
            Assert.Contains(language.RuntimeVersions.Default, language.RuntimeVersions.Supported);

            // Same versions should be in Info.RuntimeVersions
            Assert.NotNull(language.Info.RuntimeVersions);
            Assert.Equal(language.RuntimeVersions.Default, language.Info.RuntimeVersions.Default);
            Assert.Equal(language.RuntimeVersions.Supported, language.Info.RuntimeVersions.Supported);
        }
    }

    #endregion

    #region Template Parameters Tests

    [Fact]
    [LiveTestOnly]
    public async Task ExecuteAsync_TypeScript_HasNodeVersionParameter()
    {
        var languageList = await GetLanguageListAsync();
        var language = GetLanguage(languageList, "typescript");

        Assert.NotNull(language.Info.TemplateParameters);
        Assert.Single(language.Info.TemplateParameters);

        var param = language.Info.TemplateParameters[0];
        Assert.Equal("nodeVersion", param.Name);
        Assert.NotEmpty(param.Description);
        Assert.NotEmpty(param.DefaultValue);
        Assert.NotNull(param.ValidValues);
        Assert.NotEmpty(param.ValidValues);

        // ValidValues should include all supported versions
        foreach (var supported in language.RuntimeVersions.Supported)
        {
            Assert.Contains(supported, param.ValidValues);
        }
    }

    [Fact]
    [LiveTestOnly]
    public async Task ExecuteAsync_JavaScript_HasNodeVersionParameter()
    {
        var languageList = await GetLanguageListAsync();
        var language = GetLanguage(languageList, "javascript");

        Assert.NotNull(language.Info.TemplateParameters);
        Assert.Single(language.Info.TemplateParameters);

        var param = language.Info.TemplateParameters[0];
        Assert.Equal("nodeVersion", param.Name);
        Assert.NotEmpty(param.DefaultValue);
        Assert.NotNull(param.ValidValues);
        Assert.NotEmpty(param.ValidValues);
    }

    [Fact]
    [LiveTestOnly]
    public async Task ExecuteAsync_Java_HasJavaVersionParameter()
    {
        var languageList = await GetLanguageListAsync();
        var language = GetLanguage(languageList, "java");

        Assert.NotNull(language.Info.TemplateParameters);
        Assert.Single(language.Info.TemplateParameters);

        var param = language.Info.TemplateParameters[0];
        Assert.Equal("javaVersion", param.Name);
        Assert.NotEmpty(param.DefaultValue);
        Assert.NotNull(param.ValidValues);
        Assert.NotEmpty(param.ValidValues);
    }

    [Fact]
    [LiveTestOnly]
    public async Task ExecuteAsync_LanguagesWithoutTemplateParameters()
    {
        var languageList = await GetLanguageListAsync();

        // Python, C#, and PowerShell don't have template parameters
        var languagesWithoutParams = new[] { "python", "csharp", "powershell" };
        foreach (var languageKey in languagesWithoutParams)
        {
            var language = GetLanguage(languageList, languageKey);
            Assert.Null(language.Info.TemplateParameters);
        }
    }

    #endregion

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

    #region Template List Tests - All Languages

    /// <summary>
    /// Primary test that fetches from CDN and can be recorded.
    /// Other list tests depend on cached manifest and are marked [LiveTestOnly].
    /// </summary>
    [Fact]
    public async Task ExecuteAsync_ListTemplates_Python_ReturnsTemplates()
    {
        var templateList = await GetTemplateListAsync("python");

        Assert.Equal("python", templateList.Language);
        Assert.NotNull(templateList.Triggers);
        Assert.NotEmpty(templateList.Triggers);
        Output.WriteLine($"python: {templateList.Triggers.Count} templates available");
    }

    [Fact]
    [LiveTestOnly]
    public async Task ExecuteAsync_ListTemplates_TypeScript_ReturnsTemplates()
    {
        var templateList = await GetTemplateListAsync("typescript");

        Assert.Equal("typescript", templateList.Language);
        Assert.NotNull(templateList.Triggers);
        Assert.NotEmpty(templateList.Triggers);
        Output.WriteLine($"typescript: {templateList.Triggers.Count} templates available");
    }

    [Fact]
    [LiveTestOnly]
    public async Task ExecuteAsync_ListTemplates_JavaScript_ReturnsTemplates()
    {
        var templateList = await GetTemplateListAsync("javascript");

        Assert.Equal("javascript", templateList.Language);
        Assert.NotNull(templateList.Triggers);
        Assert.NotEmpty(templateList.Triggers);
        Output.WriteLine($"javascript: {templateList.Triggers.Count} templates available");
    }

    [Fact]
    [LiveTestOnly]
    public async Task ExecuteAsync_ListTemplates_CSharp_ReturnsTemplates()
    {
        var templateList = await GetTemplateListAsync("csharp");

        Assert.Equal("csharp", templateList.Language);
        Assert.NotNull(templateList.Triggers);
        Assert.NotEmpty(templateList.Triggers);
        Output.WriteLine($"csharp: {templateList.Triggers.Count} templates available");
    }

    [Fact]
    [LiveTestOnly]
    public async Task ExecuteAsync_ListTemplates_Java_ReturnsTemplates()
    {
        var templateList = await GetTemplateListAsync("java");

        Assert.Equal("java", templateList.Language);
        Assert.NotNull(templateList.Triggers);
        Assert.NotEmpty(templateList.Triggers);
        Output.WriteLine($"java: {templateList.Triggers.Count} templates available");
    }

    [Fact]
    [LiveTestOnly]
    public async Task ExecuteAsync_ListTemplates_PowerShell_ReturnsTemplates()
    {
        var templateList = await GetTemplateListAsync("powershell");

        Assert.Equal("powershell", templateList.Language);
        Assert.NotNull(templateList.Triggers);
        Assert.NotEmpty(templateList.Triggers);
        Output.WriteLine($"powershell: {templateList.Triggers.Count} templates available");
    }

    #endregion

    #region HTTP Trigger Tests - Template File Retrieval

    [Fact]
    [LiveTestOnly]
    public async Task ExecuteAsync_HttpTrigger_Python_ReturnsTemplateWithFiles()
    {
        // Arrange
        var templateList = await GetTemplateListAsync("python");
        var httpTemplate = FindTemplateByPattern(templateList, "http-trigger-python");
        Assert.NotNull(httpTemplate);

        // Act - Default mode is "New" which returns all files in "files" property
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
        Assert.NotNull(templateResult.FunctionTemplate.Files);
        Assert.NotEmpty(templateResult.FunctionTemplate.Files);
    }

    [Fact]
    [LiveTestOnly]
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
    [LiveTestOnly]
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

    [Fact]
    [LiveTestOnly]
    public async Task ExecuteAsync_HttpTrigger_AddOutput_ReturnsSeparatedFiles()
    {
        // Arrange
        var templateList = await GetTemplateListAsync("python");
        var httpTemplate = FindTemplateByPattern(templateList, "http-trigger-python");
        Assert.NotNull(httpTemplate);

        // Act - Use Add output to get separated files with merge instructions
        var result = await CallToolAsync(
            "functions_template_get",
            new()
            {
                { "language", "python" },
                { "template", httpTemplate },
                { "output", "Add" }
            });

        // Assert
        Assert.NotNull(result);
        var templateResult = JsonSerializer.Deserialize(result.Value, FunctionsJsonContext.Default.TemplateGetCommandResult);
        Assert.NotNull(templateResult?.FunctionTemplate);
        Assert.Equal("python", templateResult.FunctionTemplate.Language);

        // Add output should return separated files, not combined
        Assert.Null(templateResult.FunctionTemplate.Files);
        Assert.NotNull(templateResult.FunctionTemplate.FunctionFiles);
        Assert.NotEmpty(templateResult.FunctionTemplate.FunctionFiles);
        Assert.NotNull(templateResult.FunctionTemplate.ProjectFiles);
        Assert.NotNull(templateResult.FunctionTemplate.MergeInstructions);
    }

    #endregion

    #region Caching Tests

    [Fact]
    [LiveTestOnly]
    public async Task ExecuteAsync_LanguageListThenTemplate_UsesSharedCache()
    {
        // This test verifies that the manifest cache is shared between commands.
        // First call fetches and caches the manifest, second call uses cached manifest.
        //
        // IMPLICIT VERIFICATION: The test proxy records all HTTP calls during Record mode.
        // If caching didn't work, template_get would make a CDN manifest call that gets recorded.
        // In Playback mode, if caching breaks, it would try to make a CDN call that wasn't
        // recorded, causing the test to fail. This implicitly asserts no CDN call is made.

        // Act - First call: language_list fetches and caches the manifest
        var langResult = await CallToolAsync("functions_language_list", new());

        Assert.NotNull(langResult);
        var langList = JsonSerializer.Deserialize(langResult.Value, FunctionsJsonContext.Default.ListLanguageListResult);
        Assert.NotNull(langList);

        // Act - Second call: template_get should use cached manifest (no CDN call)
        var templateResult = await CallToolAsync(
            "functions_template_get",
            new() { { "language", "python" } });

        // Assert - Both return valid results
        Assert.NotNull(templateResult);
        var template = JsonSerializer.Deserialize(templateResult.Value, FunctionsJsonContext.Default.TemplateGetCommandResult);
        Assert.NotNull(template?.TemplateList);
        Assert.Equal("python", template.TemplateList.Language);
    }

    #endregion

    #region Runtime Version Replacement

    [Fact]
    [LiveTestOnly]
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

        // Act - Request with runtime version (default New mode returns combined files)
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
        Assert.NotNull(templateResult.FunctionTemplate.Files);

        // Verify no unreplaced placeholders in combined files list
        foreach (var file in templateResult.FunctionTemplate.Files)
        {
            Assert.DoesNotContain("{{pythonVersion}}", file.Content);
        }
    }

    #endregion

    #region Error Handling

    [Fact]
    [LiveTestOnly]
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
    [LiveTestOnly]
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
