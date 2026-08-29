// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Adme.Commands.HealthCheck;
using Azure.Mcp.Tools.Adme.Commands.Schema;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Services.Azure.Authentication;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.Adme.Tests;

public sealed class AdmeSetupTests
{
    [Fact]
    public void Setup_RegistersAndExposesCommands()
    {
        var setup = new AdmeSetup();
        var services = new ServiceCollection();
        services.AddSingleton(Substitute.For<IAzureTokenCredentialProvider>());
        setup.ConfigureServices(services);
        using var serviceProvider = services.BuildServiceProvider();

        var adme = setup.RegisterCommands(serviceProvider);

        Assert.Equal("adme", setup.Name);
        Assert.Equal("Azure Data Manager for Energy", setup.Title);
        Assert.Equal("adme", adme.Name);
        var health = Assert.Single(adme.SubGroup, group => group.Name == "health");
        Assert.True(health.Commands.ContainsKey("check"));
        var schema = Assert.Single(adme.SubGroup, group => group.Name == "schema");
        Assert.True(schema.Commands.ContainsKey("get"));
        Assert.True(schema.Commands.ContainsKey("list"));
        Assert.NotNull(serviceProvider.GetRequiredService<HealthCheckCommand>());
        Assert.NotNull(serviceProvider.GetRequiredService<SchemaGetCommand>());
        Assert.NotNull(serviceProvider.GetRequiredService<SchemaListCommand>());
    }
}
