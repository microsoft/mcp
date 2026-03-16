// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Tools.Functions.Commands;
using Azure.Mcp.Tools.Functions.Models;
using Microsoft.Mcp.Tests.Client;
using Microsoft.Mcp.Tests.Client.Helpers;
using Xunit;

namespace Azure.Mcp.Tools.Functions.LiveTests.Language;

/// <summary>
/// Live tests for the LanguageListCommand which fetches supported languages
/// and runtime versions from the Azure Functions manifest.
/// </summary>
[Trait("Command", "LanguageListCommand")]
public class LanguageListCommandLiveTests(
    ITestOutputHelper output,
    TestProxyFixture fixture,
    LiveServerFixture liveServerFixture)
    : BaseFunctionsCommandLiveTests(output, fixture, liveServerFixture)
{
    private static readonly string[] ExpectedLanguages = ["python", "typescript", "javascript", "csharp", "java", "powershell"];

    [Fact]
    public async Task ExecuteAsync_ReturnsAllSupportedLanguages()
    {
        // Act
        var result = await CallToolAsync("functions_language_list", new());

        // Assert
        Assert.NotNull(result);
        var languageResults = JsonSerializer.Deserialize(result.Value, FunctionsJsonContext.Default.ListLanguageListResult);
        Assert.NotNull(languageResults);
        Assert.Single(languageResults);

        var languageList = languageResults[0];
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

    [Theory]
    [InlineData("python", "Python", "python", "v2 (Decorator-based)")]
    [InlineData("typescript", "Node.js - TypeScript", "node", "v4 (Schema-based)")]
    [InlineData("javascript", "Node.js - JavaScript", "node", "v4 (Schema-based)")]
    [InlineData("java", "Java", "java", "Annotations-based")]
    [InlineData("csharp", "dotnet-isolated - C#", "dotnet", "Isolated worker process")]
    [InlineData("powershell", "PowerShell", "powershell", "Script-based")]
    public async Task ExecuteAsync_ReturnsCorrectLanguageInfo(
        string languageKey,
        string expectedName,
        string expectedRuntime,
        string expectedModel)
    {
        // Act
        var result = await CallToolAsync("functions_language_list", new());

        // Assert
        Assert.NotNull(result);
        var languageResults = JsonSerializer.Deserialize(result.Value, FunctionsJsonContext.Default.ListLanguageListResult);
        Assert.NotNull(languageResults);

        var languageList = languageResults[0];
        var language = languageList.Languages.FirstOrDefault(l => l.Language == languageKey);
        Assert.NotNull(language);

        // Verify LanguageInfo properties
        Assert.Equal(expectedName, language.Info.Name);
        Assert.Equal(expectedRuntime, language.Info.Runtime);
        Assert.Equal(expectedModel, language.Info.ProgrammingModel);
        Assert.NotEmpty(language.Info.Prerequisites);
        Assert.NotEmpty(language.Info.DevelopmentTools);
        Assert.NotEmpty(language.Info.InitCommand);
        Assert.NotEmpty(language.Info.RunCommand);
        Assert.NotEmpty(language.Info.InitInstructions);
        Assert.NotEmpty(language.Info.ProjectStructure);
    }

    [Theory]
    [InlineData("python")]
    [InlineData("typescript")]
    [InlineData("javascript")]
    [InlineData("java")]
    [InlineData("csharp")]
    [InlineData("powershell")]
    public async Task ExecuteAsync_ReturnsRuntimeVersionsFromManifest(string languageKey)
    {
        // Act
        var result = await CallToolAsync("functions_language_list", new());

        // Assert
        Assert.NotNull(result);
        var languageResults = JsonSerializer.Deserialize(result.Value, FunctionsJsonContext.Default.ListLanguageListResult);
        Assert.NotNull(languageResults);

        var languageList = languageResults[0];
        var language = languageList.Languages.FirstOrDefault(l => l.Language == languageKey);
        Assert.NotNull(language);

        // Verify RuntimeVersions from manifest - don't hardcode specific versions
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

    [Theory]
    [InlineData("typescript", "nodeVersion")]
    [InlineData("javascript", "nodeVersion")]
    [InlineData("java", "javaVersion")]
    public async Task ExecuteAsync_ReturnsTemplateParametersWithValidValues(string languageKey, string expectedParamName)
    {
        // Act
        var result = await CallToolAsync("functions_language_list", new());

        // Assert
        Assert.NotNull(result);
        var languageResults = JsonSerializer.Deserialize(result.Value, FunctionsJsonContext.Default.ListLanguageListResult);
        Assert.NotNull(languageResults);

        var languageList = languageResults[0];
        var language = languageList.Languages.FirstOrDefault(l => l.Language == languageKey);
        Assert.NotNull(language);

        // Verify TemplateParameters are populated from runtime versions
        Assert.NotNull(language.Info.TemplateParameters);
        Assert.Single(language.Info.TemplateParameters);

        var param = language.Info.TemplateParameters[0];
        Assert.Equal(expectedParamName, param.Name);
        Assert.NotEmpty(param.Description);
        Assert.NotEmpty(param.DefaultValue);
        Assert.NotNull(param.ValidValues);
        Assert.NotEmpty(param.ValidValues);

        // ValidValues should include all supported + preview versions
        var runtimeVersions = language.RuntimeVersions;
        foreach (var supported in runtimeVersions.Supported)
        {
            Assert.Contains(supported, param.ValidValues);
        }
        if (runtimeVersions.Preview is not null)
        {
            foreach (var preview in runtimeVersions.Preview)
            {
                Assert.Contains(preview, param.ValidValues);
            }
        }
    }

    [Theory]
    [InlineData("python")]
    [InlineData("csharp")]
    [InlineData("powershell")]
    public async Task ExecuteAsync_LanguagesWithoutTemplateParameters_ReturnsNull(string languageKey)
    {
        // Act
        var result = await CallToolAsync("functions_language_list", new());

        // Assert
        Assert.NotNull(result);
        var languageResults = JsonSerializer.Deserialize(result.Value, FunctionsJsonContext.Default.ListLanguageListResult);
        Assert.NotNull(languageResults);

        var languageList = languageResults[0];
        var language = languageList.Languages.FirstOrDefault(l => l.Language == languageKey);
        Assert.NotNull(language);

        // These languages don't have template parameters
        Assert.Null(language.Info.TemplateParameters);
    }
}
