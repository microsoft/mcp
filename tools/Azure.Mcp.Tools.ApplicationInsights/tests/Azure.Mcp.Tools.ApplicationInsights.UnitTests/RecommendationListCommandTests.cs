using System.CommandLine;
using System.Text.Json;
using System.Text.Json.Nodes;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.ApplicationInsights.Commands.Recommendation;
using Azure.Mcp.Tools.ApplicationInsights.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Models.Command;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.ApplicationInsights.UnitTests;

public class RecommendationListCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IApplicationInsightsService _applicationInsightsService;
    private readonly ILogger<RecommendationListCommand> _logger;
    private readonly RecommendationListCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;

    public RecommendationListCommandTests()
    {
        _applicationInsightsService = Substitute.For<IApplicationInsightsService>();
        _logger = Substitute.For<ILogger<RecommendationListCommand>>();
        _command = new(_logger, _applicationInsightsService);
        _commandDefinition = _command.GetCommand();
        _serviceProvider = new ServiceCollection()
            .BuildServiceProvider();
        _context = new(_serviceProvider);
    }

    [Fact]
    public async Task ExecuteAsync_WhenServiceReturnsInsights_SetsResults()
    {
        var insights = new List<JsonNode?>
        {
            JsonNode.Parse("{ \"id\": \"rec1\", \"type\": \"cpu\" }")!,
            JsonNode.Parse("{ \"id\": \"rec2\", \"type\": \"memory\" }")!
        };
        _applicationInsightsService.GetProfilerInsightsAsync(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IEnumerable<JsonNode>>(insights!));
        var args = _commandDefinition.Parse(["--subscription", "sub1"]);
        await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);
        Assert.NotNull(_context.Response.Results);
        var json = JsonSerializer.Serialize(_context.Response.Results);
        var node = JsonNode.Parse(json);
        var recs = node?["recommendations"]?.AsArray();
        Assert.NotNull(recs);
        Assert.Equal(2, recs!.Count);
    }

    [Fact]
    public async Task ExecuteAsync_WhenServiceReturnsNoInsights_NoResults()
    {
        _applicationInsightsService.GetProfilerInsightsAsync(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IEnumerable<JsonNode>>(Array.Empty<JsonNode>()));
        var args = _commandDefinition.Parse(["--subscription", "sub1"]);
        await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);
        Assert.Null(_context.Response.Results);
    }
}
