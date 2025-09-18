// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Models.Option;
using Azure.Mcp.Tools.VirtualDesktop.Options.Hostpool;
using Azure.Mcp.Tools.VirtualDesktop.Services;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.VirtualDesktop.Commands.Hostpool;

public sealed class HostpoolListCommand(ILogger<HostpoolListCommand> logger) : BaseVirtualDesktopCommand<HostpoolListOptions>
{
    private const string CommandTitle = "List hostpools";
    private readonly ILogger<HostpoolListCommand> _logger = logger;

    public override string Name => "list";

    public override string Description =>
        $"""
		List all hostpools in a subscription or resource group. This command retrieves all Azure Virtual Desktop hostpool objects available
		in the specified {OptionDefinitions.Common.Subscription}. If a resource group is specified, only hostpools in that resource group are returned.
		Results include hostpool names and are returned as a JSON array.
		""";

    public override string Title => CommandTitle;

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
            var virtualDesktopService = context.GetService<IVirtualDesktopService>();

            IReadOnlyList<Models.HostPool> hostpools;

            if (!string.IsNullOrEmpty(options.ResourceGroup))
            {
                hostpools = await virtualDesktopService.ListHostpoolsByResourceGroupAsync(
                    options.Subscription!,
                    options.ResourceGroup,
                    options.Tenant,
                    options.RetryPolicy);
            }
            else
            {
                hostpools = await virtualDesktopService.ListHostpoolsAsync(
                    options.Subscription!,
                    options.Tenant,
                    options.RetryPolicy);
            }

            context.Response.Results = ResponseResult.Create(new([.. hostpools ?? []]), VirtualDesktopJsonContext.Default.HostPoolListCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error listing hostpools. Subscription: {Subscription}, Options: {@Options}",
                options.Subscription, options);
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal record HostPoolListCommandResult(List<Models.HostPool> hostpools);
}
