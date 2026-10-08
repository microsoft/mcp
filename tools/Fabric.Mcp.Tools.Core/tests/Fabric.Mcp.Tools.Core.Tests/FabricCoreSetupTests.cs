// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Fabric.Mcp.Tools.Core.Commands;
using Fabric.Mcp.Tools.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Areas.Server;
using Microsoft.Mcp.Core.Areas.Server.Options;
using Microsoft.Mcp.Core.Helpers;
using Microsoft.Mcp.Core.Services.Http;
using Xunit;

namespace Fabric.Mcp.Tools.Core.Tests;

public class FabricCoreSetupTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ConfigureServices_DisablesRedirectsWithoutReplacingConfiguredTransport(bool recordingProxy)
    {
        var services = new ServiceCollection();
        services.Configure<HttpClientOptions>(options =>
        {
            options.AllProxy = "http://proxy.example:8080";
            options.NoProxy = "localhost";
            options.DefaultTimeout = TimeSpan.FromSeconds(37);
        });
        services.Configure<ServerRuntimeConfiguration>(options => options.Transport = TransportTypes.StdIo);
        services.ConfigureDefaultHttpClient(() => recordingProxy ? new Uri("http://recording.example:5000") : null);
        HttpMessageHandler? configuredHandler = null;
        services.ConfigureHttpClientDefaults(builder =>
            builder.ConfigurePrimaryHttpMessageHandler((handler, _) => configuredHandler = handler));
        new FabricCoreSetup().ConfigureServices(services);
        using var provider = services.BuildServiceProvider();

        using var client = provider.GetRequiredService<IHttpClientFactory>().CreateClient(nameof(IFabricCoreService));
        Assert.NotNull(configuredHandler);
        var transport = Assert.IsType<HttpClientHandler>(GetTransport(configuredHandler));
        Assert.Same(transport, GetTransport(provider.GetRequiredService<IHttpMessageHandlerFactory>().CreateHandler(nameof(IFabricCoreService))));
        Assert.False(transport.AllowAutoRedirect);
        Assert.True(transport.UseProxy);
        var proxy = Assert.IsType<WebProxy>(transport.Proxy);
        Assert.Equal(new Uri("http://proxy.example:8080"), proxy.Address);
        Assert.True(proxy.IsBypassed(new Uri("http://localhost")));
        Assert.Equal(EnvironmentHelpers.IsPlaybackTesting() ? TimeSpan.FromMinutes(11) : TimeSpan.FromSeconds(37), client.Timeout);
        Assert.NotEmpty(client.DefaultRequestHeaders.UserAgent);
#if DEBUG
        if (recordingProxy)
        {
            Assert.IsAssignableFrom<DelegatingHandler>(configuredHandler);
        }
#endif
        var unrelated = provider.GetRequiredService<IHttpMessageHandlerFactory>().CreateHandler("unrelated");
        Assert.True(Assert.IsType<HttpClientHandler>(GetTransport(unrelated)).AllowAutoRedirect);
    }

    [Fact]
    public void ConfigureServices_DisablesRedirectsOnExistingSocketsTransport()
    {
        using var transport = new SocketsHttpHandler { ConnectTimeout = TimeSpan.FromSeconds(17) };
        var services = new ServiceCollection();
        services.ConfigureHttpClientDefaults(builder => builder.ConfigurePrimaryHttpMessageHandler(() => transport));
        new FabricCoreSetup().ConfigureServices(services);
        using var provider = services.BuildServiceProvider();

        var handler = provider.GetRequiredService<IHttpMessageHandlerFactory>().CreateHandler(nameof(IFabricCoreService));

        Assert.Same(transport, GetTransport(handler));
        Assert.False(transport.AllowAutoRedirect);
        Assert.Equal(TimeSpan.FromSeconds(17), transport.ConnectTimeout);
    }

    [Fact]
    public void ConfigureServices_RegistersAllServices()
    {
        // Arrange
        var services = new ServiceCollection();
        var setup = new FabricCoreSetup();

        // Act
        setup.ConfigureServices(services);

        // Assert
        Assert.Contains(services, s => s.ServiceType == typeof(IFabricCoreService));
        Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(CapacityGetCommand) && descriptor.Lifetime == ServiceLifetime.Singleton);
        Assert.Contains(services, s => s.ServiceType == typeof(CapacityListCommand) && s.Lifetime == ServiceLifetime.Singleton);
        Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(WorkspaceGetCommand) && descriptor.Lifetime == ServiceLifetime.Singleton);
        Assert.Contains(services, s => s.ServiceType == typeof(WorkspaceListCommand) && s.Lifetime == ServiceLifetime.Singleton);
        Assert.Contains(services, s => s.ServiceType == typeof(ItemListCommand) && s.Lifetime == ServiceLifetime.Singleton);
        Assert.Contains(services, s => s.ServiceType == typeof(ItemGetCommand) && s.Lifetime == ServiceLifetime.Singleton);
        Assert.Contains(services, s => s.ServiceType == typeof(WorkspaceCreateCommand) && s.Lifetime == ServiceLifetime.Singleton);
        Assert.Contains(services, s => s.ServiceType == typeof(WorkspaceUpdateCommand) && s.Lifetime == ServiceLifetime.Singleton);
        Assert.Contains(services, s => s.ServiceType == typeof(ItemUpdateCommand) && s.Lifetime == ServiceLifetime.Singleton);
        Assert.Contains(services, s => s.ServiceType == typeof(ItemDeleteCommand) && s.Lifetime == ServiceLifetime.Singleton);
        Assert.Contains(services, s => s.ServiceType == typeof(WorkspaceDeleteCommand) && s.Lifetime == ServiceLifetime.Singleton);
        Assert.Contains(services, s => s.ServiceType == typeof(WorkspaceAssignToCapacityCommand)
            && s.Lifetime == ServiceLifetime.Singleton);
    }

    [Fact]
    public void Name_ReturnsCorrectValue()
    {
        // Arrange
        var setup = new FabricCoreSetup();

        // Act & Assert
        Assert.Equal("core", setup.Name);
    }

    [Fact]
    public void RegisterCommands_RegistersCoreCommands()
    {
        // Arrange
        var services = new ServiceCollection();
        var setup = new FabricCoreSetup();
        setup.ConfigureServices(services);
        using var provider = services.BuildServiceProvider();

        // Act
        var rootGroup = setup.RegisterCommands(provider);

        // Assert
        Assert.True(rootGroup.Commands.ContainsKey("create-item"), "Should have create-item command");
        Assert.True(rootGroup.Commands.ContainsKey("search-catalog"), "Should have search-catalog command");
        Assert.True(rootGroup.Commands.ContainsKey("get-capacity"), "Should have get-capacity command");
        Assert.True(rootGroup.Commands.ContainsKey("get-item"), "Should have get-item command");
        Assert.True(rootGroup.Commands.ContainsKey("list-capacities"), "Should have list-capacities command");
        Assert.True(rootGroup.Commands.ContainsKey("get-workspace"), "Should have get-workspace command");
        Assert.True(rootGroup.Commands.ContainsKey("list-workspaces"), "Should have list-workspaces command");
        Assert.True(rootGroup.Commands.ContainsKey("list-items"), "Should have list-items command");
        Assert.True(rootGroup.Commands.ContainsKey("create-workspace"), "Should have create-workspace command");
        Assert.True(rootGroup.Commands.ContainsKey("update-workspace"), "Should have update-workspace command");
        Assert.True(rootGroup.Commands.ContainsKey("update-item"), "Should have update-item command");
        Assert.True(rootGroup.Commands.ContainsKey("delete-item"), "Should have delete-item command");
        Assert.True(rootGroup.Commands.ContainsKey("delete-workspace"), "Should have delete-workspace command");
        Assert.True(rootGroup.Commands.ContainsKey("assign-workspace-to-capacity"), "Should have assign-workspace-to-capacity command");
        Assert.Equal(14, rootGroup.Commands.Count);
    }

    [Fact]
    public void Title_ReturnsCorrectValue()
    {
        // Arrange
        var setup = new FabricCoreSetup();

        // Act & Assert
        Assert.Equal("Microsoft Fabric Core", setup.Title);
    }

    private static HttpMessageHandler GetTransport(HttpMessageHandler handler)
    {
        while (handler is DelegatingHandler { InnerHandler: { } innerHandler })
        {
            handler = innerHandler;
        }
        return handler;
    }
}
