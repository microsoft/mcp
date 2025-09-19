// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using Azure.Mcp.Core.Models.Command;
using Fabric.Mcp.Tools.PublicApi.Commands.BestPractices;
using Fabric.Mcp.Tools.PublicApi.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Fabric.Mcp.Tools.PublicApi.Tests.Commands;

public class BestPracticesCommandsTests
{
    #region GetBestPracticesCommand Tests

    [Fact]
    public void GetBestPracticesCommand_HasCorrectProperties()
    {
        // Arrange
        var logger = LoggerFactory.Create(builder => { }).CreateLogger<GetBestPracticesCommand>();

        // Act
        var command = new GetBestPracticesCommand(logger);

        // Assert
        Assert.Equal("get", command.Name);
        Assert.NotEmpty(command.Description);
        Assert.Equal("Get API Examples", command.Title);
        Assert.False(command.Metadata.Destructive);
        Assert.True(command.Metadata.ReadOnly);
    }

    [Fact]
    public void GetBestPracticesCommand_GetCommand_ReturnsValidCommand()
    {
        // Arrange
        var logger = LoggerFactory.Create(builder => { }).CreateLogger<GetBestPracticesCommand>();
        var command = new GetBestPracticesCommand(logger);

        // Act
        var systemCommand = command.GetCommand();

        // Assert
        Assert.NotNull(systemCommand);
        Assert.Equal("get", systemCommand.Name);
        // Options are registered dynamically, not statically in the Options collection
    }

    [Fact]
    public async Task GetBestPracticesCommand_ExecuteAsync_WithValidTopic_ReturnsBestPractices()
    {
        // Arrange
        var logger = LoggerFactory.Create(builder => { }).CreateLogger<GetBestPracticesCommand>();
        var command = new GetBestPracticesCommand(logger);
        var fabricService = Substitute.For<IFabricPublicApiService>();
        var expectedPractices = new[] { "practice1", "practice2" };

        fabricService.GetTopicBestPractices("pagination").Returns(expectedPractices);

        var services = new ServiceCollection();
        services.AddSingleton(fabricService);
        var serviceProvider = services.BuildServiceProvider();

        var context = new CommandContext(serviceProvider);
        var parseResult = CreateParseResult(command.GetCommand(), ["--topic", "pagination"]);

        // Act
        var result = await command.ExecuteAsync(context, parseResult);

        // Assert
        Assert.Equal(200, result.Status);
        Assert.NotNull(result.Results);
        fabricService.Received(1).GetTopicBestPractices("pagination");
    }

    [Fact]
    public async Task GetBestPracticesCommand_ExecuteAsync_WithEmptyTopic_ReturnsBadRequest()
    {
        // Arrange
        var logger = LoggerFactory.Create(builder => { }).CreateLogger<GetBestPracticesCommand>();
        var command = new GetBestPracticesCommand(logger);
        var fabricService = Substitute.For<IFabricPublicApiService>();

        var services = new ServiceCollection();
        services.AddSingleton(fabricService);
        var serviceProvider = services.BuildServiceProvider();

        var context = new CommandContext(serviceProvider);
        var parseResult = CreateParseResult(command.GetCommand(), []);

        // Act
        var result = await command.ExecuteAsync(context, parseResult);

        // Assert
        Assert.Equal(400, result.Status);
        Assert.Equal("Missing Required options: --topic", result.Message);
        fabricService.DidNotReceive().GetTopicBestPractices(Arg.Any<string>());
    }

    [Fact]
    public async Task GetBestPracticesCommand_ExecuteAsync_WithInvalidTopic_ReturnsNotFound()
    {
        // Arrange
        var logger = LoggerFactory.Create(builder => { }).CreateLogger<GetBestPracticesCommand>();
        var command = new GetBestPracticesCommand(logger);
        var fabricService = Substitute.For<IFabricPublicApiService>();

        fabricService.GetTopicBestPractices("invalid-topic").Throws(new ArgumentException("Topic not found"));

        var services = new ServiceCollection();
        services.AddSingleton(fabricService);
        var serviceProvider = services.BuildServiceProvider();

        var context = new CommandContext(serviceProvider);
        var parseResult = CreateParseResult(command.GetCommand(), ["--topic", "invalid-topic"]);

        // Act
        var result = await command.ExecuteAsync(context, parseResult);

        // Assert
        Assert.Equal(404, result.Status);
        Assert.Contains("No best practice resources found for invalid-topic", result.Message);
    }

    [Fact]
    public async Task GetBestPracticesCommand_ExecuteAsync_WithServiceException_ReturnsInternalServerError()
    {
        // Arrange
        var logger = LoggerFactory.Create(builder => { }).CreateLogger<GetBestPracticesCommand>();
        var command = new GetBestPracticesCommand(logger);
        var fabricService = Substitute.For<IFabricPublicApiService>();

        fabricService.GetTopicBestPractices("pagination").Throws(new InvalidOperationException("Service error"));

        var services = new ServiceCollection();
        services.AddSingleton(fabricService);
        var serviceProvider = services.BuildServiceProvider();

        var context = new CommandContext(serviceProvider);
        var parseResult = CreateParseResult(command.GetCommand(), ["--topic", "pagination"]);

        // Act
        var result = await command.ExecuteAsync(context, parseResult);

        // Assert
        Assert.Equal(500, result.Status);
        Assert.NotEmpty(result.Message);
    }

    #endregion

    #region GetExamplesCommand Tests

    [Fact]
    public void GetExamplesCommand_HasCorrectProperties()
    {
        // Arrange
        var logger = LoggerFactory.Create(builder => { }).CreateLogger<GetExamplesCommand>();

        // Act
        var command = new GetExamplesCommand(logger);

        // Assert
        Assert.Equal("get", command.Name);
        Assert.NotEmpty(command.Description);
        Assert.Equal("Get API Examples", command.Title);
        Assert.False(command.Metadata.Destructive);
        Assert.True(command.Metadata.ReadOnly);
    }

    [Fact]
    public void GetExamplesCommand_GetCommand_ReturnsValidCommand()
    {
        // Arrange
        var logger = LoggerFactory.Create(builder => { }).CreateLogger<GetExamplesCommand>();
        var command = new GetExamplesCommand(logger);

        // Act
        var systemCommand = command.GetCommand();

        // Assert
        Assert.NotNull(systemCommand);
        Assert.Equal("get", systemCommand.Name);
        // Options are registered dynamically during command parsing
    }

    [Fact]
    public async Task GetExamplesCommand_ExecuteAsync_WithValidWorkloadType_ReturnsExamples()
    {
        // Arrange
        var logger = LoggerFactory.Create(builder => { }).CreateLogger<GetExamplesCommand>();
        var command = new GetExamplesCommand(logger);
        var fabricService = Substitute.For<IFabricPublicApiService>();
        var expectedExamples = new Dictionary<string, string>
        {
            { "example1.json", "content1" },
            { "example2.json", "content2" }
        };

        fabricService.GetWorkloadExamplesAsync("notebook").Returns(expectedExamples);

        var services = new ServiceCollection();
        services.AddSingleton(fabricService);
        var serviceProvider = services.BuildServiceProvider();

        var context = new CommandContext(serviceProvider);
        var parseResult = CreateParseResult(command.GetCommand(), ["--workload-type", "notebook"]);

        // Act
        var result = await command.ExecuteAsync(context, parseResult);

        // Assert
        Assert.Equal(200, result.Status);
        Assert.NotNull(result.Results);
        await fabricService.Received(1).GetWorkloadExamplesAsync("notebook");
    }

    [Fact]
    public async Task GetExamplesCommand_ExecuteAsync_WithEmptyWorkloadType_ReturnsBadRequest()
    {
        // Arrange
        var logger = LoggerFactory.Create(builder => { }).CreateLogger<GetExamplesCommand>();
        var command = new GetExamplesCommand(logger);
        var fabricService = Substitute.For<IFabricPublicApiService>();

        var services = new ServiceCollection();
        services.AddSingleton(fabricService);
        var serviceProvider = services.BuildServiceProvider();

        var context = new CommandContext(serviceProvider);
        var parseResult = CreateParseResult(command.GetCommand(), []);

        // Act
        var result = await command.ExecuteAsync(context, parseResult);

        // Assert
        Assert.Equal(400, result.Status);
        Assert.Equal("Missing Required options: --workload-type", result.Message);
        await fabricService.DidNotReceive().GetWorkloadExamplesAsync(Arg.Any<string>());
    }

    [Fact]
    public async Task GetExamplesCommand_ExecuteAsync_WithServiceException_ReturnsInternalServerError()
    {
        // Arrange
        var logger = LoggerFactory.Create(builder => { }).CreateLogger<GetExamplesCommand>();
        var command = new GetExamplesCommand(logger);
        var fabricService = Substitute.For<IFabricPublicApiService>();

        fabricService.GetWorkloadExamplesAsync("notebook").ThrowsAsync(new InvalidOperationException("Service error"));

        var services = new ServiceCollection();
        services.AddSingleton(fabricService);
        var serviceProvider = services.BuildServiceProvider();

        var context = new CommandContext(serviceProvider);
        var parseResult = CreateParseResult(command.GetCommand(), ["--workload-type", "notebook"]);

        // Act
        var result = await command.ExecuteAsync(context, parseResult);

        // Assert
        Assert.Equal(500, result.Status);
        Assert.NotEmpty(result.Message);
    }

    #endregion

    #region GetWorkloadDefinitionCommand Tests

    [Fact]
    public void GetWorkloadDefinitionCommand_HasCorrectProperties()
    {
        // Arrange
        var logger = LoggerFactory.Create(builder => { }).CreateLogger<GetWorkloadDefinitionCommand>();

        // Act
        var command = new GetWorkloadDefinitionCommand(logger);

        // Assert
        Assert.Equal("get", command.Name);
        Assert.NotEmpty(command.Description);
        Assert.Equal("Get Workload Item Definition", command.Title);
        Assert.False(command.Metadata.Destructive);
        Assert.True(command.Metadata.ReadOnly);
    }

    [Fact]
    public void GetWorkloadDefinitionCommand_GetCommand_ReturnsValidCommand()
    {
        // Arrange
        var logger = LoggerFactory.Create(builder => { }).CreateLogger<GetWorkloadDefinitionCommand>();
        var command = new GetWorkloadDefinitionCommand(logger);

        // Act
        var systemCommand = command.GetCommand();

        // Assert
        Assert.NotNull(systemCommand);
        Assert.Equal("get", systemCommand.Name);
        // Options are registered dynamically during command parsing
    }

    [Fact]
    public async Task GetWorkloadDefinitionCommand_ExecuteAsync_WithValidWorkloadType_ReturnsDefinition()
    {
        // Arrange
        var logger = LoggerFactory.Create(builder => { }).CreateLogger<GetWorkloadDefinitionCommand>();
        var command = new GetWorkloadDefinitionCommand(logger);
        var fabricService = Substitute.For<IFabricPublicApiService>();
        var expectedDefinition = "{ \"schema\": \"definition\" }";

        fabricService.GetWorkloadItemDefinition("notebook").Returns(expectedDefinition);

        var services = new ServiceCollection();
        services.AddSingleton(fabricService);
        var serviceProvider = services.BuildServiceProvider();

        var context = new CommandContext(serviceProvider);
        var parseResult = CreateParseResult(command.GetCommand(), ["--workload-type", "notebook"]);

        // Act
        var result = await command.ExecuteAsync(context, parseResult);

        // Assert
        Assert.Equal(200, result.Status);
        Assert.NotNull(result.Results);
        fabricService.Received(1).GetWorkloadItemDefinition("notebook");
    }

    [Fact]
    public async Task GetWorkloadDefinitionCommand_ExecuteAsync_WithEmptyWorkloadType_ReturnsBadRequest()
    {
        // Arrange
        var logger = LoggerFactory.Create(builder => { }).CreateLogger<GetWorkloadDefinitionCommand>();
        var command = new GetWorkloadDefinitionCommand(logger);
        var fabricService = Substitute.For<IFabricPublicApiService>();

        var services = new ServiceCollection();
        services.AddSingleton(fabricService);
        var serviceProvider = services.BuildServiceProvider();

        var context = new CommandContext(serviceProvider);
        var parseResult = CreateParseResult(command.GetCommand(), Array.Empty<string>());

        // Act
        var result = await command.ExecuteAsync(context, parseResult);

        // Assert
        Assert.Equal(400, result.Status);
        Assert.Equal("Missing Required options: --workload-type", result.Message);
        fabricService.DidNotReceive().GetWorkloadItemDefinition(Arg.Any<string>());
    }

    [Fact]
    public async Task GetWorkloadDefinitionCommand_ExecuteAsync_WithInvalidWorkloadType_ReturnsNotFound()
    {
        // Arrange
        var logger = LoggerFactory.Create(builder => { }).CreateLogger<GetWorkloadDefinitionCommand>();
        var command = new GetWorkloadDefinitionCommand(logger);
        var fabricService = Substitute.For<IFabricPublicApiService>();

        fabricService.GetWorkloadItemDefinition("invalid-workload").Throws(new ArgumentException("Workload not found"));

        var services = new ServiceCollection();
        services.AddSingleton(fabricService);
        var serviceProvider = services.BuildServiceProvider();

        var context = new CommandContext(serviceProvider);
        var parseResult = CreateParseResult(command.GetCommand(), ["--workload-type", "invalid-workload"]);

        // Act
        var result = await command.ExecuteAsync(context, parseResult);

        // Assert
        Assert.Equal(404, result.Status);
        Assert.Contains("No item definition found for workload invalid-workload", result.Message);
    }

    [Fact]
    public async Task GetWorkloadDefinitionCommand_ExecuteAsync_WithServiceException_ReturnsInternalServerError()
    {
        // Arrange
        var logger = LoggerFactory.Create(builder => { }).CreateLogger<GetWorkloadDefinitionCommand>();
        var command = new GetWorkloadDefinitionCommand(logger);
        var fabricService = Substitute.For<IFabricPublicApiService>();

        fabricService.GetWorkloadItemDefinition("notebook").Throws(new InvalidOperationException("Service error"));

        var services = new ServiceCollection();
        services.AddSingleton(fabricService);
        var serviceProvider = services.BuildServiceProvider();

        var context = new CommandContext(serviceProvider);
        var parseResult = CreateParseResult(command.GetCommand(), ["--workload-type", "notebook"]);

        // Act
        var result = await command.ExecuteAsync(context, parseResult);

        // Assert
        Assert.Equal(500, result.Status);
        Assert.NotEmpty(result.Message);
    }

    #endregion

    #region Helper Methods

    private static ParseResult CreateParseResult(Command command, string[] args)
    {
        return command.Parse(args);
    }

    #endregion
}
