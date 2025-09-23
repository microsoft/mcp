// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Net;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.Foundry.Commands;
using Azure.Mcp.Tools.Foundry.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.Foundry.UnitTests;

public class KnowledgeIndexSchemaCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IFoundryService _service;
    private readonly ILogger<KnowledgeIndexSchemaCommand> _logger;
    private readonly KnowledgeIndexSchemaCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;

    public KnowledgeIndexSchemaCommandTests()
    {
        _service = Substitute.For<IFoundryService>();
        _logger = Substitute.For<ILogger<KnowledgeIndexSchemaCommand>>();

        var collection = new ServiceCollection().AddSingleton(_service);
        _serviceProvider = collection.BuildServiceProvider();
        _command = new();
        _context = new CommandContext(_serviceProvider);
        _commandDefinition = _command.GetCommand();
    }

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = _command.GetCommand();
        Assert.Equal("schema", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
    }

    [Theory]
    [InlineData("--endpoint https://example.com --index test-index", true)]
    [InlineData("--endpoint https://example.com", false)] // Missing index name
    [InlineData("--index test-index", false)] // Missing endpoint
    [InlineData("", false)] // Missing both
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        // Arrange
        if (shouldSucceed)
        {
            var mockSchema = new Models.KnowledgeIndexSchema
            {
                Name = "test-index",
                Type = "AzureAISearchIndex",
                Version = "1.0",
                Description = "desc",
                Tags = new Dictionary<string, string?> { { "env", "test" } }
            };
            _service.GetKnowledgeIndexSchema(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>())
                .Returns(mockSchema);
        }

        var parseResult = _commandDefinition.Parse(args.Split(' ', StringSplitOptions.RemoveEmptyEntries));

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult);

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
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        // Arrange
        _service.GetKnowledgeIndexSchema(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>())
            .Returns(Task.FromException<Models.KnowledgeIndexSchema>(new Exception("Test error")));

        var parseResult = _commandDefinition.Parse(["--endpoint", "https://example.com", "--index", "test-index"]);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult);

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Test error", response.Message);
        Assert.Contains("troubleshooting", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsExpectedResults()
    {
        // Arrange
        var expectedSchema = new Models.KnowledgeIndexSchema
        {
            Name = "test-index",
            Type = "AzureAISearchIndex",
            Version = "1.0"
        };

        _service.GetKnowledgeIndexSchema(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>())
            .Returns(expectedSchema);

        var parseResult = _commandDefinition.Parse(["--endpoint", "https://example.com", "--index", "test-index"]);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
        Assert.Equal("Success", response.Message);
    }
}
