// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Net;
using System.Text.Json;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Core.Services.Http;
using Azure.Mcp.Tools.Extension.Commands;
using Azure.Mcp.Tools.Extension.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.Extension.UnitTests;

public sealed class CliInstallCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IHttpClientService _httpClientService;
    private readonly ICliInstallService _cliInstallService;
    private readonly ILogger<CliInstallCommand> _logger;
    private readonly CliInstallCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;

    public CliInstallCommandTests()
    {
        _httpClientService = Substitute.For<IHttpClientService>();
        _cliInstallService = Substitute.For<ICliInstallService>();
        _logger = Substitute.For<ILogger<CliInstallCommand>>();

        var collection = new ServiceCollection();
        collection.AddSingleton(_httpClientService);
        collection.AddSingleton(_cliInstallService);
        _serviceProvider = collection.BuildServiceProvider();
        _command = new(_logger);
        _context = new(_serviceProvider);
        _commandDefinition = _command.GetCommand();
    }

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = _command.GetCommand();
        Assert.Equal("install", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
    }

    [Theory]
    [InlineData("", false)]
    [InlineData("--cli-type azd", true)]
    [InlineData("--cli-type az", true)]
    [InlineData("--cli-type func", true)]
    [InlineData("--cli-type wrong_cli_type", false)]
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        // Arrange
        if (shouldSucceed)
        {
            _cliInstallService.GetCliInstallInstructions(Arg.Any<string>())
                .Returns(Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("Instructions")
                }));
        }

        // Build args from a single string in tests using the test-only splitter
        var parseResult = _commandDefinition.Parse(args);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult);

        // Assert
        Assert.Equal([shouldSucceed ? 200 : 400], [(int)response.Status]);
        if (shouldSucceed)
        {
            Assert.NotNull(response.Results);
            Assert.Equal("Success", response.Message);
        }
    }

    [Fact]
    public async Task ExecuteAsync_DeserializationValidation()
    {
        // Arrange
        _cliInstallService.GetCliInstallInstructions(Arg.Any<string>())
            .Returns(Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("Instructions")
            }));

        var parseResult = _commandDefinition.Parse("--cli-type az");

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult);

        // Assert
        Assert.Equal([200], [(int)response.Status]);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, ExtensionJsonContext.Default.CliInstallResult);

        Assert.NotNull(result);
        Assert.Equal("az", result.CliType);
        Assert.Equal("Instructions", result.InstallationInstructions);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        // Arrange
        _cliInstallService.GetCliInstallInstructions(Arg.Any<string>()).ThrowsAsync(new Exception("Test error"));

        var parseResult = _commandDefinition.Parse("--cli-type az");

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult);

        // Assert
        Assert.Equal([500], [(int)response.Status]);
        Assert.Contains("Test error", response.Message);
    }
}
