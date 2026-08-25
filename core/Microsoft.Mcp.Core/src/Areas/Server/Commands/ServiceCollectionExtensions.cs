// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Mcp.Core.Areas.Server.Commands.Discovery;
using Microsoft.Mcp.Core.Areas.Server.Commands.Runtime;
using Microsoft.Mcp.Core.Areas.Server.Commands.ServerInstructions;
using Microsoft.Mcp.Core.Areas.Server.Commands.ToolLoading;
using Microsoft.Mcp.Core.Areas.Server.Options;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Configuration;
using Microsoft.Mcp.Core.Extensions;
using Microsoft.Mcp.Core.Helpers;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;

namespace Microsoft.Mcp.Core.Areas.Server.Commands;

// This is intentionally placed after the namespace declaration to avoid
// conflicts with Microsoft.Mcp.Core.Areas.Server.Options
using Options = Microsoft.Extensions.Options.Options;

/// <summary>
/// Extension methods for configuring Azure MCP server services.
/// </summary>
public static partial class ServiceCollectionExtensions
{
    [GeneratedRegex("^[A-Za-z0-9_-]+$")]
    private static partial Regex ShortNamePattern();

    /// <summary>
    /// Adds the Azure MCP server services to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <param name="serverStartOptions">The options for configuring the server.</param>
    /// <returns>The service collection with MCP server services added.</returns>
    public static IServiceCollection AddAzureMcpServer(this IServiceCollection services, ServerStartOptions serverStartOptions)
    {
        // Register HTTP client services
        services.AddHttpClientServices();

        // Register ServerRuntimeConfiguration
        var serverRuntimeConfiguration = new ServerRuntimeConfiguration()
        {
            Transport = serverStartOptions.Transport,
            Mode = serverStartOptions.Mode ?? ModeTypes.Default,
            Namespace = serverStartOptions.Namespace,
            Tool = serverStartOptions.Tool,
            ThreeStepToolDiscovery = serverStartOptions.ThreeStepToolDiscovery,
            ThreeStepToolDiscoveryThresholdBytes = serverStartOptions.ThreeStepToolDiscoveryThresholdBytes,
            DisableAutomaticThreeStepToolDiscovery = serverStartOptions.DisableAutomaticThreeStepToolDiscovery,
            ReadOnly = serverStartOptions.ReadOnly ?? false,
            DangerouslyDisableElicitation = serverStartOptions.DangerouslyDisableElicitation,
            Cloud = serverStartOptions.Cloud
        };

        services.AddSingleton(serverRuntimeConfiguration);
        services.AddSingleton(Options.Create(serverRuntimeConfiguration));

        // Register dependency injected tool loaders and discovery strategies.
        // Always need CommandFactoryToolLoader as it loads tools defined in the microsoft/mcp project.
        services.AddSingleton<CommandFactoryToolLoader>();
        if (!serverStartOptions.DisableProxyTools)
        {
            // Conditionally add RegistryDiscoveryStrategy as they load proxied tools.
            services.AddSingleton<RegistryDiscoveryStrategy>();
        }

        if (serverStartOptions.Mode == ModeTypes.SingleToolProxy)
        {
            // Server is configured with '--mode single', configure for single mode.
            services.AddSingleton<IToolLoader, SingleProxyToolLoader>();
            // SingleToolProxy mode requires CommandGroupDiscoveryStrategy to discover tools in the microsoft/mcp project.
            services.AddSingleton<CommandGroupDiscoveryStrategy>();
            services.AddSingleton<IMcpDiscoveryStrategy>(sp =>
            {
                var discoveryStrategies = new List<IMcpDiscoveryStrategy>();
                if (!serverStartOptions.DisableProxyTools)
                {
                    discoveryStrategies.Add(sp.GetRequiredService<RegistryDiscoveryStrategy>());
                }

                discoveryStrategies.Add(sp.GetRequiredService<CommandGroupDiscoveryStrategy>());

                var logger = sp.GetRequiredService<ILogger<CompositeDiscoveryStrategy>>();
                return new CompositeDiscoveryStrategy(discoveryStrategies, logger);
            });
        }
        else if (serverStartOptions.Mode == ModeTypes.NamespaceProxy)
        {
            // Server is configured with either '--mode namespace' or no mode at all, configure for namespace mode.
            services.AddSingleton<NamespaceToolLoader>();
            services.AddSingleton<IToolLoader>(sp =>
            {
                var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
                var toolLoaders = new List<IToolLoader>();
                if (!serverStartOptions.DisableProxyTools)
                {
                    // If proxy tools are enabled, ServerToolLoader with RegistryDiscoveryStrategy creates proxy tools for external MCP servers.
                    toolLoaders.Add(new ServerToolLoader(
                        sp.GetRequiredService<RegistryDiscoveryStrategy>(),
                        sp.GetRequiredService<IOptions<ServerRuntimeConfiguration>>(),
                        loggerFactory.CreateLogger<ServerToolLoader>()));
                }

                // NamespaceToolLoader enables direct in-process execution for tools in Azure namespaces
                toolLoaders.Add(sp.GetRequiredService<NamespaceToolLoader>());

                // Always add utility commands (subscription, group) in namespace mode
                // so they are available regardless of which namespaces are loaded
                var additionalIncludedTools = new List<string>(DiscoveryConstants.UtilityNamespaces);

                // Append extension commands when no other namespaces are specified.
                // Extension commands aren't included in the NamespaceToolLoader.
                if (serverStartOptions.Namespace == null || serverStartOptions.Namespace.Length == 0)
                {
                    additionalIncludedTools.Add("extension");
                }

                var additionalToolsServerRuntimeConfiguration = new ServerRuntimeConfiguration
                {
                    Namespace = [.. additionalIncludedTools],
                    ReadOnly = serverRuntimeConfiguration.ReadOnly,
                    DangerouslyDisableElicitation = serverRuntimeConfiguration.DangerouslyDisableElicitation,
                    Tool = serverRuntimeConfiguration.Tool,
                    Transport = serverRuntimeConfiguration.Transport,
                    Mode = serverRuntimeConfiguration.Mode,
                    Cloud = serverRuntimeConfiguration.Cloud
                };

                toolLoaders.Add(new CommandFactoryToolLoader(
                    sp.GetRequiredService<ICommandFactory>(),
                    Options.Create(additionalToolsServerRuntimeConfiguration),
                    loggerFactory.CreateLogger<CommandFactoryToolLoader>()));

                return new CompositeToolLoader(toolLoaders, loggerFactory.CreateLogger<CompositeToolLoader>());
            });
        }
        else if (serverStartOptions.Mode == ModeTypes.ConsolidatedProxy)
        {
            // Server is configured with '--mode consolidated', configure for consolidated mode.
            services.AddSingleton<ConsolidatedToolDiscoveryStrategy>();
            services.AddSingleton<IMcpDiscoveryStrategy>(sp =>
            {
                var discoveryStrategies = new List<IMcpDiscoveryStrategy>();
                if (!serverStartOptions.DisableProxyTools)
                {
                    discoveryStrategies.Add(sp.GetRequiredService<RegistryDiscoveryStrategy>());
                }

                discoveryStrategies.Add(sp.GetRequiredService<ConsolidatedToolDiscoveryStrategy>());

                var logger = sp.GetRequiredService<ILogger<CompositeDiscoveryStrategy>>();
                return new CompositeDiscoveryStrategy(discoveryStrategies, logger);
            });
            services.AddSingleton<IToolLoader>(sp =>
            {
                var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
                var consolidatedStrategy = sp.GetRequiredService<ConsolidatedToolDiscoveryStrategy>();

                // Create a new CommandFactory with consolidated command groups
                var consolidatedCommandFactory = consolidatedStrategy.CreateConsolidatedCommandFactory();

                var toolLoaders = new List<IToolLoader>();
                if (!serverStartOptions.DisableProxyTools)
                {
                    // If proxy tools are enabled, ServerToolLoader with RegistryDiscoveryStrategy creates proxy tools for external MCP servers.
                    toolLoaders.Add(new ServerToolLoader(
                        sp.GetRequiredService<RegistryDiscoveryStrategy>(),
                        sp.GetRequiredService<IOptions<ServerRuntimeConfiguration>>(),
                        loggerFactory.CreateLogger<ServerToolLoader>()));
                }

                // NamespaceToolLoader enables direct in-process execution for consolidated tools
                toolLoaders.Add(new NamespaceToolLoader(
                    consolidatedCommandFactory,
                    sp.GetRequiredService<IOptions<ServerRuntimeConfiguration>>(),
                    loggerFactory.CreateLogger<NamespaceToolLoader>(),
                    false));

                return new CompositeToolLoader(toolLoaders, loggerFactory.CreateLogger<CompositeToolLoader>());
            });
        }
        else if (serverStartOptions.Mode == ModeTypes.All)
        {
            // Server is configured with '--mode all', configure for all mode.
            if (!serverStartOptions.DisableProxyTools)
            {
                services.AddSingleton<RegistryToolLoader>();
                services.AddSingleton<IMcpDiscoveryStrategy, RegistryDiscoveryStrategy>();
            }
            services.AddSingleton<IToolLoader>(sp =>
            {
                var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
                var toolLoaders = new List<IToolLoader>();
                if (!serverStartOptions.DisableProxyTools)
                {
                    toolLoaders.Add(sp.GetRequiredService<RegistryToolLoader>());
                }

                toolLoaders.Add(sp.GetRequiredService<CommandFactoryToolLoader>());

                return new CompositeToolLoader(toolLoaders, loggerFactory.CreateLogger<CompositeToolLoader>());
            });
        }

        // Register MCP runtimes
        services.AddSingleton<IMcpRuntime, McpRuntime>();

        var mcpServerOptions = services
            .AddOptions<McpServerOptions>()
            .Configure<IMcpRuntime, IServerInstructionsProvider, IOptions<McpServerConfiguration>>((mcpServerOptions, mcpRuntime, serverInstructionsProvider, serverConfiguration) =>
            {
                var configuration = serverConfiguration.Value;

                // Keep server identity/instructions as startup-owned metadata.
                // Runtime capability discovery remains request-driven through MCP handlers
                // (for example server/discover and tools/list) on the stateless protocol path.
                mcpServerOptions.ServerInfo = new Implementation
                {
                    Name = configuration.DisplayName,
                    Version = configuration.Version,
                };

                mcpServerOptions.Handlers = new()
                {
                    CallToolHandler = mcpRuntime.CallToolHandler,
                    ListToolsHandler = mcpRuntime.ListToolsHandler,
                };

                // Add instructions for the server
                mcpServerOptions.ServerInstructions = serverInstructionsProvider.GetServerInstructions();
            });

        var mcpServerBuilder = services.AddMcpServer();

        if (serverStartOptions.Transport == TransportTypes.Http)
        {
            mcpServerBuilder.WithHttpTransport();
        }
        else
        {
            mcpServerBuilder.WithStdioServerTransport();
        }

        return services;
    }

    /// <summary>
    /// Using <see cref="IConfiguration"/> configures <see cref="McpServerConfiguration"/>.
    /// </summary>
    /// <param name="services">Service Collection to add configuration logic to.</param>
    /// <param name="assembly">The assembly to use for configuration.</param>
    public static void InitializeConfigurationAndOptions(this IServiceCollection services, Assembly assembly)
    {
        services.AddSingleton(GetConfiguration());

        services.AddOptions<McpServerConfiguration>()
            .Configure<IConfiguration, IOptions<ServerStartOptions>>((options, rootConfiguration, serverStartOptions) =>
            {
                // Use a scoped IConfiguration for loading server settings.
                var scopedConfiguration = GetConfiguration(assembly);

                // Manually bind configuration values to avoid reflection-based binding for AOT compatibility
                var mcpConfiguration = scopedConfiguration.GetRequiredSection("MicrosoftMcp");
                options.RootCommandGroupName = mcpConfiguration[nameof(McpServerConfiguration.RootCommandGroupName)]
                    ?? throw new InvalidOperationException($"Configuration value '{nameof(McpServerConfiguration.RootCommandGroupName)}' is required.");
                options.Name = mcpConfiguration[nameof(McpServerConfiguration.Name)]
                    ?? throw new InvalidOperationException($"Configuration value '{nameof(McpServerConfiguration.Name)}' is required.");
                options.DisplayName = mcpConfiguration[nameof(McpServerConfiguration.DisplayName)]
                    ?? throw new InvalidOperationException($"Configuration value '{nameof(McpServerConfiguration.DisplayName)}' is required.");

                options.ShortName = mcpConfiguration[nameof(McpServerConfiguration.ShortName)]
                    ?? throw new InvalidOperationException($"Configuration value '{nameof(McpServerConfiguration.ShortName)}' is required.");
                options.ShortName = options.ShortName.Trim();
                if (!ShortNamePattern().IsMatch(options.ShortName))
                {
                    throw new InvalidOperationException(
                        $"Configuration value '{nameof(McpServerConfiguration.ShortName)}' must contain only letters, digits, '_', or '-'.");
                }

                options.Description = mcpConfiguration[nameof(McpServerConfiguration.Description)]
                    ?? throw new InvalidOperationException($"Configuration value '{nameof(McpServerConfiguration.Description)}' is required.");
                if (string.IsNullOrWhiteSpace(options.Description))
                {
                    throw new InvalidOperationException(
                        $"Configuration value '{nameof(McpServerConfiguration.Description)}' must not be empty or whitespace.");
                }

                // Assembly.GetEntryAssembly is used to retrieve the version of the server application as that is
                // the assembly that will run the tool calls.
                var entryAssembly = Assembly.GetEntryAssembly()
                    ?? throw new InvalidOperationException("Entry assembly must be a managed assembly.");

                options.Version = AssemblyHelper.GetAssemblyVersion(entryAssembly);

                // Disable telemetry when support logging is enabled to prevent sensitive data from being sent
                // to telemetry endpoints. Support logging captures debug-level information that may contain
                // sensitive data, so we disable all telemetry as a safety measure.
                if (!string.IsNullOrWhiteSpace(serverStartOptions.Value.DangerouslyWriteSupportLogsToDir))
                {
                    options.IsTelemetryEnabled = false;
                    return;
                }

                // This environment variable can be used to disable telemetry collection entirely. This takes precedence
                // over any other settings.
                options.IsTelemetryEnabled = rootConfiguration.GetValue("AZURE_MCP_COLLECT_TELEMETRY", true);
            });
    }

    /// <summary>
    /// Creates an IConfiguration instance based on the use case.
    /// <para>
    /// When the assembly is null, the configuration is loaded from the file system. This is for runtime settings.
    /// When the assembly is not null, the configuration is loaded from embedded resources. This is for server information settings.
    /// </para>
    /// </summary>
    /// <param name="assembly">An assembly to load embedded server information settings from.</param>
    /// <returns>An IConfiguration instance.</returns>
    private static IConfiguration GetConfiguration(Assembly? assembly = null)
    {
        var environment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Production";
        var configurationBuilder = new ConfigurationBuilder().SetBasePath(AppContext.BaseDirectory);

        if (assembly == null)
        {
            // assembly was null, loading runtime settings. Everything is optional and loaded from the file system.
            configurationBuilder.AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile($"appsettings.{environment}.json", optional: true)
                .AddEnvironmentVariables();
        }
        else
        {
            // assembly was not null, loading server information settings. These are embedded in the assembly.
            configurationBuilder.AddEmbeddedAppSettings(assembly, "appsettings.json", required: true)
                .AddEmbeddedAppSettings(assembly, $"appsettings.{environment}.json", required: false);
        }

        return configurationBuilder.Build();
    }
}
