// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Text.Json;
using System.Text.Json.Serialization;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.AzureManagedLustre.Commands.FileSystem;
using Azure.Mcp.Tools.AzureManagedLustre.Models;
using Azure.Mcp.Tools.AzureManagedLustre.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Azure.Mcp.Tools.AzureManagedLustre.UnitTests.FileSystem;

public class SkuGetCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IAzureManagedLustreService _amlfsService;
    private readonly ILogger<SkuGetCommand> _logger;
    private readonly SkuGetCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;
    private readonly string _knownSubscriptionId = "sub123";

    public SkuGetCommandTests()
    {
        _amlfsService = Substitute.For<IAzureManagedLustreService>();
        _logger = Substitute.For<ILogger<SkuGetCommand>>();

        var services = new ServiceCollection().AddSingleton(_amlfsService);
        _serviceProvider = services.BuildServiceProvider();

        _command = new(_logger);
        _context = new(_serviceProvider);
        _commandDefinition = _command.GetCommand();
    }

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = _command.GetCommand();
        Assert.Equal("get", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsSkus()
    {
        // Arrange
        var expected = new List<AzureManagedLustreSkuInfo>
        {
            new(
                name: "AMLFS-Durable-Premium-40",
                location: "eastus",
                supportsZones: true,
                capabilities: new List<AzureManagedLustreSkuCapability>
                {
                    new("maxCapacityTiB", "500"),
                }
            ),
            new(
                name: "AMLFS-Durable-Premium-125",
                location: "eastus2",
                supportsZones: false,
                capabilities: []
            )
        };

        _amlfsService.SkuGetInfoAsync(
            Arg.Is(_knownSubscriptionId),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>()
            )
            .Returns(expected);

        var args = _commandDefinition.Parse([
            "--subscription", _knownSubscriptionId
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize<ResultJson>(json);

        Assert.NotNull(result);
        Assert.NotNull(result!.Skus);
        Assert.Equal(2, result.Skus.Count);
        Assert.Equal("AMLFS-Durable-Premium-40", result.Skus[0].Name);
    }

    [Theory]
    [InlineData("", false)]
    [InlineData("--subscription sub123", true)]
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        if (shouldSucceed)
        {
            _amlfsService.SkuGetInfoAsync(Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>())
                .Returns(new List<AzureManagedLustreSkuInfo> { new("n", "eastus", false, []) });
        }

        var parseResult = _commandDefinition.Parse(args.Split(' ', StringSplitOptions.RemoveEmptyEntries));
        var response = await _command.ExecuteAsync(_context, parseResult);

        Assert.Equal(shouldSucceed ? 200 : 400, response.Status);
    }

    private class ResultJson
    {
        [JsonPropertyName("skus")]
        public List<SkuJson> Skus { get; set; } = [];
    }

    private class SkuJson
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;
        [JsonPropertyName("region")]
        public string Region { get; set; } = string.Empty;
        [JsonPropertyName("supportsZones")]
        public bool SupportsZones { get; set; }
        [JsonPropertyName("capabilities")]
        public List<CapJson> Capabilities { get; set; } = [];
    }

    private class CapJson
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;
        [JsonPropertyName("value")]
        public string Value { get; set; } = string.Empty;
    }
}
