// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Core;
using Azure.Core.Pipeline;
using Azure.ResourceManager;

namespace Azure.Mcp.Tools.ResilienceManagement.Services;

internal sealed class ResilienceManagementRoutingPolicy : HttpPipelineSynchronousPolicy
{
    internal const string EndpointEnvironmentVariable = "AZURE_RESILIENCY_ARM_ENDPOINT";
    internal const string RoutingLocationEnvironmentVariable = "AZURE_RESILIENCY_ROUTING_LOCATION";
    private const string LegacyEndpointEnvironmentVariable = "AZURE_RESILIENCY_DRILL_ARM_ENDPOINT";
    private const string LegacyRoutingLocationEnvironmentVariable =
        "AZURE_RESILIENCY_DRILL_ROUTING_LOCATION";
    private const string RoutingLocationHeader = "x-ms-rhub-routing-location";

    private readonly Uri? _endpoint;
    private readonly string? _routingLocation;

    public ResilienceManagementRoutingPolicy()
        : this(ReadConfiguration())
    {
    }

    private ResilienceManagementRoutingPolicy((string? Endpoint, string? RoutingLocation) configuration)
        : this(configuration.Endpoint, configuration.RoutingLocation)
    {
    }

    internal ResilienceManagementRoutingPolicy(string? endpoint, string? routingLocation)
    {
        if (string.IsNullOrWhiteSpace(endpoint) && string.IsNullOrWhiteSpace(routingLocation))
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(endpoint) || string.IsNullOrWhiteSpace(routingLocation))
        {
            throw new InvalidOperationException(
                $"{EndpointEnvironmentVariable} and {RoutingLocationEnvironmentVariable} must be configured together.");
        }

        if (!Uri.TryCreate(endpoint, UriKind.Absolute, out var parsedEndpoint) ||
            !string.Equals(parsedEndpoint.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase) ||
            parsedEndpoint.AbsolutePath != "/" ||
            !string.IsNullOrEmpty(parsedEndpoint.Query) ||
            !string.IsNullOrEmpty(parsedEndpoint.Fragment))
        {
            throw new InvalidOperationException(
                $"{EndpointEnvironmentVariable} must be an HTTPS origin without a path, query, or fragment.");
        }

        _endpoint = parsedEndpoint;
        _routingLocation = routingLocation.Trim();
    }

    public override void OnSendingRequest(HttpMessage message)
    {
        if (_endpoint is null || _routingLocation is null)
        {
            return;
        }

        var original = message.Request.Uri.ToUri();
        var routed = new UriBuilder(original)
        {
            Scheme = _endpoint.Scheme,
            Host = _endpoint.Host,
            Port = _endpoint.IsDefaultPort ? -1 : _endpoint.Port
        };

        message.Request.Uri.Reset(routed.Uri);
        message.Request.Headers.SetValue(RoutingLocationHeader, _routingLocation);
    }

    internal static ArmClientOptions CreateArmClientOptions()
    {
        var options = new ArmClientOptions();
        options.AddPolicy(new ResilienceManagementRoutingPolicy(), HttpPipelinePosition.PerCall);
        return options;
    }

    private static (string? Endpoint, string? RoutingLocation) ReadConfiguration()
    {
        string? endpoint = Environment.GetEnvironmentVariable(EndpointEnvironmentVariable);
        string? routingLocation =
            Environment.GetEnvironmentVariable(RoutingLocationEnvironmentVariable);
        if (!string.IsNullOrWhiteSpace(endpoint) ||
            !string.IsNullOrWhiteSpace(routingLocation))
        {
            return (endpoint, routingLocation);
        }

        return (
            Environment.GetEnvironmentVariable(LegacyEndpointEnvironmentVariable),
            Environment.GetEnvironmentVariable(LegacyRoutingLocationEnvironmentVariable));
    }
}
