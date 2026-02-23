// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine.Parsing;
using Azure.Mcp.Core.Areas.Server.Commands;
using Azure.Mcp.Core.Areas.Server.Options;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Logging;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Core.Services.Azure.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Mcp.Core.Areas.Server.Options;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Extensions;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Models.Option;
using Microsoft.Mcp.Core.Services.Telemetry;
using OpenTelemetry.Trace;

namespace Microsoft.Mcp.Core.Areas.Server.Commands;

/// <summary>
/// Command that is ran when a CLI tool call is made, ex. 'dotnet azmcp.dll storage account get' or 'fabmcp publicapis list'.
/// </summary>
[HiddenCommand]
public sealed class CliToolCallCommand : BaseCommand<CliToolCallOptions>
{
    private const string CommandTitle = "CLI Tool Call";

    /// <summary>
    /// Gets the name of the command.
    /// </summary>
    public override string Name => "cli-tool-call";


    /// <summary>
    /// Gets the description of the command.
    /// </summary>
    public override string Description => 
        """
        Handles tool calls made using a CLI instead of the MCP server, ex. 'dotnet azmcp.dll storage account get'
        or 'fabmcp publicapis list'.
        """;

    /// <summary>
    /// Gets the title of the command.
    /// </summary>
    public override string Title => CommandTitle;

    /// <summary>
    /// Gets the metadata for this command.
    /// </summary>
    // TODO (alzimmer): What to do about Metadata, as this can call any tool, so is everything true?
    public override ToolMetadata Metadata => new() { Destructive = false, ReadOnly = true };

    public static Action<IServiceCollection> ConfigureServices { get; set; } = _ => { };

    public static Func<IServiceProvider, Task> InitializeServicesAsync { get; set; } = _ => Task.CompletedTask;

    public override string Id => "e431dec9-3f08-49d0-aaf1-8a66a749ec78";

    /// <summary>
    /// Registers command options for the service start command.
    /// </summary>
    /// <param name="command">The command to register options with.</param>
    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(ServiceOptionDefinitions.Tool.AsRequired());
        command.Options.Add(ServiceOptionDefinitions.ReadOnly);
        command.Options.Add(ServiceOptionDefinitions.Debug);
        command.Options.Add(ServiceOptionDefinitions.DangerouslyDisableElicitation);
        command.Options.Add(ServiceOptionDefinitions.DangerouslyWriteSupportLogsToDir);
        command.Options.Add(ServiceOptionDefinitions.Cloud);
        command.Validators.Add(ValidateSupportLoggingFolder);
    }

    /// <summary>
    /// Validates that the support logging folder path is valid when specified.
    /// </summary>
    /// <param name="commandResult">Command result to update on failure.</param>
    private static void ValidateSupportLoggingFolder(CommandResult commandResult)
    {
        string? folderPath = commandResult.GetValueOrDefault<string?>(ServiceOptionDefinitions.DangerouslyWriteSupportLogsToDir.Name);

        if (folderPath is null)
        {
            return; // Option not specified, nothing to validate
        }

        // Validate the folder path is not empty or whitespace
        if (string.IsNullOrWhiteSpace(folderPath))
        {
            commandResult.AddError("The --dangerously-write-support-logs-to-dir option requires a valid folder path.");
            return;
        }

        // Validate the folder path is actually a valid path format
        try
        {
            // GetFullPath will throw for invalid path characters and other path format issues
            _ = Path.GetFullPath(folderPath);
        }
        catch (Exception ex) when (ex is ArgumentException or PathTooLongException or NotSupportedException)
        {
            commandResult.AddError($"The --dangerously-write-support-logs-to-dir option contains an invalid folder path '{folderPath}': {ex.Message}");
        }
    }

    /// <summary>
    /// Binds the parsed command line arguments to the CliToolCallOptions object.
    /// </summary>
    /// <param name="parseResult">The parsed command line arguments.</param>
    /// <returns>A configured CliToolCallOptions instance.</returns>
    protected override CliToolCallOptions BindOptions(ParseResult parseResult)
    {
        var options = new CliToolCallOptions
        {
            Namespace = parseResult.GetValueOrDefault<string[]?>(ServiceOptionDefinitions.Namespace.Name),
            Mode = ModeTypes.All,
            Tool = parseResult.GetValueOrDefault<string[]?>(ServiceOptionDefinitions.Tool.Name),
            ReadOnly = parseResult.GetValueOrDefault<bool?>(ServiceOptionDefinitions.ReadOnly.Name),
            Debug = parseResult.GetValueOrDefault<bool>(ServiceOptionDefinitions.Debug.Name),
            DangerouslyDisableElicitation = parseResult.GetValueOrDefault<bool>(ServiceOptionDefinitions.DangerouslyDisableElicitation.Name),
            OutgoingAuthStrategy = OutgoingAuthStrategy.UseHostingEnvironmentIdentity, // OBO is not supported for CLI tool calls as there is no incoming token to exchange
            SupportLoggingFolder = parseResult.GetValueOrDefault<string?>(ServiceOptionDefinitions.DangerouslyWriteSupportLogsToDir.Name),
            Cloud = parseResult.GetValueOrDefault<string?>(ServiceOptionDefinitions.Cloud.Name)
        };
        return options;
    }

    /// <summary>
    /// Executes the CLI tool call command, creats and starts an MCP server, executes the tool call, and shutsdown the MCP server.
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

        // Update the UserAgentPolicy for all Azure service calls to include the transport type.
        BaseAzureService.InitializeUserAgentPolicy(TransportTypes.StdIo);

        try
        {
            using var host = CreateStdioHost(options);

            await InitializeServicesAsync(host.Services);

            await host.StartAsync(cancellationToken);

            var telemetryService = host.Services.GetRequiredService<ITelemetryService>();
            LogCliToolCallTelemetry(telemetryService, options);

            var commandFactory = host.Services.GetRequiredService<ICommandFactory>();
            var rootCommand = commandFactory.RootCommand;
            var commandParseResult = rootCommand.Parse(parseResult.UnmatchedTokens);
            var status = await commandParseResult.InvokeAsync(cancellationToken: cancellationToken);

            await host.StopAsync(cancellationToken);
            await host.WaitForShutdownAsync(cancellationToken);

            return context.Response;
        }
        catch (Exception ex)
        {
            HandleException(context, ex);
            return context.Response;
        }
    }

    internal static void LogCliToolCallTelemetry(ITelemetryService telemetryService, CliToolCallOptions options)
    {
        using var activity = telemetryService.StartActivity(ActivityName.CliToolCall);

        if (activity != null)
        {
            activity.SetTag(TagName.Transport, TransportTypes.StdIo);
            activity.SetTag(TagName.ServerMode, options.Mode);
            activity.SetTag(TagName.IsReadOnly, options.ReadOnly);
            activity.SetTag(TagName.DangerouslyDisableElicitation, options.DangerouslyDisableElicitation);
            activity.SetTag(TagName.DangerouslyDisableHttpIncomingAuth, false);
            activity.SetTag(TagName.IsDebug, options.Debug);
            if (options.Tool != null && options.Tool.Length > 0)
            {
                activity.SetTag(TagName.Tool, string.Join(",", options.Tool));
            }
        }
    }

    /// <summary>
    /// Configures support logging when a support logging folder is specified.
    /// This enables debug-level logging for troubleshooting and support purposes.
    /// </summary>
    /// <param name="logging">The logging builder to configure.</param>
    /// <param name="options">The CLI tool call configuration options.</param>
    private static void ConfigureSupportLogging(ILoggingBuilder logging, CliToolCallOptions options)
    {
        if (options.SupportLoggingFolder is null)
        {
            return;
        }

        // Set minimum log level to Debug when support logging is enabled
        logging.SetMinimumLevel(LogLevel.Debug);

        // Add file logging to the specified folder
        logging.AddSupportFileLogging(options.SupportLoggingFolder);
    }

    /// <summary>
    /// Creates a host for STDIO transport.
    /// </summary>
    /// <param name="options">The CLI tool call configuration options.</param>
    /// <returns>An IHost instance configured for STDIO transport.</returns>
    private IHost CreateStdioHost(CliToolCallOptions options)
    {
        return Host.CreateDefaultBuilder()
            .ConfigureLogging(logging =>
            {
                logging.ClearProviders();
                logging.AddEventSourceLogger();

                if (options.Debug)
                {
                    // Configure console logger to emit Debug+ to stderr so tests can capture logs from StandardError
                    logging.AddConsole(options =>
                    {
                        options.LogToStandardErrorThreshold = LogLevel.Debug;
                        options.FormatterName = ConsoleFormatterNames.Simple;
                    });
                    logging.AddSimpleConsole(simple =>
                    {
                        simple.ColorBehavior = LoggerColorBehavior.Disabled;
                        simple.IncludeScopes = false;
                        simple.SingleLine = true;
                        simple.TimestampFormat = "[HH:mm:ss] ";
                    });
                    logging.AddFilter("Microsoft.Extensions.Logging.Console.ConsoleLoggerProvider", LogLevel.Debug);
                    logging.SetMinimumLevel(LogLevel.Debug);
                }

                ConfigureSupportLogging(logging, options);
            })
            .ConfigureServices(services =>
            {
                // Configure the outgoing authentication strategy.
                services.AddSingleIdentityTokenCredentialProvider();

                ConfigureServices(services);
                services.AddAzureMcpServer(options);
            })
            .Build();
    }
}
