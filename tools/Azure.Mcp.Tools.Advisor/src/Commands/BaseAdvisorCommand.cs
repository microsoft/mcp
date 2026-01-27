// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Diagnostics.CodeAnalysis;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Core.Models.Option;
using Azure.Mcp.Tools.Advisor.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Models.Option;

namespace Azure.Mcp.Tools.Advisor.Commands;

public abstract class BaseAdvisorCommand<
    [DynamicallyAccessedMembers(TrimAnnotations.CommandAnnotations)] TOptions>(ILogger<BaseAdvisorCommand<TOptions>> logger)
    : SubscriptionCommand<TOptions> where TOptions : BaseAdvisorOptions, new()
{
    protected readonly ILogger<BaseAdvisorCommand<TOptions>> _logger = logger;

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(OptionDefinitions.Common.ResourceGroup.AsOptional());
    }

    protected override TOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.ResourceGroup ??= parseResult.GetValueOrDefault<string>(OptionDefinitions.Common.ResourceGroup.Name);
        return options;
    }
}
