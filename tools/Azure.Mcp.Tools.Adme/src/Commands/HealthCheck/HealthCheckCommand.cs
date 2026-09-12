// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Adme.Models;
using Azure.Mcp.Tools.Adme.Options;
using Azure.Mcp.Tools.Adme.Services;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.Adme.Commands.HealthCheck;

/// <summary>
/// Checks authentication and connectivity for an ADME/OSDU instance.
/// </summary>
[CommandMetadata(
    Id = "1f2b6c8a-3d4e-4f5a-9b6c-7d8e9f0a1b2c",
    Name = "check",
    Title = "Check ADME/OSDU Health",
    Description = """
        Check an ADME/OSDU endpoint's health, authentication and connectivity.
        Returns health status with error details and the service HTTP status code.
        """,
    OperationPlane = ToolOperationPlane.Data,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    LocalRequired = false,
    Secret = false)]
public sealed class HealthCheckCommand(IHealthService healthService)
    : AuthenticatedCommand<HealthCheckOptions, HealthCheckResult>
{
    private readonly IHealthService _healthService = healthService;

    public override void ValidateOptions(HealthCheckOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);
        AdmeServiceValidator.ValidateTarget(options.Endpoint, options.DataPartition, validationResult);
    }

    /// <summary>
    /// Executes the requested ADME/OSDU health checks.
    /// </summary>
    public override async Task<CommandResponse> ExecuteAsync(
        CommandContext context, HealthCheckOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _healthService.CheckHealthAsync(
                options.Endpoint,
                options.DataPartition,
                options.Tenant,
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
