// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Core;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.NetAppFiles.Models;
using Azure.Mcp.Tools.NetAppFiles.Options.Volume;
using Azure.Mcp.Tools.NetAppFiles.Services;
using Azure.Mcp.Tools.NetAppFiles.Validation;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.NetAppFiles.Commands.Volume;

[CommandMetadata(
    Id = "675fbc79-bf1a-4664-b6d8-f3bdf972800e",
    Name = "create",
    Title = "Create Azure NetApp Files Volume",
    Description = "Creates an NFSv3 Azure NetApp Files volume in a capacity pool. Requires the account, pool, volume, location, delegated subnet resource ID, quota in GiB, service level, resource group, and subscription. Returns the created volume details.",
    OperationPlane = ToolOperationPlane.Control,
    Destructive = true,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class VolumeCreateCommand(
    ILogger<VolumeCreateCommand> logger,
    INetAppFilesVolumeService service,
    ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<VolumeCreateOptions, VolumeCreateCommand.VolumeCreateResult>(subscriptionResolver)
{
    private const long MinimumQuotaGib = 50;
    private const long MaximumQuotaGib = 100 * 1024;
    private static readonly HashSet<string> ServiceLevels = new(StringComparer.OrdinalIgnoreCase)
    {
        "Standard",
        "Premium",
        "Ultra"
    };

    private readonly ILogger<VolumeCreateCommand> _logger = logger;
    private readonly INetAppFilesVolumeService _service = service;

    public override async Task<CommandResponse> ExecuteAsync(
        CommandContext context,
        VolumeCreateOptions options,
        CancellationToken cancellationToken)
    {
        try
        {
            var volume = await _service.CreateVolumeAsync(
                options.Account,
                options.Pool,
                options.Volume,
                options.Location,
                options.SubnetId,
                options.QuotaGib,
                options.ServiceLevel,
                options.ResourceGroup,
                options.Subscription!,
                options.Tenant,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new VolumeCreateResult(volume),
                NetAppFilesJsonContext.Default.VolumeCreateResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error creating Azure NetApp Files volume. Account: {Account}, Pool: {Pool}, Volume: {Volume}, ResourceGroup: {ResourceGroup}, Subscription: {Subscription}",
                options.Account,
                options.Pool,
                options.Volume,
                options.ResourceGroup,
                options.Subscription);
            HandleException(context, ex);
        }

        return context.Response;
    }

    public override void ValidateOptions(VolumeCreateOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);

        if (!AccountNameValidator.IsValid(options.Account))
        {
            validationResult.Errors.Add(AccountNameValidator.ErrorMessage);
        }

        if (!NetAppFilesChildResourceNameValidator.IsValid(options.Pool))
        {
            validationResult.Errors.Add("--pool must be 1-64 characters, start and end with an alphanumeric character, and contain only alphanumerics, underscores, and hyphens.");
        }

        if (!NetAppFilesChildResourceNameValidator.IsValid(options.Volume))
        {
            validationResult.Errors.Add("--volume must be 1-64 characters, start and end with an alphanumeric character, and contain only alphanumerics, underscores, and hyphens.");
        }

        if (options.QuotaGib is < MinimumQuotaGib or > MaximumQuotaGib)
        {
            validationResult.Errors.Add($"--quota-gib must be between {MinimumQuotaGib} and {MaximumQuotaGib} GiB.");
        }

        if (!ServiceLevels.Contains(options.ServiceLevel))
        {
            validationResult.Errors.Add("--service-level must be Standard, Premium, or Ultra.");
        }

        if (!ResourceIdentifier.TryParse(options.SubnetId, out var subnetId) || subnetId is null ||
            !string.Equals(subnetId.ResourceType.ToString(), "Microsoft.Network/virtualNetworks/subnets", StringComparison.OrdinalIgnoreCase))
        {
            validationResult.Errors.Add("--subnet-id must be a valid Azure subnet resource ID.");
        }
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        KeyNotFoundException => "The resource group was not found. Verify it exists and you have access.",
        RequestFailedException requestFailedException when requestFailedException.Status == (int)HttpStatusCode.Conflict =>
            "The Azure NetApp Files volume could not be created because of a resource conflict. Verify the account, capacity pool, volume name, and subnet state.",
        RequestFailedException requestFailedException when requestFailedException.Status == (int)HttpStatusCode.Forbidden =>
            "Authorization failed creating the Azure NetApp Files volume. Verify that you have the required RBAC permissions.",
        RequestFailedException requestFailedException when requestFailedException.Status == (int)HttpStatusCode.NotFound =>
            "The resource group, Azure NetApp Files account, capacity pool, or subnet was not found.",
        _ => base.GetErrorMessage(ex)
    };

    public record VolumeCreateResult(NetAppFilesVolume Volume);
}