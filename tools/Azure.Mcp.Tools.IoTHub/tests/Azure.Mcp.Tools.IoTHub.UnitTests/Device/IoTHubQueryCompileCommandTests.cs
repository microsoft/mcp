// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Net;
using System.Text.Json;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Tools.IoTHub.Commands.IoTHub;
using Azure.Mcp.Tools.IoTHub.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.IoTHub.UnitTests.Device;

public class IoTHubQueryCompileCommandTests
{
    private readonly IoTHubQueryCompileCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;

    public IoTHubQueryCompileCommandTests()
    {
        var logger = Substitute.For<ILogger<IoTHubQueryCompileCommand>>();
        var collection = new ServiceCollection();
        _command = new IoTHubQueryCompileCommand(logger);
        _context = new CommandContext(collection.BuildServiceProvider());
        _commandDefinition = _command.GetCommand();
    }

    [Fact]
    public async Task ExecuteAsync_CompilesReportedPredicateWithMaxCount()
    {
        // Arrange
        const string filters = """
            [{"scope":"reported","field":"batteryLevel","operator":"lessThan","value":20}]
            """;

        var args = _commandDefinition.Parse([
            "--filters", filters,
            "--top", "5"
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, CancellationToken.None);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        var result = DeserializeResult(response);
        Assert.Equal("SELECT * FROM devices WHERE properties.reported.batteryLevel < 20", result.Query);
        Assert.Equal(5, result.MaxCount);
    }

    [Fact]
    public async Task ExecuteAsync_CompilesMultipleScopesWithLogicalOperator()
    {
        // Arrange
        const string filters = """
            [
              {"scope":"tags","field":"location.floor","operator":"equals","value":1},
              {"scope":"tags","field":"machineType","operator":"equals","value":"welding"},
              {"scope":"reported","field":"temperature","operator":"greaterThan","value":80}
            ]
            """;

        var args = _commandDefinition.Parse([
            "--filters", filters,
            "--logical-operator", "AND"
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, CancellationToken.None);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        var result = DeserializeResult(response);
        Assert.Equal("SELECT * FROM devices WHERE tags.location.floor = 1 AND tags.machineType = 'welding' AND properties.reported.temperature > 80", result.Query);
    }

    [Fact]
    public async Task ExecuteAsync_WithDiscoveredFields_CompilesKnownField()
    {
        // Arrange
        const string filters = """
            [{"scope":"tags","field":"location.floor","operator":"equals","value":1}]
            """;
        const string discoveredFields = """
            {
              "device": [],
              "tags": [
                {"field":"location.floor","type":"number","examples":[1,2]},
                {"field":"machineType","type":"string","examples":["welding"]}
              ],
              "desired": [],
              "reported": []
            }
            """;

        var args = _commandDefinition.Parse([
            "--filters", filters,
            "--discovered-fields", discoveredFields
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, CancellationToken.None);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        var result = DeserializeResult(response);
        Assert.Equal("SELECT * FROM devices WHERE tags.location.floor = 1", result.Query);
    }

    [Fact]
    public async Task ExecuteAsync_WithDiscoveredFields_RejectsUnknownFieldWithSuggestion()
    {
        // Arrange
        const string filters = """
            [{"scope":"tags","field":"floor","operator":"equals","value":1}]
            """;
        const string discoveredFields = """
            {
              "device": [],
              "tags": [
                {"field":"location.floor","type":"number","examples":[1,2]},
                {"field":"machineType","type":"string","examples":["welding"]}
              ],
              "desired": [],
              "reported": []
            }
            """;

        var args = _commandDefinition.Parse([
            "--filters", filters,
            "--discovered-fields", discoveredFields
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, CancellationToken.None);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Null(response.Results);
        Assert.Contains("unknown field 'tags.floor'", response.Message, StringComparison.Ordinal);
        Assert.Contains("Did you mean 'location.floor'?", response.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ExecuteAsync_InvalidDiscoveredFieldsJson_ReturnsBadRequest()
    {
        // Arrange
        const string filters = """
            [{"scope":"tags","field":"location.floor","operator":"equals","value":1}]
            """;

        var args = _commandDefinition.Parse([
            "--filters", filters,
            "--discovered-fields", "not-json"
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, CancellationToken.None);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Null(response.Results);
        Assert.Contains("--discovered-fields value is not valid JSON", response.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ExecuteAsync_InvalidJson_ReturnsBadRequest()
    {
        // Arrange
        var args = _commandDefinition.Parse([
            "--filters", "not-json"
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, CancellationToken.None);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Null(response.Results);
        Assert.Contains("not valid JSON", response.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ExecuteAsync_InvalidFieldPath_ReturnsBadRequest()
    {
        // Arrange
        const string filters = """
            [{"scope":"reported","field":"batteryLevel;DROP","operator":"lessThan","value":20}]
            """;

        var args = _commandDefinition.Parse([
            "--filters", filters
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, CancellationToken.None);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Null(response.Results);
        Assert.Contains("invalid field path", response.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ExecuteAsync_ArrayValue_ReturnsBadRequest()
    {
        // Arrange
        const string filters = """
            [{"scope":"reported","field":"batteryLevel","operator":"equals","value":[20]}]
            """;

        var args = _commandDefinition.Parse([
            "--filters", filters
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, CancellationToken.None);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Null(response.Results);
        Assert.Contains("unsupported value kind", response.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        // Assert
        Assert.Equal("iothub-query-compile", _command.Id);
        Assert.Equal("compile", _command.Name);
        Assert.Contains("structured set of typed predicates", _command.Description, StringComparison.Ordinal);
        Assert.Contains("iothub query run", _command.Description, StringComparison.Ordinal);
        Assert.True(_command.Metadata.ReadOnly);
        Assert.False(_command.Metadata.Secret);
    }

    private static IoTHubQueryCompileResult DeserializeResult(CommandResponse response)
    {
        Assert.NotNull(response.Results);
        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize<IoTHubQueryCompileResult>(json);
        Assert.NotNull(result);
        return result;
    }
}
