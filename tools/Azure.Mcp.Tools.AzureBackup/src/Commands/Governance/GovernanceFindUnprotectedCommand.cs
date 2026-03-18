// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Tools.AzureBackup.Models;
using Azure.Mcp.Tools.AzureBackup.Options;
using Azure.Mcp.Tools.AzureBackup.Options.Governance;
using Azure.Mcp.Tools.AzureBackup.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Models.Option;

namespace Azure.Mcp.Tools.AzureBackup.Commands.Governance;

public sealed class GovernanceFindUnprotectedCommand(ILogger<GovernanceFindUnprotectedCommand> logger) : SubscriptionCommand<GovernanceFindUnprotectedOptions>()
{
    private const string CommandTitle = "Find Unprotected Resources";
    private readonly ILogger<GovernanceFindUnprotectedCommand> _logger = logger;

    public override string Id => "b1a2c3d4-e5f6-7890-abcd-ef12345678c8";
    public override string Name => "find-unprotected";
    public override string Description =>
        """
        Scans the subscription to find Azure resources that are not currently protected by any
        backup policy. Optionally filter by resource type, resource group, or tags.
        """;
    public override string Title => CommandTitle;
    public override ToolMetadata Metadata => new()
    {
        Destructive = false, Idempotent = true, OpenWorld = false,
        ReadOnly = true, LocalRequired = false, Secret = false
    };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(AzureBackupOptionDefinitions.ResourceTypeFilter);
        command.Options.Add(AzureBackupOptionDefinitions.ResourceGroupFilter);
        command.Options.Add(AzureBackupOptionDefinitions.TagFilter);
    }

    protected override GovernanceFindUnprotectedOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.ResourceTypeFilter = parseResult.GetValueOrDefault<string>(AzureBackupOptionDefinitions.ResourceTypeFilter.Name);
        options.ResourceGroupFilter = parseResult.GetValueOrDefault<string>(AzureBackupOptionDefinitions.ResourceGroupFilter.Name);
        options.TagFilter = parseResult.GetValueOrDefault<string>(AzureBackupOptionDefinitions.TagFilter.Name);
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
            var service = context.GetService<IAzureBackupService>();
            var resources = await service.FindUnprotectedResourcesAsync(
                options.Subscription!,
                options.ResourceTypeFilter,
                options.ResourceGroupFilter,
                options.TagFilter,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new GovernanceFindUnprotectedCommandResult(resources),
                AzureBackupJsonContext.Default.GovernanceFindUnprotectedCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finding unprotected resources. Subscription: {Subscription}", options.Subscription);
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal record GovernanceFindUnprotectedCommandResult([property: JsonPropertyName("resources")] List<UnprotectedResourceInfo> Resources);
}
