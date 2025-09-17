using Azure.Mcp.Core.Commands;
using Azure.Mcp.Tools.Playwright.Models;
using Azure.Mcp.Tools.Playwright.Services;
using Azure.Mcp.Tools.Playwright.Options;
using Microsoft.Extensions.Logging;
using Azure.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.Playwright.Commands.PlaywrightWorkspaces;

public sealed class PlaywrightWorkspaceCreateCommand(ILogger<PlaywrightWorkspaceCreateCommand> logger) : BasePlaywrightCommand<PlaywrightWorkspaceCreateOptions>
{
    private const string _commandTitle = "Playwright Workspace Create";
    private readonly ILogger<PlaywrightWorkspaceCreateCommand> _logger = logger;
    public override string Name => "create";
    public override string Description => "Create a Playwright workspace resource.";
    public override string Title => _commandTitle;

    public override ToolMetadata Metadata => new() { Destructive = false, Idempotent = false };

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
            var request = new PlaywrightWorkspaceCreateOrUpdateRequest
            {
                Name = options.PlaywrightWorkspaceName,
                Location = options.Location,
                Tags = options.Tags
            };
            var result = await service.CreateOrUpdatePlaywrightWorkspaceAsync(options.Subscription!, options.ResourceGroup!, request, options.Tenant);
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
