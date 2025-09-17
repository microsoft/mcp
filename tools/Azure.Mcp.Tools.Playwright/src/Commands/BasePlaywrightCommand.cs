using System.Diagnostics.CodeAnalysis;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Tools.Playwright.Options;

namespace Azure.Mcp.Tools.Playwright.Commands;

public abstract class BasePlaywrightCommand<
    [DynamicallyAccessedMembers(TrimAnnotations.CommandAnnotations)] TOptions>
    : SubscriptionCommand<TOptions> where TOptions : BasePlaywrightOptions, new()
{
    protected readonly Option<string> _playwrightWorkspaceOption = PlaywrightOptionDefinitions.PlaywrightWorkspace;

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(_playwrightWorkspaceOption);
        UseResourceGroup();
    }

    protected override TOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.PlaywrightWorkspaceName = parseResult.GetValue(_playwrightWorkspaceOption);
        return options;
    }
}
