// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.ManagedCleanroom.Options.Collaborations;
using Azure.Mcp.Tools.ManagedCleanroom.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Models.Option;

namespace Azure.Mcp.Tools.ManagedCleanroom.Commands.Collaborations;

[CommandMetadata(
    Id = "0d6a0a0e-7a3a-4a7c-8e3f-2c0d2cfb91a1",
    Name = "list",
    Title = "List Cleanroom Collaborations",
    Description = "Lists Azure Cleanroom collaborations the calling user participates in via the Cleanroom Analytics Frontend service. Returns the full collaboration details from the service.",
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class CollaborationsListCommand(ILogger<CollaborationsListCommand> logger, IManagedCleanroomServiceDataPlane service)
    : AuthenticatedCommand<CollaborationsListOptions, CollaborationsListCommand.CollaborationsListCommandResult>
{
    private readonly ILogger<CollaborationsListCommand> _logger = logger;
    private readonly IManagedCleanroomServiceDataPlane _service = service;

    public override async Task<CommandResponse> ExecuteAsync(
        CommandContext context, CollaborationsListOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _service.ListCollaborationsAsync(
                options.Endpoint,
                options.ActiveOnly,
                options.AllowUntrustedCert,
                options.TokenScope,
                options.Tenant,
                cancellationToken).ConfigureAwait(false);

            context.Response.Results = ResponseResult.Create(
                result,
                ManagedCleanroomJsonContext.Default.JsonElement);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error listing cleanroom collaborations. Endpoint: {Endpoint} ActiveOnly: {ActiveOnly}",
                options.Endpoint, options.ActiveOnly);
            HandleException(context, ex);
        }

        return context.Response;
    }

    public record CollaborationsListCommandResult;
}

