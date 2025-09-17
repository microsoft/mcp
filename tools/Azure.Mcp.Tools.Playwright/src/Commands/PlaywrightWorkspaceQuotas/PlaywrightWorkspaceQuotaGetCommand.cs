using Azure.Mcp.Core.Commands;
using Azure.Mcp.Tools.Playwright.Services;
using Azure.Mcp.Tools.Playwright.Options;
using Microsoft.Extensions.Logging;
using Azure.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.Playwright.Commands.PlaywrightWorkspaceQuotas;

public sealed class PlaywrightWorkspaceQuotaGetCommand(ILogger<PlaywrightWorkspaceQuotaGetCommand> logger) : BasePlaywrightCommand<PlaywrightWorkspaceQuotaGetOptions>
{
    private const string _commandTitle = "Playwright Workspace Quota Get";
    private readonly ILogger<PlaywrightWorkspaceQuotaGetCommand> _logger = logger;
    public override string Name => "get";
    public override string Description => "Get a Playwright workspace quota resource by name.";
    public override string Title => _commandTitle;

    public override ToolMetadata Metadata => new() { ReadOnly = true, Idempotent = true };

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
            var result = await service.GetPlaywrightWorkspaceQuotaAsync(options.Subscription!, options.ResourceGroup!, options.PlaywrightWorkspaceName!, options.QuotaName!, options.Tenant);
            context.Response.Results = result != null ? ResponseResult.Create(result, PlaywrightJsonContext.Default.PlaywrightWorkspaceQuota) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {Operation}. Options: {Options}", Name, options);
            HandleException(context, ex);
        }
        return context.Response;
    }
}
