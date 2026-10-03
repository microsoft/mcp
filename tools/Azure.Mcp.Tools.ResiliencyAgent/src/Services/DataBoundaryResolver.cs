// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using Azure.Identity;
using Azure.Mcp.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Services.Azure.Authentication;

namespace Azure.Mcp.Tools.ResiliencyAgent.Services;

/// <summary>
/// Resolves the signed-in tenant's immutable ARM data boundary and maps it to the A2A routing header.
/// </summary>
/// <remarks>
/// Successful values are cached for the stdio process. Transient ARM failures are retried; unknown,
/// malformed, or exhausted responses fail closed with a generic customer-facing error so no A2A
/// request can be routed without an authoritative boundary.
/// </remarks>
public sealed class DataBoundaryResolver : IDataBoundaryResolver, IDisposable
{
    private const string BoundaryEndpoint =
        "https://management.azure.com/providers/Microsoft.Resources/dataBoundaries/default?api-version=2024-08-01";
    private const string ArmScope = "https://management.azure.com/.default";

    private readonly ILogger<DataBoundaryResolver> _logger;
    private readonly HttpClient _httpClient;
    private readonly SemaphoreSlim _gate = new(1, 1);
    private string? _cachedBoundary;

    public DataBoundaryResolver(
        IAzureTokenCredentialProvider tokenCredentialProvider,
        ILogger<DataBoundaryResolver> logger)
    {
        _logger = logger;
        _httpClient = new HttpClient(
            new AccessTokenHandler(tokenCredentialProvider, [ArmScope])
            {
                InnerHandler = new HttpClientHandler(),
            })
        {
            Timeout = TimeSpan.FromSeconds(30),
        };
    }

    internal DataBoundaryResolver(HttpClient httpClient, ILogger<DataBoundaryResolver> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    /// <summary>
    /// Returns <c>eu</c> for an ARM <c>EU</c> boundary or <c>row</c> for <c>Global</c>.
    /// </summary>
    public async Task<string> ResolveAsync(CancellationToken cancellationToken)
    {
        if (_cachedBoundary is not null)
        {
            return _cachedBoundary;
        }

        await _gate.WaitAsync(cancellationToken);
        try
        {
            if (_cachedBoundary is not null)
            {
                return _cachedBoundary;
            }

            for (int attempt = 1; attempt <= 3; attempt++)
            {
                try
                {
                    using HttpResponseMessage response =
                        await _httpClient.GetAsync(BoundaryEndpoint, cancellationToken);
                    string body = await response.Content.ReadAsStringAsync(cancellationToken);
                    if (response.IsSuccessStatusCode)
                    {
                        try
                        {
                            using JsonDocument document = JsonDocument.Parse(body);
                            string? boundary = document.RootElement
                                .GetProperty("properties")
                                .GetProperty("dataBoundary")
                                .GetString();

                            string? mappedBoundary = boundary?.ToUpperInvariant() switch
                            {
                                "EU" => "eu",
                                "GLOBAL" => "row",
                                _ => null,
                            };

                            if (mappedBoundary is not null)
                            {
                                _cachedBoundary = mappedBoundary;
                                return _cachedBoundary;
                            }

                            _logger.LogError(
                                "ARM returned unsupported tenant data-boundary value {Boundary}.",
                                boundary ?? "<null>");
                        }
                        catch (Exception ex) when (
                            ex is JsonException or KeyNotFoundException or InvalidOperationException)
                        {
                            _logger.LogError(ex, "ARM returned a malformed tenant data-boundary response.");
                        }

                        break;
                    }

                    if (!IsTransient(response.StatusCode) || attempt == 3)
                    {
                        _logger.LogError(
                            "Tenant data-boundary lookup failed with HTTP {StatusCode}. Body: {Body}",
                            (int)response.StatusCode,
                            Truncate(body));
                        break;
                    }
                }
                catch (Exception ex) when (
                    ex is HttpRequestException or TaskCanceledException or AuthenticationFailedException)
                {
                    if (attempt == 3)
                    {
                        _logger.LogError(ex, "Tenant data-boundary lookup failed after retries.");
                        break;
                    }
                }

                await Task.Delay(TimeSpan.FromMilliseconds(250 * attempt * attempt), cancellationToken);
            }

            throw new InvalidOperationException(
                "The Azure Resiliency Agent could not prepare this request. Try again.");
        }
        finally
        {
            _gate.Release();
        }
    }

    private static bool IsTransient(HttpStatusCode statusCode) =>
        statusCode is HttpStatusCode.RequestTimeout
            or HttpStatusCode.TooManyRequests
            or HttpStatusCode.InternalServerError
            or HttpStatusCode.BadGateway
            or HttpStatusCode.ServiceUnavailable
            or HttpStatusCode.GatewayTimeout;

    private static string Truncate(string value) =>
        value.Length <= 500 ? value : value[..500] + "...";

    public void Dispose()
    {
        _httpClient.Dispose();
        _gate.Dispose();
    }
}
