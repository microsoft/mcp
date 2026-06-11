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
    /// <param name="serviceStartOptions">The options for configuring the server.</param>
    /// <returns>The service collection with MCP server services added.</returns>
    public static IServiceCollection AddAzureMcpServer(this IServiceCollection services, ServiceStartOptions serviceStartOptions)
    {
        // Register HTTP client services
        services.AddHttpClientServices();

        // Register options for service start
        services.AddSingleton(serviceStartOptions);
        services.AddSingleton(Options.Create(serviceStartOptions));

        // Register default tool loader options from service start options
        var defaultToolLoaderOptions = new ToolLoaderOptions
        {
            Namespace = serviceStartOptions.Namespace,
            ReadOnly = serviceStartOptions.ReadOnly ?? false,
            DangerouslyDisableElicitation = serviceStartOptions.DangerouslyDisableElicitation,
            Tool = serviceStartOptions.Tool,
            IsHttpMode = serviceStartOptions.IsHttpMode
        };

        if (serviceStartOptions.Mode == ModeTypes.NamespaceProxy)
        {
            if (defaultToolLoaderOptions.Namespace == null || defaultToolLoaderOptions.Namespace.Length == 0)
            {
                defaultToolLoaderOptions = defaultToolLoaderOptions with { Namespace = ["extension"] };
            }
        }

        services.AddSingleton(defaultToolLoaderOptions);
        services.AddSingleton(Options.Create(defaultToolLoaderOptions));

        // Register tool loader strategies
        services.AddSingleton<CommandFactoryToolLoader>();
        services.AddSingleton<RegistryToolLoader>();

        services.AddSingleton<SingleProxyToolLoader>();
        services.AddSingleton<CompositeToolLoader>();
        services.AddSingleton<ServerToolLoader>();
        services.AddSingleton<NamespaceToolLoader>();

        // Register server discovery strategies
        services.AddSingleton<CommandGroupDiscoveryStrategy>();
        services.AddSingleton<CompositeDiscoveryStrategy>();
        services.AddSingleton<RegistryDiscoveryStrategy>();
        services.AddSingleton<ConsolidatedToolDiscoveryStrategy>();

        // Register MCP runtimes
        services.AddSingleton<IMcpRuntime, McpRuntime>();

        // Register MCP discovery strategies based on proxy mode
        if (serviceStartOptions.Mode == ModeTypes.SingleToolProxy)
        {
            services.AddSingleton<IMcpDiscoveryStrategy>(sp =>
            {
                var discoveryStrategies = new List<IMcpDiscoveryStrategy>
                {
                    sp.GetRequiredService<RegistryDiscoveryStrategy>(),
                    sp.GetRequiredService<CommandGroupDiscoveryStrategy>(),
                };

                var logger = sp.GetRequiredService<ILogger<CompositeDiscoveryStrategy>>();
                return new CompositeDiscoveryStrategy(discoveryStrategies, logger);
            });
        }
        else if (serviceStartOptions.Mode == ModeTypes.NamespaceProxy)
        {
            services.AddSingleton<IMcpDiscoveryStrategy, RegistryDiscoveryStrategy>();
        }
        else if (serviceStartOptions.Mode == ModeTypes.ConsolidatedProxy)
        {
            services.AddSingleton<IMcpDiscoveryStrategy>(sp =>
            {
                var discoveryStrategies = new List<IMcpDiscoveryStrategy>
                {
                    sp.GetRequiredService<RegistryDiscoveryStrategy>(),
                    sp.GetRequiredService<ConsolidatedToolDiscoveryStrategy>(),
                };

                var logger = sp.GetRequiredService<ILogger<CompositeDiscoveryStrategy>>();
                return new CompositeDiscoveryStrategy(discoveryStrategies, logger);
            });
        }

        // Configure tool loading based on mode
        if (serviceStartOptions.Mode == ModeTypes.SingleToolProxy)
        {
            services.AddSingleton<IToolLoader, SingleProxyToolLoader>();
        }
        else if (serviceStartOptions.Mode == ModeTypes.NamespaceProxy)
        {
            services.AddSingleton<IToolLoader>(sp =>
            {
                var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
                var toolLoaders = new List<IToolLoader>
                {
                    // ServerToolLoader with RegistryDiscoveryStrategy creates proxy tools for external MCP servers.
                    new ServerToolLoader(
                        sp.GetRequiredService<RegistryDiscoveryStrategy>(),
                        sp.GetRequiredService<IOptions<ToolLoaderOptions>>(),
                        loggerFactory.CreateLogger<ServerToolLoader>()
                    ),
                    // NamespaceToolLoader enables direct in-process execution for tools in Azure namespaces
                    sp.GetRequiredService<NamespaceToolLoader>(),
                };

                // Always add utility commands (subscription, group) in namespace mode
                // so they are available regardless of which namespaces are loaded
                var utilityToolLoaderOptions = new ToolLoaderOptions(
                    Namespace: DiscoveryConstants.UtilityNamespaces,
                    ReadOnly: defaultToolLoaderOptions.ReadOnly,
                    DangerouslyDisableElicitation: defaultToolLoaderOptions.DangerouslyDisableElicitation,
                    Tool: defaultToolLoaderOptions.Tool,
                    IsHttpMode: defaultToolLoaderOptions.IsHttpMode
                );

                toolLoaders.Add(new CommandFactoryToolLoader(
                    sp,
                    sp.GetRequiredService<ICommandFactory>(),
                    Options.Create(utilityToolLoaderOptions),
                    loggerFactory.CreateLogger<CommandFactoryToolLoader>()
                ));

                // Append extension commands when no other namespaces are specified.
                if (defaultToolLoaderOptions.Namespace?.SequenceEqual(["extension"]) == true)
                {
                    toolLoaders.Add(sp.GetRequiredService<CommandFactoryToolLoader>());
                }

                return new CompositeToolLoader(toolLoaders, loggerFactory.CreateLogger<CompositeToolLoader>());
            });
        }
        else if (serviceStartOptions.Mode == ModeTypes.ConsolidatedProxy)
        {
            services.AddSingleton<IToolLoader>(sp =>
            {
                var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
                var consolidatedStrategy = sp.GetRequiredService<ConsolidatedToolDiscoveryStrategy>();

                // Create a new CommandFactory with consolidated command groups
                var consolidatedCommandFactory = consolidatedStrategy.CreateConsolidatedCommandFactory();

                var toolLoaders = new List<IToolLoader>
                {
                    // ServerToolLoader with RegistryDiscoveryStrategy creates proxy tools for external MCP servers.
                    new ServerToolLoader(
                        sp.GetRequiredService<RegistryDiscoveryStrategy>(),
                        sp.GetRequiredService<IOptions<ToolLoaderOptions>>(),
                        loggerFactory.CreateLogger<ServerToolLoader>()
                    ),
                    // NamespaceToolLoader enables direct in-process execution for consolidated tools
                    new NamespaceToolLoader(
                        consolidatedCommandFactory,
                        sp.GetRequiredService<IOptions<ServiceStartOptions>>(),
                        sp,
                        loggerFactory.CreateLogger<NamespaceToolLoader>(),
                        false
                    ),
                };

                return new CompositeToolLoader(toolLoaders, loggerFactory.CreateLogger<CompositeToolLoader>());
            });
        }
        else if (serviceStartOptions.Mode == ModeTypes.All)
        {
            services.AddSingleton<IMcpDiscoveryStrategy, RegistryDiscoveryStrategy>();
            services.AddSingleton<IToolLoader>(sp =>
            {
                var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
                var toolLoaders = new List<IToolLoader>
                {
                    sp.GetRequiredService<RegistryToolLoader>(),
                    sp.GetRequiredService<CommandFactoryToolLoader>(),
                };

                return new CompositeToolLoader(toolLoaders, loggerFactory.CreateLogger<CompositeToolLoader>());
            });
        }

        var mcpServerOptions = services
            .AddOptions<McpServerOptions>()
            .Configure<IMcpRuntime, IServerInstructionsProvider, IOptions<McpServerConfiguration>>((mcpServerOptions, mcpRuntime, serverInstructionsProvider, serverConfiguration) =>
            {
                var configuration = serverConfiguration.Value;

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

        if (serviceStartOptions.Transport == TransportTypes.Http)
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
            .Configure<IConfiguration, IOptions<ServiceStartOptions>>((options, rootConfiguration, serviceStartOptions) =>
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
                if (!string.IsNullOrWhiteSpace(serviceStartOptions.Value.SupportLoggingFolder))
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
