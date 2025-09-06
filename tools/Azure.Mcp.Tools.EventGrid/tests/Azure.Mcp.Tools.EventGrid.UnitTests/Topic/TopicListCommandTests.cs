// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.CommandLine.Parsing;
using System.Text.Json;
using System.Text.Json.Serialization;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.EventGrid.Commands.Topic;
using Azure.Mcp.Tools.EventGrid.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.EventGrid.UnitTests.Topic;

[Trait("Area", "EventGrid")]
public class TopicListCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IEventGridService _eventGridService;
    private readonly ILogger<TopicListCommand> _logger;
    private readonly TopicListCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;

    public TopicListCommandTests()
    {
        _eventGridService = Substitute.For<IEventGridService>();
        _logger = Substitute.For<ILogger<TopicListCommand>>();

        var collection = new ServiceCollection().AddSingleton(_eventGridService);

        _serviceProvider = collection.BuildServiceProvider();
        _command = new(_logger);
        _context = new(_serviceProvider);
        _commandDefinition = _command.GetCommand();
    }

    [Fact]
    public async Task ExecuteAsync_NoParameters_ReturnsTopics()
    {
        // Arrange
        var subscriptionId = "sub123";
        var expectedTopics = new List<Azure.Mcp.Tools.EventGrid.Models.EventGridTopicInfo>
        {
            new("topic1", "eastus", "https://topic1.eastus.eventgrid.azure.net/api/events", "Succeeded", "Enabled", "EventGridSchema"),
            new("topic2", "westus", "https://topic2.westus.eventgrid.azure.net/api/events", "Succeeded", "Enabled", "EventGridSchema")
        };

        _eventGridService.GetTopicsAsync(Arg.Is(subscriptionId), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>())
            .Returns(Task.FromResult(expectedTopics));

        var args = _commandDefinition.Parse(["--subscription", subscriptionId]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize<TopicListResult>(json);

        Assert.NotNull(result);
        Assert.NotNull(result!.Topics);
        Assert.Equal(expectedTopics.Count, result.Topics!.Count);
        Assert.Equal(expectedTopics.Select(t => t.Name), result.Topics.Select(t => t.Name));
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsNull_WhenNoTopics()
    {
        // Arrange
        var subscriptionId = "sub123";

        _eventGridService.GetTopicsAsync(Arg.Is(subscriptionId), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>())
            .Returns(Task.FromResult(new List<Azure.Mcp.Tools.EventGrid.Models.EventGridTopicInfo>()));

        var args = _commandDefinition.Parse(["--subscription", subscriptionId]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.NotNull(response);
        Assert.Null(response.Results);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesException()
    {
        // Arrange
        var expectedError = "Test error";
        var subscriptionId = "sub123";

        _eventGridService.GetTopicsAsync(Arg.Is(subscriptionId), null, Arg.Any<RetryPolicyOptions>())
            .ThrowsAsync(new Exception(expectedError));

        var args = _commandDefinition.Parse(["--subscription", subscriptionId]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(500, response.Status);
        Assert.StartsWith(expectedError, response.Message);
    }

    [Theory]
    [InlineData("--subscription test-sub", true)]
    [InlineData("--subscription test-sub --tenant test-tenant", true)]
    [InlineData("--subscription test-sub --resource-group test-rg", true)]
    [InlineData("--subscription test-sub --resource-group test-rg --tenant test-tenant", true)]
    [InlineData("", false)]
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        // Arrange
        if (shouldSucceed)
        {
            var expectedTopics = new List<Azure.Mcp.Tools.EventGrid.Models.EventGridTopicInfo>
            {
                new("topic1", "eastus", "https://topic1.eastus.eventgrid.azure.net/api/events", "Succeeded", "Enabled", "EventGridSchema"),
                new("topic2", "westus", "https://topic2.westus.eventgrid.azure.net/api/events", "Succeeded", "Enabled", "EventGridSchema")
            };
            _eventGridService.GetTopicsAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>())
                .Returns(Task.FromResult(expectedTopics));
        }

        var parseResult = _commandDefinition.Parse(args.Split(' ', StringSplitOptions.RemoveEmptyEntries));

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult);

        // Assert
        if (shouldSucceed)
        {
            Assert.Equal(200, response.Status);
            Assert.NotNull(response.Results);
            Assert.Equal("Success", response.Message);
        }
        else
        {
            Assert.Equal(400, response.Status);
            Assert.Contains("required", response.Message?.ToLower() ?? "");
        }
    }

    private class TopicListResult
    {
        [JsonPropertyName("topics")]
        public List<Azure.Mcp.Tools.EventGrid.Models.EventGridTopicInfo>? Topics { get; set; }
    }
}
