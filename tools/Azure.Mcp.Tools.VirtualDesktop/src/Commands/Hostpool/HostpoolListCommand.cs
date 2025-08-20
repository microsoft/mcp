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

    public override ToolMetadata Metadata => new() { Destructive = false, ReadOnly = true };

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {
        var options = BindOptions(parseResult);

        try
        {
            if (!Validate(parseResult.CommandResult, context.Response).IsValid)
            {
                return context.Response;
            }

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

            context.Response.Results = hostpools.Count > 0
                ? ResponseResult.Create(new HostPoolListCommandResult([.. hostpools]), VirtualDesktopJsonContext.Default.HostPoolListCommandResult)
                : null;
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
