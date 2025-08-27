// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Commands.Subscription;
using AzureMcp.Ai.Options;

namespace AzureMcp.Ai.Commands;

public abstract class BaseAiCommand<
    [DynamicallyAccessedMembers(TrimAnnotations.CommandAnnotations)] T>
    : SubscriptionCommand<T>
    where T : BaseAiOptions, new()
{
    protected readonly Option<string> _resourceNameOption = AiOptionDefinitions.ResourceName;

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.AddOption(_resourceNameOption);
        RequireResourceGroup();
    }

    protected override T BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.ResourceName = parseResult.GetValueForOption(_resourceNameOption);
        return options;
    }
}
