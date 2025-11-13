// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine.Parsing;
using System.Net;
using Azure.Mcp.Core.Areas.Server.Models;
using Azure.Mcp.Core.Areas.Server.Options;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Helpers;
using Azure.Mcp.Core.Services.Azure.Authentication;
using Azure.Mcp.Core.Services.Caching;
using Azure.Mcp.Core.Services.Telemetry;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Web;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using static Azure.Mcp.Core.Services.Telemetry.TelemetryConstants;

namespace Azure.Mcp.Core.Areas.Server.Commands;

/// <summary>
/// Command to start the MCP server with specified configuration options.
/// This command is hidden from the main command list.
/// </summary>
[HiddenCommand]
public sealed class ServiceStartCommand : BaseCommand<ServiceStartOptions>
{
    private const string CommandTitle = "Start MCP Server";

    /// <summary>
    /// Gets the name of the command.
    /// </summary>
    public override string Name => "start";

    /// <summary>
    /// Gets the description of the command.
    /// </summary>
    public override string Description => "Starts Azure MCP Server.";

    /// <summary>
    /// Gets the title of the command.
    /// </summary>
    public override string Title => CommandTitle;

    /// <summary>
    /// Gets the metadata for this command.
    /// </summary>
    public override ToolMetadata Metadata => new() { Destructive = false, ReadOnly = true };

    public static Action<IServiceCollection> ConfigureServices { get; set; } = _ => { };

    public static Func<IServiceProvider, Task> InitializeServicesAsync { get; set; } = _ => Task.CompletedTask;

    public override string Id => "9953ff62-e3d7-4bdf-9b70-d569e54e3df1";

    /// <summary>
    /// Registers command options for the service start command.
    /// </summary>
    /// <param name="command">The command to register options with.</param>
    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(ServiceOptionDefinitions.Transport);
        command.Options.Add(ServiceOptionDefinitions.Namespace);
        command.Options.Add(ServiceOptionDefinitions.Mode);
        command.Options.Add(ServiceOptionDefinitions.Tool);
        command.Options.Add(ServiceOptionDefinitions.ReadOnly);
        command.Options.Add(ServiceOptionDefinitions.Debug);
        command.Options.Add(ServiceOptionDefinitions.DangerouslyDisableHttpIncomingAuth);
        command.Options.Add(ServiceOptionDefinitions.InsecureDisableElicitation);
        command.Options.Add(ServiceOptionDefinitions.OutgoingAuthStrategy);
        command.Validators.Add(commandResult =>
        {
            string transport = ResolveTransport(commandResult);
            bool httpIncomingAuthDisabled = commandResult.GetValueOrDefault<bool>(ServiceOptionDefinitions.DangerouslyDisableHttpIncomingAuth);
            ValidateMode(commandResult.GetValueOrDefault(ServiceOptionDefinitions.Mode), commandResult);
            ValidateTransportConfiguration(transport, httpIncomingAuthDisabled, commandResult);
            ValidateNamespaceAndToolMutualExclusion(
                commandResult.GetValueOrDefault<string[]?>(ServiceOptionDefinitions.Namespace.Name),
                commandResult.GetValueOrDefault<string[]?>(ServiceOptionDefinitions.Tool.Name),
                commandResult);
            ValidateOutgoingAuthStrategy(commandResult);
        });
    }

    /// <summary>
    /// Binds the parsed command line arguments to the ServiceStartOptions object.
    /// </summary>
    /// <param name="parseResult">The parsed command line arguments.</param>
    /// <returns>A configured ServiceStartOptions instance.</returns>
    protected override ServiceStartOptions BindOptions(ParseResult parseResult)
    {
        var mode = parseResult.GetValueOrDefault<string?>(ServiceOptionDefinitions.Mode.Name);
        var tools = parseResult.GetValueOrDefault<string[]?>(ServiceOptionDefinitions.Tool.Name);

        // When --tool switch is used, automatically change the mode to "all"
        if (tools != null && tools.Length > 0)
        {
            mode = ModeTypes.All;
        }

        var outgoingAuthStrategy = ResolveAuthStrategy(parseResult);

        var options = new ServiceStartOptions
        {
            Transport = ResolveTransport(parseResult),
            Namespace = parseResult.GetValueOrDefault<string[]?>(ServiceOptionDefinitions.Namespace.Name),
            Mode = mode,
            Tool = tools,
            ReadOnly = parseResult.GetValueOrDefault<bool?>(ServiceOptionDefinitions.ReadOnly.Name),
            Debug = parseResult.GetValueOrDefault<bool>(ServiceOptionDefinitions.Debug.Name),
            DangerouslyDisableHttpIncomingAuth = parseResult.GetValueOrDefault<bool>(ServiceOptionDefinitions.DangerouslyDisableHttpIncomingAuth.Name),
            InsecureDisableElicitation = parseResult.GetValueOrDefault<bool>(ServiceOptionDefinitions.InsecureDisableElicitation.Name),
            OutgoingAuthStrategy = outgoingAuthStrategy
        };
        return options;
    }

    /// <summary>
    /// Executes the service start command, creating and starting the MCP server.
    /// </summary>
    /// <param name="context">The command execution context.</param>
    /// <param name="parseResult">The parsed command options.</param>
    /// <returns>A command response indicating the result of the operation.</returns>
    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult, CancellationToken cancellationToken)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
        {
            return context.Response;
        }

        var options = BindOptions(parseResult);

        try
        {
            using var host = CreateHost(options);

            await InitializeServicesAsync(host.Services);

            await host.StartAsync(cancellationToken);

            var telemetryService = host.Services.GetRequiredService<ITelemetryService>();
            LogStartTelemetry(telemetryService, options);

            await host.WaitForShutdownAsync(cancellationToken);

            return context.Response;
        }
        catch (Exception ex)
        {
            HandleException(context, ex);
            return context.Response;
        }
    }

    internal static void LogStartTelemetry(ITelemetryService telemetryService, ServiceStartOptions options)
    {
        using var activity = telemetryService.StartActivity(ActivityName.ServerStarted);

        if (activity != null)
        {
            activity.SetTag(TagName.Transport, options.Transport);
            activity.SetTag(TagName.ServerMode, options.Mode);
            activity.SetTag(TagName.IsReadOnly, options.ReadOnly);
            activity.SetTag(TagName.InsecureDisableElicitation, options.InsecureDisableElicitation);
            activity.SetTag(TagName.DangerouslyDisableHttpIncomingAuth, options.DangerouslyDisableHttpIncomingAuth);
            activity.SetTag(TagName.IsDebug, options.Debug);

            if (options.Namespace != null && options.Namespace.Length > 0)
            {
                activity.SetTag(TagName.Namespace, string.Join(",", options.Namespace));
            }
            if (options.Tool != null && options.Tool.Length > 0)
            {
                activity.SetTag(TagName.Tool, string.Join(",", options.Tool));
            }
        }
    }

    /// <summary>
    /// Validates if the provided mode is a valid mode type.
    /// </summary>
    /// <param name="mode">The mode to validate.</param>
    /// <param name="commandResult">Command result to update on failure.</param>
    private static void ValidateMode(string? mode, CommandResult commandResult)
    {
        if (mode == ModeTypes.SingleToolProxy ||
            mode == ModeTypes.NamespaceProxy ||
            mode == ModeTypes.All ||
            mode == ModeTypes.ConsolidatedProxy)
        {
            return; // Success
        }

        commandResult.AddError($"Invalid mode '{mode}'. Valid modes are: {ModeTypes.SingleToolProxy}, {ModeTypes.NamespaceProxy}, {ModeTypes.All}, {ModeTypes.ConsolidatedProxy}.");
    }

    /// <summary>
    /// Validates the transport configuration, ensuring the transport type is valid and compatible with other options.
    /// Verifies that HTTP transport is only used when available (ENABLE_HTTP), and that --dangerously-disable-http-incoming-auth
    /// is only specified with HTTP transport.
    /// </summary>
    /// <param name="transport">The transport to validate.</param>
    /// <param name="httpIncomingAuthDisabled">Whether HTTP incoming authentication is disabled.</param>
    /// <param name="commandResult">Command result to update on failure.</param>
    private static void ValidateTransportConfiguration(string transport, bool httpIncomingAuthDisabled, CommandResult commandResult)
    {
        if (transport == TransportTypes.StdIo)
        {
            if (httpIncomingAuthDisabled)
            {
                commandResult.AddError($"The --dangerously-disable-http-incoming-auth option cannot be used with the {TransportTypes.StdIo} transport. To use this option, specify {TransportTypes.Http} transport with --transport http.");
            }
            return;
        }

        if (transport == TransportTypes.Http)
        {
#if ENABLE_HTTP
            return;
#else
            commandResult.AddError($"{TransportTypes.Http} transport is only supported in the Docker image distribution of Azure MCP Server. Please use the Docker image or switch to {TransportTypes.StdIo} transport.");
            return;
#endif
        }

        commandResult.AddError($"Invalid transport '{transport}'. Valid transports are: {TransportTypes.StdIo}, {TransportTypes.Http}.");
    }

    /// <summary>
    /// Validates that --namespace and --tool options are not used together.
    /// </summary>
    /// <param name="namespaces">The namespace values.</param>
    /// <param name="tools">The tool values.</param>
    /// <param name="commandResult">Command result to update on failure.</param>
    private static void ValidateNamespaceAndToolMutualExclusion(string[]? namespaces, string[]? tools, CommandResult commandResult)
    {
        bool hasNamespace = namespaces != null && namespaces.Length > 0;
        bool hasTool = tools != null && tools.Length > 0;

        if (hasNamespace && hasTool)
        {
            commandResult.AddError("The --namespace and --tool options cannot be used together. Please specify either --namespace to filter by service namespace or --tool to filter by specific tool names, but not both.");
        }
    }

    /// <summary>
    /// Validates that the outgoing authentication strategy is compatible with the hosting mode.
    /// </summary>
    /// <param name="commandResult">Command result to update on failure.</param>
    private static void ValidateOutgoingAuthStrategy(CommandResult commandResult)
    {
        var outgoingAuthStrategy = commandResult.GetValueOrDefault<OutgoingAuthStrategy>(ServiceOptionDefinitions.OutgoingAuthStrategy.Name);
        if (outgoingAuthStrategy == OutgoingAuthStrategy.UseOnBehalfOf)
        {
#if ENABLE_HTTP
            string transport = ResolveTransport(commandResult);
            bool httpIncomingAuthDisabled = commandResult.GetValueOrDefault<bool>(ServiceOptionDefinitions.DangerouslyDisableHttpIncomingAuth);

            if (transport != TransportTypes.Http || httpIncomingAuthDisabled)
            {
                commandResult.AddError($"The {OutgoingAuthStrategy.UseOnBehalfOf} outgoing authentication strategy requires the server to run in authenticated HTTP mode (--transport http without --{ServiceOptionDefinitions.DangerouslyDisableHttpIncomingAuthName}).");
            }
            return;
#else
            commandResult.AddError($"{OutgoingAuthStrategy.UseOnBehalfOf} outgoing authentication strategy is only supported in the Docker image distribution of Azure MCP Server. " +
                "Please use the Docker image or switch to a different outgoing authentication strategy.");
#endif
        }
    }

    /// <summary>
    /// Provides custom error messages for specific exception types to improve user experience.
    /// </summary>
    /// <param name="ex">The exception to format an error message for.</param>
    /// <returns>A user-friendly error message.</returns>
    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        ArgumentException argEx when argEx.Message.Contains("Invalid transport") =>
            "Invalid transport option specified. Use --transport stdio for the supported transport mechanism.",
        ArgumentException argEx when argEx.Message.Contains("Invalid mode") =>
            "Invalid mode option specified. Use --mode single, namespace, or all for the supported modes.",
        ArgumentException argEx when argEx.Message.Contains("--namespace and --tool options cannot be used together") =>
            "Configuration error: The --namespace and --tool options are mutually exclusive. Use either one or the other to filter available tools.",
        ArgumentException argEx when argEx.Message.Contains($"{OutgoingAuthStrategy.UseOnBehalfOf} outgoing authentication strategy") =>
            $"Configuration error: The {OutgoingAuthStrategy.UseOnBehalfOf} authentication strategy requires the server to run in authenticated HTTP mode (--transport http without --{ServiceOptionDefinitions.DangerouslyDisableHttpIncomingAuthName}).",
        InvalidOperationException invOpEx when invOpEx.Message.Contains("Using --enable-insecure-transport") =>
            "Insecure transport configuration error. Ensure proper authentication configured with Managed Identity or Workload Identity.",
        _ => base.GetErrorMessage(ex)
    };

    /// <summary>
    /// Creates the host for the MCP server with the specified options.
    /// </summary>
    /// <param name="serverOptions">The server configuration options.</param>
    /// <returns>An IHost instance configured for the MCP server.</returns>
    private IHost CreateHost(ServiceStartOptions serverOptions)
    {
#if ENABLE_HTTP
        if (serverOptions.Transport == TransportTypes.Http)
        {
            if (serverOptions.DangerouslyDisableHttpIncomingAuth)
            {
                return CreateIncomingAuthDisabledHttpHost(serverOptions);
            }
            else
            {
                return CreateHttpHost(serverOptions);
            }
        }
        else
        {
            return CreateStdioHost(serverOptions);
        }
#else
        return CreateStdioHost(serverOptions);
#endif
    }

    /// <summary>
    /// Creates a host for STDIO transport.
    /// </summary>
    /// <param name="serverOptions">The server configuration options.</param>
    /// <returns>An IHost instance configured for STDIO transport.</returns>
    private IHost CreateStdioHost(ServiceStartOptions serverOptions)
    {
        return Host.CreateDefaultBuilder()
            .ConfigureLogging(logging =>
            {
                logging.ClearProviders();
                logging.ConfigureOpenTelemetryLogger();
                logging.AddEventSourceLogger();

                if (serverOptions.Debug)
                {
                    // Configure console logger to emit Debug+ to stderr so tests can capture logs from StandardError
                    logging.AddConsole(options =>
                    {
                        options.LogToStandardErrorThreshold = LogLevel.Debug;
                        options.FormatterName = Microsoft.Extensions.Logging.Console.ConsoleFormatterNames.Simple;
                    });
                    logging.AddSimpleConsole(simple =>
                    {
                        simple.ColorBehavior = Microsoft.Extensions.Logging.Console.LoggerColorBehavior.Disabled;
                        simple.IncludeScopes = false;
                        simple.SingleLine = true;
                        simple.TimestampFormat = "[HH:mm:ss] ";
                    });
                    logging.AddFilter("Microsoft.Extensions.Logging.Console.ConsoleLoggerProvider", LogLevel.Debug);
                    logging.SetMinimumLevel(LogLevel.Debug);
                }
            })
            .ConfigureServices(services =>
            {
                // Configure the outgoing authentication strategy.
                services.AddSingleIdentityTokenCredentialProvider();

                ConfigureServices(services);
                ConfigureMcpServer(services, serverOptions);
            })
            .Build();
    }

    /// <summary>
    /// Creates a host for HTTP transport.
    /// </summary>
    /// <param name="serverOptions">The server configuration options.</param>
    /// <returns>An IHost instance configured for HTTP transport.</returns>
    private IHost CreateHttpHost(ServiceStartOptions serverOptions)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder();

        // Configure logging
        builder.Logging.ClearProviders();
        builder.Logging.ConfigureOpenTelemetryLogger();
        builder.Logging.AddEventSourceLogger();
        builder.Logging.AddConsole();

        IServiceCollection services = builder.Services;

        // Configure outgoing and incoming authentication and authorization.
        //
        // Configure incoming authentication and authorization.
        MicrosoftIdentityWebApiAuthenticationBuilderWithConfiguration authBuilder = services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddMicrosoftIdentityWebApi(builder.Configuration);

        // Configure incoming auth JWT Bearer events for OAuth protected resource metadata.
        services.Configure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
        {
            options.Events = new JwtBearerEvents
            {
                OnChallenge = context =>
                {
                    // Add resource_metadata parameter to WWW-Authenticate header
                    if (!context.Response.HasStarted)
                    {
                        HttpRequest request = context.Request;
                        string resourceMetadataUrl = $"{request.Scheme}://{request.Host}/.well-known/oauth-protected-resource";

                        // Modify the WWW-Authenticate header to include resource_metadata
                        context.Response.Headers.WWWAuthenticate =
                            $"Bearer realm=\"{request.Host}\", resource_metadata=\"{resourceMetadataUrl}\"";
                    }
                    return Task.CompletedTask;
                }
            };
        });

        // Configure authorization policy for MCP access.
        services.AddAuthorizationBuilder()
            .SetFallbackPolicy(null)
            .AddPolicy("McpAccess", policy =>
            {
                policy.RequireAuthenticatedUser();

                // Naming conventions used based on well-known Microsoft services, like MS Graph:
                // - Scopes for delegated permissions: Mcp.Tools.Verb
                // - App roles for application permissions: Mcp.Tools.Verb.All
                // As of Oct 2025, we only have ReadWrite as a verb, but this can be extended
                // in the future as needed. Other scenarios that aren't "MCP" or "Tools" can
                // also be added in the future as they become relevant.
                policy.RequireScopeOrAppPermission(
                    allowedScopeValues: ["Mcp.Tools.ReadWrite"],
                    allowedAppPermissionValues: ["Mcp.Tools.ReadWrite.All"]);
            });

        // Configure outgoing authentication strategy
        if (serverOptions.OutgoingAuthStrategy == OutgoingAuthStrategy.UseOnBehalfOf)
        {
            services.AddHttpOnBehalfOfTokenCredentialProvider(authBuilder);
        }
        else
        {
            services.AddSingleIdentityTokenCredentialProvider();
        }

        // Add a multi-user, HTTP context-aware caching strategy to isolate cache entries.
        services.AddHttpServiceCacheService();


        // Configure non-MCP controllers/endpoints/routes/etc.
        services.AddHealthChecks();

        // Configure CORS
        // We're allowing all origins, methods, and headers to support any web
        // browser clients.
        // Non-browser clients are unaffected by CORS.
        services.AddCors(options =>
        {
            options.AddPolicy("AllowAll", policy =>
            {
                policy.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader();
            });
        });

        // Configure services
        ConfigureServices(services); // Our static callback hook
        ConfigureMcpServer(services, serverOptions);

        WebApplication app = builder.Build();

        UseHttpsRedirectionIfEnabled(app);

        // Configure middleware pipeline
        app.UseCors("AllowAll");
        app.UseRouting();

        // Add OAuth protected resource metadata middleware
        //
        app.Use(async (context, next) =>
        {
            if (context.Request.Path == "/.well-known/oauth-protected-resource" &&
                context.Request.Method == "GET")
            {
                IOptionsMonitor<MicrosoftIdentityOptions> azureAdOptionsMonitor = context
                    .RequestServices
                    .GetRequiredService<IOptionsMonitor<MicrosoftIdentityOptions>>();
                MicrosoftIdentityOptions azureAdOptions = azureAdOptionsMonitor.Get(JwtBearerDefaults.AuthenticationScheme);
                HttpRequest request = context.Request;
                string baseUrl = $"{request.Scheme}://{request.Host}";
                string? clientId = azureAdOptions.ClientId;
                string? tenantId = azureAdOptions.TenantId;
                string instance = azureAdOptions.Instance?.TrimEnd('/') ?? "https://login.microsoftonline.com";

                var metadata = new OAuthProtectedResourceMetadata
                {
                    Resource = baseUrl,
                    AuthorizationServers = [$"{instance}/{tenantId}/v2.0"],

                    // Only delegated permissions for user principal authorization is listed here.
                    // Client with users send these scopes to the identity platform to acquire an
                    // access token.
                    // However, special to Entra, service principals are expected to always
                    // request the special `app-id/.default` scope because service principals use
                    // app roles/app permissions instead of scopes. At time of writing (Oct 2025),
                    // we don't solve this problem here. Instead we expect any service principal
                    // clients to be hardcoded to use the `app-id/.default` scope when requesting
                    // access tokens for our endpoint and for the owners of the client and MCP
                    // server's service principals to ensure the necessary app roles are assigned
                    // upfront.
                    ScopesSupported = [$"{clientId}/Mcp.Tools.ReadWrite"],
                    BearerMethodsSupported = ["header"],

                    // Intentionally pointing to MCP repo for documentation. Could eventually
                    // have a dedicated usage doc page, potentially provided by this service itself.
                    ResourceDocumentation = "https://github.com/Microsoft/mcp"
                };

                context.Response.ContentType = "application/json";
                await JsonSerializer.SerializeAsync(
                    context.Response.Body,
                    metadata,
                    OAuthMetadataJsonContext.Default.OAuthProtectedResourceMetadata);
                return;
            }

            await next(context);
        });

        // AuthN/Z are always required in the remote HTTP service scenario.
        app.UseAuthentication();
        app.UseAuthorization();

        IEndpointConventionBuilder mcpEndpointBuilder = app.MapMcp();
        // All MCP endpoints require MCP.All scope or role
        mcpEndpointBuilder.RequireAuthorization("McpAccess");

        // Map non-MCP endpoints.
        // Health checks are anonymous (no authentication required)
        app.MapHealthChecks("/health")
            .AllowAnonymous();

        return app;
    }

    /// <summary>
    /// Creates a host for HTTP transport without incoming authentication.
    /// </summary>
    /// <param name="serverOptions">The server configuration options.</param>
    /// <returns>An IHost instance configured for HTTP transport.</returns>
    private IHost CreateIncomingAuthDisabledHttpHost(ServiceStartOptions serverOptions)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder();

        InitializeListingUrls(builder, serverOptions);

        // Configure logging
        builder.Logging.ClearProviders();
        builder.Logging.ConfigureOpenTelemetryLogger();
        builder.Logging.AddEventSourceLogger();
        builder.Logging.AddConsole();

        IServiceCollection services = builder.Services;

        // Configure single identity token credential provider for outgoing authentication
        services.AddSingleIdentityTokenCredentialProvider();

        // Configure CORS
        // We're allowing all origins, methods, and headers to support any web
        // browser clients.
        // Non-browser clients are unaffected by CORS.
        services.AddCors(options =>
        {
            options.AddPolicy("AllowAll", policy =>
            {
                policy.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader();
            });
        });

        // Configure services
        ConfigureServices(services); // Our static callback hook
        ConfigureMcpServer(services, serverOptions);

        // We still use the multi-user, HTTP context-aware caching strategy here
        // because we don't yet know what security model we want for this "insecure" mode.
        // As a positive, it gives some isolation locally, but that's not a
        // design strategy we've fully vetted or endorsed.
        services.AddHttpServiceCacheService();

        WebApplication app = builder.Build();

        UseHttpsRedirectionIfEnabled(app);

        // Configure middleware pipeline
        app.UseCors("AllowAll");
        app.UseRouting();
        app.MapMcp();

        return app;
    }

    /// <summary>
    /// Configures the MCP server services.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="options">The server configuration options.</param>
    private static void ConfigureMcpServer(IServiceCollection services, ServiceStartOptions options)
    {
        services.AddAzureMcpServer(options);
    }

    /// <summary>
    /// Initializes the URL for ASP.NET Core to bind to.
    /// </summary>
    private static void InitializeListingUrls(WebApplicationBuilder builder, ServiceStartOptions options)
    {
        if (!options.DangerouslyDisableHttpIncomingAuth)
        {
            // When running in secured HTTP mode, allow the standard IConfiguration binding to handle
            // the ASPNETCORE_URLS value without any additional validation.
            return;
        }

        string url = Environment.GetEnvironmentVariable("ASPNETCORE_URLS") ?? "http://127.0.0.1:5001";

        if (url.Contains(';'))
        {
            throw new InvalidOperationException("Multiple endpoints in ASPNETCORE_URLS are not supported. Provide a single URL.");
        }

        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
        {
            throw new InvalidOperationException($"Invalid URL: '{url}'");
        }

        var scheme = uri.Scheme.ToLowerInvariant();
        if (scheme is not ("http" or "https"))
        {
            throw new InvalidOperationException($"Unsupported scheme '{scheme}' in URL '{url}'.");
        }

        // loopback IP: 127.0.0.0/8 range, IPv6 (::1) and localhost when resolved to loopback addresses.
        bool isLoopback = uri.IsLoopback;

        // All interfaces, allowed only with ALLOW_INSECURE_EXTERNAL_BINDING opt-in.
        bool isWildcard = uri.Host is "*" or "+" or "0.0.0.0" or "::" || (IPAddress.TryParse(uri.Host, out var anyIp) && (anyIp.Equals(IPAddress.Any) || anyIp.Equals(IPAddress.IPv6Any)));

        if (!isLoopback && !isWildcard)
        {
            throw new InvalidOperationException($"Explicit external binding is not supported for '{url}'.");
        }

        if (isWildcard && !EnvironmentHelpers.GetEnvironmentVariableAsBool("ALLOW_INSECURE_EXTERNAL_BINDING"))
        {
            throw new InvalidOperationException(
                $"External binding blocked for '{url}'. " +
                $"Set ALLOW_INSECURE_EXTERNAL_BINDING=true if you intentionally want to bind beyond loopback.");
        }

        builder.WebHost.UseUrls(url);
    }

    /// <summary>
    /// Resolves the service mode and outgoing authentication strategy based on parsed command line options, applying appropriate defaults.
    /// </summary>
    /// <param name="parseResult">The parsed command line arguments.</param>
    /// <returns>A tuple containing whether to run as remote HTTP service and the outgoing auth strategy.</returns>
    private static OutgoingAuthStrategy ResolveAuthStrategy(ParseResult parseResult)
    {
#if ENABLE_HTTP
        var outgoingAuthStrategy = parseResult.GetValueOrDefault<OutgoingAuthStrategy>(ServiceOptionDefinitions.OutgoingAuthStrategy.Name);
        if (outgoingAuthStrategy == OutgoingAuthStrategy.NotSet)
        {
            string transport = ResolveTransport(parseResult);
            if (transport == TransportTypes.Http)
            {
                bool httpIncomingAuthDisabled = parseResult.GetValueOrDefault<bool>(ServiceOptionDefinitions.DangerouslyDisableHttpIncomingAuth.Name);
                return httpIncomingAuthDisabled
                    ? OutgoingAuthStrategy.UseHostingEnvironmentIdentity
                    : OutgoingAuthStrategy.UseOnBehalfOf;
            }
            else
            {
                return OutgoingAuthStrategy.UseHostingEnvironmentIdentity;
            }
        }
        return outgoingAuthStrategy;
#else
        return OutgoingAuthStrategy.UseHostingEnvironmentIdentity;
#endif
    }

    /// <summary>
    /// Resolves the transport type from parsed command line arguments, defaulting to STDIO if not specified.
    /// </summary>
    /// <param name="parseResult">The parsed command line arguments.</param>
    /// <returns>The transport type string (stdio or http).</returns>
    private static string ResolveTransport(ParseResult parseResult)
    {
        return parseResult.GetValueOrDefault<string>(ServiceOptionDefinitions.Transport.Name) ?? TransportTypes.StdIo;
    }

    /// <summary>
    /// Resolves the transport type from command result, defaulting to STDIO if not specified.
    /// </summary>
    /// <param name="commandResult">The command result to extract transport from.</param>
    /// <returns>The transport type string (stdio or http).</returns>
    private static string ResolveTransport(CommandResult commandResult)
    {
        return commandResult.GetValueOrDefault<string>(ServiceOptionDefinitions.Transport.Name) ?? TransportTypes.StdIo;
    }

    private static WebApplication UseHttpsRedirectionIfEnabled(WebApplication app)
    {
        // Some hosting environments may not need HTTPS redirection, such as:
        // - Running behind a reverse proxy that handles TLS termination.
        // - Local development when not using self-signed development certs.
        // - The application or server's HTTP stack is not listening for non-HTTPS requests.
        //
        // Safe default to enable HTTPS redirection unless explicitly opted-out.
        string? httpsRedirectionOptOut = Environment.GetEnvironmentVariable("AZURE_MCP_DANGEROUSLY_DISABLE_HTTPS_REDIRECTION");
        if (!bool.TryParse(httpsRedirectionOptOut, out bool isOptedOut) || !isOptedOut)
        {
            app.UseHttpsRedirection();
        }

        return app;
    }
}
