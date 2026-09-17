// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.NetAppFiles.Models;
using Azure.Mcp.Tools.NetAppFiles.Options.Pool;
using Azure.Mcp.Tools.NetAppFiles.Services;
using Azure.Mcp.Tools.NetAppFiles.Validation;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.NetAppFiles.Commands.Pool;

[CommandMetadata(
    Id = "81710f89-6069-45df-9c5d-44a6063330d9",
    Name = "get",
    Title = "Get Azure NetApp Files Capacity Pool",
    Description = "Gets a capacity pool by name from an Azure NetApp Files account. Returns the pool configuration, resource ID, location, and provisioning state.",
    OperationPlane = ToolOperationPlane.Control,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class PoolGetCommand(
    ILogger<PoolGetCommand> logger,
    INetAppFilesPoolService service,
    ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<PoolGetOptions, PoolGetCommand.PoolGetResult>(subscriptionResolver)
{
    private readonly ILogger<PoolGetCommand> _logger = logger;
    private readonly INetAppFilesPoolService _service = service;

    public override async Task<CommandResponse> ExecuteAsync(
        CommandContext context,
        PoolGetOptions options,
        CancellationToken cancellationToken)
    {
        try
        {
            var pool = await _service.GetPoolAsync(
                options.Account,
                options.Pool,
                options.ResourceGroup,
                options.Subscription!,
                options.Tenant,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new PoolGetResult(pool),
                NetAppFilesJsonContext.Default.PoolGetResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error getting Azure NetApp Files capacity pool. Account: {Account}, Pool: {Pool}, ResourceGroup: {ResourceGroup}, Subscription: {Subscription}",
                options.Account,
                options.Pool,
                options.ResourceGroup,
                options.Subscription);
            HandleException(context, ex);
        }

        return context.Response;
    }

    public override void ValidateOptions(PoolGetOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);

        if (!AccountNameValidator.IsValid(options.Account))
        {
            validationResult.Errors.Add(AccountNameValidator.ErrorMessage);
        }

        if (!Validation.PoolNameValidator.IsValid(options.Pool))
        {
            validationResult.Errors.Add(Validation.PoolNameValidator.ErrorMessage);
        }
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        KeyNotFoundException => "Resource group not found. Verify the resource group exists and you have access.",
        RequestFailedException requestFailedException when requestFailedException.Status == (int)HttpStatusCode.Forbidden =>
            "Authorization failed getting the Azure NetApp Files capacity pool. Verify that you have the required RBAC permissions.",
        RequestFailedException requestFailedException when requestFailedException.Status == (int)HttpStatusCode.NotFound =>
            "Azure NetApp Files capacity pool not found. Verify the account, pool, and resource group names.",
        _ => base.GetErrorMessage(ex)
    };

    public record PoolGetResult(NetAppFilesPool Pool);
}
