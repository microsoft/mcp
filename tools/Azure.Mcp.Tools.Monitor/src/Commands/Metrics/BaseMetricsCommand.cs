// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.Monitor.Options;
using Azure.Mcp.Tools.Monitor.Options.Metrics;

namespace Azure.Mcp.Tools.Monitor.Commands.Metrics;

/// <summary>
/// Base command for all metrics operations
/// </summary>
public abstract class BaseMetricsCommand<
    [DynamicallyAccessedMembers(TrimAnnotations.CommandAnnotations)] TOptions>
    : SubscriptionCommand<TOptions>
    where TOptions : SubscriptionOptions, IMetricsOptions, new()
{
    protected readonly Option<string> _resourceTypeOption = MonitorOptionDefinitions.Metrics.ResourceType;
    protected readonly Option<string> _resourceNameOption = MonitorOptionDefinitions.Metrics.ResourceName;

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(_resourceTypeOption);
        command.Options.Add(_resourceNameOption);
        UseResourceGroup();
    }

    protected override TOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.ResourceType = parseResult.GetValue(_resourceTypeOption);
        options.ResourceName = parseResult.GetValue(_resourceNameOption);
        return options;
    }
}
