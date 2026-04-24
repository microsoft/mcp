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
public abstract class CommandUnitTestsBase<TCommand, TService>
    where TCommand : class, IBaseCommand
    where TService : class
{
    protected TService Service { get; init; }
    protected ILogger<TCommand> Logger { get; init; }
    protected IServiceCollection Services { get; init; }

    protected ServiceProvider ServiceProvider { get => field ??= Services.BuildServiceProvider(); }
    protected CommandContext Context { get => field ??= new CommandContext(ServiceProvider); }
    protected TCommand Command { get => field ??= ServiceProvider.GetRequiredService<TCommand>(); }
    protected Command CommandDefinition { get => field ??= Command.GetCommand(); }

    /// <summary>
    /// Initializes the command unit test base by setting up common dependency injection points with mocks.
    /// </summary>
    public CommandUnitTestsBase()
    {
        Service = Substitute.For<TService>();
        Logger = Substitute.For<ILogger<TCommand>>();
        Services = new ServiceCollection()
            .AddSingleton(Logger)
            .AddSingleton(Service)
            .AddSingleton<TCommand>();
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
    /// <param name="expectedStatus">The expected HTTP status code of the command response (default is OK).</param>
    /// <returns>The deserialized command response results.</returns>
    protected T ValidateAndDeserializeResponse<T>(
        CommandResponse response,
        JsonTypeInfo<T> jsonTypeInfo,
        HttpStatusCode expectedStatus = HttpStatusCode.OK)
    {
        Assert.NotNull(response);
        Assert.Equal(expectedStatus, response.Status);
        Assert.NotNull(response.Results);
        var result = DeserializeResponse(response, jsonTypeInfo);
        Assert.NotNull(result);
        return result;
    }
}
