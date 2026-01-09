// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Tools.FileShares.Options;
using Azure.Mcp.Tools.FileShares.Options.Informational;
using Azure.Mcp.Tools.FileShares.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Models.Option;

namespace Azure.Mcp.Tools.FileShares.Commands.Informational;

public sealed class FileShareGetProvisioningRecommendationCommand(ILogger<FileShareGetProvisioningRecommendationCommand> logger, IFileSharesService service)
    : SubscriptionCommand<FileShareGetProvisioningRecommendationOptions>()
{
    private readonly ILogger<FileShareGetProvisioningRecommendationCommand> _logger = logger;
    private readonly IFileSharesService _service = service;

    public override string Id => "3c5e1fb2-3a8d-4f8e-8b0a-1c2d3e4f5a6b";
    public override string Name => "rec";
    public override string Description => "Get provisioning parameter recommendations for a file share based on desired storage size";
    public override string Title => "Get File Share Provisioning Recommendation";
    public override ToolMetadata Metadata => new()
    {
        Destructive = false,
        Idempotent = true,
        OpenWorld = false,
        ReadOnly = true,
        Secret = false,
        LocalRequired = false
    };

    /// <inheritdoc />
    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(FileSharesOptionDefinitions.Location.AsRequired());
        command.Options.Add(FileSharesOptionDefinitions.ProvisionedStorageGiB.AsRequired());
    }

    /// <inheritdoc />
    protected override FileShareGetProvisioningRecommendationOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Location = parseResult.GetValueOrDefault<string>(FileSharesOptionDefinitions.Location.Name);
        options.ProvisionedStorageGiB = parseResult.GetValueOrDefault<int>(FileSharesOptionDefinitions.ProvisionedStorageGiB.Name);
        return options;
    }

    /// <inheritdoc />
    public override async Task<CommandResponse> ExecuteAsync(
        CommandContext context,
        ParseResult parseResult,
        CancellationToken cancellationToken)
    {
        var options = BindOptions(parseResult);

        try
        {
            _logger.LogInformation("Getting provisioning recommendation for subscription {Subscription} in location {Location} with storage {StorageGiB} GiB",
                options.Subscription, options.Location, options.ProvisionedStorageGiB);

            var result = await _service.GetProvisioningRecommendationAsync(
                options.Subscription!,
                options.Location!,
                options.ProvisionedStorageGiB!.Value,
                options.Tenant!,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(result, FileSharesJsonContext.Default.FileShareProvisioningRecommendationResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting provisioning recommendation. Options: {@Options}", options);
            HandleException(context, ex);
        }

        return context.Response;
    }
}

