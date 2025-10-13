using System.Text.Json;
using System.Text.Json.Nodes;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.ApplicationInsights.Commands.Recommendation;
using Azure.Mcp.Tools.ApplicationInsights.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.ApplicationInsights.UnitTests;

public class RecommendationListCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IApplicationInsightsService _serviceMock;
    private readonly RecommendationListCommand _command;
    private readonly CommandContext _context;

    public RecommendationListCommandTests()
    {
        var sc = new ServiceCollection();
        _serviceMock = Substitute.For<IApplicationInsightsService>();
        sc.AddSingleton(_serviceMock);
        var logger = Substitute.For<ILogger<RecommendationListCommand>>();
        _serviceProvider = sc.BuildServiceProvider();
        _command = new RecommendationListCommand(logger);
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
        _serviceMock.GetProfilerInsightsAsync(Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>())
            .Returns(Task.FromResult<IEnumerable<JsonNode>>(insights!));
        var args = _command.GetCommand().Parse(["--subscription", "sub1"]);
        await _command.ExecuteAsync(_context, args);
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
        _serviceMock.GetProfilerInsightsAsync(Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>())
            .Returns(Task.FromResult<IEnumerable<JsonNode>>(Array.Empty<JsonNode>()));
        var args = _command.GetCommand().Parse(["--subscription", "sub1"]);
        await _command.ExecuteAsync(_context, args);
        Assert.Null(_context.Response.Results);
    }
}
