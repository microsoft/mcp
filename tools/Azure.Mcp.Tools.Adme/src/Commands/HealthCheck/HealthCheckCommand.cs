// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

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
        Check Microsoft Entra authentication and platform connectivity for an Azure Data Manager for Energy
        (ADME) endpoint and data partition. Use this first when other ADME tools fail, to tell a sign-in or
        token problem apart from a wrong endpoint, wrong data partition, or blocked network path.

        Required: --endpoint and --data-partition.
        Optional: --include-auth, --include-connectivity

        --include-auth acquires a token for the ADME scope. --include-connectivity calls the ADME storage
        info endpoint with that token, so it implies the auth check and is skipped when auth fails. Specify
        at least one of the two; with neither, no checks are performed.

        Returns: authOk plus authError, connectivityOk plus connectivityError, and the HTTP
        connectivityStatusCode returned by ADME (401/403 points at auth or entitlements, 404 usually means a
        bad endpoint, and other 4xx often means an unknown data partition).
        """,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    LocalRequired = false,
    Secret = false)]
public sealed class HealthCheckCommand(IHealthService healthService)
    : BaseCommand<HealthCheckOptions, HealthCheckCommand.HealthCheckCommandResult>
{
    private readonly IHealthService _healthService = healthService;

    public override void ValidateOptions(HealthCheckOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);

        if (!options.IncludeAuth && !options.IncludeConnectivity)
        {
            validationResult.Errors.Add("Specify at least one of --include-auth or --include-connectivity.");
        }
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
                options.IncludeAuth,
                options.IncludeConnectivity,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new HealthCheckCommandResult(
                    result.AuthOk,
                    result.AuthError,
                    result.ConnectivityOk,
                    result.ConnectivityError,
                    result.ConnectivityStatusCode),
                AdmeJsonContext.Default.HealthCheckCommandResult);
        }
        catch (Exception ex)
        {
            HandleException(context, ex);
        }

        return context.Response;
    }

    /// <summary>
    /// Represents the outcome of ADME authentication and connectivity checks.
    /// </summary>
    public sealed record HealthCheckCommandResult(
        bool AuthOk,
        string? AuthError,
        bool ConnectivityOk,
        string? ConnectivityError,
        int? ConnectivityStatusCode);
}
