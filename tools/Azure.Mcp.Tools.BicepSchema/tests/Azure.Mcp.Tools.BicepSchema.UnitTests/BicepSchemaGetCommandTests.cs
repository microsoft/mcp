// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Text.Json;
using Azure.Mcp.Tools.BicepSchema.Commands;
using Azure.Mcp.Tools.BicepSchema.Services;
using Azure.Mcp.Tools.BicepSchema.Services.ResourceProperties.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Models.Command;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.BicepSchema.UnitTests;

public class BicepSchemaGetCommandTests
{
    private readonly IBicepSchemaService _bicepSchemaService;
    private readonly ILogger<BicepSchemaGetCommand> _logger;
    private readonly CommandContext _context;
    private readonly BicepSchemaGetCommand _command;
    private readonly Command _commandDefinition;

    public BicepSchemaGetCommandTests()
    {
        _bicepSchemaService = Substitute.For<IBicepSchemaService>();
        _logger = Substitute.For<ILogger<BicepSchemaGetCommand>>();

        _command = new(_logger, _bicepSchemaService);
        _commandDefinition = _command.GetCommand();
        _context = new(new ServiceCollection().BuildServiceProvider());
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsSchema_WhenResourceTypeExists()
    {
        var resourceTypeName = "Microsoft.Sql/servers/databases/schemas";
        var expectedResult = new TypesDefinitionResult
        {
            ResourceProvider = "Microsoft.Sql",
            ApiVersion = "2023-08-01",
            ResourceTypeEntities =
            [
                new ResourceTypeEntity
                {
                    Name = "Microsoft.Sql/servers/databases/schemas@2023-08-01",
                    BodyType = new ObjectTypeEntity { Name = "body" }
                }
            ]
        };
        _bicepSchemaService.GetResourceTypeDefinitions(resourceTypeName).Returns(expectedResult);

        var args = _commandDefinition.Parse($"--resource-type {resourceTypeName}");

        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);
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
        var resourceTypeName = "Microsoft.Unknown/virtualRandom";
        _bicepSchemaService.GetResourceTypeDefinitions(resourceTypeName)
            .Throws(new Exception($"Resource type {resourceTypeName} not found."));

        var args = _commandDefinition.Parse($"--resource-type {resourceTypeName}");

        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);
        Assert.NotNull(response);
        Assert.NotNull(response.Results);

        Assert.Contains("Resource type Microsoft.Unknown/virtualRandom not found.", response.Message);
    }
}
