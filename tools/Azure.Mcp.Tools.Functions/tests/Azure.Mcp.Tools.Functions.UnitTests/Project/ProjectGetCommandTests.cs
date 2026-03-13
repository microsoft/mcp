// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Net;
using System.Text.Json;
using Azure.Mcp.Tools.Functions.Commands;
using Azure.Mcp.Tools.Functions.Commands.Project;
using Azure.Mcp.Tools.Functions.Models;
using Azure.Mcp.Tools.Functions.Options;
using Azure.Mcp.Tools.Functions.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Models.Command;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.Functions.UnitTests.Project;

public sealed class ProjectGetCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IFunctionsService _service;
    private readonly ILogger<ProjectGetCommand> _logger;
    private readonly CommandContext _context;
    private readonly ProjectGetCommand _command;
    private readonly Command _commandDefinition;

    public ProjectGetCommandTests()
    {
        _service = Substitute.For<IFunctionsService>();
        _logger = Substitute.For<ILogger<ProjectGetCommand>>();

        var collection = new ServiceCollection();
        collection.AddSingleton(_service);
        _serviceProvider = collection.BuildServiceProvider();

        _context = new(_serviceProvider);
        _command = new(_logger);
        _commandDefinition = _command.GetCommand();
    }

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        Assert.Equal("get", _command.Name);
        Assert.NotEmpty(_command.Description);
        Assert.Equal("Get Project Template", _command.Title);
    }

    [Fact]
    public void Command_HasCorrectMetadata()
    {
        Assert.False(_command.Metadata.Destructive);
        Assert.True(_command.Metadata.Idempotent);
        Assert.False(_command.Metadata.OpenWorld);
        Assert.True(_command.Metadata.ReadOnly);
        Assert.False(_command.Metadata.LocalRequired);
        Assert.False(_command.Metadata.Secret);
    }

    [Fact]
    public void Command_HasLanguageOption()
    {
        var options = _commandDefinition.Options.ToList();
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

        _service.GetProjectTemplateAsync("python", Arg.Any<CancellationToken>()).Returns(Task.FromResult(expectedResult));

        // Act
        var args = _commandDefinition.Parse(["--language", "python"]);
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var results = JsonSerializer.Deserialize<List<ProjectTemplateResult>>(
            json, FunctionsJsonContext.Default.ListProjectTemplateResult);

        Assert.NotNull(results);
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

        _service.GetProjectTemplateAsync("typescript", Arg.Any<CancellationToken>()).Returns(Task.FromResult(expectedResult));

        // Act
        var args = _commandDefinition.Parse(["--language", "typescript"]);
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var results = JsonSerializer.Deserialize<List<ProjectTemplateResult>>(
            json, FunctionsJsonContext.Default.ListProjectTemplateResult);

        Assert.NotNull(results);
        Assert.Single(results);
        Assert.Equal("typescript", results[0].Language);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesInvalidLanguage()
    {
        // Arrange - no mock setup needed, validator catches it

        // Act
        var args = _commandDefinition.Parse(["--language", "invalid"]);
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert - validator returns error before service is called
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("Invalid language 'invalid'", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        // Arrange
        _service.GetProjectTemplateAsync("python", Arg.Any<CancellationToken>())
            .Returns<ProjectTemplateResult>(_ => throw new InvalidOperationException("Service error"));

        // Act
        var args = _commandDefinition.Parse(["--language", "python"]);
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

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

        _service.GetProjectTemplateAsync("python", Arg.Any<CancellationToken>()).Returns(Task.FromResult(expectedResult));

        // Act
        var args = _commandDefinition.Parse(["--language", "python"]);
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);

        var json = JsonSerializer.Serialize(response.Results);
        var results = JsonSerializer.Deserialize<List<ProjectTemplateResult>>(
            json, FunctionsJsonContext.Default.ListProjectTemplateResult);

        Assert.NotNull(results);
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

        _service.GetProjectTemplateAsync(language, Arg.Any<CancellationToken>()).Returns(Task.FromResult(expectedResult));

        // Act
        var args = _commandDefinition.Parse(["--language", language]);
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);

        var json = JsonSerializer.Serialize(response.Results);
        var results = JsonSerializer.Deserialize<List<ProjectTemplateResult>>(
            json, FunctionsJsonContext.Default.ListProjectTemplateResult);

        Assert.NotNull(results);
        Assert.Single(results);
        Assert.Equal(language, results[0].Language);
        Assert.True(results[0].ProjectStructure.Count > 0);
    }

    [Fact]
    public void BindOptions_BindsOptionsCorrectly()
    {
        // Arrange & Act
        var args = _commandDefinition.Parse(["--language", "java"]);

        // Use reflection to call BindOptions since it's protected
        var method = typeof(ProjectGetCommand).GetMethod(
            "BindOptions",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var options = (ProjectGetOptions?)method?.Invoke(_command, [args]);

        // Assert
        Assert.NotNull(options);
        Assert.Equal("java", options.Language);
    }
}
