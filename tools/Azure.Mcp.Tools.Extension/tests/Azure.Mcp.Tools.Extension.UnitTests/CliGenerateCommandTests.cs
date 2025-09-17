// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text;
using System.Text.Json;
using Azure.Core;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Core.Services.Http;
using Azure.Mcp.Tools.Extension.Commands;
using Azure.Mcp.Tools.Extension.Options;
using Azure.Mcp.Tools.Extension.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.Extension.UnitTests;

public sealed class CliGenerateCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<CliGenerateCommand> _logger;
    private readonly IHttpClientService _httpClientService;
    private readonly ICliGenerateService _cliGenerateService;

    public CliGenerateCommandTests()
    {
        _logger = Substitute.For<ILogger<CliGenerateCommand>>();

        _httpClientService = Substitute.For<IHttpClientService>();
        _cliGenerateService = Substitute.For<ICliGenerateService>();

        var collection = new ServiceCollection();
        collection.AddSingleton(_httpClientService);
        collection.AddSingleton(_cliGenerateService);

        _serviceProvider = collection.BuildServiceProvider();
    }

    [Fact]
    public async Task ExecuteAsync_CanAcquireToken()
    {
        var command = new CliGenerateCommand(_logger);

        const string mockResponseBody = "mock response body";
        _cliGenerateService.GenerateAzureCLICommandAsync(Arg.Any<string>())
            .Returns(
                new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(mockResponseBody)
                }
            );

        var mockIntent = "\"Create a resource group named 'TestRG' in the 'eastus' region\"";
        var mockCliType = "\"az\"";
        var args = command.GetCommand().Parse($"--{ExtensionOptionDefinitions.CliGenerate.IntentName} {mockIntent} --{ExtensionOptionDefinitions.CliGenerate.CliTypeName} {mockCliType}");
        var context = new CommandContext(_serviceProvider);

        try
        {
            // Act
            var response = await command.ExecuteAsync(context, args);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(200, response.Status);
            Assert.NotNull(response.Results);
            using var stream = new MemoryStream();
            using var writer = new Utf8JsonWriter(stream);
            response.Results.Write(writer);
            writer.Flush();
            var resultString = Encoding.UTF8.GetString(stream.ToArray());
            Assert.Contains(mockResponseBody, resultString);
            Assert.Contains(mockCliType, resultString);
        }
        finally
        {
            // Cleanup
            // noop
        }
    }
}
