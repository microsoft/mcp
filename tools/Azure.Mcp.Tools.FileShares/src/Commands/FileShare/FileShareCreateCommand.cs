// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Tools.FileShares.Options;
using Azure.Mcp.Tools.FileShares.Options.FileShare;
using Azure.Mcp.Tools.FileShares.Services;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Extensions;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Models.Option;

namespace Azure.Mcp.Tools.FileShares.Commands.FileShare;

public sealed class FileShareCreateCommand(ILogger<FileShareCreateCommand> logger, IFileSharesService service)
    : BaseFileSharesCommand<FileShareCreateOrUpdateOptions>(logger, service)
{
    private const string CommandTitle = "Create File Share";

    public override string Id => "b3c4d5e6-f7a8-4b9c-0d1e-2f3a4b5c6d7e";
    public override string Name => "create";
    public override string Description => "Create a new Azure managed file share resource in a resource group. This creates a high-performance, fully managed file share accessible via NFS protocol.";
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
        command.Options.Add(OptionDefinitions.Common.ResourceGroup.AsRequired());
        command.Options.Add(FileSharesOptionDefinitions.FileShare.Name.AsRequired());
        command.Options.Add(FileSharesOptionDefinitions.FileShare.Location.AsRequired());
        command.Options.Add(FileSharesOptionDefinitions.MountName.AsOptional());
        command.Options.Add(FileSharesOptionDefinitions.MediaTier.AsOptional());
        command.Options.Add(FileSharesOptionDefinitions.Redundancy.AsOptional());
        command.Options.Add(FileSharesOptionDefinitions.Protocol.AsOptional());
        command.Options.Add(FileSharesOptionDefinitions.ProvisionedStorageGiB.AsOptional());
        command.Options.Add(FileSharesOptionDefinitions.ProvisionedIOPerSec.AsOptional());
        command.Options.Add(FileSharesOptionDefinitions.ProvisionedThroughputMiBPerSec.AsOptional());
        command.Options.Add(FileSharesOptionDefinitions.PublicNetworkAccess.AsOptional());
        command.Options.Add(FileSharesOptionDefinitions.NfsRootSquash.AsOptional());
        command.Options.Add(FileSharesOptionDefinitions.AllowedSubnets.AsOptional());
        command.Options.Add(FileSharesOptionDefinitions.Tags.AsOptional());
    }

    protected override FileShareCreateOrUpdateOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.ResourceGroup ??= parseResult.GetValueOrDefault(OptionDefinitions.Common.ResourceGroup);
        options.FileShareName = parseResult.GetValueOrDefault(FileSharesOptionDefinitions.FileShare.Name);
        options.Location = parseResult.GetValueOrDefault(FileSharesOptionDefinitions.FileShare.Location);
        options.MountName = parseResult.GetValueOrDefault(FileSharesOptionDefinitions.MountName);
        options.MediaTier = parseResult.GetValueOrDefault(FileSharesOptionDefinitions.MediaTier);
        options.Redundancy = parseResult.GetValueOrDefault(FileSharesOptionDefinitions.Redundancy);
        options.Protocol = parseResult.GetValueOrDefault(FileSharesOptionDefinitions.Protocol);
        options.ProvisionedStorageInGiB = parseResult.GetValueOrDefault(FileSharesOptionDefinitions.ProvisionedStorageGiB);
        options.ProvisionedIOPerSec = parseResult.GetValueOrDefault(FileSharesOptionDefinitions.ProvisionedIOPerSec);
        options.ProvisionedThroughputMiBPerSec = parseResult.GetValueOrDefault(FileSharesOptionDefinitions.ProvisionedThroughputMiBPerSec);
        options.PublicNetworkAccess = parseResult.GetValueOrDefault(FileSharesOptionDefinitions.PublicNetworkAccess);
        options.NfsRootSquash = parseResult.GetValueOrDefault(FileSharesOptionDefinitions.NfsRootSquash);
        options.AllowedSubnets = parseResult.GetValueOrDefault(FileSharesOptionDefinitions.AllowedSubnets);
        options.Tags = parseResult.GetValueOrDefault(FileSharesOptionDefinitions.Tags);
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
            _logger.LogInformation("Creating file share. Subscription: {Subscription}, ResourceGroup: {ResourceGroup}, FileShareName: {FileShareName}",
                options.Subscription, options.ResourceGroup, options.FileShareName);

            // Parse tags if provided
            Dictionary<string, string>? tags = null;
            if (!string.IsNullOrEmpty(options.Tags))
            {
                try
                {
                    tags = JsonSerializer.Deserialize(options.Tags, FileSharesJsonContext.Default.DictionaryStringString);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to parse tags JSON: {Tags}", options.Tags);
                }
            }

            // Parse allowed subnets if provided
            string[]? allowedSubnets = null;
            if (!string.IsNullOrEmpty(options.AllowedSubnets))
            {
                allowedSubnets = options.AllowedSubnets.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            }

            var fileShare = await _fileSharesService.CreateOrUpdateFileShareAsync(
                options.Subscription!,
                options.ResourceGroup!,
                options.FileShareName!,
                options.Location!,
                options.MountName,
                options.MediaTier,
                options.Redundancy,
                options.Protocol,
                options.ProvisionedStorageInGiB,
                options.ProvisionedIOPerSec,
                options.ProvisionedThroughputMiBPerSec,
                options.PublicNetworkAccess,
                options.NfsRootSquash,
                allowedSubnets,
                tags,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(new(fileShare), FileSharesJsonContext.Default.FileShareCreateCommandResult);

            _logger.LogInformation("File share created successfully. FileShare: {FileShareName}", options.FileShareName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create file share");
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal record FileShareCreateCommandResult(FileShareInfo FileShare);
}
