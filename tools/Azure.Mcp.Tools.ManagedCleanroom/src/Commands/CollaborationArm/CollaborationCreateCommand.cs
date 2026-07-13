// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.ManagedCleanroom.Options.CollaborationArm;
using Azure.Mcp.Tools.ManagedCleanroom.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.ManagedCleanroom.Commands.CollaborationArm;

[CommandMetadata(
    Id = "e247b9e0-2d87-43a7-8e5d-57eea22237a3",
    Name = "create",
    Title = "Create Cleanroom Collaboration",
    Description = """
        Creates an Azure Cleanroom collaboration ARM resource in the specified resource group and location.
        Returns immediately once the request is accepted by ARM. Provisioning runs in the background and typically takes ~25 minutes.
        You can check the status by asking to get the collaboration by name once the request is accepted.
        """,
    Destructive = false,
    Idempotent = false,
    OpenWorld = true,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class CollaborationCreateCommand(
    ILogger<CollaborationCreateCommand> logger,
    IManagedCleanroomServiceControlPlane service,
    ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<CollaborationCreateOptions, JsonElement>(subscriptionResolver)
{
    private readonly ILogger<CollaborationCreateCommand> _logger = logger;
    private readonly IManagedCleanroomServiceControlPlane _service = service;

    public override async Task<CommandResponse> ExecuteAsync(
        CommandContext context, CollaborationCreateOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _service.CreateCollaborationArmResourceAsync(
                options.Name,
                options.ResourceGroup,
                options.Subscription!,
                options.Location,
                options.ResourceLocation,
                options.Collaborators,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken).ConfigureAwait(false);

            context.Response.Message = result.Message;
            context.Response.Results = ResponseResult.Create(
                result.Properties,
                ManagedCleanroomJsonContext.Default.JsonElement);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error creating cleanroom collaboration. Name: {Name}, ResourceGroup: {ResourceGroup}, Subscription: {Subscription}",
                options.Name, options.ResourceGroup, options.Subscription);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Conflict =>
            "A collaboration with this name already exists in the resource group.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden =>
            $"Authorization failed creating the collaboration. Details: {reqEx.Message}",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.NotFound =>
            "Requested resource was not found. Verify the resource group, subscription, and access.",
        RequestFailedException reqEx => reqEx.Message,
        _ => base.GetErrorMessage(ex)
    };

}
