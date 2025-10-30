// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Tools.Foundry.Commands;
using Azure.Mcp.Tools.Foundry.Models;
using Azure.Mcp.Tools.Foundry.Options;
using Azure.Mcp.Tools.Foundry.Services;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.Foundry.UnitTests;

public class AgentsGetSdkCodeSampleCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IFoundryService _foundryService;

    public AgentsGetSdkCodeSampleCommandTests()
    {
        _foundryService = Substitute.For<IFoundryService>();

        var collection = new ServiceCollection();
        collection.AddSingleton(_foundryService);

        _serviceProvider = collection.BuildServiceProvider();
    }

    [Theory]
    [InlineData("", FoundryOptionDefinitions.ProgrammingLanguage)]
    public async Task ExecuteAsync_Fails_WhenMissingRequiredParameter(string argsString, string missingArgName)
    {
        var command = new AgentsGetSdkSampleCommand();
        var args = command.GetCommand().Parse(argsString);
        var context = new CommandContext(_serviceProvider);
        var response = await command.ExecuteAsync(context, args);

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
        var expectedResult = new AgentsGetSdkCodeSampleResult()
        {
            CodeSampleText = "code sample text"
        };

        _foundryService.GetSdkCodeSample(
            Arg.Is(programmingLanguage)
            )
            .Returns(expectedResult);

        var command = new AgentsGetSdkSampleCommand();
        var args = command.GetCommand().Parse(["--programming-language", programmingLanguage]);
        var context = new CommandContext(_serviceProvider);
        var response = await command.ExecuteAsync(context, args);

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
    }
}
