// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.Mcp.Core.Services.Http;
using Azure.Mcp.Tools.Foundry.Commands;
using Azure.Mcp.Tools.Foundry.Options;
using Azure.Mcp.Tools.Foundry.Services;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.Foundry.UnitTests;

public class AgentsGetSdkCodeSampleCommandTests
{
    private readonly IServiceProvider _serviceProvider;

    public AgentsGetSdkCodeSampleCommandTests()
    {
        var collection = new ServiceCollection();
        var httpClientService = Substitute.For<IHttpClientService>();
        var subscriptionService = Substitute.For<ISubscriptionService>();
        var tenantService = Substitute.For<ITenantService>();
        collection.AddSingleton(httpClientService);
        collection.AddSingleton(subscriptionService);
        collection.AddSingleton(tenantService);
        collection.AddSingleton<IFoundryService, FoundryService>();

        _serviceProvider = collection.BuildServiceProvider();
    }

    [Theory]
    [InlineData("", FoundryOptionDefinitions.ProgrammingLanguage)]
    public async Task ExecuteAsync_Fails_WhenMissingRequiredParameter(string argsString, string missingArgName)
    {
        var command = new AgentsGetSdkSampleCommand();
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
        var command = new AgentsGetSdkSampleCommand();
        var args = command.GetCommand().Parse(["--programming-language", programmingLanguage]);
        var context = new CommandContext(_serviceProvider);
        var response = await command.ExecuteAsync(context, args, TestContext.Current.CancellationToken);

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
    }
}
