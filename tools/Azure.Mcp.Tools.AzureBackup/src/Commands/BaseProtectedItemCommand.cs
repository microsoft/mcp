// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;
using Azure.Mcp.Tools.AzureBackup.Options;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Extensions;
using Microsoft.Mcp.Core.Models.Option;

namespace Azure.Mcp.Tools.AzureBackup.Commands;

public abstract class BaseProtectedItemCommand<
    [DynamicallyAccessedMembers(TrimAnnotations.CommandAnnotations)] T>
    : BaseAzureBackupCommand<T>
    where T : BaseProtectedItemOptions, new()
{
    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(AzureBackupOptionDefinitions.ProtectedItem.AsRequired());
        command.Options.Add(AzureBackupOptionDefinitions.Container);
    }

    protected override T BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.ProtectedItem = parseResult.GetValueOrDefault<string>(AzureBackupOptionDefinitions.ProtectedItem.Name);
        options.Container = parseResult.GetValueOrDefault<string>(AzureBackupOptionDefinitions.Container.Name);
        return options;
    }
}
