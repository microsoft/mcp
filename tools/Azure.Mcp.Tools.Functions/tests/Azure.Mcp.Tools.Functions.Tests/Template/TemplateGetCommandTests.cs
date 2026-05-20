// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.Functions.Commands;
using Azure.Mcp.Tools.Functions.Commands.Template;
using Azure.Mcp.Tools.Functions.Models;
using Azure.Mcp.Tools.Functions.Options;
using Azure.Mcp.Tools.Functions.Services;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.Functions.UnitTests.Template;

public sealed class TemplateGetCommandTests : CommandUnitTestsBase<TemplateGetCommand, IFunctionsService>
{
    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        Assert.Equal("get", Command.Name);
        Assert.NotEmpty(Command.Description);
        Assert.Equal("Get Function Template", Command.Title);
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
    public void Command_HasLanguageRequiredAndTemplateOptional()
    {
        var options = CommandDefinition.Options.ToList();
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
                new()
                {
                    TemplateName = "HttpTrigger",
                    DisplayName = "HTTP Trigger",
                    Description = "A function triggered by HTTP requests",
                    Resource = null
                },
                new()
                {
                    TemplateName = "BlobTrigger",
                    DisplayName = "Blob Storage Trigger",
                    Description = "A function triggered by blob storage events",
                    Resource = "Azure Blob Storage"
                }
            ],
            InputBindings =
            [
                new()
                {
                    TemplateName = "BlobInput",
                    DisplayName = "Blob Storage Input",
                    Description = "Read from blob storage",
                    Resource = "Azure Blob Storage"
                }
            ],
            OutputBindings = []
        };

        Service.GetTemplateListAsync("python", Arg.Any<CancellationToken>()).Returns(expectedResult);

        // Act - no --template means list mode
        var response = await ExecuteCommandAsync("--language", "python");

        // Assert
        var result = ValidateAndDeserializeResponse(response, FunctionsJsonContext.Default.TemplateGetCommandResult);

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
                new() { FileName = "function_app.py", Content = "import azure.functions as func\napp = func.FunctionApp()" },
                new() { FileName = "README.md", Content = "# HTTP Trigger template" },
                new() { FileName = "host.json", Content = "{ \"version\": \"2.0\" }" },
                new() { FileName = "local.settings.json", Content = "{ \"Values\": {} }" },
                new() { FileName = "requirements.txt", Content = "azure-functions" }
            ],
            FunctionFiles =
            [
                new() { FileName = "function_app.py", Content = "import azure.functions as func\napp = func.FunctionApp()" },
                new() { FileName = "README.md", Content = "# HTTP Trigger template" }
            ],
            ProjectFiles =
            [
                new() { FileName = "host.json", Content = "{ \"version\": \"2.0\" }" },
                new() { FileName = "local.settings.json", Content = "{ \"Values\": {} }" },
                new() { FileName = "requirements.txt", Content = "azure-functions" }
            ],
            MergeInstructions = "## Merging Template Files"
        };

        Service.GetFunctionTemplateAsync("python", "HttpTrigger", null, TemplateOutput.New, Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act - with --template means get mode, default mode is New
        var response = await ExecuteCommandAsync("--language", "python", "--template", "HttpTrigger");

        // Assert
        var result = ValidateAndDeserializeResponse(response, FunctionsJsonContext.Default.TemplateGetCommandResult);

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
                new() { FileName = "function_app.py", Content = "import azure.functions as func\napp = func.FunctionApp()" },
                new() { FileName = "README.md", Content = "# HTTP Trigger template" }
            ],
            ProjectFiles =
            [
                new() { FileName = "host.json", Content = "{ \"version\": \"2.0\" }" },
                new() { FileName = "local.settings.json", Content = "{ \"Values\": {} }" },
                new() { FileName = "requirements.txt", Content = "azure-functions" }
            ],
            MergeInstructions = "## Merging Template Files"
        };

        Service.GetFunctionTemplateAsync("python", "HttpTrigger", null, TemplateOutput.Add, Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act - with --output Add
        var response = await ExecuteCommandAsync("--language", "python", "--template", "HttpTrigger", "--output", "Add");

        // Assert
        var result = ValidateAndDeserializeResponse(response, FunctionsJsonContext.Default.TemplateGetCommandResult);

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
                new() { FileName = "src/functions/httpTrigger.ts", Content = "import { app } from '@azure/functions';" },
                new() { FileName = "package.json", Content = "{ \"devDependencies\": { \"@types/node\": \"22.x\" } }" }
            ]
        };

        Service.GetFunctionTemplateAsync("typescript", "HttpTrigger", "22", TemplateOutput.New, Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        var response = await ExecuteCommandAsync("--language", "typescript", "--template", "HttpTrigger", "--runtime-version", "22");

        // Assert
        var result = ValidateAndDeserializeResponse(response, FunctionsJsonContext.Default.TemplateGetCommandResult);

        Assert.NotNull(result.FunctionTemplate);
        Assert.Equal("typescript", result.FunctionTemplate.Language);
        Assert.NotNull(result.FunctionTemplate.Files);
        Assert.Equal(2, result.FunctionTemplate.Files.Count);
        Assert.Contains("@azure/functions", result.FunctionTemplate.Files[0].Content);
    }

    [Fact]
    public async Task ExecuteAsync_ListMode_HandlesInvalidLanguage()
    {
        // Arrange & Act - no mock setup needed, validator catches it
        var response = await ExecuteCommandAsync("--language", "invalid");

        // Assert - validator returns error before service is called
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("Invalid language 'invalid'", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_GetMode_HandlesInvalidTemplate()
    {
        // Arrange
        Service.GetFunctionTemplateAsync("python", "NonExistent", null, TemplateOutput.New, Arg.Any<CancellationToken>())
            .ThrowsAsync(new ArgumentException("Template \"NonExistent\" not found for language \"python\"."));

        // Act
        var response = await ExecuteCommandAsync("--language", "python", "--template", "NonExistent");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("not found", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_GetMode_HandlesInvalidRuntimeVersion()
    {
        // Arrange
        Service.GetFunctionTemplateAsync("java", "HttpTrigger", "99", TemplateOutput.New, Arg.Any<CancellationToken>())
            .ThrowsAsync(new ArgumentException("Invalid runtime version \"99\" for java."));

        // Act
        var response = await ExecuteCommandAsync("--language", "java", "--template", "HttpTrigger", "--runtime-version", "99");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("Invalid runtime version", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_ListMode_HandlesServiceErrors()
    {
        // Arrange
        Service.GetTemplateListAsync("python", Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("Network error"));

        // Act
        var response = await ExecuteCommandAsync("--language", "python");

        // Assert
        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.Status);
        Assert.Contains("Network error", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_GetMode_HandlesServiceErrors()
    {
        // Arrange
        Service.GetFunctionTemplateAsync("python", "HttpTrigger", null, TemplateOutput.New, Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("Could not fetch template"));

        // Act
        var response = await ExecuteCommandAsync("--language", "python", "--template", "HttpTrigger");

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
                new()
                {
                    TemplateName = "HttpTrigger",
                    DisplayName = "HTTP Trigger",
                    Description = "Triggered by HTTP",
                    Resource = null
                },
                new()
                {
                    TemplateName = "TimerTrigger",
                    DisplayName = "Timer Trigger",
                    Description = "Triggered on schedule",
                    Resource = null
                }
            ],
            InputBindings =
            [
                new()
                {
                    TemplateName = "CosmosDBInput",
                    DisplayName = "Cosmos DB Input",
                    Description = "Read from Cosmos DB",
                    Resource = "Azure Cosmos DB"
                }
            ],
            OutputBindings =
            [
                new()
                {
                    TemplateName = "ServiceBusOutput",
                    DisplayName = "Service Bus Output",
                    Description = "Send to Service Bus",
                    Resource = "Azure Service Bus"
                }
            ]
        };

        Service.GetTemplateListAsync("typescript", Arg.Any<CancellationToken>()).Returns(expectedResult);

        // Act
        var response = await ExecuteCommandAsync("--language", "typescript");

        // Assert
        var result = ValidateAndDeserializeResponse(response, FunctionsJsonContext.Default.TemplateGetCommandResult);

        Assert.NotNull(result.TemplateList);
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
                new()
                {
                    FileName = "src/main/java/com/function/Function.java",
                    Content = "package com.function;\nimport com.microsoft.azure.functions.*;"
                }
            ],
            ProjectFiles =
            [
                new() { FileName = "host.json", Content = "{ \"version\": \"2.0\" }" },
                new() { FileName = "local.settings.json", Content = "{ \"Values\": {} }" }
            ],
            MergeInstructions = "## Merging Template Files"
        };

        Service.GetFunctionTemplateAsync("java", "HttpTrigger", null, TemplateOutput.Add, Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        var response = await ExecuteCommandAsync("--language", "java", "--template", "HttpTrigger", "--output", "Add");

        // Assert
        var result = ValidateAndDeserializeResponse(response, FunctionsJsonContext.Default.TemplateGetCommandResult);

        Assert.NotNull(result.FunctionTemplate);
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
                new()
                {
                    TemplateName = "HttpTrigger",
                    DisplayName = "HTTP Trigger",
                    Description = "HTTP triggered function"
                }
            ]
        };

        Service.GetTemplateListAsync(language, Arg.Any<CancellationToken>()).Returns(expectedResult);

        // Act
        var response = await ExecuteCommandAsync("--language", language);

        // Assert
        var result = ValidateAndDeserializeResponse(response, FunctionsJsonContext.Default.TemplateGetCommandResult);

        Assert.NotNull(result.TemplateList);
        Assert.Equal(language, result.TemplateList.Language);
        Assert.Single(result.TemplateList.Triggers);
    }

    [Fact]
    public void BindOptions_BindsAllOptionsCorrectly()
    {
        // Arrange & Act
        var args = CommandDefinition.Parse(["--language", "java", "--template", "HttpTrigger", "--runtime-version", "21", "--output", "Add"]);

        var method = Command.GetType().GetMethod(
            "BindOptions",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var options = (TemplateGetOptions?)method?.Invoke(Command, [args]);

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
        var args = CommandDefinition.Parse(["--language", "python"]);

        var method = Command.GetType().GetMethod(
            "BindOptions",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var options = (TemplateGetOptions?)method?.Invoke(Command, [args]);

        // Assert
        Assert.NotNull(options);
        Assert.Equal("python", options.Language);
        Assert.Null(options.Template);
        Assert.Null(options.RuntimeVersion);
        Assert.Equal(TemplateOutput.New, options.Output); // default output
    }
}
