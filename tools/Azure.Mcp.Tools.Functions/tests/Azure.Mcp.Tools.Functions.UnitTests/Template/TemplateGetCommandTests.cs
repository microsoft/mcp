// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Net;
using System.Text.Json;
using Azure.Mcp.Tools.Functions.Commands;
using Azure.Mcp.Tools.Functions.Commands.Template;
using Azure.Mcp.Tools.Functions.Models;
using Azure.Mcp.Tools.Functions.Options;
using Azure.Mcp.Tools.Functions.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Models.Command;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.Functions.UnitTests.Template;

public sealed class TemplateGetCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IFunctionsService _service;
    private readonly ILogger<TemplateGetCommand> _logger;
    private readonly CommandContext _context;
    private readonly TemplateGetCommand _command;
    private readonly Command _commandDefinition;

    public TemplateGetCommandTests()
    {
        _service = Substitute.For<IFunctionsService>();
        _logger = Substitute.For<ILogger<TemplateGetCommand>>();

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
        Assert.Equal("Get Function Template", _command.Title);
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
    public void Command_HasLanguageRequiredAndTemplateOptional()
    {
        var options = _commandDefinition.Options.ToList();
        var languageOption = options.FirstOrDefault(o => o.Name == $"--{FunctionsOptionDefinitions.LanguageName}");
        var templateOption = options.FirstOrDefault(o => o.Name == $"--{FunctionsOptionDefinitions.TemplateName}");
        var runtimeOption = options.FirstOrDefault(o => o.Name == $"--{FunctionsOptionDefinitions.RuntimeVersionName}");
        var outputOption = options.FirstOrDefault(o => o.Name == $"--{FunctionsOptionDefinitions.OutputName}");

        Assert.NotNull(languageOption);
        Assert.True(languageOption.Required);
        Assert.NotNull(templateOption);
        Assert.False(templateOption.Required); // registered with AsOptional
        Assert.NotNull(runtimeOption);
        Assert.False(runtimeOption.Required);
        Assert.NotNull(outputOption);
        Assert.False(outputOption.Required);
    }

    [Fact]
    public async Task ExecuteAsync_ListMode_ReturnsTemplateList()
    {
        // Arrange
        var expectedResult = new TemplateListResult
        {
            Language = "python",
            Triggers =
            [
                new TemplateSummary
                {
                    TemplateName = "HttpTrigger",
                    DisplayName = "HTTP Trigger",
                    Description = "A function triggered by HTTP requests",
                    Resource = null
                },
                new TemplateSummary
                {
                    TemplateName = "BlobTrigger",
                    DisplayName = "Blob Storage Trigger",
                    Description = "A function triggered by blob storage events",
                    Resource = "Azure Blob Storage"
                }
            ],
            InputBindings =
            [
                new TemplateSummary
                {
                    TemplateName = "BlobInput",
                    DisplayName = "Blob Storage Input",
                    Description = "Read from blob storage",
                    Resource = "Azure Blob Storage"
                }
            ],
            OutputBindings = []
        };

        _service.GetTemplateListAsync("python", Arg.Any<CancellationToken>()).Returns(Task.FromResult(expectedResult));

        // Act - no --template means list mode
        var args = _commandDefinition.Parse(["--language", "python"]);
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize<TemplateGetCommandResult>(
            json, FunctionsJsonContext.Default.TemplateGetCommandResult);

        Assert.NotNull(result);
        Assert.NotNull(result.TemplateList);
        Assert.Null(result.FunctionTemplate);
        Assert.Equal("python", result.TemplateList.Language);
        Assert.Equal(2, result.TemplateList.Triggers.Count);
        Assert.Single(result.TemplateList.InputBindings);
        Assert.Empty(result.TemplateList.OutputBindings);
        Assert.Equal("HttpTrigger", result.TemplateList.Triggers[0].TemplateName);
    }

    [Fact]
    public async Task ExecuteAsync_GetMode_ReturnsFunctionTemplate()
    {
        // Arrange - default mode is New which returns all files in 'Files' plus separated FunctionFiles/ProjectFiles/MergeInstructions for backward compat
        var expectedResult = new FunctionTemplateResult
        {
            Language = "python",
            TemplateName = "HttpTrigger",
            DisplayName = "HTTP Trigger",
            Description = "A function triggered by HTTP requests",
            BindingType = "trigger",
            Resource = null,
            Files =
            [
                new ProjectTemplateFile { FileName = "function_app.py", Content = "import azure.functions as func\napp = func.FunctionApp()" },
                new ProjectTemplateFile { FileName = "README.md", Content = "# HTTP Trigger template" },
                new ProjectTemplateFile { FileName = "host.json", Content = "{ \"version\": \"2.0\" }" },
                new ProjectTemplateFile { FileName = "local.settings.json", Content = "{ \"Values\": {} }" },
                new ProjectTemplateFile { FileName = "requirements.txt", Content = "azure-functions" }
            ],
            FunctionFiles =
            [
                new ProjectTemplateFile { FileName = "function_app.py", Content = "import azure.functions as func\napp = func.FunctionApp()" },
                new ProjectTemplateFile { FileName = "README.md", Content = "# HTTP Trigger template" }
            ],
            ProjectFiles =
            [
                new ProjectTemplateFile { FileName = "host.json", Content = "{ \"version\": \"2.0\" }" },
                new ProjectTemplateFile { FileName = "local.settings.json", Content = "{ \"Values\": {} }" },
                new ProjectTemplateFile { FileName = "requirements.txt", Content = "azure-functions" }
            ],
            MergeInstructions = "## Merging Template Files"
        };

        _service.GetFunctionTemplateAsync("python", "HttpTrigger", null, TemplateOutput.New, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expectedResult));

        // Act - with --template means get mode, default mode is New
        var args = _commandDefinition.Parse(["--language", "python", "--template", "HttpTrigger"]);
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize<TemplateGetCommandResult>(
            json, FunctionsJsonContext.Default.TemplateGetCommandResult);

        Assert.NotNull(result);
        Assert.Null(result.TemplateList);
        Assert.NotNull(result.FunctionTemplate);
        Assert.Equal("python", result.FunctionTemplate.Language);
        Assert.Equal("HttpTrigger", result.FunctionTemplate.TemplateName);
        Assert.Equal("HTTP Trigger", result.FunctionTemplate.DisplayName);
        Assert.Equal("trigger", result.FunctionTemplate.BindingType);
        Assert.NotNull(result.FunctionTemplate.Files);
        Assert.Equal(5, result.FunctionTemplate.Files.Count);
        Assert.NotNull(result.FunctionTemplate.FunctionFiles);
        Assert.Equal(2, result.FunctionTemplate.FunctionFiles.Count);
        Assert.NotNull(result.FunctionTemplate.ProjectFiles);
        Assert.Equal(3, result.FunctionTemplate.ProjectFiles.Count);
        Assert.NotNull(result.FunctionTemplate.MergeInstructions);
    }

    [Fact]
    public async Task ExecuteAsync_GetOutput_AddOutput_ReturnsSeparatedFiles()
    {
        // Arrange - Add output returns separated FunctionFiles and ProjectFiles
        var expectedResult = new FunctionTemplateResult
        {
            Language = "python",
            TemplateName = "HttpTrigger",
            DisplayName = "HTTP Trigger",
            Description = "A function triggered by HTTP requests",
            BindingType = "trigger",
            Resource = null,
            FunctionFiles =
            [
                new ProjectTemplateFile { FileName = "function_app.py", Content = "import azure.functions as func\napp = func.FunctionApp()" },
                new ProjectTemplateFile { FileName = "README.md", Content = "# HTTP Trigger template" }
            ],
            ProjectFiles =
            [
                new ProjectTemplateFile { FileName = "host.json", Content = "{ \"version\": \"2.0\" }" },
                new ProjectTemplateFile { FileName = "local.settings.json", Content = "{ \"Values\": {} }" },
                new ProjectTemplateFile { FileName = "requirements.txt", Content = "azure-functions" }
            ],
            MergeInstructions = "## Merging Template Files"
        };

        _service.GetFunctionTemplateAsync("python", "HttpTrigger", null, TemplateOutput.Add, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expectedResult));

        // Act - with --output Add
        var args = _commandDefinition.Parse(["--language", "python", "--template", "HttpTrigger", "--output", "Add"]);
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize<TemplateGetCommandResult>(
            json, FunctionsJsonContext.Default.TemplateGetCommandResult);

        Assert.NotNull(result);
        Assert.Null(result.TemplateList);
        Assert.NotNull(result.FunctionTemplate);
        Assert.Equal("python", result.FunctionTemplate.Language);
        Assert.Equal("HttpTrigger", result.FunctionTemplate.TemplateName);
        Assert.Null(result.FunctionTemplate.Files);
        Assert.NotNull(result.FunctionTemplate.FunctionFiles);
        Assert.NotNull(result.FunctionTemplate.ProjectFiles);
        Assert.Equal(2, result.FunctionTemplate.FunctionFiles.Count);
        Assert.Equal(3, result.FunctionTemplate.ProjectFiles.Count);
        Assert.NotEmpty(result.FunctionTemplate.MergeInstructions!);
    }

    [Fact]
    public async Task ExecuteAsync_GetMode_WithRuntimeVersion_PassesVersionToService()
    {
        // Arrange
        var expectedResult = new FunctionTemplateResult
        {
            Language = "typescript",
            TemplateName = "HttpTrigger",
            DisplayName = "HTTP Trigger",
            BindingType = "trigger",
            Files =
            [
                new ProjectTemplateFile { FileName = "src/functions/httpTrigger.ts", Content = "import { app } from '@azure/functions';" },
                new ProjectTemplateFile { FileName = "package.json", Content = "{ \"devDependencies\": { \"@types/node\": \"22.x\" } }" }
            ]
        };

        _service.GetFunctionTemplateAsync("typescript", "HttpTrigger", "22", TemplateOutput.New, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expectedResult));

        // Act
        var args = _commandDefinition.Parse(["--language", "typescript", "--template", "HttpTrigger", "--runtime-version", "22"]);
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize<TemplateGetCommandResult>(
            json, FunctionsJsonContext.Default.TemplateGetCommandResult);

        Assert.NotNull(result?.FunctionTemplate);
        Assert.Equal("typescript", result.FunctionTemplate.Language);
        Assert.NotNull(result.FunctionTemplate.Files);
        Assert.Equal(2, result.FunctionTemplate.Files.Count);
        Assert.Contains("@azure/functions", result.FunctionTemplate.Files[0].Content);
    }

    [Fact]
    public async Task ExecuteAsync_ListMode_HandlesInvalidLanguage()
    {
        // Arrange - no mock setup needed, validator catches it

        // Act
        var args = _commandDefinition.Parse(["--language", "invalid"]);
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert - validator returns error before service is called
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("Invalid language 'invalid'", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_GetMode_HandlesInvalidTemplate()
    {
        // Arrange
        _service.GetFunctionTemplateAsync("python", "NonExistent", null, TemplateOutput.New, Arg.Any<CancellationToken>())
            .Returns<FunctionTemplateResult>(_ => throw new ArgumentException(
                "Template \"NonExistent\" not found for language \"python\"."));

        // Act
        var args = _commandDefinition.Parse(["--language", "python", "--template", "NonExistent"]);
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("not found", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_GetMode_HandlesInvalidRuntimeVersion()
    {
        // Arrange
        _service.GetFunctionTemplateAsync("java", "HttpTrigger", "99", TemplateOutput.New, Arg.Any<CancellationToken>())
            .Returns<FunctionTemplateResult>(_ => throw new ArgumentException("Invalid runtime version \"99\" for java."));

        // Act
        var args = _commandDefinition.Parse(["--language", "java", "--template", "HttpTrigger", "--runtime-version", "99"]);
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("Invalid runtime version", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_ListMode_HandlesServiceErrors()
    {
        // Arrange
        _service.GetTemplateListAsync("python", Arg.Any<CancellationToken>())
            .Returns<TemplateListResult>(_ => throw new InvalidOperationException("Network error"));

        // Act
        var args = _commandDefinition.Parse(["--language", "python"]);
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.Status);
        Assert.Contains("Network error", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_GetMode_HandlesServiceErrors()
    {
        // Arrange
        _service.GetFunctionTemplateAsync("python", "HttpTrigger", null, TemplateOutput.New, Arg.Any<CancellationToken>())
            .Returns<FunctionTemplateResult>(_ => throw new InvalidOperationException("Could not fetch template"));

        // Act
        var args = _commandDefinition.Parse(["--language", "python", "--template", "HttpTrigger"]);
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.Status);
        Assert.Contains("Could not fetch template", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_DeserializationValidation_ListMode()
    {
        // Arrange
        var expectedResult = new TemplateListResult
        {
            Language = "typescript",
            Triggers =
            [
                new TemplateSummary
                {
                    TemplateName = "HttpTrigger",
                    DisplayName = "HTTP Trigger",
                    Description = "Triggered by HTTP",
                    Resource = null
                },
                new TemplateSummary
                {
                    TemplateName = "TimerTrigger",
                    DisplayName = "Timer Trigger",
                    Description = "Triggered on schedule",
                    Resource = null
                }
            ],
            InputBindings =
            [
                new TemplateSummary
                {
                    TemplateName = "CosmosDBInput",
                    DisplayName = "Cosmos DB Input",
                    Description = "Read from Cosmos DB",
                    Resource = "Azure Cosmos DB"
                }
            ],
            OutputBindings =
            [
                new TemplateSummary
                {
                    TemplateName = "ServiceBusOutput",
                    DisplayName = "Service Bus Output",
                    Description = "Send to Service Bus",
                    Resource = "Azure Service Bus"
                }
            ]
        };

        _service.GetTemplateListAsync("typescript", Arg.Any<CancellationToken>()).Returns(Task.FromResult(expectedResult));

        // Act
        var args = _commandDefinition.Parse(["--language", "typescript"]);
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize<TemplateGetCommandResult>(
            json, FunctionsJsonContext.Default.TemplateGetCommandResult);

        Assert.NotNull(result?.TemplateList);
        Assert.Equal("typescript", result.TemplateList.Language);
        Assert.Equal(2, result.TemplateList.Triggers.Count);
        Assert.Single(result.TemplateList.InputBindings);
        Assert.Single(result.TemplateList.OutputBindings);

        // Verify individual fields round-trip correctly
        var httpTrigger = result.TemplateList.Triggers[0];
        Assert.Equal("HttpTrigger", httpTrigger.TemplateName);
        Assert.Equal("HTTP Trigger", httpTrigger.DisplayName);
        Assert.Equal("Triggered by HTTP", httpTrigger.Description);

        var cosmosInput = result.TemplateList.InputBindings[0];
        Assert.Equal("Azure Cosmos DB", cosmosInput.Resource);
    }

    [Fact]
    public async Task ExecuteAsync_DeserializationValidation_GetMode()
    {
        // Arrange - using Add output mode to test separated files
        var expectedResult = new FunctionTemplateResult
        {
            Language = "java",
            TemplateName = "HttpTrigger",
            DisplayName = "HTTP Trigger",
            Description = "A function that responds to HTTP requests",
            BindingType = "trigger",
            Resource = null,
            FunctionFiles =
            [
                new ProjectTemplateFile
                {
                    FileName = "src/main/java/com/function/Function.java",
                    Content = "package com.function;\nimport com.microsoft.azure.functions.*;"
                }
            ],
            ProjectFiles =
            [
                new ProjectTemplateFile { FileName = "host.json", Content = "{ \"version\": \"2.0\" }" },
                new ProjectTemplateFile { FileName = "local.settings.json", Content = "{ \"Values\": {} }" }
            ],
            MergeInstructions = "## Merging Template Files"
        };

        _service.GetFunctionTemplateAsync("java", "HttpTrigger", null, TemplateOutput.Add, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expectedResult));

        // Act
        var args = _commandDefinition.Parse(["--language", "java", "--template", "HttpTrigger", "--output", "Add"]);
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize<TemplateGetCommandResult>(
            json, FunctionsJsonContext.Default.TemplateGetCommandResult);

        Assert.NotNull(result?.FunctionTemplate);
        Assert.Equal("java", result.FunctionTemplate.Language);
        Assert.Equal("HttpTrigger", result.FunctionTemplate.TemplateName);
        Assert.Equal("A function that responds to HTTP requests", result.FunctionTemplate.Description);
        Assert.Equal("trigger", result.FunctionTemplate.BindingType);

        // Verify function files
        Assert.NotNull(result.FunctionTemplate.FunctionFiles);
        var functionFile = result.FunctionTemplate.FunctionFiles[0];
        Assert.Equal("src/main/java/com/function/Function.java", functionFile.FileName);
        Assert.Contains("package com.function", functionFile.Content);

        // Verify project files
        Assert.NotNull(result.FunctionTemplate.ProjectFiles);
        Assert.Equal(2, result.FunctionTemplate.ProjectFiles.Count);
        Assert.Equal("host.json", result.FunctionTemplate.ProjectFiles[0].FileName);
    }

    [Theory]
    [InlineData("python")]
    [InlineData("typescript")]
    [InlineData("java")]
    [InlineData("csharp")]
    public async Task ExecuteAsync_ListMode_WorksForAllLanguages(string language)
    {
        // Arrange
        var expectedResult = new TemplateListResult
        {
            Language = language,
            Triggers =
            [
                new TemplateSummary
                {
                    TemplateName = "HttpTrigger",
                    DisplayName = "HTTP Trigger",
                    Description = "HTTP triggered function"
                }
            ]
        };

        _service.GetTemplateListAsync(language, Arg.Any<CancellationToken>()).Returns(Task.FromResult(expectedResult));

        // Act
        var args = _commandDefinition.Parse(["--language", language]);
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize<TemplateGetCommandResult>(
            json, FunctionsJsonContext.Default.TemplateGetCommandResult);

        Assert.NotNull(result?.TemplateList);
        Assert.Equal(language, result.TemplateList.Language);
        Assert.Single(result.TemplateList.Triggers);
    }

    [Fact]
    public void BindOptions_BindsAllOptionsCorrectly()
    {
        // Arrange & Act
        var args = _commandDefinition.Parse(["--language", "java", "--template", "HttpTrigger", "--runtime-version", "21", "--output", "Add"]);

        var method = typeof(TemplateGetCommand).GetMethod(
            "BindOptions",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var options = (TemplateGetOptions?)method?.Invoke(_command, [args]);

        // Assert
        Assert.NotNull(options);
        Assert.Equal("java", options.Language);
        Assert.Equal("HttpTrigger", options.Template);
        Assert.Equal("21", options.RuntimeVersion);
        Assert.Equal(TemplateOutput.Add, options.Output);
    }

    [Fact]
    public void BindOptions_TemplateIsNullWhenOmitted()
    {
        // Arrange & Act - only language provided, no template
        var args = _commandDefinition.Parse(["--language", "python"]);

        var method = typeof(TemplateGetCommand).GetMethod(
            "BindOptions",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var options = (TemplateGetOptions?)method?.Invoke(_command, [args]);

        // Assert
        Assert.NotNull(options);
        Assert.Equal("python", options.Language);
        Assert.Null(options.Template);
        Assert.Null(options.RuntimeVersion);
        Assert.Equal(TemplateOutput.New, options.Output); // default output
    }
}
