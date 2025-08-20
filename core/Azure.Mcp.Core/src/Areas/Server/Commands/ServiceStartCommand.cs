// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Areas.Server.Options;
using Azure.Mcp.Core.Commands;
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
public sealed class ServiceStartCommand : BaseCommand
{
    private const string CommandTitle = "Start MCP Server";
    private readonly Option<string> _transportOption = ServiceOptionDefinitions.Transport;
    private readonly Option<string[]?> _namespaceOption = ServiceOptionDefinitions.Namespace;
    private readonly Option<string?> _modeOption = ServiceOptionDefinitions.Mode;
    private readonly Option<bool?> _readOnlyOption = ServiceOptionDefinitions.ReadOnly;

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
        command.AddOption(_transportOption);
        command.AddOption(_namespaceOption);
        command.AddOption(_modeOption);
        command.AddOption(_readOnlyOption);
    }

    /// <summary>
    /// Executes the service start command, creating and starting the MCP server.
    /// </summary>
    /// <param name="context">The command execution context.</param>
    /// <param name="parseResult">The parsed command options.</param>
    /// <returns>A command response indicating the result of the operation.</returns>
    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {
        var namespaces = parseResult.GetValueForOption(_namespaceOption) == default
            ? ServiceOptionDefinitions.Namespace.GetDefaultValue()
            : parseResult.GetValueForOption(_namespaceOption);

        var mode = parseResult.GetValueForOption(_modeOption) == default
            ? ServiceOptionDefinitions.Mode.GetDefaultValue()
            : parseResult.GetValueForOption(_modeOption);

        var readOnly = parseResult.GetValueForOption(_readOnlyOption) == default
            ? ServiceOptionDefinitions.ReadOnly.GetDefaultValue()
            : parseResult.GetValueForOption(_readOnlyOption);

        if (!IsValidMode(mode))
        {
            throw new ArgumentException($"Invalid mode '{mode}'. Valid modes are: {ModeTypes.SingleToolProxy}, {ModeTypes.NamespaceProxy}, {ModeTypes.All}.");
        }

        var serverOptions = new ServiceStartOptions
        {
            Transport = parseResult.GetValueForOption(_transportOption) ?? TransportTypes.StdIo,
            Namespace = namespaces,
            Mode = mode,
            ReadOnly = readOnly,
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
    /// Configures the MCP server services.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="options">The server configuration options.</param>
    private static void ConfigureMcpServer(IServiceCollection services, ServiceStartOptions options)
    {
        services.AddAzureMcpServer(options);
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
