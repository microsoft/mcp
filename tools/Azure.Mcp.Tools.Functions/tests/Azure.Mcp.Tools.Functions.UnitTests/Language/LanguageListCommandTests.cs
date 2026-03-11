// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Net;
using System.Text.Json;
using Azure.Mcp.Core.Services.Caching;
using Azure.Mcp.Tools.Functions.Commands;
using Azure.Mcp.Tools.Functions.Commands.Language;
using Azure.Mcp.Tools.Functions.Models;
using Azure.Mcp.Tools.Functions.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Models.Command;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.Functions.UnitTests.Language;

public sealed class LanguageListCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IFunctionsService _service;
    private readonly ILogger<LanguageListCommand> _logger;
    private readonly CommandContext _context;
    private readonly LanguageListCommand _command;
    private readonly Command _commandDefinition;

    public LanguageListCommandTests()
    {
        _service = Substitute.For<IFunctionsService>();
        _logger = Substitute.For<ILogger<LanguageListCommand>>();

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
        Assert.Equal("list", _command.Name);
        Assert.NotEmpty(_command.Description);
        Assert.Equal("List Supported Languages", _command.Title);
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
    public async Task ExecuteAsync_ReturnsLanguageList()
    {
        // Arrange
        var expectedResult = new LanguageListResult
        {
            FunctionsRuntimeVersion = "4.x",
            ExtensionBundleVersion = "[4.*, 5.0.0)",
            Languages =
            [
                new LanguageDetails
                {
                    Language = "python",
                    Info = new LanguageInfo
                    {
                        Name = "Python",
                        Runtime = "python",
                        ProgrammingModel = "v2 (Decorator-based)",
                        Prerequisites = ["Python 3.10+", "Azure Functions Core Tools v4"],
                        DevelopmentTools = ["VS Code with Azure Functions extension", "Azure Functions Core Tools"],
                        InitCommand = "func init --worker-runtime python --model V2",
                        RunCommand = "func start",
                        BuildCommand = null,
                        ProjectFiles = ["requirements.txt"],
                        RuntimeVersions = new RuntimeVersionInfo
                        {
                            Supported = ["3.10", "3.11", "3.12", "3.13"],
                            Preview = ["3.14"],
                            Default = "3.11"
                        },
                        InitInstructions = "Test instructions",
                        ProjectStructure = ["function_app.py"]
                    },
                    RuntimeVersions = new RuntimeVersionInfo
                    {
                        Supported = ["3.10", "3.11", "3.12", "3.13"],
                        Preview = ["3.14"],
                        Default = "3.11"
                    }
                },
                new LanguageDetails
                {
                    Language = "csharp",
                    Info = new LanguageInfo
                    {
                        Name = "C#",
                        Runtime = "dotnet",
                        ProgrammingModel = "Isolated worker process",
                        Prerequisites = [".NET 8 SDK or later", "Azure Functions Core Tools v4"],
                        DevelopmentTools = ["Visual Studio 2022", "VS Code with C# + Azure Functions extensions", "Azure Functions Core Tools"],
                        InitCommand = "func init --worker-runtime dotnet-isolated",
                        RunCommand = "func start",
                        BuildCommand = "dotnet build",
                        ProjectFiles = [],
                        RuntimeVersions = new RuntimeVersionInfo
                        {
                            Supported = ["8", "9", "10"],
                            Deprecated = ["6", "7"],
                            Default = "8",
                            FrameworkSupported = ["4.8.1"]
                        },
                        InitInstructions = "Test instructions",
                        ProjectStructure = ["*.csproj"]
                    },
                    RuntimeVersions = new RuntimeVersionInfo
                    {
                        Supported = ["8", "9", "10"],
                        Deprecated = ["6", "7"],
                        Default = "8",
                        FrameworkSupported = ["4.8.1"]
                    }
                }
            ]
        };

        _service.GetLanguageListAsync(Arg.Any<CancellationToken>()).Returns(Task.FromResult(expectedResult));

        // Act
        var args = _commandDefinition.Parse([]);
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var results = JsonSerializer.Deserialize<List<LanguageListResult>>(json, FunctionsJsonContext.Default.ListLanguageListResult);

        Assert.NotNull(results);
        Assert.Single(results);

        var result = results[0];
        Assert.Equal("4.x", result.FunctionsRuntimeVersion);
        Assert.Equal("[4.*, 5.0.0)", result.ExtensionBundleVersion);
        Assert.Equal(2, result.Languages.Count);
        Assert.Equal("python", result.Languages[0].Language);
        Assert.Equal("Python", result.Languages[0].Info.Name);
        Assert.Equal("3.11", result.Languages[0].RuntimeVersions.Default);
        Assert.Equal("csharp", result.Languages[1].Language);
        Assert.Equal("C#", result.Languages[1].Info.Name);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        // Arrange
        _service.GetLanguageListAsync(Arg.Any<CancellationToken>()).Returns<LanguageListResult>(_ => throw new InvalidOperationException("Service error"));

        // Act
        var args = _commandDefinition.Parse([]);
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.Status);
        Assert.Contains("Service error", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_DeserializationValidation()
    {
        // Arrange - use the real service to verify actual data shape
        // GetLanguageListAsync uses only static data, no HTTP calls needed
        var mockHttpClientFactory = Substitute.For<IHttpClientFactory>();
        var languageMetadata = new LanguageMetadataProvider();
        var mockManifestService = Substitute.For<IManifestService>();
        var mockLogger = Substitute.For<ILogger<FunctionsService>>();
        var realService = new FunctionsService(mockHttpClientFactory, languageMetadata, mockManifestService, mockLogger);
        var realResult = await realService.GetLanguageListAsync(TestContext.Current.CancellationToken);
        _service.GetLanguageListAsync(Arg.Any<CancellationToken>()).Returns(Task.FromResult(realResult));

        // Act
        var args = _commandDefinition.Parse([]);
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);

        var json = JsonSerializer.Serialize(response.Results);
        var results = JsonSerializer.Deserialize<List<LanguageListResult>>(json, FunctionsJsonContext.Default.ListLanguageListResult);

        Assert.NotNull(results);
        Assert.Single(results);

        var result = results[0];
        Assert.Equal("4.x", result.FunctionsRuntimeVersion);
        Assert.Equal(6, result.Languages.Count);

        // Verify all expected languages are present
        var languageNames = result.Languages.Select(l => l.Language).ToList();
        Assert.Contains("python", languageNames);
        Assert.Contains("typescript", languageNames);
        Assert.Contains("javascript", languageNames);
        Assert.Contains("java", languageNames);
        Assert.Contains("csharp", languageNames);
        Assert.Contains("powershell", languageNames);
    }
}
