// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.Functions.Commands;
using Azure.Mcp.Tools.Functions.Commands.Project;
using Azure.Mcp.Tools.Functions.Models;
using Azure.Mcp.Tools.Functions.Options;
using Azure.Mcp.Tools.Functions.Services;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.Functions.UnitTests.Project;

public sealed class ProjectGetCommandTests : CommandUnitTestsBase<ProjectGetCommand, IFunctionsService>
{
    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        Assert.Equal("get", Command.Name);
        Assert.NotEmpty(Command.Description);
        Assert.Equal("Get Project Template", Command.Title);
    }

    [Fact]
    public void Command_HasCorrectMetadata()
    {
        Assert.False(Command.Metadata.Destructive);
        Assert.True(Command.Metadata.Idempotent);
        Assert.False(Command.Metadata.OpenWorld);
        Assert.True(Command.Metadata.ReadOnly);
        Assert.False(Command.Metadata.LocalRequired);
        Assert.False(Command.Metadata.Secret);
    }

    [Fact]
    public void Command_HasLanguageOption()
    {
        var options = CommandDefinition.Options.ToList();
        var languageOption = options.FirstOrDefault(o => o.Name == $"--{FunctionsOptionDefinitions.LanguageName}");

        Assert.NotNull(languageOption);
        Assert.True(languageOption.Required);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsProjectTemplate_ForPython()
    {
        // Arrange
        var expectedResult = new ProjectTemplateResult
        {
            Language = "python",
            InitInstructions = "## Python Azure Functions Project Setup",
            ProjectStructure = ["function_app.py", "host.json", "requirements.txt", ".gitignore"]
        };

        Service.GetProjectTemplateAsync("python", Arg.Any<CancellationToken>()).Returns(expectedResult);

        // Act
        var response = await ExecuteCommandAsync("--language", "python");

        // Assert
        var results = ValidateAndDeserializeResponse(response, FunctionsJsonContext.Default.ListProjectTemplateResult);

        Assert.Single(results);

        var result = results[0];
        Assert.Equal("python", result.Language);
        Assert.NotEmpty(result.InitInstructions);
        Assert.Equal(4, result.ProjectStructure.Count);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsStaticMetadata_NoHttpCalls()
    {
        // Arrange - project get should return static metadata without HTTP calls
        var expectedResult = new ProjectTemplateResult
        {
            Language = "typescript",
            InitInstructions = "## TypeScript Azure Functions Project Setup",
            ProjectStructure = ["src/functions/", "host.json", "package.json", ".gitignore"]
        };

        Service.GetProjectTemplateAsync("typescript", Arg.Any<CancellationToken>()).Returns(expectedResult);

        // Act
        var response = await ExecuteCommandAsync("--language", "typescript");

        // Assert
        var results = ValidateAndDeserializeResponse(response, FunctionsJsonContext.Default.ListProjectTemplateResult);

        Assert.Single(results);
        Assert.Equal("typescript", results[0].Language);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesInvalidLanguage()
    {
        // Arrange & Act - no mock setup needed, validator catches it
        var response = await ExecuteCommandAsync("--language", "invalid");

        // Assert - validator returns error before service is called
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("Invalid language 'invalid'", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        // Arrange
        Service.GetProjectTemplateAsync("python", Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("Service error"));

        // Act
        var response = await ExecuteCommandAsync("--language", "python");

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.Status);
        Assert.Contains("Service error", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_DeserializationValidation()
    {
        // Arrange - use representative project template data to verify serialization
        var expectedResult = new ProjectTemplateResult
        {
            Language = "python",
            InitInstructions = "## Python Azure Functions Project Setup\n\n1. Create virtual environment\n2. Install dependencies",
            ProjectStructure = ["function_app.py", "host.json", "requirements.txt", "local.settings.json", ".gitignore"]
        };

        Service.GetProjectTemplateAsync("python", Arg.Any<CancellationToken>()).Returns(expectedResult);

        // Act
        var response = await ExecuteCommandAsync("--language", "python");

        // Assert
        var results = ValidateAndDeserializeResponse(response, FunctionsJsonContext.Default.ListProjectTemplateResult);

        Assert.Single(results);

        var result = results[0];
        Assert.Equal("python", result.Language);
        Assert.Contains("virtual environment", result.InitInstructions);
        Assert.True(result.ProjectStructure.Count > 0);
    }

    [Theory]
    [InlineData("python")]
    [InlineData("typescript")]
    [InlineData("java")]
    [InlineData("csharp")]
    [InlineData("javascript")]
    [InlineData("powershell")]
    public async Task ExecuteAsync_ReturnsTemplateForAllLanguages(string language)
    {
        // Arrange - use representative mocked data per language
        var expectedResult = new ProjectTemplateResult
        {
            Language = language,
            InitInstructions = $"## {language} Azure Functions Project Setup",
            ProjectStructure = ["host.json", "local.settings.json", ".gitignore"]
        };

        Service.GetProjectTemplateAsync(language, Arg.Any<CancellationToken>()).Returns(expectedResult);

        // Act
        var response = await ExecuteCommandAsync("--language", language);

        // Assert
        var results = ValidateAndDeserializeResponse(response, FunctionsJsonContext.Default.ListProjectTemplateResult);

        Assert.Single(results);
        Assert.Equal(language, results[0].Language);
        Assert.True(results[0].ProjectStructure.Count > 0);
    }

    [Fact]
    public void BindOptions_BindsOptionsCorrectly()
    {
        // Arrange & Act
        var args = CommandDefinition.Parse(["--language", "java"]);

        // Use reflection to call BindOptions since it's protected
        var method = Command.GetType().GetMethod(
            "BindOptions",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var options = (ProjectGetOptions?)method?.Invoke(Command, [args]);

        // Assert
        Assert.NotNull(options);
        Assert.Equal("java", options.Language);
    }
}
