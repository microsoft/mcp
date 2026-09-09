// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.AzureBackup.Models;
using Azure.Mcp.Tools.AzureBackup.Options.ResourceGuard;
using Azure.Mcp.Tools.AzureBackup.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.AzureBackup.Commands.ResourceGuard;

[CommandMetadata(
    Id = "8f2c1a5d-3e6b-4c9a-8d1f-7b4e2a5c9d31",
    Name = "create",
    Title = "Create Resource Guard",
    Description = """
        Creates a Microsoft.DataProtection/resourceGuards resource used to protect Azure Backup
        vaults (both Recovery Services and Backup vaults) via Multi-User Authorization (MUA).
        The Resource Guard should live in a different subscription/tenant than the vaults it
        protects so that a compromise of one identity cannot approve destructive operations on
        the other. The --location must match the region of vaults that will link to this guard.
        Optionally pass --excluded-operations to exempt operations from MUA (e.g., 'updatePolicy').
        """,
    OperationPlane = ToolOperationPlane.Control,
    Destructive = true,
    Idempotent = false,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class ResourceGuardCreateCommand(ILogger<ResourceGuardCreateCommand> logger, IAzureBackupService azureBackupService, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<ResourceGuardCreateOptions, ResourceGuardCreateCommand.ResourceGuardCreateCommandResult>(subscriptionResolver)
{
    private readonly ILogger<ResourceGuardCreateCommand> _logger = logger;
    private readonly IAzureBackupService _azureBackupService = azureBackupService;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ResourceGuardCreateOptions options, CancellationToken cancellationToken)
    {
        AzureBackupTelemetryTags.AddSubscriptionTag(context.Activity, options.Subscription);
        AzureBackupTelemetryTags.AddResourceGuardOperationTag(context.Activity, "create");

        try
        {
            var excluded = ParseCsv(options.ExcludedOperations);
            var tags = ParseTags(options.Tags);

            var guard = await _azureBackupService.CreateResourceGuardAsync(
                options.ResourceGuard,
                options.ResourceGroup,
                options.Subscription!,
                options.Location,
                excluded,
                tags,
                options.Tenant,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new(guard),
                AzureBackupJsonContext.Default.ResourceGuardCreateCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating Resource Guard. Name: {ResourceGuard}, ResourceGroup: {ResourceGroup}",
                options.ResourceGuard, options.ResourceGroup);
            HandleException(context, ex);
        }

        return context.Response;
    }

    private static IReadOnlyList<string>? ParseCsv(string? csv)
    {
        if (string.IsNullOrWhiteSpace(csv))
        {
            return null;
        }

        return csv
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToArray();
    }

    private static IReadOnlyDictionary<string, string>? ParseTags(string? tags)
    {
        if (string.IsNullOrWhiteSpace(tags))
        {
            return null;
        }

        var dict = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var pair in tags.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var idx = pair.IndexOf('=');
            if (idx < 0)
            {
                throw new ArgumentException($"Invalid --tags value '{pair}'. Expected format: 'key1=value1,key2=value2'.");
            }
            var key = pair[..idx].Trim();
            var value = pair[(idx + 1)..].Trim();
            if (key.Length == 0 || value.Length == 0)
            {
                throw new ArgumentException($"Invalid --tags value '{pair}'. Tag key and value must be non-empty.");
            }
            dict[key] = value;
        }
        return dict;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        ArgumentException argEx => argEx.Message,
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.BadRequest =>
            $"Bad request creating Resource Guard. Check the --location and --excluded-operations values. Details: {reqEx.Message}",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Conflict =>
            $"A Resource Guard with this name already exists in the resource group. Details: {reqEx.Message}",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden =>
            $"Authorization failed creating the Resource Guard. Ensure you have Contributor role on the resource group. Details: {reqEx.Message}",
        RequestFailedException reqEx => reqEx.Message,
        _ => base.GetErrorMessage(ex)
    };

    protected override HttpStatusCode GetStatusCode(Exception ex) => ex switch
    {
        ArgumentException or FormatException => HttpStatusCode.BadRequest,
        RequestFailedException reqEx => (HttpStatusCode)reqEx.Status,
        _ => base.GetStatusCode(ex)
    };

    public sealed record ResourceGuardCreateCommandResult(ResourceGuardInfo Guard);
}
