// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.WellArchitectedFramework.Services.UsageGuide;

public interface IUsageGuideService
{
    /// <summary>
    /// Gets the Azure Well-Architected Framework usage guide content.
    /// </summary>
    /// <returns>The usage guide content as a string.</returns>
    string GetUsageGuide();
}
