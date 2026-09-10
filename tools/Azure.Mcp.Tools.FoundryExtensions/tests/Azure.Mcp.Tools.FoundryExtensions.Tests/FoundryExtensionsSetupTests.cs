// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.FoundryExtensions.Tests;

public sealed class FoundryExtensionsSetupTests()
{
    [Fact]
    public void RegisterCommands_DescriptionDisambiguatesFoundryTools()
    {
        var setup = new FoundryExtensionsSetup();
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton(Substitute.For<IAzureService>());
        services.AddSingleton(Substitute.For<ISubscriptionResolver>());
        setup.ConfigureServices(services);

        var group = setup.RegisterCommands(services.BuildServiceProvider());

        Assert.Contains("resource inventory", group.Description, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("OpenAI-compatible inference", group.Description, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("do not use Microsoft Foundry MCP", group.Description, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("do not use Azure Resource Manager MCP", group.Description, StringComparison.OrdinalIgnoreCase);
    }
}