// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Core.Models.Option;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.Monitor.Options;
using Azure.Mcp.Tools.Monitor.Options.HealthModels.Entity;

namespace Azure.Mcp.Tools.Monitor.Commands.HealthModels;

public abstract class BaseMonitorHealthModelsCommand<
    [DynamicallyAccessedMembers(TrimAnnotations.CommandAnnotations)] TOptions>
    : SubscriptionCommand<TOptions>
    where TOptions : BaseMonitorHealthModelsOptions, new()
{
    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(OptionDefinitions.Common.ResourceGroup.AsRequired());
        command.Options.Add(MonitorOptionDefinitions.Health.Entity);
        command.Options.Add(MonitorOptionDefinitions.Health.HealthModel);
    }

    protected override TOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.ResourceGroup ??= parseResult.GetValueOrDefault<string>(OptionDefinitions.Common.ResourceGroup.Name);
        options.Entity = parseResult.GetValueOrDefault<string>(MonitorOptionDefinitions.Health.Entity.Name);
        options.HealthModelName = parseResult.GetValueOrDefault<string>(MonitorOptionDefinitions.Health.HealthModel.Name);
        return options;
    }
}
