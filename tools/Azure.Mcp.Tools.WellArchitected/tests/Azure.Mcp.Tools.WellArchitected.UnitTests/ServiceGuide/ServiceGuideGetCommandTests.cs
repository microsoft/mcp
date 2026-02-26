// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.CommandLine.Parsing;
using System.Net;
using Azure.Mcp.Tools.WellArchitected.Commands.ServiceGuide;
using Azure.Mcp.Tools.WellArchitected.Models;
using Azure.Mcp.Tools.WellArchitected.Options;
using Azure.Mcp.Tools.WellArchitected.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Models.Command;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.WellArchitected.UnitTests.ServiceGuide;

public class ServiceGuideGetCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IWellArchitectedService _service;
    private readonly ILogger<ServiceGuideGetCommand> _logger;
    private readonly ServiceGuideGetCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;

    public ServiceGuideGetCommandTests()
    {
        _service = Substitute.For<IWellArchitectedService>();
        _logger = Substitute.For<ILogger<ServiceGuideGetCommand>>();

        var collection = new ServiceCollection().AddSingleton(_service);
        _serviceProvider = collection.BuildServiceProvider();
        _command = new(_logger);
        _context = new(_serviceProvider);
        _commandDefinition = _command.GetCommand();
    }

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = _command.GetCommand();
        Assert.Equal("get", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsServiceGuide_WhenFound()
    {
        var serviceName = "Azure Storage";
        var mockServiceGuide = new WafServiceGuide(
            serviceName,
            "This is the service guide content for Azure Storage")
        {
            Recommendations =
            [
                new WafRecommendation("rec-1", "Enable encryption", "Encrypt data", "Security", "Details", serviceName),
                new WafRecommendation("rec-2", "Configure backup", "Set up backup", "Reliability", "Details", serviceName)
            ]
        };

        _service.GetServiceGuideAsync(serviceName, Arg.Any<CancellationToken>())
            .Returns(mockServiceGuide);

        var parseResult = _commandDefinition.Parse(["--service", serviceName]);
        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
        Assert.Empty(response.Message);

        await _service.Received(1).GetServiceGuideAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsNull_WhenServiceNotFound()
    {
        var serviceName = "NonexistentService";

        _service.GetServiceGuideAsync(serviceName, Arg.Any<CancellationToken>())
            .Returns((WafServiceGuide?)null);

        var parseResult = _commandDefinition.Parse(["--service", serviceName]);
        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);

        await _service.Received(1).GetServiceGuideAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        var serviceName = "Azure Storage";
        var exception = new InvalidOperationException("Service error");

        _service.GetServiceGuideAsync(serviceName, Arg.Any<CancellationToken>())
            .ThrowsAsync(exception);

        var parseResult = _commandDefinition.Parse(["--service", serviceName]);
        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.NotNull(response.Message);
        Assert.NotEmpty(response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_ValidatesInput_Success()
    {
        _service.GetServiceGuideAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new WafServiceGuide("TestService", "Content"));

        var parseResult = _commandDefinition.Parse(["--service", "Azure Storage"]);
        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.Status);
    }

    [Fact]
    public async Task ExecuteAsync_ValidatesInput_MissingService()
    {
        var parseResult = _commandDefinition.Parse([]);
        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

        Assert.NotEqual(HttpStatusCode.OK, response.Status);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesEmptyService()
    {
        var parseResult = _commandDefinition.Parse(["--service", ""]);
        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

        Assert.NotEqual(HttpStatusCode.OK, response.Status);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsServiceGuideWithNoRecommendations()
    {
        var serviceName = "BasicService";
        var mockServiceGuide = new WafServiceGuide(
            serviceName,
            "Service guide content without recommendations");

        _service.GetServiceGuideAsync(serviceName, Arg.Any<CancellationToken>())
            .Returns(mockServiceGuide);

        var parseResult = _commandDefinition.Parse(["--service", serviceName]);
        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceNameWithSpaces()
    {
        var serviceName = "Azure App Service";
        var mockServiceGuide = new WafServiceGuide(
            serviceName,
            "Guide for Azure App Service")
        {
            Recommendations = [new WafRecommendation("rec-1", "Title", "Desc", "Security", "Content", serviceName)]
        };

        _service.GetServiceGuideAsync(serviceName, Arg.Any<CancellationToken>())
            .Returns(mockServiceGuide);

        var parseResult = _commandDefinition.Parse(["--service", serviceName]);
        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);

        await _service.Received(1).GetServiceGuideAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public void BindOptions_BindsOptionsCorrectly()
    {
        var serviceName = "Azure Kubernetes Service";
        var parseResult = _commandDefinition.Parse(["--service", serviceName]);

        var options = _command.GetType()
            .GetMethod("BindOptions", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.Invoke(_command, [parseResult]) as ServiceGuideGetOptions;

        Assert.NotNull(options);
        Assert.Equal(serviceName, options.Service);
    }
}
