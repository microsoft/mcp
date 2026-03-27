// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

using Azure.Mcp.Tools.AppConfig.Commands.Account;
using Azure.Mcp.Tools.AppConfig.Commands.KeyValue;
using Azure.Mcp.Tools.AppConfig.Commands.KeyValue.Lock;
using Azure.Mcp.Tools.AppConfig.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Areas;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Commands.Descriptors;

namespace Azure.Mcp.Tools.AppConfig;

public sealed class AppConfigRegistration : IAreaRegistration
{
    public static string AreaName => "appconfig";

    public static string AreaTitle => "App Configuration Management";

    public static CommandCategory Category => CommandCategory.AzureServices;

    public static CommandGroupDescriptor GetCommandDescriptors() => new()
    {
        Name = AreaName,
        Description = "App Configuration operations - Commands for managing Azure App Configuration stores and key-value settings. Includes operations for listing configuration stores, managing key-value pairs, setting labels, locking/unlocking settings, and retrieving configuration data.",
        Title = AreaTitle,
        SubGroups =
        [
            new CommandGroupDescriptor
            {
                Name = "account",
                Description = "App Configuration store operations",
                Commands =
                [
                    new CommandDescriptor
                    {
                        Id = "e403c988-b57b-4ac1-afb7-25ba3fdd6e6a",
                        Name = "list",
                        Description = "List all App Configuration stores in a subscription. This command retrieves and displays all App Configuration stores available in the specified subscription. Results include store names returned as a JSON array.",
                        Title = "List",
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
                        Kind = CommandKind.Subscription,
                        HandlerType = nameof(AccountListCommand)
                    },
                ],
            },
            new CommandGroupDescriptor
            {
                Name = "kv",
                Description = "App Configuration key-value setting operations - Commands for managing complete configuration settings including values, labels, and metadata",
                Commands =
                [
                    new CommandDescriptor
                    {
                        Id = "f885a499-82ec-4897-a788-fb6b4615ab06",
                        Name = "delete",
                        Description = "Delete a key-value pair from an App Configuration store. This command removes the specified key-value pair from the store. If a label is specified, only the labeled version is deleted. If no label is specified, the key-value with the matching key and the default label will be deleted.",
                        Title = "Delete",
                        Annotations = new ToolAnnotations
                        {
                            Destructive = true,
                            Idempotent = true,
                            OpenWorld = false,
                            ReadOnly = false,
                            LocalRequired = false,
                            Secret = false,
                        },
                        Options =
                        [
                            new OptionDescriptor
                            {
                                Name = "account",
                                Description = "The name of the App Configuration store (e.g., my-appconfig).",
                                TypeName = "string",
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "key",
                                Description = "The name of the key to access within the App Configuration store.",
                                TypeName = "string",
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "label",
                                Description = "The label to apply to the configuration key. Labels are used to group and organize settings.",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "content-type",
                                Description = "The content type of the configuration value. This is used to indicate how the value should be interpreted or parsed.",
                                TypeName = "string",
                            },
                        ],
                        Kind = CommandKind.Subscription,
                        HandlerType = nameof(KeyValueDeleteCommand)
                    },
                    new CommandDescriptor
                    {
                        Id = "abc28800-ae4a-4369-9ec0-2653a578e82a",
                        Name = "get",
                        Description = "Gets key-values in an App Configuration store. This command can either retrieve a specific key-value by its key and optional label, or list key-values if no key is provided. Listing key-values can optionally be filtered by a key filter and label filter. Each key-value includes its key, value, label, content type, ETag, last modified time, and lock status.",
                        Title = "Get",
                        Annotations = new ToolAnnotations
                        {
                            Destructive = false,
                            Idempotent = true,
                            OpenWorld = false,
                            ReadOnly = true,
                            LocalRequired = false,
                            Secret = false,
                        },
                        Options =
                        [
                            new OptionDescriptor
                            {
                                Name = "account",
                                Description = "The name of the App Configuration store (e.g., my-appconfig).",
                                TypeName = "string",
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "key",
                                Description = "The name of the key to access within the App Configuration store.",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "label",
                                Description = "The label to apply to the configuration key. Labels are used to group and organize settings.",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "key-filter",
                                Description = "Specifies the key filter, if any, to be used when retrieving key-values. The filter can be an exact match, for example a filter of 'foo' would get all key-values with a key of 'foo', or the filter can include a '*' character at the end of the string for wildcard searches (e.g., 'App*'). If omitted all keys will be retrieved.",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "label-filter",
                                Description = "Specifies the label filter, if any, to be used when retrieving key-values. The filter can be an exact match, for example a filter of 'foo' would get all key-values with a label of 'foo', or the filter can include a '*' character at the end of the string for wildcard searches (e.g., 'Prod*'). This filter is case-sensitive. If omitted, all labels will be retrieved.",
                                TypeName = "string",
                            },
                        ],
                        Kind = CommandKind.Subscription,
                        HandlerType = nameof(KeyValueGetCommand)
                    },
                    new CommandDescriptor
                    {
                        Id = "a89086eb-acf4-4168-9d32-de5cd7384030",
                        Name = "set",
                        Description = "Set a key-value setting in an App Configuration store. This command creates or updates a key-value setting with the specified value. You must specify an account name, key, and value. Optionally, you can specify a label otherwise the default label will be used. You can also specify a content type to indicate how the value should be interpreted. You can add tags in the format 'key=value' to associate metadata with the setting.",
                        Title = "Set",
                        Annotations = new ToolAnnotations
                        {
                            Destructive = true,
                            Idempotent = true,
                            OpenWorld = false,
                            ReadOnly = false,
                            LocalRequired = false,
                            Secret = false,
                        },
                        Options =
                        [
                            new OptionDescriptor
                            {
                                Name = "account",
                                Description = "The name of the App Configuration store (e.g., my-appconfig).",
                                TypeName = "string",
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "key",
                                Description = "The name of the key to access within the App Configuration store.",
                                TypeName = "string",
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "label",
                                Description = "The label to apply to the configuration key. Labels are used to group and organize settings.",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "content-type",
                                Description = "The content type of the configuration value. This is used to indicate how the value should be interpreted or parsed.",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "value",
                                Description = "The value to set for the configuration key.",
                                TypeName = "string",
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "tags",
                                Description = "The tags to associate with the configuration key. Tags should be in the format 'key=value'. Multiple tags can be specified.",
                                TypeName = "string",
                            },
                        ],
                        Kind = CommandKind.Subscription,
                        HandlerType = nameof(KeyValueSetCommand)
                    },
                ],
                SubGroups =
                [
                    new CommandGroupDescriptor
                    {
                        Name = "lock",
                        Description = "App Configuration key-value lock operations - Commands for locking and unlocking key-value settings to prevent or allow modifications",
                        Commands =
                        [
                            new CommandDescriptor
                            {
                                Id = "b48fd781-d74a-4dfd-a29c-421ded9a6ce9",
                                Name = "set",
                                Description = "Sets the lock state of a key-value in an App Configuration store. This command can lock and unlock key-values. Locking sets a key-value to read-only mode, preventing any modifications to its value. Unlocking removes the read-only mode from a key-value setting, allowing modifications to its value. You must specify an account name and key. Optionally, you can specify a label to lock or unlock a specific labeled version of the key-value. Default is unlocking the key-value.",
                                Title = "Set",
                                Annotations = new ToolAnnotations
                                {
                                    Destructive = false,
                                    Idempotent = true,
                                    OpenWorld = false,
                                    ReadOnly = false,
                                    LocalRequired = false,
                                    Secret = false,
                                },
                                Options =
                                [
                                    new OptionDescriptor
                                    {
                                        Name = "account",
                                        Description = "The name of the App Configuration store (e.g., my-appconfig).",
                                        TypeName = "string",
                                        Required = true,
                                    },
                                    new OptionDescriptor
                                    {
                                        Name = "key",
                                        Description = "The name of the key to access within the App Configuration store.",
                                        TypeName = "string",
                                        Required = true,
                                    },
                                    new OptionDescriptor
                                    {
                                        Name = "label",
                                        Description = "The label to apply to the configuration key. Labels are used to group and organize settings.",
                                        TypeName = "string",
                                    },
                                    new OptionDescriptor
                                    {
                                        Name = "content-type",
                                        Description = "The content type of the configuration value. This is used to indicate how the value should be interpreted or parsed.",
                                        TypeName = "string",
                                    },
                                    new OptionDescriptor
                                    {
                                        Name = "lock",
                                        Description = "Whether a key-value will be locked (set to read-only) or unlocked (read-only removed).",
                                        TypeName = "string",
                                    },
                                ],
                                Kind = CommandKind.Subscription,
                                HandlerType = nameof(KeyValueSetCommand)
                            },
                        ],
                    },
                ],
            },
        ],
    };

    public static void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton<IAppConfigService, AppConfigService>();
        services.AddSingleton<AccountListCommand>();
        services.AddSingleton<KeyValueDeleteCommand>();
        services.AddSingleton<KeyValueGetCommand>();
        services.AddSingleton<KeyValueSetCommand>();
        services.AddSingleton<KeyValueLockSetCommand>();
    }

    public static IBaseCommand ResolveHandler(string handlerTypeName, IServiceProvider serviceProvider) =>
        handlerTypeName switch
        {
            nameof(AccountListCommand) => serviceProvider.GetRequiredService<AccountListCommand>(),
            nameof(KeyValueDeleteCommand) => serviceProvider.GetRequiredService<KeyValueDeleteCommand>(),
            nameof(KeyValueGetCommand) => serviceProvider.GetRequiredService<KeyValueGetCommand>(),
            nameof(KeyValueSetCommand) => serviceProvider.GetRequiredService<KeyValueSetCommand>(),
            _ => throw new InvalidOperationException($"Unknown handler type '{{handlerTypeName}}' in appconfig area.")
        };
}
