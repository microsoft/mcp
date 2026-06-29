// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using AnalyticsFrontendAPI;
using Azure;
using Azure.Core;
using Azure.Core.Pipeline;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.Mcp.Tools.ManagedCleanroom.Commands;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.ManagedCleanroom.Services;

public class ManagedCleanroomDataPlaneService(ISubscriptionService subscriptionService, ITenantService tenantService, IHttpClientFactory httpClientFactory)
    : BaseAzureResourceService(subscriptionService, tenantService), IManagedCleanroomServiceDataPlane
{
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));

    public async Task<JsonElement> ListCollaborationsAsync(
        string endpoint,
        bool? activeOnly = null,
        string? tokenScope = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        var client = await BuildClientAsync(endpoint, tokenScope, tenant, cancellationToken)
            .ConfigureAwait(false);

        var requestContext = new RequestContext { CancellationToken = cancellationToken };
        Response response = await client.GetGetsAsync(activeOnly, requestContext).ConfigureAwait(false);

        return ParseResponse(response);
    }

    private async Task<CollaborationClient> BuildClientAsync(
        string endpoint,
        string? tokenScope,
        string? tenant,
        CancellationToken cancellationToken)
    {
        ValidateRequiredParameters((nameof(endpoint), endpoint));

        if (!Uri.TryCreate(endpoint, UriKind.Absolute, out var endpointUri))
        {
            throw new ArgumentException($"Endpoint '{endpoint}' is not a valid absolute URI.", nameof(endpoint));
        }

        if (endpointUri.Scheme != Uri.UriSchemeHttps)
        {
            throw new ArgumentException("Endpoint must use HTTPS.", nameof(endpoint));
        }

        var credential = await GetCredential(tenant, cancellationToken).ConfigureAwait(false);
        var options = new CollaborationClientOptions();
        var scope = ResolveTokenScope(tokenScope, TenantService.CloudConfiguration.ArmEnvironment.DefaultScope);
        options.AddPolicy(
            new BearerTokenAuthenticationPolicy(credential, scope),
            HttpPipelinePosition.PerCall);

        var testProxyUrl = Environment.GetEnvironmentVariable("TEST_PROXY_URL");
        if (!string.IsNullOrWhiteSpace(testProxyUrl))
        {
            options.Transport = new HttpClientTransport(_httpClientFactory.CreateClient());
        }
        else
        {
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            };
            options.Transport = new HttpClientTransport(handler);
        }

        return new CollaborationClient(endpointUri, options);
    }

    internal static string ResolveTokenScope(string? tokenScope, string defaultScope)
    {
        if (!string.IsNullOrWhiteSpace(tokenScope))
        {
            return tokenScope;
        }

        return defaultScope;
    }

    internal static Uri BuildCollaborationsListUri(Uri endpointUri, bool? activeOnly)
    {
        var builder = new UriBuilder(endpointUri);
        var path = builder.Path;
        if (string.IsNullOrEmpty(path) || path == "/")
        {
            builder.Path = "/gets";
        }
        else
        {
            builder.Path = path.TrimEnd('/') + "/gets";
        }

        if (activeOnly.HasValue)
        {
            var activeOnlyQuery = $"activeOnly={(activeOnly.Value ? "true" : "false")}";
            builder.Query = string.IsNullOrEmpty(builder.Query)
                ? activeOnlyQuery
                : builder.Query.TrimStart('?') + "&" + activeOnlyQuery;
        }

        return builder.Uri;
    }

    private static JsonElement ParseResponse(Response response)
    {
        if (response.Content is null)
        {
            return default;
        }

        return JsonSerializer.Deserialize(
            response.Content.ToMemory().Span,
            ManagedCleanroomJsonContext.Default.JsonElement);
    }
}