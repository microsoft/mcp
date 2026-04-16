// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Net;
using Azure.Mcp.Tools.FoundryExtensions.Commands;
using Azure.Mcp.Tools.FoundryExtensions.Models;
using Azure.Mcp.Tools.FoundryExtensions.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Options;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.FoundryExtensions.UnitTests;

public class KnowledgeIndexListCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IFoundryExtensionsService _service;
    private readonly KnowledgeIndexListCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;

    public KnowledgeIndexListCommandTests()
    {
        _service = Substitute.For<IFoundryExtensionsService>();

        var collection = new ServiceCollection();
        _serviceProvider = collection.BuildServiceProvider();
        _command = new(_service);
        _context = new CommandContext(_serviceProvider);
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
    [InlineData("--endpoint https://my-foundry.services.ai.azure.com/api/projects/my-project", true)]
    [InlineData("", false)]
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        // Arrange
        if (shouldSucceed)
        {
            _service.ListKnowledgeIndexes(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
                .Returns(
                [
                    new() { Name = "test-index", Type = "aisearch", Version = "1.0", Description = "Test index" }
                ]);
        }

        var parseResult = _commandDefinition.Parse(args);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

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
        _service.ListKnowledgeIndexes(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<List<KnowledgeIndexInformation>>(new Exception("Test error")));

        var parseResult = _commandDefinition.Parse(["--endpoint", "https://my-foundry.services.ai.azure.com/api/projects/my-project"]);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Test error", response.Message);
        Assert.Contains("troubleshooting", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsExpectedResults()
    {
        // Arrange
        var expectedIndexes = new List<KnowledgeIndexInformation>
        {
            new() { Name = "test-index1", Type = "aisearch", Version = "1.0", Description = "First test index" },
            new() { Name = "test-index2", Type = "aisearch", Version = "1.1", Description = "Second test index" }
        };

        _service.ListKnowledgeIndexes(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .Returns(expectedIndexes);

        var parseResult = _commandDefinition.Parse(["--endpoint", "https://my-foundry.services.ai.azure.com/api/projects/my-project"]);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
        Assert.Equal("Success", response.Message);
    }
}
