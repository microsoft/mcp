// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Net;
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
/// <typeparam name="TCommand">The tool command.</typeparam>
/// <typeparam name="TService">The tool service.</typeparam>
public abstract class CommandUnitTestsBase<TCommand, TService> : IDisposable
    where TCommand : class, IBaseCommand
    where TService : class
{
    private bool _disposed = false;
    protected TService Service { get; init; }
    protected TCommand Command { get; init; }
    protected ILogger<TCommand> Logger { get; init; }
    protected CommandContext Context { get; init; }
    protected Command CommandDefinition { get; init; }
    protected ServiceProvider ServiceProvider { get; init; }

    /// <summary>
    /// Initializes the command unit test base by setting up common dependency injection points with mocks.
    /// </summary>
    /// <param name="extensions">Optional additional service registrations for the test.</param>
    public CommandUnitTestsBase(Action<IServiceCollection>? extensions = null)
    {
        Service = Substitute.For<TService>();
        Logger = Substitute.For<ILogger<TCommand>>();
        var serviceCollection = new ServiceCollection()
            .AddSingleton(Logger)
            .AddSingleton(Service)
            .AddSingleton<TCommand>();
        extensions?.Invoke(serviceCollection);
        ServiceProvider = serviceCollection.BuildServiceProvider();
        Command = ServiceProvider.GetRequiredService<TCommand>();
        Context = new(ServiceProvider);
        CommandDefinition = Command.GetCommand();
    }

    protected Task<CommandResponse> ExecuteCommandAsync(params string[] args)
        => Command.ExecuteAsync(Context, CommandDefinition.Parse(args), TestContext.Current.CancellationToken);

    protected Task<CommandResponse> ExecuteCommandAsync(string args)
        => Command.ExecuteAsync(Context, CommandDefinition.Parse(args), TestContext.Current.CancellationToken);

    /// <summary>
    /// Deserializes the command response results into the specified type using the provided JSON type information.
    /// </summary>
    /// <typeparam name="T">The type to deserialize the command response results into.</typeparam>
    /// <param name="response">The command response containing the results to deserialize.</param>
    /// <param name="jsonTypeInfo">The JSON type information.</param>
    /// <returns>The deserialized command response results.</returns>
    protected T? DeserializeResponse<T>(CommandResponse response, JsonTypeInfo<T> jsonTypeInfo)
        => JsonSerializer.Deserialize(JsonSerializer.Serialize(response.Results), jsonTypeInfo);

    /// <summary>
    /// Validates that the command response indicates a successful execution and deserializes the results into the specified type.
    /// </summary>
    /// <typeparam name="T">The type to deserialize the command response results into.</typeparam>
    /// <param name="response">The command response containing the results to deserialize.</param>
    /// <param name="jsonTypeInfo">The JSON type information.</param>
    /// <returns>The deserialized command response results.</returns>
    protected T ValidateAndDeserializeResponse<T>(CommandResponse response, JsonTypeInfo<T> jsonTypeInfo)
    {
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
        var result = DeserializeResponse(response, jsonTypeInfo);
        Assert.NotNull(result);
        return result;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public void Dispose(bool disposing)
    {
        if (_disposed)
            return;
        if (disposing)
            ServiceProvider.Dispose();
        _disposed = true;
    }
}
