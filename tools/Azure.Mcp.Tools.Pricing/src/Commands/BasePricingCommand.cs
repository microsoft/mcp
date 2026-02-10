// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.CommandLine.Parsing;
using System.Diagnostics.CodeAnalysis;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Helpers;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.Pricing.Options;

namespace Azure.Mcp.Tools.Pricing.Commands;

/// <summary>
/// Base command for all Pricing commands.
/// </summary>
public abstract class BasePricingCommand<
    [DynamicallyAccessedMembers(TrimAnnotations.CommandAnnotations)] TOptions>
    : GlobalCommand<TOptions> where TOptions : BasePricingOptions, new()
{
    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(PricingOptionDefinitions.Currency);
    }

    protected override TOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Currency = parseResult.GetValue(PricingOptionDefinitions.Currency);
        return options;
    }
}
