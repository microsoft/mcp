using Azure.Mcp.Core.Commands;
using Azure.Mcp.Tools.Playwright.Models;
using Azure.Mcp.Tools.Playwright.Services;
using Azure.Mcp.Tools.Playwright.Options;
using Microsoft.Extensions.Logging;
using Azure.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.Playwright.Commands.PlaywrightWorkspaces;

public sealed class PlaywrightWorkspaceUpdateCommand(ILogger<PlaywrightWorkspaceUpdateCommand> logger) : BasePlaywrightCommand<PlaywrightWorkspaceUpdateOptions>
{
    private const string _commandTitle = "Playwright Workspace Update";
    private readonly ILogger<PlaywrightWorkspaceUpdateCommand> _logger = logger;
    public override string Name => "update";
    public override string Description => "Update a Playwright workspace resource.";
    public override string Title => _commandTitle;

    public override ToolMetadata Metadata => new() { Destructive = false, Idempotent = true };

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
            var request = new PlaywrightWorkspaceUpdateRequest
            {
                Tags = options.Tags
            };
            var result = await service.UpdatePlaywrightWorkspaceAsync(options.Subscription!, options.ResourceGroup!, options.PlaywrightWorkspaceName!, request, options.Tenant);
            context.Response.Results = ResponseResult.Create(result, PlaywrightJsonContext.Default.PlaywrightWorkspace);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {Operation}. Options: {Options}", Name, options);
            HandleException(context, ex);
        }
        return context.Response;
    }
}
