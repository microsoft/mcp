// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Tools.Functions.Commands;
using Azure.Mcp.Tools.Functions.Models;
using Microsoft.Mcp.Tests.Attributes;
using Microsoft.Mcp.Tests.Client;
using Microsoft.Mcp.Tests.Client.Helpers;
using Xunit;

namespace Azure.Mcp.Tools.Functions.LiveTests.Language;

/// <summary>
/// Live tests for the LanguageListCommand which fetches supported languages
/// and runtime versions from the Azure Functions manifest.
/// 
/// Note: Most tests are marked [LiveTestOnly] because they depend on in-memory cached
/// CDN manifest data. Only the first test (ExecuteAsync_ReturnsAllSupportedLanguages)
/// fetches from CDN and can be recorded. Recorded tests with non-deterministic execution order would fail.
/// </summary>
[Trait("Command", "LanguageListCommand")]
public class LanguageListCommandLiveTests(
    ITestOutputHelper output,
    TestProxyFixture fixture,
    LiveServerFixture liveServerFixture)
    : BaseFunctionsCommandLiveTests(output, fixture, liveServerFixture)
{
    private static readonly string[] ExpectedLanguages = ["python", "typescript", "javascript", "csharp", "java", "powershell"];

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
        foreach (var expected in ExpectedLanguages)
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
        foreach (var languageKey in ExpectedLanguages)
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
}
