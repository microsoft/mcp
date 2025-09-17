// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Models.Option;
using Azure.Mcp.Tools.Monitor.Commands;
using Azure.Mcp.Tools.Monitor.Options;

namespace Azure.Mcp.Tools.Monitor.Commands.HealthModels;

public abstract class BaseMonitorHealthModelsCommand<
    [DynamicallyAccessedMembers(TrimAnnotations.CommandAnnotations)] TOptions>
    : BaseMonitorCommand<TOptions>
    where TOptions : BaseMonitorHealthModelsOptions, new()
{
    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(MonitorOptionDefinitions.Health.Entity);
        command.Options.Add(MonitorOptionDefinitions.Health.HealthModel);
    }

    protected override TOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Entity = parseResult.GetValueOrDefault<string>(MonitorOptionDefinitions.Health.Entity.Name);
        options.HealthModelName = parseResult.GetValueOrDefault<string>(MonitorOptionDefinitions.Health.HealthModel.Name);
        return options;
    }
}
