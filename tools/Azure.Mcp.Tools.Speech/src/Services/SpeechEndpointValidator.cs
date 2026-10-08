// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Security;
using Azure.Mcp.Core.Services.Azure;
using Azure.ResourceManager;
using Microsoft.Mcp.Core.Helpers;

namespace Azure.Mcp.Tools.Speech.Services;

/// <summary>
/// Centralizes endpoint construction and validation for Speech command options, recognizers, and synthesizers.
/// </summary>
/// <remarks>
/// Keeps Speech-specific URI shape checks and fixed operation paths together rather than duplicating them in each
/// service. Domain authorization is delegated to <see cref="EndpointValidator"/> and its cloud-specific allow-lists.
/// Services use the operation-specific CreateAndValidate methods to obtain the URI to pass to their SDK or HTTP client.
/// Early command-option validation accepts any supported cloud only for feedback; the selected service must still
/// validate its operation endpoint against the configured cloud before credential acquisition or network use.
/// </remarks>
internal static class SpeechEndpointValidator
{
    private const string FastTranscriptionApiVersion = "2024-11-15";

    // Command option validation does not know which Azure cloud the service will use. Keep this list explicit so
    // early validation accepts only clouds whose Speech endpoint suffixes are intentionally supported.
    // Azure Speech documents the Government and 21Vianet endpoint formats here:
    // https://learn.microsoft.com/azure/ai-services/speech-service/sovereign-clouds
    private static readonly ArmEnvironment[] s_supportedArmEnvironments =
    [
        ArmEnvironment.AzurePublicCloud,
        ArmEnvironment.AzureChina,
        ArmEnvironment.AzureGovernment
    ];

    /// <summary>
    /// Determines whether an endpoint is valid for at least one supported Azure cloud.
    /// </summary>
    /// <param name="endpointValidator">The host's endpoint validator for early option feedback.</param>
    /// <param name="endpoint">The user-supplied HTTPS resource-root URL.</param>
    /// <returns>Whether at least one supported cloud authorizes the resource-root endpoint.</returns>
    /// <remarks>
    /// This provides early command-validation feedback only. The service boundary validates the endpoint again
    /// against the configured cloud and propagates any validation failure before network-capable work begins.
    /// </remarks>
    internal static bool IsValidForAnySupportedCloud(IEndpointValidator endpointValidator, string endpoint)
    {
        Uri serviceEndpoint;
        try
        {
            serviceEndpoint = CreateServiceEndpoint(endpoint);
        }
        catch (SecurityException)
        {
            return false;
        }
        catch (ArgumentException)
        {
            return false;
        }

        foreach (ArmEnvironment armEnvironment in s_supportedArmEnvironments)
        {
            try
            {
                endpointValidator.ValidateAzureServiceEndpoint(
                    endpoint: serviceEndpoint.AbsoluteUri,
                    serviceType: "speech",
                    armEnvironment: armEnvironment);
                return true;
            }
            catch (SecurityException)
            {
                // A host rejected for this cloud may belong to another supported cloud, so continue the explicit
                // list. If no cloud accepts it, the method reports one command-validation failure by returning false.
            }
            catch (ArgumentException)
            {
                // The shared Speech allow-list is common to every cloud, so a configuration failure cannot succeed
                // on another iteration. Early option validation reports it as an invalid option; authoritative
                // service-boundary calls propagate the exception.
                return false;
            }
        }

        return false;
    }

    /// <summary>
    /// Creates and validates the resource-root endpoint used by Azure AI Speech realtime transcription.
    /// </summary>
    /// <param name="azureService">The Azure service using this host's immutable endpoint policy.</param>
    /// <param name="endpoint">The user-supplied HTTPS resource-root URL.</param>
    /// <returns>The parsed URI after resource-root shape checks and shared endpoint validation.</returns>
    /// <remarks>
    /// Combines <see cref="CreateServiceEndpoint"/> for URI shape checks with
    /// <see cref="ValidateAuthorizedHttpsEndpoint"/> for the shared cloud-specific domain policy. No additional
    /// hostname-label requirements are imposed, and validation does not establish that a Speech resource exists.
    /// The Speech SDK permits nonstandard paths and gives endpoint query parameters precedence over SDK properties:
    /// <see href="https://learn.microsoft.com/dotnet/api/microsoft.cognitiveservices.speech.speechconfig.fromendpoint"/>.
    /// Requiring a resource-root URL is this tool's input contract, not a general Speech SDK limitation; it prevents
    /// MCP input from overriding the operation paths and query parameters chosen by the tool.
    /// </remarks>
    /// <exception cref="ArgumentException">
    /// Thrown when the endpoint is empty or the Speech endpoint allow-list is unavailable.
    /// </exception>
    /// <exception cref="SecurityException">
    /// Thrown when the endpoint is malformed, is not HTTPS, is outside the configured cloud's Speech domains,
    /// or includes URL components outside this tool's resource-root contract.
    /// </exception>
    internal static Uri CreateAndValidateRealtimeTranscriptionEndpoint(IAzureService azureService, string endpoint)
    {
        Uri serviceEndpoint = CreateServiceEndpoint(endpoint);
        ValidateAuthorizedHttpsEndpoint(azureService, serviceEndpoint);
        return serviceEndpoint;
    }

    /// <summary>
    /// Creates and validates the completed Azure AI Speech fast-transcription request endpoint.
    /// </summary>
    /// <param name="azureService">The Azure service using this host's immutable endpoint policy.</param>
    /// <param name="endpoint">The user-supplied HTTPS resource endpoint.</param>
    /// <returns>The validated fast-transcription request URI.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when the endpoint is empty or the Speech endpoint allow-list is unavailable.
    /// </exception>
    /// <exception cref="SecurityException">
    /// Thrown when the resource endpoint or completed request endpoint is invalid or unauthorized.
    /// </exception>
    internal static Uri CreateAndValidateFastTranscriptionEndpoint(IAzureService azureService, string endpoint)
    {
        Uri serviceEndpoint = CreateServiceEndpoint(endpoint);
        // Azure documents this fixed path and API-version query for synchronous fast transcription:
        // https://learn.microsoft.com/azure/ai-services/speech-service/fast-transcription-create
        Uri transcriptionEndpoint = new UriBuilder(serviceEndpoint)
        {
            Path = "/speechtotext/transcriptions:transcribe",
            Query = $"api-version={FastTranscriptionApiVersion}"
        }.Uri;

        ValidateAuthorizedHttpsEndpoint(azureService, transcriptionEndpoint);
        return transcriptionEndpoint;
    }

    /// <summary>
    /// Creates and validates the WebSocket endpoint used by Azure AI Speech text-to-speech synthesis.
    /// </summary>
    /// <param name="azureService">The Azure service using this host's immutable endpoint policy.</param>
    /// <param name="endpoint">The user-supplied HTTPS resource endpoint.</param>
    /// <returns>The validated WSS synthesis endpoint.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when the endpoint is empty or the Speech endpoint allow-list is unavailable.
    /// </exception>
    /// <exception cref="SecurityException">
    /// Thrown when the resource endpoint or derived WebSocket endpoint is not authorized for the configured cloud.
    /// </exception>
    internal static Uri CreateAndValidateWebSocketEndpoint(IAzureService azureService, string endpoint)
    {
        Uri serviceEndpoint = CreateServiceEndpoint(endpoint);
        // SpeechConfig.FromEndpoint supports nonstandard resource paths. This exact TTS WebSocket route and the
        // local-MCP traffic tag are the established integration added in microsoft/mcp#1431:
        // https://github.com/microsoft/mcp/pull/1431
        // Derive them from the shape-checked resource root so no caller-controlled path, query, port, or fragment is retained.
        Uri websocketEndpoint = new UriBuilder(serviceEndpoint)
        {
            Scheme = "wss",
            Port = -1,
            Path = "/tts/cognitiveservices/websocket/v1",
            Query = "traffictype=localmcp"
        }.Uri;

        // The Speech SDK consumes WSS, while EndpointValidator deliberately authorizes HTTPS Azure service URLs.
        // Project the completed WSS URI to HTTPS solely for validation so the shared cloud allow-list, namespace
        // bypass, and error behavior authorize the exact final destination host without changing the SDK endpoint.
        // See core/Microsoft.Mcp.Core/src/Helpers/EndpointValidator.cs for that HTTPS validator contract.
        Uri validationEndpoint = new UriBuilder(websocketEndpoint)
        {
            Scheme = Uri.UriSchemeHttps,
            Port = -1
        }.Uri;
        ValidateAuthorizedHttpsEndpoint(azureService, validationEndpoint);

        return websocketEndpoint;
    }

    /// <summary>
    /// Parses the supplied resource endpoint and checks its URI shape without authorizing its destination domain.
    /// </summary>
    /// <param name="endpoint">The user-supplied HTTPS resource-root URL.</param>
    /// <returns>
    /// An absolute HTTPS URI with the default port and no user information, non-root path, query, or fragment.
    /// This URI is not yet authorized for network use.
    /// </returns>
    /// <remarks>
    /// Callers must reuse the returned URI, or derive an operation URI from it using fixed paths and queries, then call
    /// <see cref="ValidateAuthorizedHttpsEndpoint"/> for the configured cloud before acquiring credentials or creating
    /// a network client. Parsing and domain authorization are separate so operation helpers can construct their final
    /// endpoint before authorizing it once, rather than checking both the resource root and the derived endpoint.
    /// </remarks>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="endpoint"/> is null, empty, or whitespace.
    /// </exception>
    /// <exception cref="SecurityException">
    /// Thrown when the endpoint is not an absolute URI or does not meet the HTTPS resource-root shape described above.
    /// </exception>
    private static Uri CreateServiceEndpoint(string endpoint)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(endpoint);

        if (!Uri.TryCreate(endpoint, UriKind.Absolute, out Uri? endpointUri))
        {
            throw new SecurityException("Speech endpoint is not a valid absolute URI.");
        }

        // Enforce the tool's resource-root contract before deriving an operation endpoint. As documented above,
        // SpeechConfig.FromEndpoint supports nonstandard paths and caller-supplied query overrides.
        if (endpointUri.Scheme != Uri.UriSchemeHttps ||
            !string.IsNullOrEmpty(endpointUri.UserInfo) ||
            !endpointUri.IsDefaultPort ||
            endpointUri.AbsolutePath != "/" ||
            !string.IsNullOrEmpty(endpointUri.Query) ||
            !string.IsNullOrEmpty(endpointUri.Fragment))
        {
            throw new SecurityException("Speech endpoint must be an HTTPS resource root without user information, a custom port, path, query, or fragment.");
        }

        return endpointUri;
    }

    /// <summary>
    /// Checks a completed HTTPS endpoint against the shared Speech domain policy for the specified Azure cloud.
    /// </summary>
    /// <param name="azureService">The Azure service using this host's immutable endpoint policy.</param>
    /// <param name="endpoint">The parsed HTTPS endpoint to authorize without modifying or replacing it.</param>
    /// <remarks>
    /// This method returns no URI and delegates scheme and domain authorization to <see cref="EndpointValidator"/>,
    /// including its configured namespace-bypass behavior. It does not enforce the resource-root shape: operation
    /// endpoints legitimately contain paths and queries. Callers must first use <see cref="CreateServiceEndpoint"/>
    /// on the supplied resource URL, then construct any fixed operation path and query before calling this method.
    /// After successful validation, reuse that completed URI for the request. For WSS, validate its HTTPS projection
    /// as shown in <see cref="CreateAndValidateWebSocketEndpoint"/> and retain the original WSS URI for the Speech SDK.
    /// </remarks>
    /// <exception cref="ArgumentException">
    /// Thrown when the shared validator cannot find the Speech endpoint allow-list.
    /// </exception>
    /// <exception cref="SecurityException">
    /// Thrown when the shared validator rejects the endpoint's scheme or domain for the specified cloud.
    /// </exception>
    private static void ValidateAuthorizedHttpsEndpoint(IAzureService azureService, Uri endpoint)
    {
        azureService.ValidateAzureServiceEndpoint(
            endpoint: endpoint.AbsoluteUri,
            serviceType: "speech");
    }
}
