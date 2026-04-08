// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json.Serialization;
using Azure.Mcp.Tools.AzureBackup.Models;
using Azure.Mcp.Tools.AzureBackup.Options.Dr;
using Azure.Mcp.Tools.AzureBackup.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Extensions;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Models.Option;

namespace Azure.Mcp.Tools.AzureBackup.Commands.Dr;

public sealed class DrEnableCrrCommand(ILogger<DrEnableCrrCommand> logger, IAzureBackupService azureBackupService) : BaseAzureBackupCommand<DrEnableCrrOptions>()
{
    private const string CommandTitle = "Enable Cross-Region Restore";
    private readonly ILogger<DrEnableCrrCommand> _logger = logger;
    private readonly IAzureBackupService _azureBackupService = azureBackupService;

    public override string Id => "917b66e5-483f-43ac-9620-9403e1689dbe";
    public override string Name => "enablecrr";
    public override string Description => "Enables Cross-Region Restore on a GRS-enabled vault.";
    public override string Title => CommandTitle;
    public override ToolMetadata Metadata => new() { Destructive = true, Idempotent = true, OpenWorld = false, ReadOnly = false, LocalRequired = false, Secret = false };

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult, CancellationToken cancellationToken)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
        {
            return context.Response;
        }

        var options = BindOptions(parseResult);

        try
        {
            var result = await _azureBackupService.ConfigureCrossRegionRestoreAsync(
                options.Vault!,
                options.ResourceGroup!,
                options.Subscription!,
                options.VaultType,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new DrEnableCrrCommandResult(result),
                AzureBackupJsonContext.Default.DrEnableCrrCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error enabling CRR. Vault: {Vault}, ResourceGroup: {ResourceGroup}",
                options.Vault, options.ResourceGroup);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        ArgumentException argEx => argEx.Message,
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.NotFound =>
            "Vault not found. Verify the vault name and resource group.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.BadRequest =>
            $"Bad request enabling CRR (often means vault isn't GRS). Details: {reqEx.Message}",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Conflict =>
            "Cross-Region Restore is already enabled on this vault.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden =>
            $"Authorization failed enabling CRR. Details: {reqEx.Message}",
        RequestFailedException reqEx => reqEx.Message,
        _ => base.GetErrorMessage(ex)
    };

    internal record DrEnableCrrCommandResult([property: JsonPropertyName("result")] OperationResult Result);
}
