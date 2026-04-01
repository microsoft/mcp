// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Microsoft.Mcp.Core.Extensions;
using Azure.Mcp.Tools.AzureBackup.Models;
using Azure.Mcp.Tools.AzureBackup.Options.Dr;
using Azure.Mcp.Tools.AzureBackup.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Models.Option;

namespace Azure.Mcp.Tools.AzureBackup.Commands.Dr;

public sealed class DrEnableCrrCommand(ILogger<DrEnableCrrCommand> logger) : BaseAzureBackupCommand<DrEnableCrrOptions>()
{
    private const string CommandTitle = "Enable Cross-Region Restore";
    private readonly ILogger<DrEnableCrrCommand> _logger = logger;

    public override string Id => "917b66e5-483f-43ac-9620-9403e1689dbe";
    public override string Name => "enablecrr";
    public override string Description => "Enables Cross-Region Restore on a GRS-enabled vault.";
    public override string Title => CommandTitle;
    public override ToolMetadata Metadata => new() { Destructive = true, Idempotent = true, OpenWorld = false, ReadOnly = false, LocalRequired = false, Secret = false };

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult, CancellationToken cancellationToken)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
            return context.Response;
        var options = BindOptions(parseResult);
        try
        {
            var service = context.GetService<IAzureBackupService>();
            var result = await service.ConfigureCrossRegionRestoreAsync(options.Vault!, options.ResourceGroup!, options.Subscription!, options.VaultType, options.Tenant, options.RetryPolicy, cancellationToken);
            context.Response.Results = ResponseResult.Create(new DrEnableCrrCommandResult(result), AzureBackupJsonContext.Default.DrEnableCrrCommandResult);
        }
        catch (Exception ex) { _logger.LogError(ex, "Error enabling CRR"); HandleException(context, ex); }
        return context.Response;
    }

    internal record DrEnableCrrCommandResult([property: JsonPropertyName("result")] OperationResult Result);
}
