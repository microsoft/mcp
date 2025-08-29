using Microsoft.Mcp.Core.Commands;

namespace Azure.Mcp.Core.Commands;

public abstract class AzureBaseCommand : BaseCommand
{
    private const string MissingRequiredOptionsPrefix = "Missing Required options: ";

    private bool _usesResourceGroup;
    private bool _requiresResourceGroup;

    public override ValidationResult Validate(CommandResult commandResult, CommandResponse? commandResponse = null)
    {
        var result = base.Validate(commandResult, commandResponse);

        if (!result.IsValid)
        {
            return result;
        }

        // Check logical requirements (e.g., resource group requirement)
        if (_requiresResourceGroup)
        {
            var rg = commandResult.GetValueForOption(Models.Option.OptionDefinitions.Common.ResourceGroup);
            if (string.IsNullOrWhiteSpace(rg))
            {
                result.IsValid = false;
                result.ErrorMessage = $"{MissingRequiredOptionsPrefix}--resource-group";
                SetValidationError(commandResponse, result.ErrorMessage);
            }
        }

        return result;
    }

    protected void UseResourceGroup()
    {
        if (_usesResourceGroup)
            return;
        _usesResourceGroup = true;
        GetCommand().AddOption(Models.Option.OptionDefinitions.Common.ResourceGroup);
    }

    protected void RequireResourceGroup()
    {
        UseResourceGroup();
        _requiresResourceGroup = true;
    }

    protected string? GetResourceGroup(ParseResult parseResult) =>
        _usesResourceGroup ? parseResult.GetValueForOption(Models.Option.OptionDefinitions.Common.ResourceGroup) : null;

    protected bool UsesResourceGroup => _usesResourceGroup;
}
