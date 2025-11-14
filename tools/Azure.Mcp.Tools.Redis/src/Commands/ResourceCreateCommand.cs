// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Core.Models.Option;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.Redis.Models;
using Azure.Mcp.Tools.Redis.Options;
using Azure.Mcp.Tools.Redis.Services;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.Redis.Commands;

/// <summary>
/// Creates a new Azure Managed Redis resource.
/// </summary>
public sealed class ResourceCreateCommand(ILogger<ResourceCreateCommand> logger) : SubscriptionCommand<ResourceCreateOptions>()
{
    private const string CommandTitle = "Create Redis Resource";
    private readonly ILogger<ResourceCreateCommand> _logger = logger;

    public override string Id => "750133dd-d57f-4ed4-9488-c1d406ad4a83";

    public override string Name => "create";

    public override string Description =>
       """
       Create a new Azure Managed Redis resource in Azure. Use this command to provision a new Redis resource in your subscription.
       """;

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new()
    {
        Destructive = true,
        Idempotent = false,
        OpenWorld = false,
        ReadOnly = false,
        LocalRequired = false,
        Secret = false
    };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(RedisOptionDefinitions.Resource);
        command.Options.Add(OptionDefinitions.Common.ResourceGroup.AsRequired());
        command.Options.Add(RedisOptionDefinitions.Sku);
        command.Options.Add(RedisOptionDefinitions.Location);
        command.Options.Add(RedisOptionDefinitions.AccessKeyAuthenticationEnabled);
        command.Options.Add(RedisOptionDefinitions.Modules);
    }

    protected override ResourceCreateOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Name = parseResult.GetValueOrDefault<string>(RedisOptionDefinitions.Resource.Name);
        options.ResourceGroup = parseResult.GetValueOrDefault<string>(OptionDefinitions.Common.ResourceGroup.Name);
        options.Sku = parseResult.GetValueOrDefault<string>(RedisOptionDefinitions.Sku.Name);
        options.Location = parseResult.GetValueOrDefault<string>(RedisOptionDefinitions.Location.Name);
        options.AccessKeyAuthenticationEnabled = parseResult.GetValueOrDefault<bool>(RedisOptionDefinitions.AccessKeyAuthenticationEnabled.Name);
        options.Modules = parseResult.GetValueOrDefault<string[]>(RedisOptionDefinitions.Modules.Name);

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
            var redisService = context.GetService<IRedisService>() ?? throw new InvalidOperationException("Redis service is not available.");

            var resource = await redisService.CreateResourceAsync(
                options.Subscription!,
                options.ResourceGroup!,
                options.Name!,
                options.Location!,
                options.Sku,
                options.AccessKeyAuthenticationEnabled,
                options.Modules,
                options.Tenant,
                options.RetryPolicy);

            context.Response.Results = ResponseResult.Create(
                new ResourceCreateCommandResult(resource),
                RedisJsonContext.Default.ResourceCreateCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create Redis resource");
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal record ResourceCreateCommandResult(Resource Resource);
}
