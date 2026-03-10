// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.FoundryExtensions.Commands;
using Azure.Mcp.Tools.FoundryExtensions.Models;
using Azure.Mcp.Tools.FoundryExtensions.Options;
using Azure.Mcp.Tools.FoundryExtensions.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Models.Command;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.FoundryExtensions.UnitTests;

public class AgentsGetSdkCodeSampleCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IFoundryExtensionsService _foundryService;

    public AgentsGetSdkCodeSampleCommandTests()
    {
        _foundryService = Substitute.For<IFoundryExtensionsService>();

        _serviceProvider = new ServiceCollection().BuildServiceProvider();
    }

    [Theory]
    [InlineData("", FoundryExtensionsOptionDefinitions.ProgrammingLanguage)]
    public async Task ExecuteAsync_Fails_WhenMissingRequiredParameter(string argsString, string missingArgName)
    {
        var command = new AgentsGetSdkSampleCommand(_foundryService);
        var args = command.GetCommand().Parse(argsString);
        var context = new CommandContext(_serviceProvider);
        var response = await command.ExecuteAsync(context, args, TestContext.Current.CancellationToken);

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains(missingArgName, response.Message);
    }

    [Theory]
    [InlineData("python")]
    [InlineData("csharp")]
    [InlineData("typescript")]

    public async Task ExecuteAsync_ReturnsSdkCodeSample(string programmingLanguage)
    {
        _foundryService.GetSdkCodeSample(Arg.Any<string>())
            .Returns(new AgentsGetSdkCodeSampleResult { CodeSampleText = "sample code" });

        var command = new AgentsGetSdkSampleCommand(_foundryService);
        var args = command.GetCommand().Parse(["--programming-language", programmingLanguage]);
        var context = new CommandContext(_serviceProvider);
        var response = await command.ExecuteAsync(context, args, TestContext.Current.CancellationToken);

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
    }
}
