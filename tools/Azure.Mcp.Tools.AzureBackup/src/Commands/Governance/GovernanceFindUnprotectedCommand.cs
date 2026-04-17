// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Tools.AzureBackup.Models;
using Azure.Mcp.Tools.AzureBackup.Options;
using Azure.Mcp.Tools.AzureBackup.Options.Governance;
using Azure.Mcp.Tools.AzureBackup.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Extensions;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Models.Option;

namespace Azure.Mcp.Tools.AzureBackup.Commands.Governance;

public sealed class GovernanceFindUnprotectedCommand(ILogger<GovernanceFindUnprotectedCommand> logger, IAzureBackupService azureBackupService) : SubscriptionCommand<GovernanceFindUnprotectedOptions>()
{
    private const string CommandTitle = "Find Unprotected Resources";
    private readonly ILogger<GovernanceFindUnprotectedCommand> _logger = logger;
    private readonly IAzureBackupService _azureBackupService = azureBackupService;

    public override string Id => "73b050ca-2e20-448c-a76c-08e8cd5bbe25";
    public override string Name => "find-unprotected";
    public override string Description =>
        """
        Scans the subscription to find Azure resources that are not currently protected by any
        backup policy. Optionally filter by resource type, resource group, or tags.
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
            var resources = await _azureBackupService.FindUnprotectedResourcesAsync(
                options.Subscription!,
                options.ResourceTypeFilter,
                options.ResourceGroupFilter,
                options.TagFilter,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new(resources),
                AzureBackupJsonContext.Default.GovernanceFindUnprotectedCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finding unprotected resources. Subscription: {Subscription}", options.Subscription);
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal record GovernanceFindUnprotectedCommandResult(List<UnprotectedResourceInfo> Resources);
}
