// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;
using System.Net;
using System.Text.Json;
using Azure.Mcp.Core.Helpers;
using Azure.Mcp.Core.Logging;
using Azure.Mcp.Core.Services.Azure.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Mcp.Core.Areas.Server.Options;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Services.Telemetry;

namespace Microsoft.Mcp.Core.Areas.Server.Commands;

/// <summary>
/// Command to publish skill telemetry from agent hooks.
/// This command is hidden from the main tool list and intended for programmatic use only.
/// </summary>
[HiddenCommand]
public sealed class SkillTelemetryCommand: BaseCommand<SkillTelemetryOptions>
{
    private const string CommandTitle = "Publish Skill Telemetry";

    public override string Id => "b3e7c1a2-4f85-4d9e-a6c3-8f2b1e0d7a94";

    public override string Name => "publish-skill-telemetry";

    public override string Description =>
        """
        Publish skill-related telemetry events from agent hooks.
        Accepts JSONL (JSON Lines) format - one JSON object per line - with fields such as 'timestamp', 'event_type',
        'tool_name', and 'session_id'. Use this command from agent hooks in clients like VS Code,
        Claude, or Copilot CLI to emit usage metrics.
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

    private static HashSet<string>? _allowedSkillPaths;
    private static readonly object _allowListLock = new();

    /// <summary>
    /// Gets the allowlist of skill-relative file paths that are permitted for telemetry.
    /// Only events with file paths matching these exact paths will be logged (with PII stripped).
    /// Loaded from allowed-skill-paths.json embedded resource.
    /// Format: skill-name\references\file.md
    /// </summary>
    private static HashSet<string> AllowedSkillPaths
    {
        get
        {
            if (_allowedSkillPaths == null)
            {
                lock (_allowListLock)
                {
                    _allowedSkillPaths ??= LoadAllowedSkillPaths();
                }
            }
            return _allowedSkillPaths;
        }
    }

    /// <summary>
    /// Loads allowed skill paths from the embedded JSON configuration file.
    /// </summary>
    /// <returns>A HashSet of allowed skill-relative paths, or an empty set if loading fails.</returns>
    private static HashSet<string> LoadAllowedSkillPaths()
    {
        try
        {
            var assembly = typeof(SkillTelemetryCommand).Assembly;
            var resourceName = "Microsoft.Mcp.Core.Areas.Server.Commands.ToolLoading.allowed-skill-paths.json";

            var json = EmbeddedResourceHelper.ReadEmbeddedResource(assembly, resourceName);
            using var jsonDocument = JsonDocument.Parse(json);
            var paths = new List<string>();
            
            foreach (var element in jsonDocument.RootElement.EnumerateArray())
            {
                var path = element.GetString();
                if (path != null)
                {
                    paths.Add(path);
                }
            }

            return new HashSet<string>(paths, StringComparer.OrdinalIgnoreCase);
        }
        catch (Exception)
        {
            // Return empty set if loading fails (fail-closed for security)
            return new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        }
    }

    /// <summary>
    /// Checks if a skill-relative path is allowed based on the exact path allowlist.
    /// </summary>
    /// <param name="skillRelativePath">The skill-relative path to check.</param>
    /// <returns>True if the path is in the allowlist, false otherwise.</returns>
    private static bool IsPathAllowed(string skillRelativePath)
    {
        return AllowedSkillPaths.Contains(skillRelativePath);
    }

    /// <summary>
    /// Extracts the skill-relative path from a full file path, removing PII.
    /// Example: C:\Users\username\.copilot\installed-plugins\azure-skills\azure\skills\azure-prepare\references\architecture.md
    /// Returns: azure-prepare\references\architecture.md
    /// </summary>
    /// <param name="fullPath">The full file path containing user information.</param>
    /// <param name="skillRelativePath">The sanitized skill-relative path if found.</param>
    /// <returns>True if a valid skill path was extracted, false otherwise.</returns>
    private static bool TryExtractSkillRelativePath(string fullPath, out string? skillRelativePath)
    {
        skillRelativePath = null;

        if (string.IsNullOrWhiteSpace(fullPath))
            return false;

        // Normalize path separators to backslash
        var normalizedPath = fullPath.Replace("/", @"\");

        // Look for common skill directory patterns
        var skillMarkers = new[] { @"\skills\", @"\azure-skills\azure\skills\" };
        
        foreach (var marker in skillMarkers)
        {
            var markerIndex = normalizedPath.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
            if (markerIndex >= 0)
            {
                // Extract everything after the marker
                var startIndex = markerIndex + marker.Length;
                skillRelativePath = normalizedPath.Substring(startIndex);
                return true;
            }
        }

        return false;
    }

    protected override void RegisterOptions(Command command)
    {
        command.Options.Add(SkillTelemetryOptionDefinitions.Timestamp);
        command.Options.Add(SkillTelemetryOptionDefinitions.EventType);
        command.Options.Add(SkillTelemetryOptionDefinitions.SessionId);
        command.Options.Add(SkillTelemetryOptionDefinitions.SkillName);
        command.Options.Add(SkillTelemetryOptionDefinitions.ToolName);
        command.Options.Add(SkillTelemetryOptionDefinitions.FilePath);
    }

    protected override SkillTelemetryOptions BindOptions(ParseResult parseResult)
    {
        return new SkillTelemetryOptions
        {
            Timestamp = parseResult.GetValueOrDefault<string?>(SkillTelemetryOptionDefinitions.Timestamp.Name),
            EventType = parseResult.GetValueOrDefault<string?>(SkillTelemetryOptionDefinitions.EventType.Name),
            SessionId = parseResult.GetValueOrDefault<string?>(SkillTelemetryOptionDefinitions.SessionId.Name),
            SkillName = parseResult.GetValueOrDefault<string?>(SkillTelemetryOptionDefinitions.SkillName.Name),
            ToolName = parseResult.GetValueOrDefault<string?>(SkillTelemetryOptionDefinitions.ToolName.Name),
            FilePath = parseResult.GetValueOrDefault<string?>(SkillTelemetryOptionDefinitions.FilePath.Name)
        };
    }

    /// <summary>
    /// Executes the skill telemetry command by validating and logging telemetry.
    /// This method validates required options, extracts and sanitizes file paths (removing PII),
    /// checks paths against the allowlist, creates a host with telemetry services,
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
            // Validate and sanitize file path if provided
            if (!string.IsNullOrWhiteSpace(options.FilePath))
            {
                // Extract skill-relative path, removing PII
                if (!TryExtractSkillRelativePath(options.FilePath, out var skillRelativePath))
                {
                    context.Response.Status = HttpStatusCode.BadRequest;
                    context.Response.Message = "Could not extract skill-relative path from file path. Path must contain a skill directory marker.";
                    return context.Response;
                }

                // Validate against allowlist
                if (!IsPathAllowed(skillRelativePath!))
                {
                    context.Response.Status = HttpStatusCode.Forbidden;
                    context.Response.Message = $"Skill path '{skillRelativePath}' is not in the allowlist and will not be logged.";
                    return context.Response;
                }

                // Store the sanitized path (PII removed) for telemetry
                options.FilePath = skillRelativePath;
            }

            // Create host and initialize telemetry service
            using var host = CreateStdioHost(options);
            await InitializeServicesAsync(host.Services);
            await host.StartAsync(cancellationToken);

            var telemetryService = host.Services.GetRequiredService<ITelemetryService>();
            LogSkillTelemetry(telemetryService, options);

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
    /// Logs skill telemetry by creating an activity and adding relevant tags.
    /// Only non-empty fields are added as tags. File paths should already be sanitized
    /// (PII removed) before calling this method.
    /// </summary>
    /// <param name="telemetryService">The telemetry service used to create and track activities.</param>
    /// <param name="options">The skill telemetry options containing event data (timestamp, event type, session ID, etc.).</param>
    internal static void LogSkillTelemetry(ITelemetryService telemetryService, SkillTelemetryOptions options)
    {
        using var activity = telemetryService.StartActivity(ActivityName.SkillsExecuted);
        
        if (activity != null)
        {
            // Add all fields as tags (FilePath has already been sanitized - PII removed in ExecuteAsync)
            var tags = new (string Key, string? Value)[]
            {
                ("Skills_EventType", options.EventType),
                ("Skills_SessionId", options.SessionId),
                ("Skills_SkillName", options.SkillName),
                ("Skills_ToolName", options.ToolName),
                ("Skills_Timestamp", options.Timestamp),
                ("Skills_FilePath", options.FilePath)
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
    /// <param name="options">The skill telemetry options that may contain a support logging folder path.</param>
    private static void ConfigureSupportLogging(ILoggingBuilder logging, SkillTelemetryOptions options)
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
    /// SkillTelemetryOptions inherits from ServiceStartOptions to enable complete service registration.
    /// </summary>
    /// <param name="options">The skill telemetry configuration options including debug and support logging settings.</param>
    /// <returns>An IHost instance configured for telemetry publishing with all required services registered.</returns>
    private IHost CreateStdioHost(SkillTelemetryOptions options)
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
