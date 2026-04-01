// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;
using System.Net;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Mcp.Core.Areas.Server.Commands.ToolLoading;
using Microsoft.Mcp.Core.Areas.Server.Options;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Extensions;
using Microsoft.Mcp.Core.Logging;
using Microsoft.Mcp.Core.Models;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Services.Azure.Authentication;
using Microsoft.Mcp.Core.Services.Telemetry;

namespace Microsoft.Mcp.Core.Areas.Server.Commands;

/// <summary>
/// Command to publish plugin telemetry from agent hooks.
/// This command is hidden from the main tool list and intended for programmatic use only.
/// </summary>
[HiddenCommand]
public sealed class PluginTelemetryCommand(
    IPluginFileReferenceAllowlistProvider fileReferenceAllowlistProvider,
    IPluginSkillNameAllowlistProvider skillNameAllowlistProvider,
    IServiceProvider serviceProvider) : BaseCommand<PluginTelemetryOptions>
{
    private const string CommandTitle = "Plugin Telemetry";
    private readonly IPluginFileReferenceAllowlistProvider _fileReferenceAllowlistProvider = fileReferenceAllowlistProvider;
    private readonly IPluginSkillNameAllowlistProvider _skillNameAllowlistProvider = skillNameAllowlistProvider;
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    public override string Id => "b3e7c1a2-4f85-4d9e-a6c3-8f2b1e0d7a94";

    public override string Name => "plugin-telemetry";

    public override string Description =>
        """
        Publish plugin-related telemetry events from agent hooks.
        Accepts command-line options such as '--timestamp', '--event-type', '--session-id', '--client-type', '--client-name', 
        '--plugin-name', '--plugin-version', '--skill-name', '--skill-version', '--tool-name', and '--file-reference'. 
        Use this command from agent hooks in clients like VS Code, Claude Desktop, or Copilot CLI to emit usage metrics.
        """;

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new()
    {
        Destructive = false,
        Idempotent = false,
        OpenWorld = false,
        ReadOnly = false,
        LocalRequired = false,
        Secret = false
    };

    /// <summary>
    /// Gets or sets the service configuration action.
    /// This is set by the host application (e.g., Program.cs) to provide service registration.
    /// </summary>
    public static Action<IServiceCollection> ConfigureServices { get; set; } = _ => { };

    /// <summary>
    /// Gets or sets the service initialization action.
    /// This is set by the host application (e.g., Program.cs) to provide service initialization logic.
    /// </summary>
    public static Func<IServiceProvider, Task> InitializeServicesAsync { get; set; } = _ => Task.CompletedTask;

    /// <summary>
    /// Checks if a plugin-relative path is allowed based on the exact path allowlist.
    /// </summary>
    /// <param name="pluginRelativePath">The plugin-relative path to check.</param>
    /// <param name="allowlistProvider">The provider that validates allowed file references.</param>
    /// <returns>True if the path is in the allowlist, false otherwise.</returns>
    private static bool IsPathAllowed(string pluginRelativePath, IPluginFileReferenceAllowlistProvider allowlistProvider)
    {
        return allowlistProvider.IsPathAllowed(pluginRelativePath);
    }

    /// <summary>
    /// Checks if a skill name is allowed based on the exact skill name allowlist.
    /// </summary>
    /// <param name="skillName">The skill name to check.</param>
    /// <param name="allowlistProvider">The provider that validates allowed skill names.</param>
    /// <returns>True if the skill name is in the allowlist, false otherwise.</returns>
    private static bool IsSkillNameAllowed(string skillName, IPluginSkillNameAllowlistProvider allowlistProvider)
    {
        return allowlistProvider.IsSkillNameAllowed(skillName);
    }

    /// <summary>
    /// Known client-specific prefixes for tool names, ordered most specific first.
    /// Each client wraps the azmcp command name with a different prefix:
    /// - Claude Code: mcp__plugin_azure_azure__
    /// - VS Code: mcp_azure_mcp_
    /// - Copilot CLI: azure-
    /// </summary>
    private static readonly string[] KnownToolNamePrefixes =
    [
        "mcp__plugin_azure_azure__",
        "mcp_azure_mcp_",
        "azure-"
    ];

    /// <summary>
    /// Strips known client-specific prefixes from a tool name to extract the azmcp command name.
    /// For example, "mcp__plugin_azure_azure__pricing" becomes "pricing".
    /// Returns the original name if no known prefix is found.
    /// </summary>
    internal static string StripClientPrefix(string toolName)
    {
        foreach (var prefix in KnownToolNamePrefixes)
        {
            if (toolName.StartsWith(prefix, StringComparison.Ordinal))
            {
                return toolName[prefix.Length..];
            }
        }

        return toolName;
    }

    /// <summary>
    /// Azure extension tool names that are not azmcp commands but should still be tracked.
    /// These are tools from the @azure VS Code extension (e.g., auth, resource graph, templates).
    /// </summary>
    private static readonly HashSet<string> AllowlistedExtensionTools = new(StringComparer.Ordinal)
    {
        "azure_auth-set_auth_context",
        "azure_get_auth_context",
        "azure_get_dotnet_template_tags",
        "azure_get_dotnet_templates_for_tag",
        "azure_query_azure_resource_graph",
        "azure_recommend_custom_modes"
    };

    /// <summary>
    /// Validates a tool name against registered commands by stripping client prefixes
    /// and checking if the result is a known command name or area name.
    /// Also allows explicitly allowlisted Azure extension tools to pass through as-is.
    /// Returns the normalized azmcp tool name if valid, or null if not recognized.
    /// </summary>
    internal static string? ValidateAndNormalizeToolName(string toolName, ICommandFactory commandFactory)
    {
        // Check allowlisted Azure extension tools first (pass through without normalization)
        if (AllowlistedExtensionTools.Contains(toolName))
        {
            return toolName;
        }

        var normalizedName = StripClientPrefix(toolName);

        if (string.IsNullOrEmpty(normalizedName))
        {
            return null;
        }

        // Check for exact command match (e.g., "group_resource_list", "subscription_list")
        if (commandFactory.FindCommandByName(normalizedName) != null)
        {
            return normalizedName;
        }

        // Check for area-level match (e.g., "pricing" matches area with commands like "pricing_get")
        // Case-sensitive: area names must be lowercase as registered
        foreach (var subGroup in commandFactory.RootGroup.SubGroup)
        {
            if (string.Equals(subGroup.Name, normalizedName, StringComparison.Ordinal))
            {
                return normalizedName;
            }
        }

        return null;
    }

    protected override void RegisterOptions(Command command)
    {
        command.Options.Add(PluginTelemetryOptionDefinitions.Timestamp);
        command.Options.Add(PluginTelemetryOptionDefinitions.EventType);
        command.Options.Add(PluginTelemetryOptionDefinitions.SessionId);
        command.Options.Add(PluginTelemetryOptionDefinitions.ClientType);
        command.Options.Add(PluginTelemetryOptionDefinitions.ClientName);
        command.Options.Add(PluginTelemetryOptionDefinitions.PluginName);
        command.Options.Add(PluginTelemetryOptionDefinitions.PluginVersion);
        command.Options.Add(PluginTelemetryOptionDefinitions.SkillName);
        command.Options.Add(PluginTelemetryOptionDefinitions.SkillVersion);
        command.Options.Add(PluginTelemetryOptionDefinitions.ToolName);
        command.Options.Add(PluginTelemetryOptionDefinitions.FileReference);
    }

    protected override PluginTelemetryOptions BindOptions(ParseResult parseResult)
    {
        return new PluginTelemetryOptions
        {
            Timestamp = parseResult.GetValueOrDefault<string?>(PluginTelemetryOptionDefinitions.Timestamp.Name),
            EventType = parseResult.GetValueOrDefault<string?>(PluginTelemetryOptionDefinitions.EventType.Name),
            SessionId = parseResult.GetValueOrDefault<string?>(PluginTelemetryOptionDefinitions.SessionId.Name),
            ClientType = parseResult.GetValueOrDefault<string?>(PluginTelemetryOptionDefinitions.ClientType.Name),
            ClientName = parseResult.GetValueOrDefault<string?>(PluginTelemetryOptionDefinitions.ClientName.Name),
            PluginName = parseResult.GetValueOrDefault<string?>(PluginTelemetryOptionDefinitions.PluginName.Name),
            PluginVersion = parseResult.GetValueOrDefault<string?>(PluginTelemetryOptionDefinitions.PluginVersion.Name),
            SkillName = parseResult.GetValueOrDefault<string?>(PluginTelemetryOptionDefinitions.SkillName.Name),
            SkillVersion = parseResult.GetValueOrDefault<string?>(PluginTelemetryOptionDefinitions.SkillVersion.Name),
            ToolName = parseResult.GetValueOrDefault<string?>(PluginTelemetryOptionDefinitions.ToolName.Name),
            FileReference = parseResult.GetValueOrDefault<string?>(PluginTelemetryOptionDefinitions.FileReference.Name)
        };
    }

    /// <summary>
    /// Executes the plugin telemetry command by validating and logging telemetry.
    /// This method validates required options, checks paths and skill names against allowlists,
    /// validates tool names against registered commands, creates a host with telemetry services,
    /// and logs the telemetry event.
    /// </summary>
    /// <param name="context">The command execution context containing the response object.</param>
    /// <param name="parseResult">The parsed command-line arguments containing telemetry event data.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>A task containing the command response with status and any error messages.</returns>
    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult, CancellationToken cancellationToken)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
        {
            return context.Response;
        }

        var options = BindOptions(parseResult);

        try
        {
            // Validate file reference if provided
            if (!string.IsNullOrWhiteSpace(options.FileReference))
            {
                // Validate against allowlist
                if (!IsPathAllowed(options.FileReference, _fileReferenceAllowlistProvider))
                {
                    context.Response.Status = HttpStatusCode.Forbidden;
                    context.Response.Message = $"Plugin file reference '{options.FileReference}' is not in the allowlist and will not be logged.";
                    return context.Response;
                }
            }

            // Validate skill name if provided
            if (!string.IsNullOrWhiteSpace(options.SkillName))
            {
                // Validate against allowlist
                if (!IsSkillNameAllowed(options.SkillName, _skillNameAllowlistProvider))
                {
                    context.Response.Status = HttpStatusCode.Forbidden;
                    context.Response.Message = $"Skill name '{options.SkillName}' is not in the allowlist and will not be logged.";
                    return context.Response;
                }
            }

            // Validate tool name if provided: strip client-specific prefixes and check against registered commands/areas
            if (!string.IsNullOrWhiteSpace(options.ToolName))
            {
                // Resolve ICommandFactory lazily to avoid circular dependency during construction
                var commandFactory = _serviceProvider.GetRequiredService<ICommandFactory>();

                var normalizedToolName = ValidateAndNormalizeToolName(options.ToolName, commandFactory);
                if (normalizedToolName == null)
                {
                    context.Response.Status = HttpStatusCode.Forbidden;
                    context.Response.Message = $"Tool name '{options.ToolName}' is not a recognized azmcp command and will not be logged.";
                    return context.Response;
                }

                // Replace with normalized name for clean telemetry data
                options.ToolName = normalizedToolName;
            }

            // Create host and log telemetry
            using var host = CreateStdioHost(options);
            await InitializeServicesAsync(host.Services);
            await host.StartAsync(cancellationToken);

            var telemetryService = host.Services.GetRequiredService<ITelemetryService>();
            LogPluginTelemetry(telemetryService, options);

            await host.StopAsync(cancellationToken);
            await host.WaitForShutdownAsync(cancellationToken);

            return context.Response;
        }
        catch (Exception ex)
        {
            HandleException(context, ex);
        }

        return context.Response;
    }

    /// <summary>
    /// Logs plugin telemetry by creating an activity and adding relevant tags.
    /// Only non-empty fields are added as tags. File references and skill names
    /// should be validated against their respective allowlists, and tool names
    /// should be validated as registered commands, before calling this method.
    /// </summary>
    /// <param name="telemetryService">The telemetry service used to create and track activities.</param>
    /// <param name="options">The plugin telemetry options containing event data (timestamp, event type, session ID, etc.).</param>
    internal static void LogPluginTelemetry(ITelemetryService telemetryService, PluginTelemetryOptions options)
    {
        using var activity = telemetryService.StartActivity(ActivityName.PluginExecuted);

        if (activity != null)
        {
            // Add all fields as tags (FileReference has already been validated in ExecuteAsync)
            var tags = new (string Key, string? Value)[]
            {
                ("Plugin_EventType", options.EventType),
                ("Plugin_SessionId", options.SessionId),
                ("Plugin_ClientType", options.ClientType),
                ("Plugin_ClientName", options.ClientName),
                ("Plugin_PluginName", options.PluginName),
                ("Plugin_PluginVersion", options.PluginVersion),
                ("Plugin_SkillName", options.SkillName),
                ("Plugin_SkillVersion", options.SkillVersion),
                ("Plugin_ToolName", options.ToolName),
                ("Plugin_Timestamp", options.Timestamp),
                ("Plugin_FileReference", options.FileReference)
            };

            foreach (var (key, value) in tags)
            {
                if (!string.IsNullOrEmpty(value))
                {
                    activity.AddTag(key, value);
                }
            }

            activity.SetStatus(ActivityStatusCode.Ok);
        }
    }

    /// <summary>
    /// Configures support logging when a support logging folder is specified.
    /// This enables debug-level logging for troubleshooting and support purposes.
    /// If no support logging folder is specified, this method does nothing.
    /// </summary>
    /// <param name="logging">The logging builder to configure.</param>
    /// <param name="options">The plugin telemetry options that may contain a support logging folder path.</param>
    private static void ConfigureSupportLogging(ILoggingBuilder logging, PluginTelemetryOptions options)
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
    /// Creates a host for STDIO transport with full MCP server services.
    /// The host is configured with logging (including debug and support logging if enabled),
    /// authentication services, custom services from ConfigureServices, and the full Azure MCP server stack.
    /// PluginTelemetryOptions inherits from ServiceStartOptions to enable complete service registration.
    /// </summary>
    /// <param name="options">The plugin telemetry configuration options including debug and support logging settings.</param>
    /// <returns>An IHost instance configured for telemetry publishing with all required services registered.</returns>
    private IHost CreateStdioHost(PluginTelemetryOptions options)
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
                // Configure the outgoing authentication strategy
                services.AddSingleIdentityTokenCredentialProvider();

                // Allow custom service configuration
                ConfigureServices(services);

                // Register full Azure MCP Server services (works because SkillTelemetryOptions inherits from ServiceStartOptions)
                services.AddAzureMcpServer(options);
            })
            .Build();
    }
}
