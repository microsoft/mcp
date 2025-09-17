using Azure.Mcp.Core.Commands;
using Azure.Mcp.Tools.Playwright.Models;
using Azure.Mcp.Tools.Playwright.Services;
using Azure.Mcp.Tools.Playwright.Options;
using Microsoft.Extensions.Logging;
using Azure.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.Playwright.Commands.PlaywrightWorkspaces;

public sealed class PlaywrightWorkspaceListCommand(ILogger<PlaywrightWorkspaceListCommand> logger) : BasePlaywrightCommand<PlaywrightWorkspaceListOptions>
{
    private const string _commandTitle = "Playwright Workspace List";
    private readonly ILogger<PlaywrightWorkspaceListCommand> _logger = logger;
    public override string Name => "list";
    public override string Description => "List Playwright workspace resources in the current subscription/resource-group.";
    public override string Title => _commandTitle;

    public override ToolMetadata Metadata => new()
    {
        Destructive = false,
        Idempotent = true,
        OpenWorld = true,
        ReadOnly = true,
        LocalRequired = false,
        Secret = false
    };

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
        {
            return context.Response;
        }

        var options = BindOptions(parseResult);
        try
        {
            var service = context.GetService<IPlaywrightService>();
            var results = await service.GetPlaywrightWorkspacesAsync(options.Subscription!, options.ResourceGroup, options.PlaywrightWorkspaceName, options.Tenant);
            context.Response.Results = results != null ? ResponseResult.Create(new PlaywrightWorkspaceListResult(results), PlaywrightJsonContext.Default.PlaywrightWorkspaceListResult) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {Operation}. Options: {Options}", Name, options);
            HandleException(context, ex);
        }
        return context.Response;
    }

    internal record PlaywrightWorkspaceListResult(List<PlaywrightWorkspace> PlaywrightWorkspaces);
}
