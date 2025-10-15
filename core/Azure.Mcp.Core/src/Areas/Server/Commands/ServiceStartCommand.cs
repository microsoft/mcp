// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine.Parsing;
using System.Net;
using Azure.Mcp.Core.Areas.Server.Options;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Helpers;
using Azure.Mcp.Core.Services.Telemetry;
using Azure.Mcp.Core.Services.Logging;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
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
        command.Options.Add(ServiceOptionDefinitions.EnableInsecureTransports);
        command.Options.Add(ServiceOptionDefinitions.InsecureDisableElicitation);
        command.Options.Add(ServiceOptionDefinitions.LogLevel);
        command.Options.Add(ServiceOptionDefinitions.LogFile);
        command.Validators.Add(commandResult =>
        {
            ValidateMode(commandResult.GetValueOrDefault(ServiceOptionDefinitions.Mode), commandResult);
            ValidateTransport(commandResult.GetValueOrDefault(ServiceOptionDefinitions.Transport), commandResult);
            ValidateInsecureTransportsConfiguration(commandResult.GetValueOrDefault(ServiceOptionDefinitions.EnableInsecureTransports), commandResult);
            ValidateNamespaceAndToolMutualExclusion(
                commandResult.GetValueOrDefault<string[]?>(ServiceOptionDefinitions.Namespace.Name),
                commandResult.GetValueOrDefault<string[]?>(ServiceOptionDefinitions.Tool.Name),
                commandResult);
        });
    }

    /// <summary>
    /// Resolves logging options from command line and environment variables.
    /// Environment variables take precedence over defaults but not over explicit command line options.
    /// </summary>
    private static ServiceStartOptions ResolveLoggingOptions(ServiceStartOptions options)
    {
        // Environment variables (only if command line option wasn't explicitly set)
        if (string.IsNullOrEmpty(options.LogLevel))
        {
            var envLogLevel = Environment.GetEnvironmentVariable("AZMCP_LOG_LEVEL");
            if (!string.IsNullOrEmpty(envLogLevel))
            {
                options.LogLevel = envLogLevel;
            }
        }



        if (string.IsNullOrEmpty(options.LogFile))
        {
            var envLogFile = Environment.GetEnvironmentVariable("AZMCP_LOG_FILE");
            if (!string.IsNullOrEmpty(envLogFile))
            {
                options.LogFile = envLogFile;
            }
        }
        return options;
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

        var options = new ServiceStartOptions
        {
            Transport = parseResult.GetValueOrDefault<string>(ServiceOptionDefinitions.Transport.Name) ?? TransportTypes.StdIo,
            Namespace = parseResult.GetValueOrDefault<string[]?>(ServiceOptionDefinitions.Namespace.Name),
            Mode = mode,
            Tool = tools,
            ReadOnly = parseResult.GetValueOrDefault<bool?>(ServiceOptionDefinitions.ReadOnly.Name),
            Debug = parseResult.GetValueOrDefault<bool>(ServiceOptionDefinitions.Debug.Name),
            EnableInsecureTransports = parseResult.GetValueOrDefault<bool>(ServiceOptionDefinitions.EnableInsecureTransports.Name),
            InsecureDisableElicitation = parseResult.GetValueOrDefault<bool>(ServiceOptionDefinitions.InsecureDisableElicitation.Name),
            LogLevel = parseResult.GetValueOrDefault<string?>(ServiceOptionDefinitions.LogLevel.Name),
            LogFile = parseResult.GetValueOrDefault<string?>(ServiceOptionDefinitions.LogFile.Name)
        };
        return ResolveLoggingOptions(options);
    }

    /// <summary>
    /// Resolves log file path with placeholder substitution.
    /// </summary>
    private static string ResolveLogFilePath(string logFilePath)
    {
        if (string.IsNullOrEmpty(logFilePath))
            return logFilePath;

        var resolved = logFilePath
            .Replace("{timestamp}", DateTime.UtcNow.ToString("yyyyMMdd-HHmmss"))
            .Replace("{pid}", Environment.ProcessId.ToString());

        // Ensure directory exists
        var directory = Path.GetDirectoryName(resolved);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        return resolved;
    }

    /// <summary>
    /// Parses log level string to LogLevel enum. Determines the effective log level based on various option sources.
    /// </summary>
    /// <param name="serverOptions">The server configuration options.</param>
    /// <returns>The effective LogLevel to use.</returns>
    private static LogLevel ParseLogLevel(ServiceStartOptions serverOptions)
    {
        // Priority order:
        // 1. Explicit LogLevel option
        // 2. Debug flag -> Debug level
        // 3. Default -> Information level

        // Priority 1: If LogLevel was explicitly set (not null), use it regardless of other flags
        if (!string.IsNullOrEmpty(serverOptions.LogLevel))
        {
            return serverOptions.LogLevel.ToLowerInvariant() switch
            {
                "trace" => LogLevel.Trace,
                "debug" => LogLevel.Debug,
                "info" or "information" => LogLevel.Information,
                "warn" or "warning" => LogLevel.Warning,
                "error" => LogLevel.Error,
                "critical" => LogLevel.Critical,
                _ => LogLevel.Information
            };
        }

        // Priority 2: If no explicit log level, check debug flag
        if (serverOptions.Debug)
        {
            return LogLevel.Debug;
        }

        return LogLevel.Information;
    }

    /// <summary>
    /// Executes the service start command, creating and starting the MCP server.
    /// </summary>
    /// <param name="context">The command execution context.</param>
    /// <param name="parseResult">The parsed command options.</param>
    /// <returns>A command response indicating the result of the operation.</returns>
    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
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

            var telemetryService = host.Services.GetRequiredService<ITelemetryService>();
            LogStartTelemetry(telemetryService, options);

            await host.StartAsync(CancellationToken.None);
            await host.WaitForShutdownAsync(CancellationToken.None);

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
            activity.SetTag(TagName.EnableInsecureTransports, options.EnableInsecureTransports);
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
            mode == ModeTypes.All)
        {
            return; // Success
        }

        commandResult.AddError($"Invalid mode '{mode}'. Valid modes are: {ModeTypes.SingleToolProxy}, {ModeTypes.NamespaceProxy}, {ModeTypes.All}.");
    }

    /// <summary>
    /// Validates if the provided transport is valid.
    /// </summary>
    /// <param name="transport">The transport to validate.</param>
    /// <param name="commandResult">Command result to update on failure.</param>
    private static void ValidateTransport(string? transport, CommandResult commandResult)
    {
        if (transport is null || transport == TransportTypes.StdIo)
        {
            return; // Success
        }

        commandResult.AddError($"Invalid transport '{transport}'. Valid transports are: {TransportTypes.StdIo}.");
    }

    /// <summary>
    /// Validates if the insecure transport configuration is valid.
    /// </summary>
    /// <param name="enableInsecureTransports">Whether insecure transports are enabled.</param>
    /// <param name="commandResult">Command result to update on failure.</param>
    private static void ValidateInsecureTransportsConfiguration(bool enableInsecureTransports, CommandResult commandResult)
    {
        // If insecure transports are not enabled, configuration is valid
        if (!enableInsecureTransports)
        {
            return; // Success
        }

        // If insecure transports are enabled, check if proper credentials are configured
        var hasCredentials = EnvironmentHelpers.GetEnvironmentVariableAsBool("AZURE_MCP_INCLUDE_PRODUCTION_CREDENTIALS");
        if (hasCredentials)
        {
            return; // Success
        }

        commandResult.AddError("Using --enable-insecure-transport requires the host to have either Managed Identity or Workload Identity enabled. Please refer to the troubleshooting guidelines here at https://aka.ms/azmcp/troubleshooting.");
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
        if (serverOptions.EnableInsecureTransports)
        {
            return CreateHttpHost(serverOptions);
        }
        else
        {
            return CreateStdioHost(serverOptions);
        }
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

                // Determine effective log level from new options
                var effectiveLogLevel = ParseLogLevel(serverOptions);
                logging.SetMinimumLevel(effectiveLogLevel);

                // Always configure console logger for STDIO mode to send logs to STDERR
                // This keeps STDOUT clean for MCP protocol communication
                logging.AddConsole(options =>
                {
                    options.LogToStandardErrorThreshold = effectiveLogLevel;
                    options.FormatterName = Microsoft.Extensions.Logging.Console.ConsoleFormatterNames.Simple;
                });

                logging.AddSimpleConsole(simple =>
                {
                    simple.ColorBehavior = Microsoft.Extensions.Logging.Console.LoggerColorBehavior.Disabled;
                    simple.IncludeScopes = false;
                    simple.SingleLine = true;
                    simple.TimestampFormat = "[HH:mm:ss] ";
                });

                // Add file logging if specified
                if (!string.IsNullOrEmpty(serverOptions.LogFile))
                {
                    var resolvedPath = ResolveLogFilePath(serverOptions.LogFile);
                    logging.AddProvider(new FileLoggerProvider(resolvedPath, effectiveLogLevel));
                }

                logging.AddFilter("Microsoft.Extensions.Logging.Console.ConsoleLoggerProvider", effectiveLogLevel);
            })
            .ConfigureServices(services =>
            {
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
        return Host.CreateDefaultBuilder()
            .ConfigureLogging(logging =>
            {
                logging.ClearProviders();
                logging.ConfigureOpenTelemetryLogger();
                logging.AddEventSourceLogger();

                // Determine effective log level from new options
                var effectiveLogLevel = ParseLogLevel(serverOptions);
                logging.SetMinimumLevel(effectiveLogLevel);

                // For HTTP mode, we can log to console normally (no STDIO separation needed)
                logging.AddConsole(options =>
                {
                    options.FormatterName = Microsoft.Extensions.Logging.Console.ConsoleFormatterNames.Simple;
                });

                logging.AddSimpleConsole(simple =>
                {
                    simple.ColorBehavior = Microsoft.Extensions.Logging.Console.LoggerColorBehavior.Disabled;
                    simple.IncludeScopes = false;
                    simple.SingleLine = true;
                    simple.TimestampFormat = "[HH:mm:ss] ";
                });

                // Add file logging if specified
                if (!string.IsNullOrEmpty(serverOptions.LogFile))
                {
                    var resolvedPath = ResolveLogFilePath(serverOptions.LogFile);
                    logging.AddProvider(new FileLoggerProvider(resolvedPath, effectiveLogLevel));
                }

                logging.AddFilter("Microsoft.Extensions.Logging.Console.ConsoleLoggerProvider", effectiveLogLevel);
            })
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.ConfigureServices(services =>
                {
                    services.AddCors(options =>
                    {
                        options.AddPolicy("AllowAll", policy =>
                        {
                            policy.AllowAnyOrigin()
                                  .AllowAnyMethod()
                                  .AllowAnyHeader();
                        });
                    });

                    ConfigureServices(services);
                    ConfigureMcpServer(services, serverOptions);
                });

                webBuilder.Configure(app =>
                {
                    app.UseCors("AllowAll");
                    app.UseRouting();
                    app.UseEndpoints(endpoints =>
                    {
                        endpoints.MapMcp();
                    });
                });

                var url = GetSafeAspNetCoreUrl();
                webBuilder.UseUrls(url);
            })
            .Build();
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
    /// Gets a safe ASP.NET Core URL with security validation.
    /// </summary>
    /// <returns>A validated URL string for ASP.NET Core binding.</returns>
    private static string GetSafeAspNetCoreUrl()
    {
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

        return url;
    }
}
