// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Tools.FoundryExtensions.Models;
using Azure.Mcp.Tools.FoundryExtensions.Options;
using Azure.Mcp.Tools.FoundryExtensions.Options.Models;
using Azure.Mcp.Tools.FoundryExtensions.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Models.Option;

namespace Azure.Mcp.Tools.FoundryExtensions.Commands;

public sealed class ResourceGetCommand(ILogger<ResourceGetCommand> logger, IFoundryExtensionsService foundryExtensionsService) : SubscriptionCommand<ResourceGetOptions>()
{
    private const string CommandTitle = "Get Microsoft Foundry Resource Details";
    private readonly ILogger<ResourceGetCommand> _logger = logger;
    private readonly IFoundryExtensionsService _foundryExtensionsService = foundryExtensionsService;

    public override string Id => "b8c9d0e1-8901-cdef-1234-567890123456";

    public override string Name => "get";

    public override string Description =>
        """
        Gets detailed information about Microsoft Foundry (Cognitive Services) resources, including endpoint URL,
        location, SKU, and all deployed models with their configuration. If a specific resource name is provided,
        returns details for that resource only. If no resource name is provided, lists all Microsoft Foundry resources
        in the subscription or resource group. Use this tool when users need endpoint information, want to discover
        available AI resources, or need to see all models deployed on AI resources.
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

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(OptionDefinitions.Common.ResourceGroup.AsOptional());
        command.Options.Add(FoundryExtensionsOptionDefinitions.ResourceNameOption.AsOptional());
    }

    protected override ResourceGetOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.ResourceGroup = parseResult.GetValueOrDefault<string>(OptionDefinitions.Common.ResourceGroup.Name);
        options.ResourceName = parseResult.GetValueOrDefault<string>(FoundryExtensionsOptionDefinitions.ResourceNameOption.Name);
        return options;
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult, CancellationToken cancellationToken)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
        {
            return context.Response;
        }

        var options = BindOptions(parseResult);

        try
        {
            var service = _foundryExtensionsService;

            // If resource name and resource group are provided, get specific resource
            if (!string.IsNullOrEmpty(options.ResourceName) && !string.IsNullOrEmpty(options.ResourceGroup))
            {
                var resource = await service.GetAiResourceAsync(
                    options.Subscription!,
                    options.ResourceGroup!,
                    options.ResourceName!,
                    options.Tenant,
                    options.RetryPolicy,
                    cancellationToken: cancellationToken);

                context.Response.Results = ResponseResult.Create(
                    new([resource]),
                    FoundryExtensionsJsonContext.Default.ResourceGetCommandResult);
            }
            // Otherwise, list all resources in subscription/resource group
            else
            {
                var resources = await service.ListAiResourcesAsync(
                    options.Subscription!,
                    options.ResourceGroup,
                    options.Tenant,
                    options.RetryPolicy,
                    cancellationToken: cancellationToken);

                context.Response.Results = ResponseResult.Create(
                    new(resources ?? []),
                    FoundryExtensionsJsonContext.Default.ResourceGetCommandResult);
            }
        }
        catch (Exception ex)
        {
            if (string.IsNullOrEmpty(options.ResourceName))
            {
                _logger.LogError(ex, "Error listing AI resources. Subscription: {Subscription}, ResourceGroup: {ResourceGroup}.",
                    options.Subscription, options.ResourceGroup);
            }
            else
            {
                _logger.LogError(ex, "Error getting AI resource. ResourceName: {ResourceName}, ResourceGroup: {ResourceGroup}, Subscription: {Subscription}.",
                    options.ResourceName, options.ResourceGroup, options.Subscription);
            }
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal record ResourceGetCommandResult(List<AiResourceInformation> Resources);
}
