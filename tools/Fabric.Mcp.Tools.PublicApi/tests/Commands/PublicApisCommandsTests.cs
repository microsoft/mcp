// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Net;
using Azure.Mcp.Core.Models.Command;
using Fabric.Mcp.Tools.PublicApi.Commands.PublicApis;
using Fabric.Mcp.Tools.PublicApi.Models;
using Fabric.Mcp.Tools.PublicApi.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Fabric.Mcp.Tools.PublicApi.Tests.Commands;

public class PublicApisCommandsTests
{
    [Fact]
    public void DiscoverPublicWorkloadsCommand_HasCorrectProperties()
    {
        // Arrange
        var logger = LoggerFactory.Create(builder => { }).CreateLogger<ListWorkloadsCommand>();

        // Act
        var command = new ListWorkloadsCommand(logger);

        // Assert
        Assert.Equal("list", command.Name);
        Assert.NotEmpty(command.Description);
        Assert.Equal("List Available Fabric Workloads", command.Title);
        Assert.False(command.Metadata.Destructive);
        Assert.True(command.Metadata.ReadOnly);
    }

    [Fact]
    public void DiscoverPublicWorkloadsCommand_GetCommand_ReturnsValidCommand()
    {
        // Arrange
        var logger = LoggerFactory.Create(builder => { }).CreateLogger<ListWorkloadsCommand>();
        var command = new ListWorkloadsCommand(logger);

        // Act
        var systemCommand = command.GetCommand();

        // Assert
        Assert.NotNull(systemCommand);
        Assert.Equal("list", systemCommand.Name);
    }

    [Fact]
    public async Task DiscoverPublicWorkloadsCommand_ExecuteAsync_ReturnsWorkloads()
    {
        // Arrange
        var logger = LoggerFactory.Create(builder => { }).CreateLogger<ListWorkloadsCommand>();
        var command = new ListWorkloadsCommand(logger);
        var fabricService = Substitute.For<IFabricPublicApiService>();
        var expectedWorkloads = new[] { "notebook", "report", "platform" };

        fabricService.ListWorkloadsAsync().Returns(expectedWorkloads);

        var services = new ServiceCollection();
        services.AddSingleton(fabricService);
        var serviceProvider = services.BuildServiceProvider();

        var context = new CommandContext(serviceProvider);
        var parseResult = CreateParseResult(command.GetCommand(), Array.Empty<string>());

        // Act
        var result = await command.ExecuteAsync(context, parseResult);

        // Assert
        Assert.Equal(200, result.Status);
        Assert.NotNull(result.Results);
        await fabricService.Received(1).ListWorkloadsAsync();
    }

    [Fact]
    public async Task DiscoverPublicWorkloadsCommand_ExecuteAsync_HandlesException()
    {
        // Arrange
        var logger = LoggerFactory.Create(builder => { }).CreateLogger<ListWorkloadsCommand>();
        var command = new ListWorkloadsCommand(logger);
        var fabricService = Substitute.For<IFabricPublicApiService>();

        fabricService.ListWorkloadsAsync().ThrowsAsync(new InvalidOperationException("Service error"));

        var services = new ServiceCollection();
        services.AddSingleton(fabricService);
        var serviceProvider = services.BuildServiceProvider();

        var context = new CommandContext(serviceProvider);
        var parseResult = CreateParseResult(command.GetCommand(), Array.Empty<string>());

        // Act
        var result = await command.ExecuteAsync(context, parseResult);

        // Assert
        Assert.Equal(500, result.Status);
        Assert.NotEmpty(result.Message);
    }

    [Fact]
    public void GetPlatformApisCommand_HasCorrectProperties()
    {
        // Arrange
        var logger = LoggerFactory.Create(builder => { }).CreateLogger<GetPlatformApisCommand>();

        // Act
        var command = new GetPlatformApisCommand(logger);

        // Assert
        Assert.Equal("get", command.Name);
        Assert.NotEmpty(command.Description);
        Assert.Equal("Get Platform API Specification", command.Title);
        Assert.False(command.Metadata.Destructive);
        Assert.True(command.Metadata.ReadOnly);
    }

    [Fact]
    public void GetPlatformApisCommand_GetCommand_ReturnsValidCommand()
    {
        // Arrange
        var logger = LoggerFactory.Create(builder => { }).CreateLogger<GetPlatformApisCommand>();
        var command = new GetPlatformApisCommand(logger);

        // Act
        var systemCommand = command.GetCommand();

        // Assert
        Assert.NotNull(systemCommand);
        Assert.Equal("get", systemCommand.Name);
    }

    [Fact]
    public async Task GetPlatformApisCommand_ExecuteAsync_ReturnsPlatformApis()
    {
        // Arrange
        var logger = LoggerFactory.Create(builder => { }).CreateLogger<GetPlatformApisCommand>();
        var command = new GetPlatformApisCommand(logger);
        var fabricService = Substitute.For<IFabricPublicApiService>();
        var expectedApi = new FabricWorkloadPublicApi("api-spec", new Dictionary<string, string> { { "model1", "definition1" } });

        fabricService.GetWorkloadPublicApis("platform").Returns(expectedApi);

        var services = new ServiceCollection();
        services.AddSingleton(fabricService);
        var serviceProvider = services.BuildServiceProvider();

        var context = new CommandContext(serviceProvider);
        var parseResult = CreateParseResult(command.GetCommand(), Array.Empty<string>());

        // Act
        var result = await command.ExecuteAsync(context, parseResult);

        // Assert
        Assert.Equal(200, result.Status);
        Assert.NotNull(result.Results);
        await fabricService.Received(1).GetWorkloadPublicApis("platform");
    }

    [Fact]
    public async Task GetPlatformApisCommand_ExecuteAsync_HandlesException()
    {
        // Arrange
        var logger = LoggerFactory.Create(builder => { }).CreateLogger<GetPlatformApisCommand>();
        var command = new GetPlatformApisCommand(logger);
        var fabricService = Substitute.For<IFabricPublicApiService>();

        fabricService.GetWorkloadPublicApis("platform").ThrowsAsync(new InvalidOperationException("Service error"));

        var services = new ServiceCollection();
        services.AddSingleton(fabricService);
        var serviceProvider = services.BuildServiceProvider();

        var context = new CommandContext(serviceProvider);
        var parseResult = CreateParseResult(command.GetCommand(), Array.Empty<string>());

        // Act
        var result = await command.ExecuteAsync(context, parseResult);

        // Assert
        Assert.Equal(500, result.Status);
        Assert.NotEmpty(result.Message);
    }

    [Fact]
    public void GetWorkloadApisCommand_HasCorrectProperties()
    {
        // Arrange
        var logger = LoggerFactory.Create(builder => { }).CreateLogger<GetWorkloadApisCommand>();

        // Act
        var command = new GetWorkloadApisCommand(logger);

        // Assert
        Assert.Equal("get", command.Name);
        Assert.NotEmpty(command.Description);
        Assert.Equal("Get Workload API Specification", command.Title);
        Assert.False(command.Metadata.Destructive);
        Assert.True(command.Metadata.ReadOnly);
    }

    [Fact]
    public void GetWorkloadApisCommand_GetCommand_ReturnsValidCommand()
    {
        // Arrange
        var logger = LoggerFactory.Create(builder => { }).CreateLogger<GetWorkloadApisCommand>();
        var command = new GetWorkloadApisCommand(logger);

        // Act
        var systemCommand = command.GetCommand();

        // Assert
        Assert.NotNull(systemCommand);
        Assert.Equal("get", systemCommand.Name);
        // Options are registered dynamically during command parsing
    }

    [Fact]
    public async Task GetWorkloadApisCommand_ExecuteAsync_WithValidWorkloadType_ReturnsApis()
    {
        // Arrange
        var logger = LoggerFactory.Create(builder => { }).CreateLogger<GetWorkloadApisCommand>();
        var command = new GetWorkloadApisCommand(logger);
        var fabricService = Substitute.For<IFabricPublicApiService>();
        var expectedApi = new FabricWorkloadPublicApi("api-spec", new Dictionary<string, string> { { "model1", "definition1" } });

        fabricService.GetWorkloadPublicApis("notebook").Returns(expectedApi);

        var services = new ServiceCollection();
        services.AddSingleton(fabricService);
        var serviceProvider = services.BuildServiceProvider();

        var context = new CommandContext(serviceProvider);
        var parseResult = CreateParseResult(command.GetCommand(), new[] { "--workload-type", "notebook" });

        // Act
        var result = await command.ExecuteAsync(context, parseResult);

        // Assert
        Assert.Equal(200, result.Status);
        Assert.NotNull(result.Results);
        await fabricService.Received(1).GetWorkloadPublicApis("notebook");
    }

    [Fact]
    public async Task GetWorkloadApisCommand_ExecuteAsync_WithEmptyWorkloadType_ReturnsBadRequest()
    {
        // Arrange
        var logger = LoggerFactory.Create(builder => { }).CreateLogger<GetWorkloadApisCommand>();
        var command = new GetWorkloadApisCommand(logger);
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
        await fabricService.DidNotReceive().GetWorkloadPublicApis(Arg.Any<string>());
    }

    [Fact]
    public async Task GetWorkloadApisCommand_ExecuteAsync_WithCommonWorkloadType_ReturnsNotFound()
    {
        // Arrange
        var logger = LoggerFactory.Create(builder => { }).CreateLogger<GetWorkloadApisCommand>();
        var command = new GetWorkloadApisCommand(logger);
        var fabricService = Substitute.For<IFabricPublicApiService>();

        var services = new ServiceCollection();
        services.AddSingleton(fabricService);
        var serviceProvider = services.BuildServiceProvider();

        var context = new CommandContext(serviceProvider);
        var parseResult = CreateParseResult(command.GetCommand(), new[] { "--workload-type", "common" });

        // Act
        var result = await command.ExecuteAsync(context, parseResult);

        // Assert
        Assert.Equal(404, result.Status);
        Assert.Contains("No workload of type 'common' exists", result.Message);
        Assert.Contains("Did you mean 'platform'?", result.Message);
        await fabricService.DidNotReceive().GetWorkloadPublicApis(Arg.Any<string>());
    }

    [Fact]
    public async Task GetWorkloadApisCommand_ExecuteAsync_WithHttpNotFoundError_ReturnsNotFound()
    {
        // Arrange
        var logger = LoggerFactory.Create(builder => { }).CreateLogger<GetWorkloadApisCommand>();
        var command = new GetWorkloadApisCommand(logger);
        var fabricService = Substitute.For<IFabricPublicApiService>();

        var httpException = new HttpRequestException("Not found", null, HttpStatusCode.NotFound);
        fabricService.GetWorkloadPublicApis("invalid-workload").ThrowsAsync(httpException);

        var services = new ServiceCollection();
        services.AddSingleton(fabricService);
        var serviceProvider = services.BuildServiceProvider();

        var context = new CommandContext(serviceProvider);
        var parseResult = CreateParseResult(command.GetCommand(), new[] { "--workload-type", "invalid-workload" });

        // Act
        var result = await command.ExecuteAsync(context, parseResult);

        // Assert
        Assert.Equal(404, result.Status);
        Assert.Contains("No workload of type 'invalid-workload' exists", result.Message);
        Assert.Contains("discover-workloads command", result.Message);
    }

    [Fact]
    public async Task GetWorkloadApisCommand_ExecuteAsync_WithHttpError_ReturnsMappedStatusCode()
    {
        // Arrange
        var logger = LoggerFactory.Create(builder => { }).CreateLogger<GetWorkloadApisCommand>();
        var command = new GetWorkloadApisCommand(logger);
        var fabricService = Substitute.For<IFabricPublicApiService>();

        var httpException = new HttpRequestException("Service unavailable", null, HttpStatusCode.ServiceUnavailable);
        fabricService.GetWorkloadPublicApis("notebook").ThrowsAsync(httpException);

        var services = new ServiceCollection();
        services.AddSingleton(fabricService);
        var serviceProvider = services.BuildServiceProvider();

        var context = new CommandContext(serviceProvider);
        var parseResult = CreateParseResult(command.GetCommand(), new[] { "--workload-type", "notebook" });

        // Act
        var result = await command.ExecuteAsync(context, parseResult);

        // Assert
        Assert.Equal(503, result.Status);
        Assert.Equal("Service unavailable", result.Message);
    }

    [Fact]
    public async Task GetWorkloadApisCommand_ExecuteAsync_WithGeneralException_ReturnsInternalServerError()
    {
        // Arrange
        var logger = LoggerFactory.Create(builder => { }).CreateLogger<GetWorkloadApisCommand>();
        var command = new GetWorkloadApisCommand(logger);
        var fabricService = Substitute.For<IFabricPublicApiService>();

        fabricService.GetWorkloadPublicApis("notebook").ThrowsAsync(new InvalidOperationException("Service error"));

        var services = new ServiceCollection();
        services.AddSingleton(fabricService);
        var serviceProvider = services.BuildServiceProvider();

        var context = new CommandContext(serviceProvider);
        var parseResult = CreateParseResult(command.GetCommand(), new[] { "--workload-type", "notebook" });

        // Act
        var result = await command.ExecuteAsync(context, parseResult);

        // Assert
        Assert.Equal(500, result.Status);
        Assert.NotEmpty(result.Message);
    }

    private static ParseResult CreateParseResult(Command command, string[] args)
    {
        return command.Parse(args);
    }
}
