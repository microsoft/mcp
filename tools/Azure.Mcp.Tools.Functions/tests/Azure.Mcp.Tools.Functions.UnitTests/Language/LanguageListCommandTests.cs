// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.Functions.Commands;
using Azure.Mcp.Tools.Functions.Commands.Language;
using Azure.Mcp.Tools.Functions.Models;
using Azure.Mcp.Tools.Functions.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Services.Caching;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.Functions.UnitTests.Language;

public sealed class LanguageListCommandTests : CommandUnitTestsBase<LanguageListCommand, IFunctionsService>
{
    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        Assert.Equal("list", Command.Name);
        Assert.NotEmpty(Command.Description);
        Assert.Equal("List Supported Languages", Command.Title);
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
    public async Task ExecuteAsync_ReturnsLanguageList()
    {
        // Arrange
        var expectedResult = new LanguageListResult
        {
            FunctionsRuntimeVersion = "4.x",
            ExtensionBundleVersion = "[4.*, 5.0.0)",
            Languages =
            [
                new()
                {
                    Language = "python",
                    Info = new()
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
                        RuntimeVersions = new()
                        {
                            Supported = ["3.10", "3.11", "3.12", "3.13"],
                            Preview = ["3.14"],
                            Default = "3.11"
                        },
                        InitInstructions = "Test instructions",
                        ProjectStructure = ["function_app.py"]
                    },
                    RuntimeVersions = new()
                    {
                        Supported = ["3.10", "3.11", "3.12", "3.13"],
                        Preview = ["3.14"],
                        Default = "3.11"
                    }
                },
                new()
                {
                    Language = "csharp",
                    Info = new()
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
                        RuntimeVersions = new()
                        {
                            Supported = ["8", "9", "10"],
                            Deprecated = ["6", "7"],
                            Default = "8",
                            FrameworkSupported = ["4.8.1"]
                        },
                        InitInstructions = "Test instructions",
                        ProjectStructure = ["*.csproj"]
                    },
                    RuntimeVersions = new()
                    {
                        Supported = ["8", "9", "10"],
                        Deprecated = ["6", "7"],
                        Default = "8",
                        FrameworkSupported = ["4.8.1"]
                    }
                }
            ]
        };

        Service.GetLanguageListAsync(Arg.Any<CancellationToken>()).Returns(expectedResult);

        // Act
        var response = await ExecuteCommandAsync();

        // Assert
        var results = ValidateAndDeserializeResponse(response, FunctionsJsonContext.Default.ListLanguageListResult);

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
        Service.GetLanguageListAsync(Arg.Any<CancellationToken>()).ThrowsAsync(new InvalidOperationException("Service error"));

        // Act
        var response = await ExecuteCommandAsync();

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.Status);
        Assert.Contains("Service error", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_DeserializationValidation()
    {
        // Arrange - use the real service to verify actual data shape
        // GetLanguageListAsync now fetches manifest for runtime versions
        var mockManifestService = Substitute.For<IManifestService>();

        // Set up manifest with runtime versions
        var manifest = new TemplateManifest
        {
            RuntimeVersions = new Dictionary<string, RuntimeVersionInfo>
            {
                ["Python"] = new RuntimeVersionInfo { Supported = ["3.10", "3.11", "3.12", "3.13"], Preview = ["3.14"], Default = "3.11" },
                ["TypeScript"] = new RuntimeVersionInfo { Supported = ["20", "22"], Preview = ["24"], Default = "22" },
                ["JavaScript"] = new RuntimeVersionInfo { Supported = ["20", "22"], Preview = ["24"], Default = "22" },
                ["Java"] = new RuntimeVersionInfo { Supported = ["8", "11", "17", "21"], Preview = ["25"], Default = "21" },
                ["CSharp"] = new RuntimeVersionInfo { Supported = ["8", "9", "10"], FrameworkSupported = ["4.8.1"], Default = "8" },
                ["PowerShell"] = new RuntimeVersionInfo { Supported = ["7.4"], Default = "7.4" }
            }
        };
        mockManifestService.FetchManifestAsync(Arg.Any<CancellationToken>()).Returns(manifest);

        var realService = new FunctionsService(
            Substitute.For<IHttpClientFactory>(),
            new LanguageMetadataProvider(),
            mockManifestService,
            Substitute.For<ICacheService>(),
            Substitute.For<ILogger<FunctionsService>>());
        var realResult = await realService.GetLanguageListAsync(TestContext.Current.CancellationToken);
        Service.GetLanguageListAsync(Arg.Any<CancellationToken>()).Returns(realResult);

        // Act
        var response = await ExecuteCommandAsync();

        // Assert
        var results = ValidateAndDeserializeResponse(response, FunctionsJsonContext.Default.ListLanguageListResult);

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
