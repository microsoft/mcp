// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.ContainerApps.Options.ContainerApp;
using Azure.Mcp.Tools.ContainerApps.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.ContainerApps.Commands.ContainerApp;

public sealed class ContainerAppListCommand(ILogger<ContainerAppListCommand> logger, IContainerAppsService containerAppsService) : BaseContainerAppsCommand<ContainerAppListOptions>
{
    private const string CommandTitle = "List Container Apps";
    private readonly ILogger<ContainerAppListCommand> _logger = logger;
    private readonly IContainerAppsService _containerAppsService = containerAppsService;

    public override string Id => "d4e5f6a7-b8c9-0d1e-2f3a-4b5c6d7e8f90";

    public override string Name => "list";

    public override string Description =>
        $"""
        List Azure Container Apps in a subscription. Optionally filter by resource group. Each container app result
        includes: name, location, resourceGroup, managedEnvironmentId, provisioningState. If no container apps are
        found the tool returns an empty list of results (consistent with other list commands).
        """;

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new()
    {
        Destructive = false,
        Idempotent = true,
        OpenWorld = false,
        ReadOnly = true,
        LocalRequired = false,
        Secret = false
    };

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult, CancellationToken cancellationToken)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
        {
            return context.Response;
        }

        var options = BindOptions(parseResult);

        try
        {
            var containerApps = await _containerAppsService.ListContainerApps(
                options.Subscription!,
                options.ResourceGroup,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(new(containerApps?.Results ?? [], containerApps?.AreResultsTruncated ?? false), ContainerAppsJsonContext.Default.ContainerAppListCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error listing container apps. Subscription: {Subscription}, ResourceGroup: {ResourceGroup}.",
                options.Subscription, options.ResourceGroup);
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal record ContainerAppListCommandResult(List<Models.ContainerAppInfo> ContainerApps, bool AreResultsTruncated);
}
