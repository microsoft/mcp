// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Net;
using System.Text.Json;
using Azure.Mcp.Tools.AzureBestPractices.Commands;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Models.Command;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.AzureBestPractices.UnitTests;

public class AIAppBestPracticesCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<AIAppBestPracticesCommand> _logger;
    private readonly CommandContext _context;
    private readonly AIAppBestPracticesCommand _command;
    private readonly Command _commandDefinition;

    public AIAppBestPracticesCommandTests()
    {
        var collection = new ServiceCollection();
        _serviceProvider = collection.BuildServiceProvider();

        _context = new(_serviceProvider);
        _logger = Substitute.For<ILogger<AIAppBestPracticesCommand>>();
        _command = new(_logger);
        _commandDefinition = _command.GetCommand();
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsAzureAIAppBestPractices()
    {
        var args = _commandDefinition.Parse([]);
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize<string[]>(json);

        Assert.NotNull(result);
        Assert.Contains("Microsoft Agent Framework", result[0]);
        Assert.Contains("AIProjectClient", result[0]);
        Assert.Contains("Build and Verification", result[0]);
        Assert.Contains("Understanding AI Models Hierarchy", result[0]);
        Assert.Contains("CORRECT Pattern", result[0]);
    }

    [Fact]
    public void Command_HasCorrectProperties()
    {
        Assert.Equal("ai_app", _command.Name);
        Assert.Equal("Get AI App Best Practices", _command.Title);
        Assert.Equal("6c29659e-406d-4b9b-8150-e3d4fd7ba31c", _command.Id);
        Assert.Contains("AI applications in Azure", _command.Description);
        Assert.Contains("Microsoft Foundry", _command.Description);
        Assert.Contains("AI agents, chatbots, workflows", _command.Description);
    }

    [Fact]
    public void Command_HasCorrectMetadata()
    {
        var metadata = _command.Metadata;

        Assert.False(metadata.Destructive);
        Assert.True(metadata.Idempotent);
        Assert.False(metadata.OpenWorld);
        Assert.True(metadata.ReadOnly);
        Assert.False(metadata.LocalRequired);
        Assert.False(metadata.Secret);
    }
}
