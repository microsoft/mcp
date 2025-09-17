using Azure.Mcp.Core.Commands;
using Azure.Mcp.Tools.Playwright.Models;
using Azure.Mcp.Tools.Playwright.Services;
using Azure.Mcp.Tools.Playwright.Options;
using Microsoft.Extensions.Logging;
using Azure.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.Playwright.Commands.PlaywrightQuotas;

public sealed class PlaywrightQuotaListBySubscriptionCommand(ILogger<PlaywrightQuotaListBySubscriptionCommand> logger) : BasePlaywrightCommand<PlaywrightQuotaListBySubscriptionOptions>
{
    private const string _commandTitle = "Playwright Subscription Quota List";
    private readonly ILogger<PlaywrightQuotaListBySubscriptionCommand> _logger = logger;
    public override string Name => "list";
    public override string Description => "List subscription-level Playwright quotas for a location.";
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
            var results = await service.GetSubscriptionPlaywrightQuotasAsync(options.Subscription!, options.Location!, options.Tenant);
            context.Response.Results = results != null ? ResponseResult.Create(new PlaywrightQuotaListResult(results), PlaywrightJsonContext.Default.PlaywrightQuotaListResult) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {Operation}. Options: {Options}", Name, options);
            HandleException(context, ex);
        }
        return context.Response;
    }

    internal record PlaywrightQuotaListResult(List<PlaywrightQuota> PlaywrightQuotas);
}
