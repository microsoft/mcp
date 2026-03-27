// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Areas;
using Microsoft.Mcp.Core.Areas.Server.Commands;
using Microsoft.Mcp.Core.Areas.Server.Commands.ToolLoading;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Commands.Descriptors;

namespace Microsoft.Mcp.Core.Areas.Server;

public sealed class ServerRegistration : IAreaRegistration
{
    public static string AreaName => "server";

    public static string AreaTitle => "MCP Server Management";

    public static CommandCategory Category => CommandCategory.Mcp;

    public static CommandGroupDescriptor GetCommandDescriptors() => new()
    {
        Name = AreaName,
        Description = "MCP Server operations - Commands for managing and interacting with the MCP Server.",
        Title = AreaTitle,
        Commands =
        [
            new CommandDescriptor
            {
                Id = "9953ff62-e3d7-4bdf-9b70-d569e54e3df1",
                Name = "start",
                Description = "Starts Azure MCP Server.",
                Title = "Start MCP Server",
                Annotations = new ToolAnnotations
                {
                    Destructive = false,
                    Idempotent = false,
                    OpenWorld = false,
                    ReadOnly = true,
                    LocalRequired = false,
                    Secret = false,
                },
                Options =
                [
                    new OptionDescriptor
                    {
                        Name = "transport",
                        Description = "The transport protocol to use (stdio or http).",
                        TypeName = "string",
                    },
                    new OptionDescriptor
                    {
                        Name = "namespace",
                        Description = "Filter tools by namespace.",
                        TypeName = "string",
                    },
                    new OptionDescriptor
                    {
                        Name = "mode",
                        Description = "Server mode.",
                        TypeName = "string",
                    },
                    new OptionDescriptor
                    {
                        Name = "tool",
                        Description = "Specific tool to expose.",
                        TypeName = "string",
                    },
                    new OptionDescriptor
                    {
                        Name = "read-only",
                        Description = "Run in read-only mode.",
                        TypeName = "boolean",
                    },
                    new OptionDescriptor
                    {
                        Name = "debug",
                        Description = "Enable debug mode.",
                        TypeName = "boolean",
                    },
                    new OptionDescriptor
                    {
                        Name = "dangerously-disable-http-incoming-auth",
                        Description = "Disable HTTP incoming authentication (dangerous).",
                        TypeName = "boolean",
                    },
                    new OptionDescriptor
                    {
                        Name = "dangerously-disable-elicitation",
                        Description = "Disable elicitation (dangerous).",
                        TypeName = "boolean",
                    },
                    new OptionDescriptor
                    {
                        Name = "outgoing-auth-strategy",
                        Description = "Outgoing authentication strategy.",
                        TypeName = "string",
                    },
                    new OptionDescriptor
                    {
                        Name = "dangerously-write-support-logs-to-dir",
                        Description = "Write support logs to directory (dangerous).",
                        TypeName = "string",
                    },
                    new OptionDescriptor
                    {
                        Name = "dangerously-disable-retry-limits",
                        Description = "Disable retry limits (dangerous).",
                        TypeName = "boolean",
                    },
                    new OptionDescriptor
                    {
                        Name = "cloud",
                        Description = "Cloud environment.",
                        TypeName = "string",
                    },
                ],
                Kind = CommandKind.Basic,
                HandlerType = nameof(ServiceStartCommand)
            },
            new CommandDescriptor
            {
                Id = "add0f6fe-258c-45c4-af74-0c165d4913cb",
                Name = "info",
                Description = "Displays running MCP server information.",
                Title = "Server information.",
                Hidden = true,
                Annotations = new ToolAnnotations
                {
                    Destructive = false,
                    Idempotent = true,
                    OpenWorld = false,
                    ReadOnly = true,
                    LocalRequired = false,
                    Secret = false,
                },
                Options = [],
                Kind = CommandKind.Basic,
                HandlerType = nameof(ServiceInfoCommand)
            },
            new CommandDescriptor
            {
                Id = "b3e7c1a2-4f85-4d9e-a6c3-8f2b1e0d7a94",
                Name = "plugin-telemetry",
                Description = "Publish plugin-related telemetry events from agent hooks.",
                Title = "Plugin Telemetry",
                Hidden = true,
                Annotations = new ToolAnnotations
                {
                    Destructive = false,
                    Idempotent = false,
                    OpenWorld = false,
                    ReadOnly = false,
                    LocalRequired = false,
                    Secret = false,
                },
                Options =
                [
                    new OptionDescriptor
                    {
                        Name = "timestamp",
                        Description = "Event timestamp.",
                        TypeName = "string",
                    },
                    new OptionDescriptor
                    {
                        Name = "event-type",
                        Description = "Type of telemetry event.",
                        TypeName = "string",
                    },
                    new OptionDescriptor
                    {
                        Name = "session-id",
                        Description = "Session identifier.",
                        TypeName = "string",
                    },
                    new OptionDescriptor
                    {
                        Name = "client-type",
                        Description = "Client type identifier.",
                        TypeName = "string",
                    },
                    new OptionDescriptor
                    {
                        Name = "plugin-name",
                        Description = "Plugin name.",
                        TypeName = "string",
                    },
                    new OptionDescriptor
                    {
                        Name = "plugin-version",
                        Description = "Plugin version.",
                        TypeName = "string",
                    },
                    new OptionDescriptor
                    {
                        Name = "skill-name",
                        Description = "Skill name.",
                        TypeName = "string",
                    },
                    new OptionDescriptor
                    {
                        Name = "skill-version",
                        Description = "Skill version.",
                        TypeName = "string",
                    },
                    new OptionDescriptor
                    {
                        Name = "tool-name",
                        Description = "Tool name.",
                        TypeName = "string",
                    },
                    new OptionDescriptor
                    {
                        Name = "file-reference",
                        Description = "File reference.",
                        TypeName = "string",
                    },
                ],
                Kind = CommandKind.Basic,
                HandlerType = nameof(PluginTelemetryCommand)
            },
        ],
    };

    public static void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton<ServiceStartCommand>();
        services.AddSingleton<ServiceInfoCommand>();
        services.AddSingleton<PluginTelemetryCommand>();
    }

    public static IBaseCommand ResolveHandler(string handlerTypeName, IServiceProvider serviceProvider) =>
        handlerTypeName switch
        {
            nameof(ServiceStartCommand) => serviceProvider.GetRequiredService<ServiceStartCommand>(),
            nameof(ServiceInfoCommand) => serviceProvider.GetRequiredService<ServiceInfoCommand>(),
            nameof(PluginTelemetryCommand) => serviceProvider.GetRequiredService<PluginTelemetryCommand>(),
            _ => throw new InvalidOperationException($"Unknown handler type '{handlerTypeName}' in server area.")
        };
}
