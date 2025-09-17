using Azure.Mcp.Core.Commands;
using Azure.Mcp.Tools.Playwright.Models;
using Azure.Mcp.Tools.Playwright.Services;
using Azure.Mcp.Tools.Playwright.Options;
using Microsoft.Extensions.Logging;
using Azure.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.Playwright.Commands.PlaywrightWorkspaceQuotas;

public sealed class PlaywrightWorkspaceQuotaListCommand(ILogger<PlaywrightWorkspaceQuotaListCommand> logger) : BasePlaywrightCommand<PlaywrightWorkspaceQuotaListOptions>
{
    private const string _commandTitle = "Playwright Workspace Quota List";
    private readonly ILogger<PlaywrightWorkspaceQuotaListCommand> _logger = logger;
    public override string Name => "list";
    public override string Description => "List quota resources for a Playwright workspace.";
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
            var results = await service.GetPlaywrightWorkspaceQuotasAsync(options.Subscription!, options.ResourceGroup!, options.PlaywrightWorkspaceName!, options.Tenant);
            context.Response.Results = results != null ? ResponseResult.Create(new PlaywrightWorkspaceQuotaListResult(results), PlaywrightJsonContext.Default.PlaywrightWorkspaceQuotaListResult) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {Operation}. Options: {Options}", Name, options);
            HandleException(context, ex);
        }
        return context.Response;
    }

    internal record PlaywrightWorkspaceQuotaListResult(List<PlaywrightWorkspaceQuota> PlaywrightWorkspaceQuotas);
}
