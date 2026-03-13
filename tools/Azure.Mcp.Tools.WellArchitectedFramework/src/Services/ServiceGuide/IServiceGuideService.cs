// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.WellArchitectedFramework.Services.ServiceGuide;

public interface IServiceGuideService
{
    /// <summary>
    /// Gets the service guide URL for the specified service name.
    /// </summary>
    /// <param name="serviceName">The service name (case-insensitive; spaces and hyphens are normalized).</param>
    /// <returns>The service guide URL if found; otherwise, null.</returns>
    string? GetServiceGuideUrl(string serviceName);

    /// <summary>
    /// Gets all supported service names.
    /// </summary>
    /// <returns>List of all supported service names in alphabetically sorted order.</returns>
    List<string> GetAllServiceNames();
}
