// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Adme.Models;
using Azure.Mcp.Tools.Adme.Options;
using Azure.Mcp.Tools.Adme.Services;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.Adme.Commands.HealthCheck;

/// <summary>
/// Checks authentication and connectivity for an ADME instance.
/// </summary>
[CommandMetadata(
    Id = "1f2b6c8a-3d4e-4f5a-9b6c-7d8e9f0a1b2c",
    Name = "check",
    Title = "Check ADME Health",
    Description = """
        Check Microsoft Entra authentication and platform connectivity for an endpoint and data partition.
        Use this first when other tools fail, to tell a sign-in or token problem apart from a wrong endpoint,
        wrong data partition, or blocked network path.

        Required: --endpoint and --data-partition.

        Returns: authOk plus authError, connectivityOk plus connectivityError, and the HTTP
        connectivityStatusCode returned by the service (401/403 points at auth or entitlements, 404 usually means a
        bad endpoint, and other 4xx often means an unknown data partition).
        """,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    LocalRequired = false,
    Secret = false)]
public sealed class HealthCheckCommand(IHealthService healthService)
    : BaseCommand<HealthCheckOptions, HealthCheckResult>
{
    private readonly IHealthService _healthService = healthService;

    public override void ValidateOptions(HealthCheckOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);
        AdmeServiceHelper.ValidateTarget(options.Endpoint, options.DataPartition, validationResult);
    }

    /// <summary>
    /// Executes the requested ADME health checks.
    /// </summary>
    public override async Task<CommandResponse> ExecuteAsync(
        CommandContext context, HealthCheckOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _healthService.CheckHealthAsync(
                options.Endpoint,
                options.DataPartition,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                result,
                AdmeJsonContext.Default.HealthCheckResult);
        }
        catch (Exception ex)
        {
            HandleException(context, ex);
        }

        return context.Response;
    }
}
