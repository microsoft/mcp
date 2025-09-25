// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Text.Json;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Tools.BicepSchema.Commands;
using Azure.Mcp.Tools.BicepSchema.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.BicepSchema.UnitTests;

public class BicepSchemaGetCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IBicepSchemaService _bicepSchemaService;
    private readonly ILogger<BicepSchemaGetCommand> _logger;
    private readonly CommandContext _context;
    private readonly BicepSchemaGetCommand _command;
    private readonly Command _commandDefinition;

    public BicepSchemaGetCommandTests()
    {
        _bicepSchemaService = Substitute.For<IBicepSchemaService>();
        _logger = Substitute.For<ILogger<BicepSchemaGetCommand>>();

        var collection = new ServiceCollection();
        collection.AddSingleton(_bicepSchemaService);

        _serviceProvider = collection.BuildServiceProvider();
        _context = new(_serviceProvider);
        _command = new(_logger);
        _commandDefinition = _command.GetCommand();
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsSchema_WhenResourceTypeExists()
    {
        var args = _commandDefinition.Parse("--resource-type Microsoft.Sql/servers/databases/schemas");

        var response = await _command.ExecuteAsync(_context, args);
        Assert.NotNull(response);
        Assert.NotNull(response.Results);


        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, BicepSchemaJsonContext.Default.BicepSchemaGetCommandResult);
        var name = result?.BicepSchemaResult.FirstOrDefault()?.Name;

        Assert.Contains("Microsoft.Sql/servers/databases/schemas@2023-08-01", name);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsError_WhenResourceTypeDoesNotExist()
    {

        var args = _commandDefinition.Parse("--resource-type Microsoft.Unknown/virtualRandom");

        var response = await _command.ExecuteAsync(_context, args);
        Assert.NotNull(response);
        Assert.NotNull(response.Results);

        Assert.Contains("Resource type Microsoft.Unknown/virtualRandom not found.", response.Message);
    }
}
