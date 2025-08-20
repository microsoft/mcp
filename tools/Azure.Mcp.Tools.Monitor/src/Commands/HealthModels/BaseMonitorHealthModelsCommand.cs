// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.Monitor.Options;

namespace Azure.Mcp.Tools.Monitor.Commands.HealthModels;

public abstract class BaseMonitorHealthModelsCommand<
    [DynamicallyAccessedMembers(TrimAnnotations.CommandAnnotations)] TOptions>
    : SubscriptionCommand<TOptions>
    where TOptions : SubscriptionOptions, new()
{
    protected readonly Option<string> _entityOption = MonitorOptionDefinitions.Health.Entity;
    protected readonly Option<string> _healthModelOption = MonitorOptionDefinitions.Health.HealthModel;

    protected BaseMonitorHealthModelsCommand() : base()
    {
    }
}
