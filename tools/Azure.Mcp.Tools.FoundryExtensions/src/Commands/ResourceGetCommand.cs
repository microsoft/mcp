// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.FoundryExtensions.Models;
using Azure.Mcp.Tools.FoundryExtensions.Options.Models;
using Azure.Mcp.Tools.FoundryExtensions.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.FoundryExtensions.Commands;

[CommandMetadata(
    Id = "b8c9d0e1-8901-cdef-1234-567890123456",
    Name = "get",
    Title = "Get Microsoft Foundry Resource Details",
    Description = """
        Lists or gets Microsoft Foundry resources (accounts/services) and returns resource-level details such as
        endpoint URL, location, SKU, kind, and provisioning state. Use this when users ask to list all Microsoft
        Foundry resources in a subscription, show resources in a resource group, or get details for a specific
        resource by name. If resource-name is provided with resource-group, returns that single resource; otherwise
        returns the resource inventory in scope. This command is for Foundry resource metadata, not model
        deployment inventory. For Azure OpenAI model deployments inside a Foundry resource, use the 'openai models-list' tool.
        """,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class ResourceGetCommand(ILogger<ResourceGetCommand> logger, IFoundryExtensionsService foundryExtensionsService, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<ResourceGetOptions, ResourceGetCommand.ResourceGetCommandResult>(subscriptionResolver)
{
    private readonly ILogger<ResourceGetCommand> _logger = logger;
    private readonly IFoundryExtensionsService _foundryExtensionsService = foundryExtensionsService;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ResourceGetOptions options, CancellationToken cancellationToken)
    {
        try
        {
            // Validate that if resource name is provided, resource group must also be provided
            if (!string.IsNullOrEmpty(options.ResourceName) && string.IsNullOrEmpty(options.ResourceGroup))
            {
                throw new ArgumentException("Resource group is required when resource name is specified.");
            }

            // If resource name and resource group are provided, get specific resource
            if (!string.IsNullOrEmpty(options.ResourceName) && !string.IsNullOrEmpty(options.ResourceGroup))
            {
                var resource = await _foundryExtensionsService.GetAiResourceAsync(
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
                var resources = await _foundryExtensionsService.ListAiResourcesAsync(
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

    public sealed record ResourceGetCommandResult(List<AiResourceInformation> Resources);
}
