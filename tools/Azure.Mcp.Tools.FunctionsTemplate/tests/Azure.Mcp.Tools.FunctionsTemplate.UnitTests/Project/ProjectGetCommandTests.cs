// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Net;
using System.Text.Json;
using Azure.Mcp.Tools.FunctionsTemplate.Commands;
using Azure.Mcp.Tools.FunctionsTemplate.Commands.Project;
using Azure.Mcp.Tools.FunctionsTemplate.Models;
using Azure.Mcp.Tools.FunctionsTemplate.Options;
using Azure.Mcp.Tools.FunctionsTemplate.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Models.Command;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.FunctionsTemplate.UnitTests.Project;

public sealed class ProjectGetCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IFunctionTemplatesService _service;
    private readonly ILogger<ProjectGetCommand> _logger;
    private readonly CommandContext _context;
    private readonly ProjectGetCommand _command;
    private readonly Command _commandDefinition;

    public ProjectGetCommandTests()
    {
        _service = Substitute.For<IFunctionTemplatesService>();
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
    public void Command_HasLanguageAndRuntimeVersionOptions()
    {
        var options = _commandDefinition.Options.ToList();
        var languageOption = options.FirstOrDefault(o => o.Name == $"--{FunctionTemplatesOptionDefinitions.LanguageName}");
        var runtimeOption = options.FirstOrDefault(o => o.Name == $"--{FunctionTemplatesOptionDefinitions.RuntimeVersionName}");

        Assert.NotNull(languageOption);
        Assert.True(languageOption.Required);
        Assert.NotNull(runtimeOption);
        Assert.False(runtimeOption.Required);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsProjectTemplate_ForPython()
    {
        // Arrange
        var expectedResult = new ProjectTemplateResult
        {
            Language = "python",
            Files =
            [
                new ProjectTemplateFile { FileName = "host.json", Content = "{}" },
                new ProjectTemplateFile { FileName = "local.settings.json", Content = "{}" },
                new ProjectTemplateFile { FileName = "requirements.txt", Content = "azure-functions" },
                new ProjectTemplateFile { FileName = ".funcignore", Content = ".venv/" }
            ],
            InitInstructions = "## Python Azure Functions Project Setup",
            ProjectStructure = ["function_app.py", "host.json", "requirements.txt"]
        };

        _service.GetProjectTemplateAsync("python", null, Arg.Any<CancellationToken>()).Returns(Task.FromResult(expectedResult));

        // Act
        var args = _commandDefinition.Parse(["--language", "python"]);
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var results = JsonSerializer.Deserialize<List<ProjectTemplateResult>>(
            json, FunctionTemplatesJsonContext.Default.ListProjectTemplateResult);

        Assert.NotNull(results);
        Assert.Single(results);

        var result = results[0];
        Assert.Equal("python", result.Language);
        Assert.Equal(4, result.Files.Count);
        Assert.Equal("host.json", result.Files[0].FileName);
        Assert.NotEmpty(result.InitInstructions);
        Assert.Equal(3, result.ProjectStructure.Count);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsProjectTemplate_WithRuntimeVersion()
    {
        // Arrange
        var expectedResult = new ProjectTemplateResult
        {
            Language = "typescript",
            Files =
            [
                new ProjectTemplateFile { FileName = "host.json", Content = "{}" },
                new ProjectTemplateFile { FileName = "package.json", Content = "{ \"devDependencies\": { \"@types/node\": \"22.x\" } }" }
            ],
            InitInstructions = "## TypeScript Azure Functions Project Setup",
            ProjectStructure = ["src/functions/", "host.json", "package.json"],
            Parameters = null // Parameters omitted when runtimeVersion is provided
        };

        _service.GetProjectTemplateAsync("typescript", "22", Arg.Any<CancellationToken>()).Returns(Task.FromResult(expectedResult));

        // Act
        var args = _commandDefinition.Parse(["--language", "typescript", "--runtimeVersion", "22"]);
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var results = JsonSerializer.Deserialize<List<ProjectTemplateResult>>(
            json, FunctionTemplatesJsonContext.Default.ListProjectTemplateResult);

        Assert.NotNull(results);
        Assert.Single(results);
        Assert.Equal("typescript", results[0].Language);
        Assert.Null(results[0].Parameters);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesInvalidLanguage()
    {
        // Arrange
        _service.GetProjectTemplateAsync("invalid", null, Arg.Any<CancellationToken>())
            .Returns<ProjectTemplateResult>(_ => throw new ArgumentException("Invalid language: \"invalid\"."));

        // Act
        var args = _commandDefinition.Parse(["--language", "invalid"]);
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("Invalid language", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesInvalidRuntimeVersion()
    {
        // Arrange
        _service.GetProjectTemplateAsync("java", "99", Arg.Any<CancellationToken>())
            .Returns<ProjectTemplateResult>(_ => throw new ArgumentException("Invalid runtime version \"99\" for java."));

        // Act
        var args = _commandDefinition.Parse(["--language", "java", "--runtimeVersion", "99"]);
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("Invalid runtime version", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        // Arrange
        _service.GetProjectTemplateAsync("python", null, Arg.Any<CancellationToken>())
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
            Files =
            [
                new ProjectTemplateFile { FileName = "host.json", Content = "{ \"version\": \"2.0\", \"extensionBundle\": {} }" },
                new ProjectTemplateFile { FileName = "local.settings.json", Content = "{ \"Values\": { \"FUNCTIONS_WORKER_RUNTIME\": \"python\" } }" },
                new ProjectTemplateFile { FileName = "requirements.txt", Content = "azure-functions" },
                new ProjectTemplateFile { FileName = ".funcignore", Content = ".venv/" }
            ],
            InitInstructions = "## Python Azure Functions Project Setup",
            ProjectStructure = ["function_app.py", "host.json", "requirements.txt"]
        };

        _service.GetProjectTemplateAsync("python", null, Arg.Any<CancellationToken>()).Returns(Task.FromResult(expectedResult));

        // Act
        var args = _commandDefinition.Parse(["--language", "python"]);
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);

        var json = JsonSerializer.Serialize(response.Results);
        var results = JsonSerializer.Deserialize<List<ProjectTemplateResult>>(
            json, FunctionTemplatesJsonContext.Default.ListProjectTemplateResult);

        Assert.NotNull(results);
        Assert.Single(results);

        var result = results[0];
        Assert.Equal("python", result.Language);
        Assert.Equal(4, result.Files.Count);

        // Verify host.json is present
        var hostJson = result.Files.FirstOrDefault(f => f.FileName == "host.json");
        Assert.NotNull(hostJson);
        Assert.Contains("extensionBundle", hostJson.Content);

        // Verify local.settings.json is present
        var localSettings = result.Files.FirstOrDefault(f => f.FileName == "local.settings.json");
        Assert.NotNull(localSettings);
        Assert.Contains("FUNCTIONS_WORKER_RUNTIME", localSettings.Content);

        Assert.NotEmpty(result.InitInstructions);
        Assert.True(result.ProjectStructure.Count > 0);
    }

    [Theory]
    [InlineData("python")]
    [InlineData("typescript")]
    [InlineData("java")]
    [InlineData("csharp")]
    public async Task ExecuteAsync_ReturnsTemplateForAllLanguages(string language)
    {
        // Arrange - use representative mocked data per language
        var expectedResult = new ProjectTemplateResult
        {
            Language = language,
            Files =
            [
                new ProjectTemplateFile { FileName = "host.json", Content = "{}" },
                new ProjectTemplateFile { FileName = "local.settings.json", Content = "{}" }
            ],
            InitInstructions = $"## {language} Azure Functions Project Setup",
            ProjectStructure = ["host.json", "local.settings.json"]
        };

        _service.GetProjectTemplateAsync(language, null, Arg.Any<CancellationToken>()).Returns(Task.FromResult(expectedResult));

        // Act
        var args = _commandDefinition.Parse(["--language", language]);
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);

        var json = JsonSerializer.Serialize(response.Results);
        var results = JsonSerializer.Deserialize<List<ProjectTemplateResult>>(
            json, FunctionTemplatesJsonContext.Default.ListProjectTemplateResult);

        Assert.NotNull(results);
        Assert.Single(results);
        Assert.Equal(language, results[0].Language);
        Assert.True(results[0].Files.Count > 0);
    }

    [Fact]
    public void BindOptions_BindsOptionsCorrectly()
    {
        // Arrange & Act
        var args = _commandDefinition.Parse(["--language", "java", "--runtimeVersion", "21"]);

        // Use reflection to call BindOptions since it's protected
        var method = typeof(ProjectGetCommand).GetMethod(
            "BindOptions",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var options = (ProjectGetOptions?)method?.Invoke(_command, [args]);

        // Assert
        Assert.NotNull(options);
        Assert.Equal("java", options.Language);
        Assert.Equal("21", options.RuntimeVersion);
    }
}
