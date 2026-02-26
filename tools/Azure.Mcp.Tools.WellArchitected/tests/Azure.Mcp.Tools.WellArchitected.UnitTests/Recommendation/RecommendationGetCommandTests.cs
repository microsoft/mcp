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

public class RecommendationGetCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IWellArchitectedService _service;
    private readonly ILogger<RecommendationGetCommand> _logger;
    private readonly RecommendationGetCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;

    public RecommendationGetCommandTests()
    {
        _service = Substitute.For<IWellArchitectedService>();
        _logger = Substitute.For<ILogger<RecommendationGetCommand>>();

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
    public async Task ExecuteAsync_ReturnsRecommendation_WhenFound()
    {
        var recommendationId = "rec-123";
        var mockRecommendation = new WafRecommendation(
            recommendationId,
            "Test Recommendation",
            "Test Description",
            "Security",
            "Test Content",
            "Azure Storage"
        );

        _service.GetRecommendationAsync(recommendationId, Arg.Any<CancellationToken>())
            .Returns(mockRecommendation);

        var parseResult = _commandDefinition.Parse(["--recommendation-id", recommendationId]);
        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
        Assert.Empty(response.Message);

        await _service.Received(1).GetRecommendationAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsNull_WhenNotFound()
    {
        var recommendationId = "nonexistent";

        _service.GetRecommendationAsync(recommendationId, Arg.Any<CancellationToken>())
            .Returns((WafRecommendation?)null);

        var parseResult = _commandDefinition.Parse(["--recommendation-id", recommendationId]);
        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);

        await _service.Received(1).GetRecommendationAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        var recommendationId = "rec-123";
        var exception = new InvalidOperationException("Service error");

        _service.GetRecommendationAsync(recommendationId, Arg.Any<CancellationToken>())
            .ThrowsAsync(exception);

        var parseResult = _commandDefinition.Parse(["--recommendation-id", recommendationId]);
        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.NotNull(response.Message);
        Assert.NotEmpty(response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_ValidatesInput_Success()
    {
        _service.GetRecommendationAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new WafRecommendation("rec-123", "Title", "Desc", "Security", "Content"));

        var parseResult = _commandDefinition.Parse(["--recommendation-id", "rec-123"]);
        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.Status);
    }

    [Fact]
    public async Task ExecuteAsync_ValidatesInput_MissingRecommendationId()
    {
        var parseResult = _commandDefinition.Parse([]);
        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

        Assert.NotEqual(HttpStatusCode.OK, response.Status);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesEmptyRecommendationId()
    {
        var parseResult = _commandDefinition.Parse(["--recommendation-id", ""]);
        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

        Assert.NotEqual(HttpStatusCode.OK, response.Status);
    }

    [Fact]
    public void BindOptions_BindsOptionsCorrectly()
    {
        var recommendationId = "SE:01";
        var parseResult = _commandDefinition.Parse(["--recommendation-id", recommendationId]);

        var options = _command.GetType()
            .GetMethod("BindOptions", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.Invoke(_command, [parseResult]) as RecommendationGetOptions;

        Assert.NotNull(options);
        Assert.Equal(recommendationId, options.RecommendationId);
    }
}
