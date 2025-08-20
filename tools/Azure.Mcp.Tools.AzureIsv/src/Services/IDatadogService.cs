// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.AzureIsv.Services;

public interface IDatadogService
{
    /// <summary>
    /// Lists the monitored resources for a given Datadog resource in a specific resource group and subscription.
    /// </summary>
    /// <param name="resourceGroup">The name of the resource group containing the Datadog resource.</param>
    /// <param name="subscription">The subscription ID or name where the resource group resides.</param>
    /// <param name="datadogResource">The name of the Datadog resource to query.</param>
    /// <returns>A list of monitored resources.</returns>
    /// <exception cref="AuthenticationFailedException">Thrown when authentication fails.</exception>
    /// <exception cref="RequestFailedException">Thrown when the service request fails.</exception>
    Task<List<string>> ListMonitoredResources(string resourceGroup, string subscription, string datadogResource);
}
