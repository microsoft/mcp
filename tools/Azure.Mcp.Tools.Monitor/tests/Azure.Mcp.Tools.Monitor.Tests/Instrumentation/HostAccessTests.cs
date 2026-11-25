// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Monitor.Instrumentation.Detectors;
using Azure.Mcp.Tools.Monitor.Instrumentation.Generators;
using Azure.Mcp.Tools.Monitor.Instrumentation.Pipeline;
using Azure.Mcp.Tools.Monitor.Models.Instrumentation;
using Azure.Mcp.Tools.Monitor.Tools.Instrumentation;
using Microsoft.Mcp.Core.Areas.Server;
using Microsoft.Mcp.Core.Services.Host;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.Monitor.Tests.Instrumentation;

public sealed class HostAccessTests()
{
    [Fact]
    public void HttpMode_OrchestratorThrowsInsteadOfReturningAnAnalysisResult()
    {
        var fileSystem = CreateFileSystem("http");
        var orchestrator = new OrchestratorTool(new WorkspaceAnalyzer([], [], [], [], fileSystem));

        var exception = Assert.Throws<UnauthorizedAccessException>(() => orchestrator.Start("workspace"));

        Assert.Contains("HTTP mode", exception.Message, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(Language.DotNet)]
    [InlineData(Language.NodeJs)]
    [InlineData(Language.Python)]
    public void HttpMode_DetectorsDenyWorkspaceAccess(Language language)
    {
        var fileSystem = CreateFileSystem("http");
        ILanguageDetector languageDetector = language switch
        {
            Language.DotNet => new DotNetLanguageDetector(fileSystem),
            Language.NodeJs => new NodeJsLanguageDetector(fileSystem),
            _ => new PythonLanguageDetector(fileSystem)
        };
        IAppTypeDetector appTypeDetector = language switch
        {
            Language.DotNet => new DotNetAppTypeDetector(fileSystem),
            Language.NodeJs => new NodeJsAppTypeDetector(fileSystem),
            _ => new PythonAppTypeDetector(fileSystem, new PythonInstrumentationRegistry(fileSystem))
        };
        IInstrumentationDetector instrumentationDetector = language switch
        {
            Language.DotNet => new DotNetInstrumentationDetector(fileSystem),
            Language.NodeJs => new NodeJsInstrumentationDetector(fileSystem),
            _ => new PythonInstrumentationDetector(fileSystem)
        };

        Assert.Throws<UnauthorizedAccessException>(() => languageDetector.CanHandle("workspace"));
        Assert.Throws<UnauthorizedAccessException>(() => appTypeDetector.DetectProjects("workspace"));
        Assert.Throws<UnauthorizedAccessException>(() => instrumentationDetector.Detect("workspace"));
    }

    [Fact]
    public void HttpMode_PythonRegistryDoesNotReturnFallbackData()
    {
        var registry = new PythonInstrumentationRegistry(CreateFileSystem("http"));

        Assert.Throws<UnauthorizedAccessException>(() => registry.GetByCategory("framework"));
    }

    [Fact]
    public void DotNetDetectors_DoNotSwallowDeniedProjectReads()
    {
        var fileSystem = Substitute.For<IFileSystem>();
        fileSystem.GetFiles("workspace", "*.csproj", SearchOption.AllDirectories).Returns(["app.csproj"]);
        fileSystem.ReadAllText("app.csproj").Returns(_ => throw new UnauthorizedAccessException("Access denied"));

        Assert.Throws<UnauthorizedAccessException>(() => new DotNetAppTypeDetector(fileSystem).DetectProjects("workspace"));
        Assert.Throws<UnauthorizedAccessException>(() => new DotNetInstrumentationDetector(fileSystem).Detect("workspace"));
    }

    [Fact]
    public void HttpMode_LangchainGeneratorDoesNotDefaultToCommonJs()
    {
        var generator = new LangchainJsGreenfieldGenerator(CreateFileSystem("http"));
        var analysis = new Analysis
        {
            Language = Language.NodeJs,
            State = InstrumentationState.Greenfield,
            Projects = [new ProjectInfo { ProjectFile = "package.json", AppType = AppType.LangchainJs }]
        };

        Assert.Throws<UnauthorizedAccessException>(() => generator.Generate(analysis));
    }

    [Fact]
    public void LocalMode_DotNetDetectorsReadProjectFiles()
    {
        var workspace = Path.Combine(AppContext.BaseDirectory, $"host-access-{Guid.NewGuid():N}");
        Directory.CreateDirectory(workspace);
        var project = Path.Combine(workspace, "app.csproj");
        File.WriteAllText(project, """
            <Project Sdk="Microsoft.NET.Sdk.Web">
              <ItemGroup>
                <PackageReference Include="Microsoft.ApplicationInsights.AspNetCore" Version="2.23.0" />
              </ItemGroup>
            </Project>
            """);
        File.WriteAllText(Path.Combine(workspace, "Program.cs"), "var builder = WebApplication.CreateBuilder(args);");

        try
        {
            var fileSystem = CreateFileSystem("stdio");
            Assert.True(new DotNetLanguageDetector(fileSystem).CanHandle(workspace));
            var detectedProject = Assert.Single(new DotNetAppTypeDetector(fileSystem).DetectProjects(workspace));
            Assert.Equal(AppType.AspNetCore, detectedProject.AppType);
            Assert.Equal(HostingPattern.MinimalApi, detectedProject.HostingPattern);
            Assert.Equal(InstrumentationState.Brownfield, new DotNetInstrumentationDetector(fileSystem).Detect(workspace).State);
        }
        finally
        {
            Directory.Delete(workspace, true);
        }
    }

    private static FileSystem CreateFileSystem(string transport) =>
        new(Microsoft.Extensions.Options.Options.Create(new ServerRuntimeConfiguration { Transport = transport }));
}
