// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.CommandLine.Parsing;
using System.Net;
using System.Text.Json;
using Azure.Mcp.Tools.WellArchitected.Commands;
using Azure.Mcp.Tools.WellArchitected.Commands.Analyze;
using Azure.Mcp.Tools.WellArchitected.Models;
using Azure.Mcp.Tools.WellArchitected.Options;
using Azure.Mcp.Tools.WellArchitected.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Models.Command;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.WellArchitected.UnitTests.Analyze;

public class AnalyzeCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IWellArchitectedService _service;
    private readonly ILogger<AnalyzeCommand> _logger;
    private readonly AnalyzeCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;

    public AnalyzeCommandTests()
    {
        _service = Substitute.For<IWellArchitectedService>();
        _logger = Substitute.For<ILogger<AnalyzeCommand>>();

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
        Assert.Equal("analyze", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsAnalysisResults_WhenValidInput()
    {
        var config = "{\"resources\":[{\"type\":\"Microsoft.Storage/storageAccounts\"}]}";
        var intent = "Analyze storage account configuration";
        var mockResponse = new WafAnalyzeResponse
        {
            AnalysisContext = new AnalysisContext
            {
                Intent = intent,
                DetectedResourceTypes = ["Microsoft.Storage/storageAccounts"],
                DetectedServices = ["azure-blob-storage"],
                ResourceCount = 1,
                PropertySignals = new PropertySignals()
            },
            WafGuidance = new WafGuidance
            {
                AgentInstructions = "Test instructions",
                MatchedServiceGuides = [new ServiceGuideMatch { Service = "azure-blob-storage", ResourceType = "Microsoft.Storage/storageAccounts", Content = "Guide content" }],
                RelevantRecommendations = new Dictionary<string, PillarRecommendations>
                {
                    ["Security"] = new PillarRecommendations { Items = [new RelevantRecommendation { Id = "SE:01", Title = "Enable Firewall", Pillar = "Security", RelevanceReason = "test" }] }
                },
                ChecklistItems = new Dictionary<string, List<string>> { ["Security"] = ["SE:01: Test item"] }
            }
        };

        _service.AnalyzeAsync(config, intent, Arg.Any<CancellationToken>())
            .Returns(mockResponse);

        var parseResult = _commandDefinition.Parse(["--infrastructure-config", config, "--intent", intent]);
        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
        Assert.Empty(response.Message);

        await _service.Received(1).AnalyzeAsync(config, intent, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        var config = "{\"resources\":[]}";
        var intent = "test";
        var exception = new InvalidOperationException("Service error");

        _service.AnalyzeAsync(config, intent, Arg.Any<CancellationToken>())
            .ThrowsAsync(exception);

        var parseResult = _commandDefinition.Parse(["--infrastructure-config", config, "--intent", intent]);
        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.NotNull(response.Message);
        Assert.NotEmpty(response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_ValidatesInput_Success()
    {
        _service.AnalyzeAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new WafAnalyzeResponse());

        var parseResult = _commandDefinition.Parse(["--infrastructure-config", "{\"resources\":[]}", "--intent", "test"]);
        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.Status);
    }

    [Fact]
    public async Task ExecuteAsync_ValidatesInput_MissingInfrastructureConfig()
    {
        var parseResult = _commandDefinition.Parse(["--intent", "test"]);
        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

        Assert.NotEqual(HttpStatusCode.OK, response.Status);
    }

    [Fact]
    public async Task ExecuteAsync_ValidatesInput_MissingIntent()
    {
        var parseResult = _commandDefinition.Parse(["--infrastructure-config", "{}"]);
        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

        Assert.NotEqual(HttpStatusCode.OK, response.Status);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsAnalysisResults_WhenMultiplePillars()
    {
        var config = "{\"resources\":[]}";
        var intent = "test";
        var mockResponse = new WafAnalyzeResponse
        {
            AnalysisContext = new AnalysisContext
            {
                Intent = intent,
                DetectedResourceTypes = [],
                DetectedServices = [],
                ResourceCount = 0,
                PropertySignals = new PropertySignals()
            },
            WafGuidance = new WafGuidance
            {
                AgentInstructions = "Instructions",
                RelevantRecommendations = new Dictionary<string, PillarRecommendations>
                {
                    ["Reliability"] = new PillarRecommendations { Items = [new RelevantRecommendation { Id = "RE:01", Title = "Design for redundancy", Pillar = "Reliability", RelevanceReason = "General recommendation" }] },
                    ["Security"] = new PillarRecommendations { Items = [new RelevantRecommendation { Id = "SE:01", Title = "Security baseline", Pillar = "Security", RelevanceReason = "General recommendation" }] }
                }
            }
        };

        _service.AnalyzeAsync(config, intent, Arg.Any<CancellationToken>())
            .Returns(mockResponse);

        var parseResult = _commandDefinition.Parse(["--infrastructure-config", config, "--intent", intent]);
        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
    }

    [Fact]
    public void BindOptions_BindsOptionsCorrectly()
    {
        var config = "{\"resources\":[{\"type\":\"Microsoft.Storage/storageAccounts\"}]}";
        var intent = "Analyze configuration";
        var parseResult = _commandDefinition.Parse(["--infrastructure-config", config, "--intent", intent]);

        var options = _command.GetType()
            .GetMethod("BindOptions", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.Invoke(_command, [parseResult]) as AnalyzeOptions;

        Assert.NotNull(options);
        Assert.Equal(config, options.InfrastructureConfig);
        Assert.Equal(intent, options.Intent);
    }
}
