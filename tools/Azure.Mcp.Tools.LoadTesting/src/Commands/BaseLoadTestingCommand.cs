// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Core.Models.Option;
using Azure.Mcp.Tools.LoadTesting.Options;
using Microsoft.Mcp.Core.Models.Option;

namespace Azure.Mcp.Tools.LoadTesting.Commands;

public abstract class BaseLoadTestingCommand<
    [DynamicallyAccessedMembers(TrimAnnotations.CommandAnnotations)] TOptions>
    (bool resourceGroupRequired = false, bool testResourceRequired = false)
    : SubscriptionCommand<TOptions> where TOptions : BaseLoadTestingOptions, new()
{
    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(resourceGroupRequired
            ? OptionDefinitions.Common.ResourceGroup.AsRequired()
            : OptionDefinitions.Common.ResourceGroup.AsOptional());
        command.Options.Add(testResourceRequired
            ? LoadTestingOptionDefinitions.TestResource.AsRequired()
            : LoadTestingOptionDefinitions.TestResource.AsOptional());
    }

    protected override TOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.TestResourceName = parseResult.GetValueOrDefault<string>(LoadTestingOptionDefinitions.TestResource.Name);
        options.ResourceGroup ??= parseResult.GetValueOrDefault<string>(OptionDefinitions.Common.ResourceGroup.Name);
        return options;
    }
}
