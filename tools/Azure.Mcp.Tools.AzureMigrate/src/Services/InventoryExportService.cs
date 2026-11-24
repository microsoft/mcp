// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.AzureMigrate.Services;

/// <summary>
/// Gets a fresh read SAS link for an inventory output from the Discovery Service export
/// (POST /mcprestapi/sessions/{id}/export), forwarding the caller's bearer token unchanged.
/// </summary>
public interface IInventoryExportService
{
    /// <summary>Exports the offline collector ZIP (Discovery Service 'artifacts=internal') of the session.</summary>
    Task<InventoryExportResult> ExportOfflineAsync(string sessionId, string token, CancellationToken cancellationToken);
}

/// <summary>
/// Result of an inventory export. <see cref="Uri"/> is a read SAS URL, so it is left out of <see cref="ToString"/>.
/// <see cref="HttpStatus"/> is null on a network error or timeout; <see cref="Error"/> is set when there is no link.
/// </summary>
public sealed record InventoryExportResult(int? HttpStatus, string? Uri, string? Error)
{
    /// <inheritdoc/>
    public override string ToString() => $"InventoryExportResult {{ HttpStatus = {HttpStatus}, Error = {Error} }}";
}

/// <summary>
/// Calls the Discovery Service export for the signed-in user's inventory session.
/// </summary>
public sealed class InventoryExportService(ILogger<InventoryExportService> logger) : IInventoryExportService
{
    private readonly ILogger<InventoryExportService> _logger = logger;

    /// <inheritdoc/>
    public async Task<InventoryExportResult> ExportOfflineAsync(string sessionId, string token, CancellationToken cancellationToken)
    {
        DiscoveryServiceApi.DsResponse response = await DiscoveryServiceApi.SendAsync(
            _logger, HttpMethod.Post,
            DiscoveryServiceApi.ExportUrl(DiscoveryServiceApi.BaseUrl, sessionId, DiscoveryServiceApi.ToArtifacts(DiscoveryServiceApi.OfflineExportOutput)),
            token, null, Guid.NewGuid().ToString(), "export", cancellationToken);
        if (response.Status is not (>= 200 and < 300))
        {
            return new(response.Status, null, response.Error);
        }

        string? uri = DiscoveryServiceApi.ReadValue(response.Body, "uri");
        return string.IsNullOrWhiteSpace(uri)
            ? new(response.Status, null, "The Discovery Service export response did not include a download URL.")
            : new(response.Status, uri, null);
    }
}
