// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.CommandLine.Parsing;
using System.Net;
using Azure.Mcp.Tools.WellArchitected.Commands.Recommendation;
using Azure.Mcp.Tools.WellArchitected.Models;
using Azure.Mcp.Tools.WellArchitected.Options;
using Azure.Mcp.Tools.WellArchitected.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Models.Command;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.WellArchitected.UnitTests.Recommendation;

public class RecommendationListCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IWellArchitectedService _service;
    private readonly ILogger<RecommendationListCommand> _logger;
    private readonly RecommendationListCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;

    public RecommendationListCommandTests()
    {
        _service = Substitute.For<IWellArchitectedService>();
        _logger = Substitute.For<ILogger<RecommendationListCommand>>();

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
        Assert.Equal("list", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsRecommendations_WhenNoFilters()
    {
        var mockRecommendations = new List<WafRecommendation>
        {
            new("rec-1", "Security Recommendation", "Description 1", "Security", "Content 1"),
            new("rec-2", "Cost Optimization", "Description 2", "Cost Optimization", "Content 2")
        };

        _service.ListRecommendationsAsync(null, null, Arg.Any<CancellationToken>())
            .Returns(mockRecommendations);

        var parseResult = _commandDefinition.Parse("");
        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
        Assert.Empty(response.Message);

        await _service.Received(1).ListRecommendationsAsync(null, null, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_FiltersByPillar()
    {
        var pillar = "Security";
        var mockRecommendations = new List<WafRecommendation>
        {
            new("rec-1", "Security Recommendation", "Description 1", "Security", "Content 1")
        };

        _service.ListRecommendationsAsync(pillar, null, Arg.Any<CancellationToken>())
            .Returns(mockRecommendations);

        var parseResult = _commandDefinition.Parse(["--pillar", pillar]);
        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);

        await _service.Received(1).ListRecommendationsAsync(Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        var exception = new InvalidOperationException("Service error");

        _service.ListRecommendationsAsync(null, null, Arg.Any<CancellationToken>())
            .ThrowsAsync(exception);

        var parseResult = _commandDefinition.Parse("");
        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.NotNull(response.Message);
        Assert.NotEmpty(response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsEmpty_WhenNoRecommendations()
    {
        _service.ListRecommendationsAsync(null, null, Arg.Any<CancellationToken>())
            .Returns(new List<WafRecommendation>());

        var parseResult = _commandDefinition.Parse("");
        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
    }

    [Fact]
    public async Task ExecuteAsync_FiltersByService()
    {
        var service = "Azure Storage";
        var mockRecommendations = new List<WafRecommendation>
        {
            new("rec-1", "Storage Security", "Description", "Security", "Content", service)
        };

        _service.ListRecommendationsAsync(null, service, Arg.Any<CancellationToken>())
            .Returns(mockRecommendations);

        var parseResult = _commandDefinition.Parse(["--service", service]);
        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);

        await _service.Received(1).ListRecommendationsAsync(Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_FiltersByPillarAndService()
    {
        var pillar = "Security";
        var service = "Azure Storage";
        var mockRecommendations = new List<WafRecommendation>
        {
            new("rec-1", "Storage Security", "Description", pillar, "Content", service)
        };

        _service.ListRecommendationsAsync(pillar, service, Arg.Any<CancellationToken>())
            .Returns(mockRecommendations);

        var parseResult = _commandDefinition.Parse(["--pillar", pillar, "--service", service]);
        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);

        await _service.Received(1).ListRecommendationsAsync(Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public void BindOptions_BindsOptionsCorrectly()
    {
        var pillar = "Security";
        var service = "Azure Storage";
        var parseResult = _commandDefinition.Parse(["--pillar", pillar, "--service", service]);

        var options = _command.GetType()
            .GetMethod("BindOptions", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.Invoke(_command, [parseResult]) as RecommendationListOptions;

        Assert.NotNull(options);
        Assert.Equal(pillar, options.Pillar);
        Assert.Equal(service, options.Service);
    }
}
