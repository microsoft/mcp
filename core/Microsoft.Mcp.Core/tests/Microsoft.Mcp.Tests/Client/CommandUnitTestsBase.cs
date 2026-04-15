// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;
using NSubstitute;
using Xunit;

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
    protected ToolService Service { get; init; }
    protected ToolCommand Command { get; init; }
    protected ILogger<ToolCommand> Logger { get; init; }
    protected CommandContext Context { get; init; }
    protected Command CommandDefinition { get; init; }
    protected ServiceProvider ServiceProvider { get; init; }

    /// <summary>
    /// Initializes the command unit test base by setting up common dependency injection points with mocks.
    /// </summary>
    /// <param name="extensions">Optional additional service registrations for the test.</param>
    public CommandUnitTestsBase(Action<IServiceCollection>? extensions = null)
    {
        Service = Substitute.For<ToolService>();
        Logger = Substitute.For<ILogger<ToolCommand>>();
        var serviceCollection = new ServiceCollection()
            .AddSingleton(Logger)
            .AddSingleton(Service)
            .AddSingleton<ToolCommand>();
        extensions?.Invoke(serviceCollection);
        ServiceProvider = serviceCollection.BuildServiceProvider();
        Command = ServiceProvider.GetRequiredService<ToolCommand>();
        Context = new(ServiceProvider);
        CommandDefinition = Command.GetCommand();
    }

    protected Task<CommandResponse> ExecuteCommandAsync(params string[] args)
        => Command.ExecuteAsync(Context, CommandDefinition.Parse(args), TestContext.Current.CancellationToken);

    protected Task<CommandResponse> ExecuteCommandAsync(string args)
        => Command.ExecuteAsync(Context, CommandDefinition.Parse(args), TestContext.Current.CancellationToken);

    protected T? ConvertResponse<T>(CommandResponse response, JsonTypeInfo<T> jsonTypeInfo)
        => JsonSerializer.Deserialize(JsonSerializer.Serialize(response.Results), jsonTypeInfo);
}
