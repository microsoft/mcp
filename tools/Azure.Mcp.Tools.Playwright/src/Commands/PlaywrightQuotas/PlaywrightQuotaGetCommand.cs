using Azure.Mcp.Core.Commands;
using Azure.Mcp.Tools.Playwright.Services;
using Azure.Mcp.Tools.Playwright.Options;
using Microsoft.Extensions.Logging;
using Azure.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.Playwright.Commands.PlaywrightQuotas;

public sealed class PlaywrightQuotaGetCommand(ILogger<PlaywrightQuotaGetCommand> logger) : BasePlaywrightCommand<PlaywrightQuotaGetOptions>
{
    private const string _commandTitle = "Playwright Subscription Quota Get";
    private readonly ILogger<PlaywrightQuotaGetCommand> _logger = logger;
    public override string Name => "get";
    public override string Description => "Get a subscription-level Playwright quota for a location.";
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
            var result = await service.GetSubscriptionPlaywrightQuotaAsync(options.Subscription!, options.Location!, options.QuotaName!, options.Tenant);
            context.Response.Results = result != null ? ResponseResult.Create(result, PlaywrightJsonContext.Default.PlaywrightQuota) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {Operation}. Options: {Options}", Name, options);
            HandleException(context, ex);
        }
        return context.Response;
    }
}
