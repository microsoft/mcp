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

public sealed class CliGenerateCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IHttpClientService _httpClientService;
    private readonly ICliGenerateService _cliGenerateService;
    private readonly ILogger<CliGenerateCommand> _logger;
    private readonly CliGenerateCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;

    public CliGenerateCommandTests()
    {
        _httpClientService = Substitute.For<IHttpClientService>();
        _cliGenerateService = Substitute.For<ICliGenerateService>();
        _logger = Substitute.For<ILogger<CliGenerateCommand>>();

        var collection = new ServiceCollection();
        collection.AddSingleton(_httpClientService);
        collection.AddSingleton(_cliGenerateService);
        _serviceProvider = collection.BuildServiceProvider();
        _command = new(_logger);
        _context = new(_serviceProvider);
        _commandDefinition = _command.GetCommand();
    }

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = _command.GetCommand();
        Assert.Equal("generate", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
    }

    [Theory]
    [InlineData("", false)]
    [InlineData("--intent mock_intent", false)]
    [InlineData("--cli-type az", false)]
    [InlineData("--cli-type wrong_cli_type", false)]
    [InlineData("--intent mock_intent --cli-type az", true)]
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        // Arrange
        if (shouldSucceed)
        {
            _cliGenerateService.GenerateAzureCLICommandAsync(Arg.Any<string>())
                .Returns(Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("Command")
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
        else
        {
            Assert.Contains("required", response.Message.ToLower());
        }
    }

    [Fact]
    public async Task ExecuteAsync_DeserializationValidation()
    {
        // Arrange
        _cliGenerateService.GenerateAzureCLICommandAsync(Arg.Any<string>())
            .Returns(Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("Command")
            }));

        var parseResult = _commandDefinition.Parse("--intent mock_intent --cli-type az");

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult);

        // Assert
        Assert.Equal([200], [(int)response.Status]);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, ExtensionJsonContext.Default.CliGenerateResult);

        Assert.NotNull(result);
        Assert.Equal("az", result.CliType);
        Assert.Equal("Command", result.Command);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        // Arrange
        _cliGenerateService.GenerateAzureCLICommandAsync(Arg.Any<string>()).ThrowsAsync(new Exception("Test error"));

        var parseResult = _commandDefinition.Parse("--intent mock_intent --cli-type az");

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult);

        // Assert
        Assert.Equal([500], [(int)response.Status]);
        Assert.Contains("Test error", response.Message);
    }
}
