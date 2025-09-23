// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Areas.Server.Commands;
using Azure.Mcp.Core.Areas.Server.Commands.Discovery;
using Azure.Mcp.Core.Areas.Server.Commands.Runtime;
using Azure.Mcp.Core.Areas.Server.Commands.ToolLoading;
using Azure.Mcp.Core.Areas.Server.Options;
using Azure.Mcp.Core.Services.Telemetry;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using ModelContextProtocol.Server;
using Xunit;

namespace Azure.Mcp.Core.UnitTests.Areas.Server.Commands;

public class ServiceCollectionExtensionsTests
{
    // TransportTypes is internal, so we'll use strings directly
    private const string StdioTransport = "stdio";

    private IServiceCollection SetupBaseServices()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton(CommandFactoryHelpers.CreateCommandFactory);
        services.AddSingleton<ITelemetryService, CommandFactoryHelpers.NoOpTelemetryService>();

        return services;
    }

    [Fact]
    public void AddAzureMcpServer_RegistersCommonServices()
    {
        // Arrange
        var services = SetupBaseServices();
        var options = new ServiceStartOptions
        {
            Transport = StdioTransport
        };

        // Act
        services.AddAzureMcpServer(options);

        // Assert
        // Verify common services are registered
        var provider = services.BuildServiceProvider();

        // Verify base discovery strategies
        Assert.NotNull(provider.GetService<CommandGroupDiscoveryStrategy>());
        Assert.NotNull(provider.GetService<RegistryDiscoveryStrategy>());

        // Verify base tool loaders
        Assert.NotNull(provider.GetService<CommandFactoryToolLoader>());
        Assert.NotNull(provider.GetService<RegistryToolLoader>());

        // Verify runtime
        Assert.NotNull(provider.GetService<IMcpRuntime>());
        Assert.IsType<McpRuntime>(provider.GetService<IMcpRuntime>());
    }

    [Fact]
    public void AddAzureMcpServer_WithSingleProxy_RegistersSingleProxyToolLoader()
    {
        // Arrange
        var services = SetupBaseServices();
        var options = new ServiceStartOptions
        {
            Transport = StdioTransport,
            Mode = "single"
        };

        // Act
        services.AddAzureMcpServer(options);

        // Assert
        var provider = services.BuildServiceProvider();

        // Verify the correct tool loader is registered
        Assert.NotNull(provider.GetService<IToolLoader>());
        Assert.IsType<SingleProxyToolLoader>(provider.GetService<IToolLoader>());

        // Verify discovery strategy is registered
        Assert.NotNull(provider.GetService<IMcpDiscoveryStrategy>());
        Assert.IsType<CompositeDiscoveryStrategy>(provider.GetService<IMcpDiscoveryStrategy>());
    }

    [Fact]
    public void AddAzureMcpServer_WithNamespaceProxy_RegistersCompositeToolLoader()
    {
        // Arrange
        var services = SetupBaseServices();
        var options = new ServiceStartOptions
        {
            Transport = StdioTransport,
            Mode = "namespace"
        };

        // Act
        services.AddAzureMcpServer(options);

        // Assert
        var provider = services.BuildServiceProvider();

        // Verify the correct tool loader is registered
        // In namespace mode, we now use CompositeToolLoader that includes ServerToolLoader
        Assert.NotNull(provider.GetService<IToolLoader>());
        Assert.IsType<CompositeToolLoader>(provider.GetService<IToolLoader>());

        // Verify discovery strategy is registered
        Assert.NotNull(provider.GetService<IMcpDiscoveryStrategy>());
        Assert.IsType<CompositeDiscoveryStrategy>(provider.GetService<IMcpDiscoveryStrategy>());
    }

    [Fact]
    public void AddAzureMcpServer_WithDefaultMode_RegistersServerToolLoader()
    {
        // Arrange
        var services = SetupBaseServices();
        var options = new ServiceStartOptions
        {
            Transport = StdioTransport,
            // No mode specified - should use default "namespace" mode
        };

        // Act
        services.AddAzureMcpServer(options);

        // Assert
        var provider = services.BuildServiceProvider();

        // Verify the correct tool loader is registered
        Assert.NotNull(provider.GetService<IToolLoader>());
        Assert.IsType<CompositeToolLoader>(provider.GetService<IToolLoader>());
    }

    [Fact]
    public void AddAzureMcpServer_WithStdioTransport_ConfiguresStdioTransport()
    {
        // Arrange
        var services = SetupBaseServices();
        var options = new ServiceStartOptions
        {
            Transport = StdioTransport,
            // Define proxy as "single" to prevent CompositeDiscoveryStrategy error
            Mode = "single"
        };

        // Act
        services.AddAzureMcpServer(options);

        // Assert
        // Build the provider to verify that service registration succeeded without exceptions
        var provider = services.BuildServiceProvider();

        // Check that appropriate registration was completed
        Assert.NotNull(provider.GetService<IMcpRuntime>());

        // Verify that the service collection contains an IMcpServer registration
        Assert.Contains(services, sd => sd.ServiceType == typeof(IMcpServer));
    }

    [Fact]
    public void AddAzureMcpServer_ConfiguresMcpServerOptions()
    {
        // Arrange
        var services = SetupBaseServices();
        var options = new ServiceStartOptions
        {
            Transport = StdioTransport
        };

        // Act
        services.AddAzureMcpServer(options);

        // Assert
        var provider = services.BuildServiceProvider();
        var mcpServerOptions = provider.GetService<IOptions<McpServerOptions>>()?.Value;

        // Verify server options are configured
        Assert.NotNull(mcpServerOptions);
        Assert.Equal("2024-11-05", mcpServerOptions.ProtocolVersion);
        Assert.NotNull(mcpServerOptions.ServerInfo);
        Assert.NotNull(mcpServerOptions.Capabilities);
        Assert.NotNull(mcpServerOptions.Capabilities.Tools);
    }

    [Fact]
    public void AddAzureMcpServer_RegistersOptionsWithSameInstance()
    {
        // Arrange
        var services = SetupBaseServices();
        var options = new ServiceStartOptions
        {
            Transport = StdioTransport,
            ReadOnly = true
        };

        // Act
        services.AddAzureMcpServer(options);

        // Assert
        var provider = services.BuildServiceProvider();
        var registeredOptions = provider.GetService<ServiceStartOptions>();
        var wrappedOptions = provider.GetService<IOptions<ServiceStartOptions>>()?.Value;

        // Verify both registrations point to the same instance
        Assert.NotNull(registeredOptions);
        Assert.NotNull(wrappedOptions);
        Assert.Same(options, registeredOptions);
        Assert.Same(options, wrappedOptions);
        Assert.True(registeredOptions.ReadOnly);
    }

    [Fact]
    public void AddAzureMcpServer_WithReadOnlyOption_RegistersOption()
    {
        // Arrange
        var services = SetupBaseServices();
        var options = new ServiceStartOptions
        {
            Transport = StdioTransport,
            ReadOnly = true
        };

        // Act
        services.AddAzureMcpServer(options);

        // Assert
        var provider = services.BuildServiceProvider();
        var registeredOptions = provider.GetService<ServiceStartOptions>();

        Assert.NotNull(registeredOptions);
        Assert.True(registeredOptions.ReadOnly);

        // Verify the option is also available as IOptions<ServiceStartOptions>
        var optionsMonitor = provider.GetService<IOptions<ServiceStartOptions>>();
        Assert.NotNull(optionsMonitor);
        Assert.True(optionsMonitor.Value.ReadOnly);
    }

    [Fact]
    public void AddAzureMcpServer_WithSpecificServiceAreas_RegistersCompositeToolLoader()
    {
        // Arrange
        var services = SetupBaseServices();
        var options = new ServiceStartOptions
        {
            Transport = StdioTransport,
            Namespace = ["keyvault", "storage"]
        };

        // Act
        services.AddAzureMcpServer(options);

        // Assert
        var provider = services.BuildServiceProvider();

        // Verify the correct tool loader is registered
        Assert.NotNull(provider.GetService<IToolLoader>());
        Assert.IsType<CompositeToolLoader>(provider.GetService<IToolLoader>());

        // Verify the presence of CommandFactoryToolLoader which is used for service areas
        Assert.NotNull(provider.GetService<CommandFactoryToolLoader>());

        // Verify runtime
        Assert.NotNull(provider.GetService<IMcpRuntime>());
        Assert.IsType<McpRuntime>(provider.GetService<IMcpRuntime>());
    }

    [Theory]
    [InlineData("keyvault")]
    [InlineData("storage")]
    [InlineData("servicebus")]
    public void AddAzureMcpServer_WithSingleServiceArea_RegistersAppropriateServices(string serviceArea)
    {
        // Arrange
        var services = SetupBaseServices();
        var options = new ServiceStartOptions
        {
            Transport = StdioTransport,
            Namespace = [serviceArea]
        };

        // Act
        services.AddAzureMcpServer(options);

        // Assert
        var provider = services.BuildServiceProvider();

        // Verify the tool loader is registered correctly
        Assert.NotNull(provider.GetService<IToolLoader>());
        Assert.IsType<CompositeToolLoader>(provider.GetService<IToolLoader>());

        // Verify runtime
        Assert.NotNull(provider.GetService<IMcpRuntime>());
        Assert.IsType<McpRuntime>(provider.GetService<IMcpRuntime>());
    }

    [Fact]
    public void AddAzureMcpServer_WithInvalidProxyMode_UsesDefaultLoader()
    {
        // Arrange
        var services = SetupBaseServices();
        var options = new ServiceStartOptions
        {
            Transport = StdioTransport,
            Mode = "invalid-mode"
        };

        // Act
        services.AddAzureMcpServer(options);

        // Assert
        var provider = services.BuildServiceProvider();

        Assert.Null(provider.GetService<IToolLoader>());
        Assert.Null(provider.GetService<IMcpDiscoveryStrategy>());
    }

    [Fact]
    public void AddAzureMcpServer_WithNullProxy_UsesDefaultLoader()
    {
        // Arrange
        var services = SetupBaseServices();
        var options = new ServiceStartOptions
        {
            Transport = StdioTransport,
            Mode = null
        };

        // Act
        services.AddAzureMcpServer(options);

        // Assert
        var provider = services.BuildServiceProvider();

        Assert.Null(provider.GetService<IToolLoader>());
        Assert.Null(provider.GetService<IMcpDiscoveryStrategy>());
    }

    [Fact]
    public void AddAzureMcpServer_WithAllMode_RegistersCompositeToolLoader()
    {
        // Arrange
        var services = SetupBaseServices();
        var options = new ServiceStartOptions
        {
            Transport = StdioTransport,
            Mode = "all"
        };

        // Act
        services.AddAzureMcpServer(options);

        // Assert
        var provider = services.BuildServiceProvider();

        // Verify the correct tool loader is registered (CompositeToolLoader for all tools mode)
        Assert.NotNull(provider.GetService<IToolLoader>());
        Assert.IsType<CompositeToolLoader>(provider.GetService<IToolLoader>());

        // Verify discovery strategy is registered for individual tools
        Assert.NotNull(provider.GetService<IMcpDiscoveryStrategy>());
        Assert.IsType<RegistryDiscoveryStrategy>(provider.GetService<IMcpDiscoveryStrategy>());
    }

    [Fact]
    public void AddAzureMcpServer_ConfiguresServerInstructions()
    {
        // Arrange
        var services = SetupBaseServices();
        var options = new ServiceStartOptions
        {
            Transport = StdioTransport
        };

        // Act
        services.AddAzureMcpServer(options);

        // Assert
        var provider = services.BuildServiceProvider();
        var mcpServerOptions = provider.GetService<IOptions<McpServerOptions>>()?.Value;

        // Verify server instructions are configured
        Assert.NotNull(mcpServerOptions);
        Assert.NotNull(mcpServerOptions.ServerInstructions);
        Assert.NotEmpty(mcpServerOptions.ServerInstructions);

        // Output the actual content for debugging
        var instructions = mcpServerOptions.ServerInstructions;

        // Verify the instructions contain expected sections
        Assert.Contains("Azure MCP server usage rules:", instructions);
        Assert.Contains("Use Azure Code Gen Best Practices:", instructions);
        Assert.Contains("Use Azure SWA Best Practices:", instructions);
    }
}
