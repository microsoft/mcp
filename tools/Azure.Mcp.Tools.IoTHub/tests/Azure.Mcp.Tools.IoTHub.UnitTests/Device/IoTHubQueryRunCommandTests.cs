// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Net;
using System.Text.Json;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Tools.IoTHub.Commands.IoTHub;
using Azure.Mcp.Tools.IoTHub.Models;
using Azure.Mcp.Tools.IoTHub.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.IoTHub.UnitTests.Device;

public class IoTHubQueryRunCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IIoTHubDeviceService _service;
    private readonly ILogger<IoTHubQueryRunCommand> _logger;
    private readonly IoTHubQueryRunCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;

    public IoTHubQueryRunCommandTests()
    {
        _service = Substitute.For<IIoTHubDeviceService>();
        _logger = Substitute.For<ILogger<IoTHubQueryRunCommand>>();

        var collection = new ServiceCollection().AddSingleton(_service);

        _serviceProvider = collection.BuildServiceProvider();
        _command = new IoTHubQueryRunCommand(_service, _logger);
        _context = new CommandContext(_serviceProvider);
        _commandDefinition = _command.GetCommand();
    }

    private static List<JsonElement> SampleItems(int count)
    {
        var items = new List<JsonElement>();
        for (var i = 0; i < count; i++)
        {
            items.Add(JsonSerializer.SerializeToElement(new { deviceId = $"device{i}" }));
        }
        return items;
    }

    [Fact]
    public async Task ExecuteAsync_RunQuery_Success()
    {
        // Arrange
        var name = "test-hub";
        var resourceGroup = "test-rg";
        var subscription = "sub-id";
        var query = "SELECT * FROM devices";

        _service.RunQuery(
            query,
            name,
            resourceGroup,
            subscription,
            Arg.Any<int?>(),
            Arg.Any<string?>(),
            Arg.Any<Core.Options.RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns(new IoTHubQueryPage(SampleItems(2), null));

        var args = _commandDefinition.Parse([
            "--name", name,
            "--resource-group", resourceGroup,
            "--subscription", subscription,
            "--query", query
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, CancellationToken.None);

        // Assert
        Assert.NotNull(response.Results);
    }

    [Fact]
    public async Task ExecuteAsync_DefaultsPageSizeTo100()
    {
        // Arrange
        var name = "test-hub";
        var resourceGroup = "test-rg";
        var subscription = "sub-id";
        var query = "SELECT * FROM devices";

        _service.RunQuery(
            query,
            name,
            resourceGroup,
            subscription,
            Arg.Any<int?>(),
            Arg.Any<string?>(),
            Arg.Any<Core.Options.RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns(new IoTHubQueryPage(SampleItems(1), null));

        var args = _commandDefinition.Parse([
            "--name", name,
            "--resource-group", resourceGroup,
            "--subscription", subscription,
            "--query", query
        ]);

        // Act
        await _command.ExecuteAsync(_context, args, CancellationToken.None);

        // Assert
        await _service.Received(1).RunQuery(
            query,
            name,
            resourceGroup,
            subscription,
            100,
            Arg.Any<string?>(),
            Arg.Any<Core.Options.RetryPolicyOptions>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_CapsPageSizeAt100()
    {
        // Arrange
        var name = "test-hub";
        var resourceGroup = "test-rg";
        var subscription = "sub-id";
        var query = "SELECT * FROM devices";

        _service.RunQuery(
            query,
            name,
            resourceGroup,
            subscription,
            Arg.Any<int?>(),
            Arg.Any<string?>(),
            Arg.Any<Core.Options.RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns(new IoTHubQueryPage(SampleItems(1), null));

        var args = _commandDefinition.Parse([
            "--name", name,
            "--resource-group", resourceGroup,
            "--subscription", subscription,
            "--query", query,
            "--max-count", "1000"
        ]);

        // Act
        await _command.ExecuteAsync(_context, args, CancellationToken.None);

        // Assert
        await _service.Received(1).RunQuery(
            query,
            name,
            resourceGroup,
            subscription,
            100,
            Arg.Any<string?>(),
            Arg.Any<Core.Options.RetryPolicyOptions>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_HasMore_PassesContinuationToken_AndReportsMore()
    {
        // Arrange
        var name = "test-hub";
        var resourceGroup = "test-rg";
        var subscription = "sub-id";
        var query = "SELECT * FROM devices";
        var inToken = "prev-token";

        _service.RunQuery(
            query,
            name,
            resourceGroup,
            subscription,
            Arg.Any<int?>(),
            inToken,
            Arg.Any<Core.Options.RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns(new IoTHubQueryPage(SampleItems(100), "next-token"));

        var args = _commandDefinition.Parse([
            "--name", name,
            "--resource-group", resourceGroup,
            "--subscription", subscription,
            "--query", query,
            "--max-count", "500",
            "--continuation-token", inToken
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, CancellationToken.None);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        var result = DeserializeResult(response);
        Assert.Equal(100, result.Count);
        Assert.True(result.HasMore);
        Assert.Equal("next-token", result.ContinuationToken);
        Assert.Contains("More results are available", result.Message, StringComparison.Ordinal);
        Assert.Contains("return this page now", result.Message, StringComparison.Ordinal);
        Assert.Contains("later explicit next-page request", result.Message, StringComparison.Ordinal);

        await _service.Received(1).RunQuery(
            query,
            name,
            resourceGroup,
            subscription,
            100,
            inToken,
            Arg.Any<Core.Options.RetryPolicyOptions>(),
            Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("true")]
    [InlineData("false")]
    [InlineData(" TRUE ")]
    public async Task ExecuteAsync_BooleanContinuationToken_ReturnsBadRequest(string continuationToken)
    {
        // Arrange
        var args = _commandDefinition.Parse([
            "--name", "test-hub",
            "--resource-group", "test-rg",
            "--subscription", "sub-id",
            "--query", "SELECT * FROM devices",
            "--continuation-token", continuationToken
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, CancellationToken.None);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Null(response.Results);
        Assert.Contains("opaque continuationToken string", response.Message, StringComparison.Ordinal);

        await _service.DidNotReceive().RunQuery(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<int?>(),
            Arg.Any<string?>(),
            Arg.Any<Core.Options.RetryPolicyOptions>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_FinalPage_ReportsNoMoreResults()
    {
        // Arrange
        var name = "test-hub";
        var resourceGroup = "test-rg";
        var subscription = "sub-id";
        var query = "SELECT * FROM devices";

        _service.RunQuery(
            query,
            name,
            resourceGroup,
            subscription,
            Arg.Any<int?>(),
            Arg.Any<string?>(),
            Arg.Any<Core.Options.RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns(new IoTHubQueryPage(SampleItems(2), null));

        var args = _commandDefinition.Parse([
            "--name", name,
            "--resource-group", resourceGroup,
            "--subscription", subscription,
            "--query", query,
            "--max-count", "500"
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, CancellationToken.None);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        var result = DeserializeResult(response);
        Assert.Equal(2, result.Count);
        Assert.False(result.HasMore);
        Assert.Null(result.ContinuationToken);
        Assert.Contains("No more results are available", result.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ExecuteAsync_MaxCountLessThanOne_ReturnsBadRequest()
    {
        // Arrange
        var name = "test-hub";
        var resourceGroup = "test-rg";
        var subscription = "sub-id";
        var query = "SELECT * FROM devices";

        var args = _commandDefinition.Parse([
            "--name", name,
            "--resource-group", resourceGroup,
            "--subscription", subscription,
            "--query", query,
            "--max-count", "0"
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, CancellationToken.None);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.Status);
        Assert.Null(response.Results);
        Assert.Contains("less than 1", response.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        // Assert
        Assert.Equal("iothub-query-run", _command.Id);
        Assert.Equal("run", _command.Name);
        Assert.NotNull(_command.Description);
        Assert.Contains("maximum 100", _command.Description, StringComparison.Ordinal);
        Assert.Contains("Values greater than 100 are capped at 100", _command.Description, StringComparison.Ordinal);
        Assert.Contains("more than 100 matching records", _command.Description, StringComparison.Ordinal);
        Assert.Contains("compact projection", _command.Description, StringComparison.Ordinal);
        Assert.Contains("avoid 'SELECT *'", _command.Description, StringComparison.Ordinal);
        Assert.Contains("large device sets", _command.Description, StringComparison.Ordinal);
        Assert.Contains("SELECT * FROM devices", _command.Description, StringComparison.Ordinal);
        Assert.Contains("Never make repeated calls or loop", _command.Description, StringComparison.Ordinal);
        Assert.Contains("Return exactly one page", _command.Description, StringComparison.Ordinal);
        Assert.Contains("opaque continuationToken string", _command.Description, StringComparison.Ordinal);
        Assert.Contains("do not pass hasMore=true/false", _command.Description, StringComparison.Ordinal);
        Assert.DoesNotContain("Azure CLI", _command.Description);
        Assert.DoesNotContain("az iot hub query", _command.Description);
        Assert.True(_command.Metadata.ReadOnly);
        Assert.False(_command.Metadata.Secret);
    }

    private static IoTHubQueryRunResult DeserializeResult(CommandResponse response)
    {
        Assert.NotNull(response.Results);
        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize<IoTHubQueryRunResult>(json);
        Assert.NotNull(result);
        return result;
    }
}
