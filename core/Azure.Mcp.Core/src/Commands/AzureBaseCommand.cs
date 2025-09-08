// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine.Parsing;
using Azure.Mcp.Core.Models.Option;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Extensions;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Core.Commands;

public abstract class AzureBaseCommand : BaseCommand
{
    private bool _usesResourceGroup;
    private bool _requiresResourceGroup;

    protected AzureBaseCommand() : base()
    {
    }

    public override ValidationResult Validate(CommandResult commandResult, CommandResponse? commandResponse = null)
    {
        var result = base.Validate(commandResult, commandResponse);

        // Enforce logical requirements (e.g., resource group required by this command)
        if (result.IsValid && _requiresResourceGroup)
        {
            if (!commandResult.HasOptionResult(OptionDefinitions.Common.ResourceGroup))
            {
                result.IsValid = false;
                result.ErrorMessage = $"{MissingRequiredOptionsPrefix}--resource-group";
                SetValidationError(commandResponse, result.ErrorMessage);
            }
        }

        return result;
    }


    // TODO(jongio): Consider a stronger, declarative model for option requirements.

    protected void UseResourceGroup()
    {
        if (_usesResourceGroup)
        {
            return;
        }

        _usesResourceGroup = true;

        GetCommand().Options.Add(OptionDefinitions.Common.ResourceGroup);
    }

    protected void RequireResourceGroup()
    {
        UseResourceGroup();
        _requiresResourceGroup = true;
    }

    protected string? GetResourceGroup(ParseResult parseResult)
    {
        if (!UsesResourceGroup)
            return null;

        return parseResult.CommandResult.HasOptionResult(OptionDefinitions.Common.ResourceGroup)
                ? parseResult.CommandResult.GetValue(OptionDefinitions.Common.ResourceGroup)
                : null;
    }

    protected bool UsesResourceGroup => _usesResourceGroup;
}
