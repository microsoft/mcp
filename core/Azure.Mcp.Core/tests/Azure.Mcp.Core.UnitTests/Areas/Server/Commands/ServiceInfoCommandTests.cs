// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Text.Json;
using Azure.Mcp.Core.Areas.Server.Commands;
using Azure.Mcp.Core.Configuration;
using Azure.Mcp.Core.Models.Command;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Core.UnitTests.Areas.Server.Commands;

public class ServiceInfoCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ServiceInfoCommand> _logger;
    private readonly AzureMcpServerConfiguration _mcpServerConfiguration;
    private readonly CommandContext _context;
    private readonly ServiceInfoCommand _command;
    private readonly Command _commandDefinition;

    public ServiceInfoCommandTests()
    {
        var collection = new ServiceCollection();
        _serviceProvider = collection.BuildServiceProvider();

        _context = new(_serviceProvider);
        _logger = Substitute.For<ILogger<ServiceInfoCommand>>();
        _mcpServerConfiguration = new AzureMcpServerConfiguration
        {
            Name = "Test-Name?",
            Version = "Test-Version?"
        };
        _command = new(Microsoft.Extensions.Options.Options.Create(_mcpServerConfiguration), _logger);
        _commandDefinition = _command.GetCommand();
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsCorrectProperties()
    {
        var args = _commandDefinition.Parse([]);
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize<ServiceInfoCommand.ServiceInfoCommandResult>(json);

        Assert.NotNull(result);
        Assert.Equal(_mcpServerConfiguration.Name, result.Name);
        Assert.Equal(_mcpServerConfiguration.Version, result.Version);
    }
}
