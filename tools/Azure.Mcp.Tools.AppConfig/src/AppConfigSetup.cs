// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Areas;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Tools.AppConfig.Commands.Account;
using Azure.Mcp.Tools.AppConfig.Commands.KeyValue;
using Azure.Mcp.Tools.AppConfig.Commands.KeyValue.Lock;
using Azure.Mcp.Tools.AppConfig.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Azure.Mcp.Tools.AppConfig;

public class AppConfigSetup : IAreaSetup
{
    public string Name => "appconfig";

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IAppConfigService, AppConfigService>();

        services.AddSingleton<AccountListCommand>();

        services.AddSingleton<KeyValueDeleteCommand>();
        services.AddSingleton<KeyValueGetCommand>();
        services.AddSingleton<KeyValueSetCommand>();

        services.AddSingleton<KeyValueLockSetCommand>();
    }

    public CommandGroup RegisterCommands(IServiceProvider serviceProvider)
    {
        // Create AppConfig command group
        var appConfig = new CommandGroup(Name, "App Configuration operations - Commands for managing Azure App Configuration stores and key-value settings. Includes operations for listing configuration stores, managing key-value pairs, setting labels, locking/unlocking settings, and retrieving configuration data.");

        // Create AppConfig subgroups
        var accounts = new CommandGroup("account", "App Configuration store operations");
        appConfig.AddSubGroup(accounts);

        var keyValue = new CommandGroup("kv", "App Configuration key-value setting operations - Commands for managing complete configuration settings including values, labels, and metadata");
        appConfig.AddSubGroup(keyValue);

        // Create Lock subgroup under KeyValue
        var lockGroup = new CommandGroup("lock", "App Configuration key-value lock operations - Commands for locking and unlocking key-value settings to prevent or allow modifications");
        keyValue.AddSubGroup(lockGroup);

        // Register AppConfig commands
        var accountList = serviceProvider.GetRequiredService<AccountListCommand>();
        accounts.AddCommand(accountList.Name, accountList);

        var keyValueDelete = serviceProvider.GetRequiredService<KeyValueDeleteCommand>();
        keyValue.AddCommand(keyValueDelete.Name, keyValueDelete);
        var keyValueGet = serviceProvider.GetRequiredService<KeyValueGetCommand>();
        keyValue.AddCommand(keyValueGet.Name, keyValueGet);
        var keyValueSet = serviceProvider.GetRequiredService<KeyValueSetCommand>();
        keyValue.AddCommand(keyValueSet.Name, keyValueSet);

        var keyValueLockSet = serviceProvider.GetRequiredService<KeyValueLockSetCommand>();
        lockGroup.AddCommand(keyValueLockSet.Name, keyValueLockSet);

        return appConfig;
    }
}
