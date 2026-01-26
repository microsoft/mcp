// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using ToolMetadataExporter.Models;
using ToolMetadataExporter.Services;
using Xunit;

namespace ToolMetadataExporter.UnitTests.Models;

public class RunInformationTests
{
    private static AzmcpProgram CreateMockMcpServer()
    {
        var utilityLogger = Substitute.For<ILogger<Utility>>();
        var utility = Substitute.ForPartsOf<Utility>(utilityLogger);
        var logger = Substitute.For<ILogger<AzmcpProgram>>();
        var programOptions = Substitute.For<IOptions<AppConfiguration>>();
        var programConfig = new AppConfiguration
        {
            WorkDirectory = Path.GetTempPath(),
            AzmcpExe = "azmcp"
        };
        programOptions.Value.Returns(programConfig);

        var mockMcpServer = Substitute.ForPartsOf<AzmcpProgram>(utility, programOptions, logger);
        return mockMcpServer;
    }

    [Fact]
    public void Constructor_InitializesProperties()
    {
        // Arrange
        var mockMcpServer = CreateMockMcpServer();

        // Act
        var runInfo = new RunInformation(mockMcpServer);

        // Assert
        Assert.NotNull(runInfo.Id);
        Assert.NotEqual(Guid.Empty.ToString(), runInfo.Id);
        Assert.Same(mockMcpServer, runInfo.McpServer);
    }

    [Fact]
    public void Constructor_GeneratesUniqueId()
    {
        // Arrange
        var mockMcpServer = CreateMockMcpServer();

        // Act
        var runInfo1 = new RunInformation(mockMcpServer);
        var runInfo2 = new RunInformation(mockMcpServer);

        // Assert
        Assert.NotEqual(runInfo1.Id, runInfo2.Id);
    }

    [Fact]
    public async Task GetRunInfoFileNameAsync_ReturnsVersionAndBaseFileName()
    {
        // Arrange
        var mockMcpServer = CreateMockMcpServer();
        mockMcpServer.GetServerVersionAsync().Returns(Task.FromResult("2.0.0"));

        var runInfo = new RunInformation(mockMcpServer);
        var baseFileName = "output_data";

        // Act
        var result = await runInfo.GetRunInfoFileNameAsync(baseFileName);

        // Assert
        Assert.Equal("2.0.0_output_data", result);
    }

    [Fact]
    public async Task GetRunInfoFileNameAsync_IncludesVersionAndBaseFileName()
    {
        // Arrange
        var mockMcpServer = CreateMockMcpServer();
        mockMcpServer.GetServerVersionAsync().Returns(Task.FromResult("3.5.7"));

        var runInfo = new RunInformation(mockMcpServer);
        var baseFileName = "metrics_export";

        // Act
        var result = await runInfo.GetRunInfoFileNameAsync(baseFileName);

        // Assert
        Assert.Equal("3.5.7_metrics_export", result);
        Assert.StartsWith("3.5.7_", result);
        Assert.EndsWith("_export", result);
    }

    [Fact]
    public async Task GetRunInfoFileNameAsync_HandlesVersionWithMetadata()
    {
        // Arrange
        var mockMcpServer = CreateMockMcpServer();
        mockMcpServer.GetServerVersionAsync().Returns(Task.FromResult("1.0.0-beta+commit123"));

        var runInfo = new RunInformation(mockMcpServer);
        var baseFileName = "test";

        // Act
        var result = await runInfo.GetRunInfoFileNameAsync(baseFileName);

        // Assert
        Assert.Equal("1.0.0-beta+commit123_test", result);
    }

    [Fact]
    public async Task GetRunInfoFileNameAsync_HandlesDifferentBaseFileNames()
    {
        // Arrange
        var mockMcpServer = CreateMockMcpServer();
        mockMcpServer.GetServerVersionAsync().Returns(Task.FromResult("1.0.0"));

        var runInfo = new RunInformation(mockMcpServer);

        // Act
        var result1 = await runInfo.GetRunInfoFileNameAsync("export_data");
        var result2 = await runInfo.GetRunInfoFileNameAsync("tool_changes");
        var result3 = await runInfo.GetRunInfoFileNameAsync("metrics");

        // Assert
        Assert.Equal("1.0.0_export_data", result1);
        Assert.Equal("1.0.0_tool_changes", result2);
        Assert.Equal("1.0.0_metrics", result3);
    }

    [Fact]
    public async Task GetRunInfoFileNameAsync_CallsGetServerVersionAsync()
    {
        // Arrange
        var mockMcpServer = CreateMockMcpServer();
        mockMcpServer.GetServerVersionAsync().Returns(Task.FromResult("1.0.0"));

        var runInfo = new RunInformation(mockMcpServer);

        // Act
        await runInfo.GetRunInfoFileNameAsync("test");

        // Assert
        await mockMcpServer.Received(1).GetServerVersionAsync();
    }

    [Fact]
    public void Id_IsValidGuid()
    {
        // Arrange
        var mockMcpServer = CreateMockMcpServer();

        // Act
        var runInfo = new RunInformation(mockMcpServer);

        // Assert
        Assert.True(Guid.TryParse(runInfo.Id, out var parsedGuid));
        Assert.NotEqual(Guid.Empty, parsedGuid);
    }

    [Fact]
    public void McpServer_ReturnsInjectedInstance()
    {
        // Arrange
        var mockMcpServer = CreateMockMcpServer();

        // Act
        var runInfo = new RunInformation(mockMcpServer);

        // Assert
        Assert.Same(mockMcpServer, runInfo.McpServer);
    }

    [Fact]
    public async Task GetRunInfoFileNameAsync_WithEmptyBaseFileName_ReturnsCorrectFormat()
    {
        // Arrange
        var mockMcpServer = CreateMockMcpServer();
        mockMcpServer.GetServerVersionAsync().Returns(Task.FromResult("1.0.0"));

        var runInfo = new RunInformation(mockMcpServer);

        // Act
        var result = await runInfo.GetRunInfoFileNameAsync(string.Empty);

        // Assert
        Assert.Equal("1.0.0_", result);
    }

    [Fact]
    public async Task GetRunInfoFileNameAsync_MultipleCalls_ReturnsConsistentVersion()
    {
        // Arrange
        var mockMcpServer = CreateMockMcpServer();
        mockMcpServer.GetServerVersionAsync().Returns(Task.FromResult("2.3.4"));

        var runInfo = new RunInformation(mockMcpServer);

        // Act
        var result1 = await runInfo.GetRunInfoFileNameAsync("test1");
        var result2 = await runInfo.GetRunInfoFileNameAsync("test2");

        // Assert
        Assert.Equal("2.3.4_test1", result1);
        Assert.Equal("2.3.4_test2", result2);
    }
}
