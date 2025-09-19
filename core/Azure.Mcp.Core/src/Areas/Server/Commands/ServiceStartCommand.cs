// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine.Parsing;
using System.Net;
using Azure.Mcp.Core.Areas.Server.Options;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Helpers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

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
        command.Options.Add(ServiceOptionDefinitions.ReadOnly);
        command.Options.Add(ServiceOptionDefinitions.Debug);
        command.Options.Add(ServiceOptionDefinitions.EnableInsecureTransports);
        command.Options.Add(ServiceOptionDefinitions.InsecureDisableElicitation);
    }

    /// <summary>
    /// Binds the parsed command line arguments to the ServiceStartOptions object.
    /// </summary>
    /// <param name="parseResult">The parsed command line arguments.</param>
    /// <returns>A configured ServiceStartOptions instance.</returns>
    protected override ServiceStartOptions BindOptions(ParseResult parseResult)
    {
        var options = new ServiceStartOptions
        {
            Transport = parseResult.GetValueOrDefault<string>(ServiceOptionDefinitions.Transport.Name) ?? TransportTypes.StdIo,
            Namespace = parseResult.GetValueOrDefault<string[]?>(ServiceOptionDefinitions.Namespace.Name),
            Mode = parseResult.GetValueOrDefault<string?>(ServiceOptionDefinitions.Mode.Name),
            ReadOnly = parseResult.GetValueOrDefault<bool?>(ServiceOptionDefinitions.ReadOnly.Name),
            Debug = parseResult.GetValueOrDefault<bool>(ServiceOptionDefinitions.Debug.Name),
            EnableInsecureTransports = parseResult.GetValueOrDefault<bool>(ServiceOptionDefinitions.EnableInsecureTransports.Name),
            InsecureDisableElicitation = parseResult.GetValueOrDefault<bool>(ServiceOptionDefinitions.InsecureDisableElicitation.Name)
        };
        return options;
    }

    /// <summary>
    /// Validates the command options and arguments.
    /// </summary>
    /// <param name="commandResult">The command result to validate.</param>
    /// <param name="commandResponse">Optional response object to set error details.</param>
    /// <returns>A ValidationResult indicating whether the validation passed.</returns>
    public override ValidationResult Validate(CommandResult commandResult, CommandResponse? commandResponse)
    {
        // First run the base validation for required options and parser errors
        var baseResult = base.Validate(commandResult, commandResponse);
        if (!baseResult.IsValid)
        {
            return baseResult;
        }

        // Get option values directly from commandResult
        var mode = commandResult.GetValueOrDefault(ServiceOptionDefinitions.Mode);
        var transport = commandResult.GetValueOrDefault(ServiceOptionDefinitions.Transport);

        if (!IsValidMode(mode))
        {
            var result = new ValidationResult
            {
                IsValid = false,
                ErrorMessage = $"Invalid mode '{mode}'. Valid modes are: {ModeTypes.SingleToolProxy}, {ModeTypes.NamespaceProxy}, {ModeTypes.All}."
            };

            if (commandResponse != null)
            {
                commandResponse.Status = 400;
                commandResponse.Message = result.ErrorMessage;
            }

            return result;
        }

        try
        {
            ValidateTransport(transport);
        }
        catch (ArgumentException ex)
        {
            var result = new ValidationResult
            {
                IsValid = false,
                ErrorMessage = ex.Message
            };

            if (commandResponse != null)
            {
                commandResponse.Status = 400;
                commandResponse.Message = result.ErrorMessage;
            }

            return result;
        }

        return new ValidationResult { IsValid = true };
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
            if (options.EnableInsecureTransports)
            {
                var includeProdCreds = EnvironmentHelpers.GetEnvironmentVariableAsBool("AZURE_MCP_INCLUDE_PRODUCTION_CREDENTIALS");
                if (!includeProdCreds)
                {
                    throw new InvalidOperationException("Using --enable-insecure-transport requires the host to have either Managed Identity or Workload Identity enabled. Please refer to the troubleshooting guidelines here at https://aka.ms/azmcp/troubleshooting.");
                }
            }

            using var host = CreateHost(options);
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

    /// <summary>
    /// Validates if the provided mode is a valid mode type.
    /// </summary>
    /// <param name="mode">The mode to validate.</param>
    /// <returns>True if the mode is valid, otherwise false.</returns>
    private static bool IsValidMode(string? mode)
    {
        return mode == ModeTypes.SingleToolProxy ||
               mode == ModeTypes.NamespaceProxy ||
               mode == ModeTypes.All;
    }

    /// <summary>
    /// Validates the transport parameter and throws an exception if invalid.
    /// </summary>
    /// <param name="transport">The transport to validate.</param>
    /// <exception cref="ArgumentException">Thrown when the transport is not null and invalid.</exception>
    private static void ValidateTransport(string? transport)
    {
        if (transport is not null && transport != TransportTypes.StdIo)
        {
            throw new ArgumentException($"Invalid transport '{transport}'. Valid transports are: {TransportTypes.StdIo}.");
        }
    }

    /// <summary>
    /// Provides custom error messages for service start specific exceptions.
    /// </summary>
    /// <param name="ex">The exception to get an error message for.</param>
    /// <returns>A user-friendly error message.</returns>
    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        ArgumentException argEx when argEx.Message.Contains("transport") =>
            $"Invalid transport option specified. {argEx.Message} Use --transport stdio for standard input/output.",
        ArgumentException argEx when argEx.Message.Contains("mode") =>
            $"Invalid mode option specified. {argEx.Message} Use --mode single, namespace, or all.",
        InvalidOperationException opEx when opEx.Message.Contains("enable-insecure-transport") =>
            "Insecure transport configuration error. Ensure your environment has proper authentication configured (Managed Identity or Workload Identity).",
        _ => ex.Message
    };

    /// <summary>
    /// Provides appropriate HTTP status codes for service start specific exceptions.
    /// </summary>
    /// <param name="ex">The exception to get a status code for.</param>
    /// <returns>An appropriate HTTP status code.</returns>
    protected override int GetStatusCode(Exception ex) => ex switch
    {
        ArgumentException => 400,  // Bad Request for invalid arguments
        InvalidOperationException => 422,  // Unprocessable Entity for configuration errors
        _ => 500
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
                logging.AddConsole();
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

    /// <summary>
    /// Hosted service for running the MCP server using standard input/output.
    /// </summary>
    private sealed class StdioMcpServerHostedService(IMcpServer session) : BackgroundService
    {
        /// <inheritdoc />
        protected override Task ExecuteAsync(CancellationToken stoppingToken) => session.RunAsync(stoppingToken);
    }
}
