// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Core.Models.Option;
using Azure.Mcp.Tools.Foundry.Models;
using Azure.Mcp.Tools.Foundry.Options;
using Azure.Mcp.Tools.Foundry.Options.Models;
using Azure.Mcp.Tools.Foundry.Services;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.Foundry.Commands;

public sealed class ResourceGetCommand(ILogger<ResourceGetCommand> logger) : SubscriptionCommand<ResourceGetOptions>()
{
    private const string CommandTitle = "Get AI Foundry Resource Details";
    private readonly ILogger<ResourceGetCommand> _logger = logger;

    public override string Name => "get";

    public override string Description =>
        """
        Gets detailed information about Azure AI Foundry (Cognitive Services) resources, including endpoint URL, 
        location, SKU, and all deployed models with their configuration. If a specific resource name is provided, 
        returns details for that resource only. If no resource name is provided, lists all AI Foundry resources 
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
        command.Options.Add(FoundryOptionDefinitions.ResourceNameOption.AsOptional());
    }

    protected override ResourceGetOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.ResourceGroup = parseResult.GetValueOrDefault<string>(OptionDefinitions.Common.ResourceGroup.Name);
        options.ResourceName = parseResult.GetValueOrDefault<string>(FoundryOptionDefinitions.ResourceNameOption.Name);
        return options;
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
        {
            return context.Response;
        }

        var options = BindOptions(parseResult);

        try
        {
            var service = context.GetService<IFoundryService>();

            // If resource name and resource group are provided, get specific resource
            if (!string.IsNullOrEmpty(options.ResourceName) && !string.IsNullOrEmpty(options.ResourceGroup))
            {
                var resource = await service.GetAiResourceAsync(
                    options.Subscription!,
                    options.ResourceGroup!,
                    options.ResourceName!,
                    options.Tenant,
                    options.RetryPolicy);

                context.Response.Results = ResponseResult.Create(
                    new ResourceGetCommandResult([resource]),
                    FoundryJsonContext.Default.ResourceGetCommandResult);
            }
            // Otherwise, list all resources in subscription/resource group
            else
            {
                var resources = await service.ListAiResourcesAsync(
                    options.Subscription!,
                    options.ResourceGroup,
                    options.Tenant,
                    options.RetryPolicy);

                context.Response.Results = ResponseResult.Create(
                    new ResourceGetCommandResult(resources ?? []),
                    FoundryJsonContext.Default.ResourceGetCommandResult);
            }
        }
        catch (Exception ex)
        {
            if (string.IsNullOrEmpty(options.ResourceName))
            {
                _logger.LogError(ex, "Error listing AI resources. Subscription: {Subscription}, ResourceGroup: {ResourceGroup}, Options: {@Options}",
                    options.Subscription, options.ResourceGroup, options);
            }
            else
            {
                _logger.LogError(ex, "Error getting AI resource. ResourceName: {ResourceName}, ResourceGroup: {ResourceGroup}, Subscription: {Subscription}, Options: {@Options}",
                    options.ResourceName, options.ResourceGroup, options.Subscription, options);
            }
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal record ResourceGetCommandResult([property: JsonPropertyName("resources")] List<AiResourceInformation> Resources);
}
