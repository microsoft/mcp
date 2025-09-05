// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Core.Areas.Server.Options;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Helpers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.AspNetCore;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace Azure.Mcp.Core.Areas.Server.Commands;

/// <summary>
/// Command to start the MCP server with specified configuration options.
/// This command is hidden from the main command list.
/// </summary>
[HiddenCommand]
public sealed class ServiceStartCommand : BaseCommand
{
    private const string CommandTitle = "Start MCP Server";
    private readonly Option<string> _transportOption = ServiceOptionDefinitions.Transport;
    private readonly Option<string[]?> _namespaceOption = ServiceOptionDefinitions.Namespace;
    private readonly Option<string?> _modeOption = ServiceOptionDefinitions.Mode;
    private readonly Option<bool?> _readOnlyOption = ServiceOptionDefinitions.ReadOnly;
    private readonly Option<bool> _enableInsecureTransportsOption = ServiceOptionDefinitions.EnableInsecureTransports;

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
        command.Options.Add(_transportOption);
        command.Options.Add(_namespaceOption);
        command.Options.Add(_modeOption);
        command.Options.Add(_readOnlyOption);
        command.Options.Add(_enableInsecureTransportsOption);
    }

    /// <summary>
    /// Executes the service start command, creating and starting the MCP server.
    /// </summary>
    /// <param name="context">The command execution context.</param>
    /// <param name="parseResult">The parsed command options.</param>
    /// <returns>A command response indicating the result of the operation.</returns>
    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {
        string[]? namespaces = parseResult.GetValue(_namespaceOption);
        string? mode = parseResult.GetValue(_modeOption);
        bool? readOnly = parseResult.GetValue(_readOnlyOption);

        if (!IsValidMode(mode))
        {
            throw new ArgumentException($"Invalid mode '{mode}'. Valid modes are: {ModeTypes.SingleToolProxy}, {ModeTypes.NamespaceProxy}, {ModeTypes.All}.");
        }

        var enableInsecureTransports = parseResult.GetValueOrDefault(_enableInsecureTransportsOption);

        if (enableInsecureTransports)
        {
            var includeProdCreds = EnvironmentHelpers.GetEnvironmentVariableAsBool("AZURE_MCP_INCLUDE_PRODUCTION_CREDENTIALS");
            if (!includeProdCreds)
            {
                throw new InvalidOperationException("Using --enable-insecure-transport requires the host to have either Managed Identity or Workload Identity enabled. Please refer to the troubleshooting guidelines here at https://aka.ms/azmcp/troubleshooting.");
            }
        }

        var serverOptions = new ServiceStartOptions
        {
            Transport = parseResult.GetValue(_transportOption) ?? TransportTypes.StdIo,
            Namespace = namespaces,
            Mode = mode,
            ReadOnly = readOnly,
            EnableInsecureTransports = enableInsecureTransports,
        };

        using var host = CreateHost(serverOptions);
        await host.StartAsync(CancellationToken.None);
        await host.WaitForShutdownAsync(CancellationToken.None);

        return context.Response;
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
