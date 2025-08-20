// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.AppConfig.Commands.Account;
using Azure.Mcp.Tools.AppConfig.Commands.KeyValue;
using Azure.Mcp.Tools.AppConfig.Services;
using Azure.Mcp.Core.Areas;
using Azure.Mcp.Core.Commands;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.AppConfig;

public class AppConfigSetup : IAreaSetup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IAppConfigService, AppConfigService>();
    }

    public void RegisterCommands(CommandGroup rootGroup, ILoggerFactory loggerFactory)
    {
        // Create AppConfig command group
        var appConfig = new CommandGroup("appconfig", "App Configuration operations - Commands for managing Azure App Configuration stores and key-value settings. Includes operations for listing configuration stores, managing key-value pairs, setting labels, locking/unlocking settings, and retrieving configuration data.");
        rootGroup.AddSubGroup(appConfig);

        // Create AppConfig subgroups
        var accounts = new CommandGroup("account", "App Configuration store operations");
        appConfig.AddSubGroup(accounts);

        var keyValue = new CommandGroup("kv", "App Configuration key-value setting operations - Commands for managing complete configuration settings including values, labels, and metadata");
        appConfig.AddSubGroup(keyValue);

        // Register AppConfig commands
        accounts.AddCommand("list", new AccountListCommand(
            loggerFactory.CreateLogger<AccountListCommand>()));

        keyValue.AddCommand("list", new KeyValueListCommand(
            loggerFactory.CreateLogger<KeyValueListCommand>()));
        keyValue.AddCommand("lock", new KeyValueLockCommand(
            loggerFactory.CreateLogger<KeyValueLockCommand>()));
        keyValue.AddCommand("unlock", new KeyValueUnlockCommand(
            loggerFactory.CreateLogger<KeyValueUnlockCommand>()));
        keyValue.AddCommand("set", new KeyValueSetCommand(
            loggerFactory.CreateLogger<KeyValueSetCommand>()));
        keyValue.AddCommand("show", new KeyValueShowCommand(
            loggerFactory.CreateLogger<KeyValueShowCommand>()));
        keyValue.AddCommand("delete", new KeyValueDeleteCommand(
            loggerFactory.CreateLogger<KeyValueDeleteCommand>()));
    }
}
