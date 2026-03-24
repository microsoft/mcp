// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Net;
using System.Text.Json;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Tools.NetAppFiles.Commands;
using Azure.Mcp.Tools.NetAppFiles.Commands.Volume;
using Azure.Mcp.Tools.NetAppFiles.Models;
using Azure.Mcp.Tools.NetAppFiles.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Models.Command;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.NetAppFiles.UnitTests.Volume;

public class VolumeGetCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly INetAppFilesService _netAppFilesService;
    private readonly ILogger<VolumeGetCommand> _logger;
    private readonly VolumeGetCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;

    public VolumeGetCommandTests()
    {
        _netAppFilesService = Substitute.For<INetAppFilesService>();
        _logger = Substitute.For<ILogger<VolumeGetCommand>>();

        var collection = new ServiceCollection().AddSingleton(_netAppFilesService);

        _serviceProvider = collection.BuildServiceProvider();
        _command = new(_logger);
        _context = new(_serviceProvider);
        _commandDefinition = _command.GetCommand();
    }

    [Fact]
    public async Task ExecuteAsync_NoVolumeParameter_ReturnsAllVolumes()
    {
        // Arrange
        var subscription = "sub123";
        var expectedVolumes = new ResourceQueryResults<NetAppVolumeInfo>(
        [
            new("account1/pool1/vol1", "eastus", "rg1", "Succeeded", "Premium", 107374182400, "vol1", "/subscriptions/sub/resourceGroups/rg/providers/Microsoft.Network/virtualNetworks/vnet/subnets/subnet", ["NFSv3"], "Standard"),
            new("account1/pool1/vol2", "westus", "rg2", "Succeeded", "Standard", 53687091200, "vol2", "/subscriptions/sub/resourceGroups/rg/providers/Microsoft.Network/virtualNetworks/vnet/subnets/subnet", ["NFSv4.1"], "Standard")
        ], false);

        _netAppFilesService.GetVolumeDetails(
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Is(subscription),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expectedVolumes));

        var args = _commandDefinition.Parse(["--subscription", subscription]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, NetAppFilesJsonContext.Default.VolumeGetCommandResult);

        Assert.NotNull(result);
        Assert.NotNull(result.Volumes);
        Assert.Equal(expectedVolumes.Results.Count, result.Volumes.Count);
        Assert.Equal(expectedVolumes.Results.Select(v => v.Name), result.Volumes.Select(v => v.Name));
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsEmpty_WhenNoVolumes()
    {
        // Arrange
        var subscription = "sub123";

        _netAppFilesService.GetVolumeDetails(
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Is(subscription),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns(new ResourceQueryResults<NetAppVolumeInfo>([], false));

        var args = _commandDefinition.Parse(["--subscription", subscription]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, NetAppFilesJsonContext.Default.VolumeGetCommandResult);

        Assert.NotNull(result);
        Assert.Empty(result.Volumes);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesException()
    {
        // Arrange
        var expectedError = "Test error";
        var subscription = "sub123";

        _netAppFilesService.GetVolumeDetails(
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Is(subscription),
            null,
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception(expectedError));

        var args = _commandDefinition.Parse(["--subscription", subscription]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.StartsWith(expectedError, response.Message);
    }

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = _command.GetCommand();
        Assert.Equal("get", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
    }

    [Theory]
    [InlineData("--subscription sub123", true)]
    [InlineData("--subscription sub123 --account myanfaccount", true)]
    [InlineData("--subscription sub123 --account myanfaccount --pool mypool", true)]
    [InlineData("--subscription sub123 --account myanfaccount --pool mypool --volume myvol", true)]
    [InlineData("--subscription sub123 --volume myvol", true)] // Volume without account/pool is valid
    [InlineData("--account myanfaccount", false)] // Missing subscription
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        if (shouldSucceed)
        {
            var expectedVolumes = new ResourceQueryResults<NetAppVolumeInfo>(
                [new("account1/pool1/vol1", "eastus", "rg1", "Succeeded", "Premium", 107374182400, "vol1", "/subscriptions/sub/resourceGroups/rg/providers/Microsoft.Network/virtualNetworks/vnet/subnets/subnet", ["NFSv3"], "Standard")],
                false);

            _netAppFilesService.GetVolumeDetails(
                Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(expectedVolumes));
        }

        var parseResult = _commandDefinition.Parse(args);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(shouldSucceed ? HttpStatusCode.OK : HttpStatusCode.BadRequest, response.Status);
        if (shouldSucceed)
        {
            Assert.NotNull(response.Results);
            Assert.Equal("Success", response.Message);
        }
        else
        {
            Assert.Contains("required", response.Message.ToLower());
        }
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsVolumeDetails_WhenVolumeExists()
    {
        // Arrange
        var account = "myanfaccount";
        var pool = "mypool";
        var volume = "myvol";
        var subscription = "sub123";
        var expectedVolumes = new ResourceQueryResults<NetAppVolumeInfo>(
            [new($"{account}/{pool}/{volume}", "eastus", "rg1", "Succeeded", "Premium", 107374182400, volume, "/subscriptions/sub/resourceGroups/rg/providers/Microsoft.Network/virtualNetworks/vnet/subnets/subnet", ["NFSv3"], "Standard")],
            false);

        _netAppFilesService.GetVolumeDetails(
            Arg.Is(account), Arg.Is(pool), Arg.Is(volume), Arg.Is(subscription), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expectedVolumes));

        var args = _commandDefinition.Parse(["--account", account, "--pool", pool, "--volume", volume, "--subscription", subscription]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Results);
        Assert.Equal(HttpStatusCode.OK, response.Status);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, NetAppFilesJsonContext.Default.VolumeGetCommandResult);

        Assert.NotNull(result);
        Assert.Single(result.Volumes);
        Assert.Equal($"{account}/{pool}/{volume}", result.Volumes[0].Name);
        Assert.Equal("eastus", result.Volumes[0].Location);
        Assert.Equal("rg1", result.Volumes[0].ResourceGroup);
        Assert.Equal("Premium", result.Volumes[0].ServiceLevel);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        // Arrange
        var subscription = "sub123";

        _netAppFilesService.GetVolumeDetails(
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Is(subscription), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Test error"));

        var parseResult = _commandDefinition.Parse(["--subscription", subscription]);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Test error", response.Message);
        Assert.Contains("troubleshooting", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesNotFound()
    {
        // Arrange
        var subscription = "sub123";
        var volume = "nonexistentvol";

        _netAppFilesService.GetVolumeDetails(
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Is(volume), Arg.Is(subscription), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException((int)HttpStatusCode.NotFound, "Volume not found"));

        var parseResult = _commandDefinition.Parse(["--volume", volume, "--subscription", subscription]);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.Status);
        Assert.Contains("Volume not found", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesAuthorizationFailure()
    {
        // Arrange
        var subscription = "sub123";

        _netAppFilesService.GetVolumeDetails(
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Is(subscription), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException((int)HttpStatusCode.Forbidden, "Authorization failed"));

        var parseResult = _commandDefinition.Parse(["--subscription", subscription]);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.Status);
        Assert.Contains("Authorization failed", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_DeserializationValidation()
    {
        // Arrange
        var subscription = "sub123";
        var expectedVolumes = new ResourceQueryResults<NetAppVolumeInfo>(
            [new("account1/pool1/vol1", "eastus", "rg1", "Succeeded", "Premium", 107374182400, "vol1", "/subscriptions/sub/resourceGroups/rg/providers/Microsoft.Network/virtualNetworks/vnet/subnets/subnet", ["NFSv3", "NFSv4.1"], "Standard")],
            false);

        _netAppFilesService.GetVolumeDetails(
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Is(subscription), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expectedVolumes));

        var args = _commandDefinition.Parse(["--subscription", subscription]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, NetAppFilesJsonContext.Default.VolumeGetCommandResult);

        Assert.NotNull(result);
        Assert.Single(result.Volumes);
        var vol = result.Volumes[0];
        Assert.Equal("account1/pool1/vol1", vol.Name);
        Assert.Equal("eastus", vol.Location);
        Assert.Equal("rg1", vol.ResourceGroup);
        Assert.Equal("Succeeded", vol.ProvisioningState);
        Assert.Equal("Premium", vol.ServiceLevel);
        Assert.Equal(107374182400, vol.UsageThreshold);
        Assert.Equal("vol1", vol.CreationToken);
        Assert.NotNull(vol.ProtocolTypes);
        Assert.Equal(2, vol.ProtocolTypes.Count);
        Assert.Contains("NFSv3", vol.ProtocolTypes);
        Assert.Contains("NFSv4.1", vol.ProtocolTypes);
        Assert.Equal("Standard", vol.NetworkFeatures);
    }
}
