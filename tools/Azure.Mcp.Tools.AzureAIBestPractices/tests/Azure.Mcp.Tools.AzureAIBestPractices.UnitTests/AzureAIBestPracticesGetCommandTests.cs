// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Text.Json;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Tools.AzureAIBestPractices.Commands;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.AzureAIBestPractices.UnitTests;

public class AzureAIBestPracticesGetCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<AzureAIBestPracticesGetCommand> _logger;
    private readonly CommandContext _context;
    private readonly AzureAIBestPracticesGetCommand _command;
    private readonly Command _commandDefinition;

    public AzureAIBestPracticesGetCommandTests()
    {
        var collection = new ServiceCollection();
        _serviceProvider = collection.BuildServiceProvider();

        _context = new(_serviceProvider);
        _logger = Substitute.For<ILogger<AzureAIBestPracticesGetCommand>>();
        _command = new(_logger);
        _commandDefinition = _command.GetCommand();
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsAzureAIBestPractices()
    {
        var args = _commandDefinition.Parse([]);
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize<string[]>(json);

        Assert.NotNull(result);
        Assert.Contains("Best Practices for Building AI Apps with Azure AI Services", result[0]);
        Assert.Contains("Key Principles", result[0]);
        Assert.Contains("Microsoft Agent Framework", result[0]);
        Assert.Contains("Tool Use Guidelines", result[0]);
        Assert.Contains("Gather Information", result[0]);
    }
}
