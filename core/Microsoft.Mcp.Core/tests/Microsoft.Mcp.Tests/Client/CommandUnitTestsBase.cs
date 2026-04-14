// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;
using NSubstitute;

namespace Microsoft.Mcp.Tests.Client;

/// <summary>
/// Base class for command unit tests, providing common setup for testing command execution and validation logic.
/// </summary>
/// <typeparam name="ToolCommand">The tool command.</typeparam>
/// <typeparam name="ToolService">The tool service.</typeparam>
public abstract class CommandUnitTestsBase<ToolCommand, ToolService>
    where ToolCommand : class, IBaseCommand
    where ToolService : class
{
    protected readonly ToolService _service;
    protected readonly ToolCommand _command;
    protected readonly ILogger<ToolCommand> _logger;
    protected readonly CommandContext _context;
    protected readonly Command _commandDefinition;
    protected readonly ServiceProvider _serviceProvider;

    /// <summary>
    /// Initializes the command unit test base by setting up common dependency injection points with mocks.
    /// </summary>
    /// <param name="extensions">Optional additional service registrations for the test.</param>
    public CommandUnitTestsBase(Action<IServiceCollection>? extensions = null)
    {
        _service = Substitute.For<ToolService>();
        _logger = Substitute.For<ILogger<ToolCommand>>();
        var serviceCollection = new ServiceCollection()
            .AddSingleton(_logger)
            .AddSingleton(_service)
            .AddSingleton<ToolCommand>();
        extensions?.Invoke(serviceCollection);
        _serviceProvider = serviceCollection.BuildServiceProvider();
        _command = _serviceProvider.GetRequiredService<ToolCommand>();
        _context = new(_serviceProvider);
        _commandDefinition = _command.GetCommand();
    }
}
