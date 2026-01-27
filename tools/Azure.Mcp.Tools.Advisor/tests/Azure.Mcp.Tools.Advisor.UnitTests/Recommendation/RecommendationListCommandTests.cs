using System.CommandLine;
using System.Net;
using System.Text.Json;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.Advisor.Commands;
using Azure.Mcp.Tools.Advisor.Commands.Recommendation;
using Azure.Mcp.Tools.Advisor.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Models.Command;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.Advisor.UnitTests.Recommendation;

public class RecommendationListCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IAdvisorService _advisorService;
    private readonly ILogger<RecommendationListCommand> _logger;
    private readonly RecommendationListCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;

    public RecommendationListCommandTests()
    {
        _advisorService = Substitute.For<IAdvisorService>();
        _logger = Substitute.For<ILogger<RecommendationListCommand>>();

        var collection = new ServiceCollection().AddSingleton(_advisorService);
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

    [Theory]
    [InlineData("--subscription sub1 --resource-group rg1", true)]
    [InlineData("--subscription sub1", true)]  // Missing resource-group
    [InlineData("", false)]                    // Missing all required options
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        // Arrange
        if (shouldSucceed)
        {
            _advisorService.ListRecommendationsAsync(
                Arg.Any<string>(),
                Arg.Any<string?>(),
                Arg.Any<RetryPolicyOptions>(),
                Arg.Any<CancellationToken>())
                .Returns([]);
        }

        var context = new CommandContext(_serviceProvider);
        var parseResult = _command.GetCommand().Parse(args);

        // Act
        var response = await _command.ExecuteAsync(context, parseResult, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(shouldSucceed ? HttpStatusCode.OK : HttpStatusCode.BadRequest, response.Status);
        if (shouldSucceed)
        {
            Assert.NotNull(response.Results);
            Assert.Equal("Success", response.Message);
        }
        else
        {
            Assert.Contains("required", response.Message.ToLower());
        }
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsRecommendationsList()
    {
        // Arrange
        var expectedRecommendations = new List<Models.Recommendation>
        {
            new(ResourceId: "recId1", RecommendationText: "Recommendation 1", Category: "HighAvailability"),
            new(ResourceId: "recId2", RecommendationText: "Recommendation 2", Category: "Cost"),
            new(ResourceId: "recId3", RecommendationText: "Recommendation 3", Category: "Performance")
        };
        _advisorService.ListRecommendationsAsync(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns(expectedRecommendations);

        var context = new CommandContext(_serviceProvider);
        var parseResult = _command.GetCommand().Parse("--subscription sub123");

        // Act
        var response = await _command.ExecuteAsync(context, parseResult, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);

        // Verify the mock was called
        await _advisorService.Received(1).ListRecommendationsAsync(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>());

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, AdvisorJsonContext.Default.RecommendationListResult);

        Assert.NotNull(result);
        Assert.Equal(expectedRecommendations.Count, result.Recommendations.Count);
        Assert.Equal(expectedRecommendations[0].ResourceId, result.Recommendations[0].ResourceId);
        Assert.Equal(expectedRecommendations[0].RecommendationText, result.Recommendations[0].RecommendationText);
        Assert.Equal(expectedRecommendations[0].Category, result.Recommendations[0].Category);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsEmptyWhenNoRecommendations()
    {
        // Arrange
        _advisorService.ListRecommendationsAsync(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns([]);

        var context = new CommandContext(_serviceProvider);
        var parseResult = _command.GetCommand().Parse("--subscription sub123");

        // Act
        var response = await _command.ExecuteAsync(context, parseResult, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, AdvisorJsonContext.Default.RecommendationListResult);

        Assert.NotNull(result);
        Assert.Empty(result.Recommendations);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        // Arrange
        _advisorService.ListRecommendationsAsync(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns(Task.FromException<List<Models.Recommendation>>(new Exception("Test error")));

        var context = new CommandContext(_serviceProvider);
        var parseResult = _command.GetCommand().Parse("--subscription sub123");

        // Act
        var response = await _command.ExecuteAsync(context, parseResult, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Test error", response.Message);
        Assert.Contains("troubleshooting", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_Handles403Forbidden()
    {
        // Arrange
        var forbiddenException = new RequestFailedException((int)HttpStatusCode.Forbidden, "Authorization failed");
        _advisorService.ListRecommendationsAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<List<Models.Recommendation>>(forbiddenException));

        var parseResult = _commandDefinition.Parse(["--subscription", "test-subscription", "--resource-group", "test-rg"]);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.Status);
        Assert.Contains("Authorization failed", response.Message);
    }
}
