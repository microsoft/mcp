// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Tools.Advisor.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Extensions;
using Microsoft.Mcp.Core.Models.Option;

namespace Azure.Mcp.Tools.Advisor.Commands;

public abstract class BaseAdvisorCommand<
    [DynamicallyAccessedMembers(TrimAnnotations.CommandAnnotations)] TOptions,
    TResult>(ILogger<BaseAdvisorCommand<TOptions, TResult>> logger)
    : SubscriptionCommand<TOptions, TResult> where TOptions : BaseAdvisorOptions, new()
{
    protected readonly ILogger<BaseAdvisorCommand<TOptions, TResult>> _logger = logger;

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(OptionDefinitions.Common.ResourceGroup.AsOptional());
    }

    protected override TOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.ResourceGroup ??= parseResult.GetValueOrDefault(OptionDefinitions.Common.ResourceGroup);
        return options;
    }
}
